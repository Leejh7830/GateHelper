using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        // FUNC TEST 버튼 클릭 시 호출: RDP 실행 여부 메시지 안내
        public static void ShowRdpStatus()
        {
            bool isRunning = IsRdpClientRunning();
            string msg = isRunning ? "RDP(원격 데스크톱)가 실행 중." : "RDP(원격 데스크톱) 없음.";
            MessageBox.Show(msg, "RDP 감지 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // UDP 브로드캐스트 송신 테스트
        public static void BroadcastSend(string message, int port = 9876)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.EnableBroadcast = true;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                    udpClient.Send(data, data.Length, endPoint);
                }
                MessageBox.Show("브로드캐스트 송신 완료", "Broadcast Send", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"브로드캐스트 송신 오류: {ex.Message}", "Broadcast Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // UDP 브로드캐스트 수신 1회 테스트 (버튼 클릭 시 사용)
        public static void BroadcastReceiveOnceAsync(int port = 9876)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                try
                {
                    using (UdpClient udpClient = new UdpClient(port))
                    {
                        udpClient.EnableBroadcast = true;
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, port);
                        MessageBox.Show("UDP 브로드캐스트 수신 대기 중...", "Broadcast Receive");

                        byte[] data = udpClient.Receive(ref remoteEP);
                        string msg = Encoding.UTF8.GetString(data);
                        MessageBox.Show($"수신: {msg}", "UDP Broadcast", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"브로드캐스트 수신 오류: {ex.Message}", "Broadcast Receive Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

    }
}