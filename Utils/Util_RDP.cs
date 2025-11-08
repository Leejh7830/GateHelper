using System;
using System.Diagnostics;
using System.Drawing;
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
        // 현재 PC에서 RDP 클라이언트(mstsc.exe) 실행 여부 반환
        public static bool IsRdpClientRunning()
        {
            return Process.GetProcessesByName("mstsc").Any();
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
                        catch
                        {
                        }
                    }
                }
            });
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }


        public static void StopBroadcastReceiveLoop()
        {
            keepReceiving = false;
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