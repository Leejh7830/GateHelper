using BrightIdeasSoftware;
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
        public static bool EnterTestMode(Form form, Control TabSelector1)
        {
            DialogResult result = MessageBox.Show("테스트 모드로 전환할까요?", "테스트 모드", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                form.Size = MainUI.TestFormExtendedSize;
                TabSelector1.Size = new Size(520, 32);
                return true;
            }
            else
            {
                MessageBox.Show("테스트 모드가 취소되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        public static void LoadTestServers(ComboBox comboBox)
        {
            List<string> testServers = new List<string>
            {
                "Test A 11111",
                "Test B 22222",
                "Test C 33333",
                "Test D 44444",
                "Test E 55555",
                "Test F 66666"
            };

            comboBox.Items.Clear();
            foreach (string server in testServers)
            {
                comboBox.Items.Add(server);
            }
        }

        // ListView에 서버 접속 정보 추가 (일반 listview)
        public static void SimulateServerConnect(Form form, ObjectListView listView, ComboBox comboBox, bool testMode, bool isDuplicateCheck)
        {
            try
            {
                // ⭐ 이 메서드는 testMode 값을 읽기만 하므로 수정할 필요가 없습니다.
                if (testMode)
                {
                    string selectedServer = comboBox.SelectedItem.ToString();
                    LogMessage($"테스트 모드: 서버 '{selectedServer}'에 접속 시도", Level.Info);

                    Util_ServerList.AddServerToListView(listView, selectedServer, DateTime.Now, isDuplicateCheck, 30);
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

        // ListView에 서버 접속 정보 추가 (ObjectListView 사용)
        public static void SimulateServerConnect(Form form, ObjectListView listView, ComboBox comboBox, ref bool testMode)
        {
            try
            {
                if (testMode)
                {
                    string selectedServer = comboBox.SelectedItem.ToString();
                    LogMessage($"테스트 모드: 서버 '{selectedServer}'에 접속 시도", Level.Info);

                    AddServerToHistoryListView(listView, selectedServer, DateTime.Now, "ListView 추가 완료");
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


        private static void AddServerToHistoryListView(ObjectListView listView, string serverName, DateTime lastConnectedTime, string userMemo, bool isFavorite = false)
        {
            var serverInfo = new Util_ServerList.ServerInfo
            {
                No = listView.GetItemCount() + 1, // int로 직접 할당
                ServerName = serverName,
                LastConnected = lastConnectedTime, // DateTime으로 직접 할당
                Memo = userMemo,
                IsFavorite = isFavorite // ⭐ 즐겨찾기 상태 할당
            };

            listView.AddObject(serverInfo);
            listView.Invalidate();

            LogMessage($"Server '{serverName}' added to history list.", Level.Info);
        }



    }
}
