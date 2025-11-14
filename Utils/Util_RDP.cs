using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace GateHelper
{
    public static class Util_Rdp
    {
        private static readonly object udpLogLock = new object();
        private static string lastUdpLogMsg = null; // 최근 기록한 메시지
        private static bool _initialConnectSent = false; // 접속 시 송신

        public static void WriteUdpReceiveLog(string msg)
        {
            try
            {
                lock (udpLogLock)
                {
                    // 최근 메시지와 동일하면 기록하지 않음
                    if (msg == lastUdpLogMsg)
                        return;
                    lastUdpLogMsg = msg;

                    string logPath = Path.Combine(Application.StartupPath, "Log");
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }

                    logPath = Path.Combine(logPath, $"UdpReceiveLog_{DateTime.Now:yyyyMMdd}.txt");

                    string logLine;
                    if (Regex.IsMatch(msg, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} \|"))
                    {
                        logLine = msg;
                    }
                    else
                    {
                        logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} (Local) | {msg}";
                    }

                    File.AppendAllText(logPath, logLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"[UDP 로그 기록 오류] {ex.Message}", LogManager.Level.Error);
            }
        }

        // 표준 메시지 송신
        public static void BroadcastSend(Config config, string serverName, int port = 9876)
        {
            string message = CreateBroadcastMessage(config, serverName);
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.EnableBroadcast = true;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                    udpClient.Send(data, data.Length, endPoint);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"브로드캐스트 송신 오류: {ex.Message}", LogManager.Level.Error);
            }
        }

        // 일반 메시지 또는 RAW 메시지 송신 (OVERLOAD)
        public static void BroadcastSend(Config config, string serverNameOrMessage, bool raw = false, int port = 9876)
        {
            string message;
            if (raw)
            {
                message = serverNameOrMessage;
            }
            else
            {
                string userId = config.UserID ?? "UnknownUser";
                string utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " [UTC]";
                message = $"{userId} / {serverNameOrMessage} / {utcNow}";
            }

            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.EnableBroadcast = true;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                    udpClient.Send(data, data.Length, endPoint);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"브로드캐스트 송신 오류: {ex.Message}", LogManager.Level.Error);
            }
        }

        public static string CreateBroadcastMessage(Config config, string serverName)
        {
            string userId = config?.UserID ?? "UnknownUser";
            string name = serverName ?? "UnknownServer";
            string time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // UTC로 송수신
            return $"{userId} / {name} / {time} [UTC]";
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool keepReceiving = false;
        private static Thread receiveThread;
        public static bool IsUdpReceiving
        {
            get { return keepReceiving; }
        }


        public static void StartBroadcastReceiveLoop(Action<string> onMessageReceived, int port = 9876)
        {
            if (keepReceiving) return;
            keepReceiving = true;
            receiveThread = new Thread(() =>
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient(port))
                    {
                        udpClient.EnableBroadcast = true;
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                        while (keepReceiving)
                        {
                            try
                            {
                                byte[] data = udpClient.Receive(ref remoteEP);
                                string msg = Encoding.UTF8.GetString(data);

                                // StartBroadcastReceiveLoop 내에서만 로그 기록, 콜백에서는 기록하지 않도록
                                LogManager.LogMessage($"[UDP 수신] {msg}", LogManager.Level.Info);
                                WriteUdpReceiveLog(msg); // 여기서만 기록
                                onMessageReceived?.Invoke(msg); // 콜백에서는 로그 기록하지 않음
                            }
                            catch (SocketException ex)
                            {
                                // 포트가 이미 사용 중이거나, 네트워크 오류 등
                                LogManager.LogMessage($"[UDP 수신] SocketException: {ex.Message}", LogManager.Level.Error);
                                break;
                            }
                            catch (ObjectDisposedException)
                            {
                                // udpClient가 Dispose된 경우 루프 종료
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogManager.LogMessage($"[UDP 수신] 예외: {ex.Message}", LogManager.Level.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.LogMessage($"[UDP 수신 시작] 예외: {ex.Message}", LogManager.Level.Critical);
                }
            });
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }


        public static void StopBroadcastReceiveLoop()
        {
            keepReceiving = false;
            if (receiveThread != null && receiveThread.IsAlive)
            {
                try
                {
                    receiveThread.Join(500); // 최대 0.5초 대기
                }
                catch (Exception ex)
                {
                    LogManager.LogMessage($"[UDP 수신 종료] 예외: {ex.Message}", LogManager.Level.Error);
                }
                receiveThread = null;
            }
            // 라벨 갱신은 Tick에서 처리
        }

        // UDP 상태 라벨 갱신
        public static void UpdateUDPStatusLabel(bool isOn)
        {
            if (Application.OpenForms.Count > 0)
            {
                var form = Application.OpenForms[0];
                var lbl = form.Controls["lblUDPStatus"] as Label;
                if (lbl != null)
                {
                    if (lbl.InvokeRequired)
                    {
                        lbl.Invoke(new Action(() =>
                        {
                            lbl.Text = isOn ? "UDP ON" : "UDP OFF";
                            lbl.BackColor = isOn ? Color.FromArgb(0x4C, 0xAF, 0x50) : Color.FromArgb(0xF4, 0x43, 0x36); // 초록/빨강
                            lbl.ForeColor = Color.White;
                        }));
                    }
                    else
                    {
                        lbl.Text = isOn ? "UDP ON" : "UDP OFF";
                        lbl.BackColor = isOn ? Color.FromArgb(0x4C, 0xAF, 0x50) : Color.FromArgb(0xF4, 0x43, 0x36);
                        lbl.ForeColor = Color.White;
                    }
                }
            }
        }
        // [UDP] 유저 접속 메시지 송신
        public static void SendInitialConnect(Config config)
        {
            if (!_initialConnectSent)
            {
                BroadcastSend(config, "INITIAL_CONNECT", false);
                _initialConnectSent = true;
            }
        }
        // [UDP] 유저 종료 메시지 송신
        public static void SendExitMessage(Config config)
        {
            string userId = config.UserID ?? "UnknownUser";
            string utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " [UTC]";
            string message = $"{userId} / EXIT / {utcNow}";
            BroadcastSend(config, message, raw: true);
        }

    }
}