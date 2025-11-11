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

namespace GateHelper
{
    public static class Util_Rdp
    {
        private static readonly object udpLogLock = new object();

        public static void WriteUdpReceiveLog(string msg)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, $"UdpReceiveLog_{DateTime.Now:yyyyMMdd}.txt");
                string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {msg}";

                lock (udpLogLock)
                {
                    File.AppendAllText(logPath, logLine + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"[UDP 로그 기록 오류] {ex.Message}", LogManager.Level.Error);
            }
        }


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

        public static string CreateBroadcastMessage(Config config, string serverName)
        {
            string userId = config?.UserID ?? "UnknownUser";
            string name = serverName ?? "UnknownServer";
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return $"{userId} / {name} / {time}";
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

                                LogManager.LogMessage($"[UDP 수신] {msg}", LogManager.Level.Info);

                                // 메시지 처리 콜백 호출
                                onMessageReceived?.Invoke(msg);
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




    }
}