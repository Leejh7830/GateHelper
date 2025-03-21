using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    internal class Util_Test
    {
        public static void EnterTestMode(Form form, Control TabSelector1, ref bool testModeFlag)
        {
            DialogResult result = MessageBox.Show("테스트 모드로 전환할까요?", "테스트 모드", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                form.Size = new Size(1400, 800);
                TabSelector1.Size = new Size(520, 30);
                testModeFlag = true;
            }
            else
            {
                testModeFlag = false;
                MessageBox.Show("테스트 모드가 취소되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public static void LoadTestServers(ComboBox comboBox)
        {
            List<string> testServers = new List<string>
            {
                "Test A Server1",
                "Test B Server2",
                "Test C Server3",
                "Test D Client1",
                "Test E Client2",
                "Test F Client3"
            };

            comboBox.Items.Clear();
            foreach (string server in testServers)
            {
                comboBox.Items.Add(server);
            }
        }

        // 서버 접속 시뮬레이션 (Connect)
        public static void SimulateServerConnect(Form form, ListView listView, ComboBox comboBox, ref bool testMode)
        {
            try
            {
                if (testMode)
                {
                    string selectedServer = comboBox.SelectedItem.ToString();
                    LogMessage($"테스트 모드: 서버 '{selectedServer}'에 접속 시도", Level.Info);
                    string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    AddServerToHistoryListView(listView, selectedServer, currentTime, "ListView 추가 완료");

                    // MessageBox.Show($"테스트 모드: 서버 '{selectedServer}'에 접속!", "테스트 모드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("테스트 모드가 아닙니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ListView에 서버 접속 정보 추가
        private static void AddServerToHistoryListView(ListView listView, string serverName, string lastConnectedTime, string userMemo)
        {
            ListViewItem listViewItem = new ListViewItem(new[]
            {
                (listView.Items.Count + 1).ToString(), // 현재 항목의 갯수 + 1
                serverName, // Server Name
                lastConnectedTime, // Last Connected Time
                userMemo // User Memo
            });

            listView.Items.Add(listViewItem);
            listView.Invalidate();
        }

    }
}
