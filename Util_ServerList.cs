using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    internal class Util_ServerList // ServerList 및 ListView에 관련된 메서드
    {
        // 저장 파일 경로를 클래스 내에 정의
        private static readonly string _serverFilePath = Path.Combine(Application.StartupPath, "serverData.dat");

        // 서버 정보 모델
        [Serializable]
        public class ServerInfo
        {
            public string No { get; set; }
            public string ServerName { get; set; }
            public string LastConnected { get; set; }
            public string Memo { get; set; }
        }

        // 서버 데이터를 ListView에 추가
        // 25.08.19 Added - Remove Duplicate Server
        public static void AddServerToListView(ListView listView, string serverName, string lastConnected, bool isDuplicateCheck, int maxCount = 100)
        {
            // 중복 제거 옵션이 활성화된 경우
            if (isDuplicateCheck)
            {
                int removedCount = 0;

                for (int i = listView.Items.Count - 1; i >= 0; i--)
                {
                    if (listView.Items[i].SubItems[1].Text.Equals(serverName, StringComparison.OrdinalIgnoreCase))
                    {
                        listView.Items.RemoveAt(i);
                        removedCount++;
                    }
                }

                if (removedCount > 1)
                {
                    LogMessage($"Duplicate server entry for '{serverName}' removed. Total: {removedCount}", Level.Info);
                }
            }

            TrimHistoryList(listView, maxCount);

            // 새 항목 추가
            ListViewItem listViewItem = new ListViewItem(new[]
            {
                "TEMP", // 일단 임시로 넣고, 나중에 No 재정렬
                serverName,
                lastConnected,
                "" // Memo 값
            });

            listView.Items.Add(listViewItem);
            ReorderListViewItems(listView); // 재정렬
        }

        // 서버 데이터를 파일로 저장하는 메서드
        public static void SaveServerDataToFile(ListView listView)
        {
            try
            {
                List<ServerInfo> serverList = new List<ServerInfo>();

                foreach (ListViewItem item in listView.Items)
                {
                    var serverInfo = new ServerInfo
                    {
                        No = item.SubItems[0].Text,
                        ServerName = item.SubItems[1].Text,
                        LastConnected = item.SubItems[2].Text,
                        Memo = item.SubItems[3].Text
                    };
                    serverList.Add(serverInfo);
                }

                // 데이터가 없으면 파일을 새로 생성
                using (Stream stream = File.Open(_serverFilePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, serverList);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to save Server Data: {ex.Message}", Level.Error);
            }
        }

        // 서버 데이터를 파일에서 읽어오는 메서드
        public static void LoadServerDataFromFile(ListView listView)
        {
            try
            {
                // 파일이 존재하지 않으면 새로 생성하고 빈 리스트를 저장하고 종료
                if (!File.Exists(_serverFilePath))
                {
                    CreateEmptyServerDataFile();
                    return;
                }

                // 파일이 존재하면 데이터 로드
                using (Stream stream = File.Open(_serverFilePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    List<ServerInfo> serverList = (List<ServerInfo>)formatter.Deserialize(stream);

                    // 리스트뷰에 항목 추가
                    foreach (var server in serverList)
                    {
                        ListViewItem item = new ListViewItem(new[]
                        {
                            server.No,
                            server.ServerName,
                            server.LastConnected,
                            server.Memo
                        });
                        listView.Items.Add(item);
                    }

                    LogMessage("Server Data loaded successfully.", Level.Info);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to load Server Data: {ex.Message}", Level.Error);
            }
        }

        // Data 파일이 없을 경우 빈 파일을 생성
        private static void CreateEmptyServerDataFile()
        {
            try
            {
                List<ServerInfo> emptyServerList = new List<ServerInfo>(); // 빈 서버 리스트 생성

                using (Stream stream = File.Open(_serverFilePath, FileMode.Create)) // 빈 데이터 파일 생성
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, emptyServerList);
                }

                LogMessage("Empty server data file created.", Level.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to create empty server data file: {ex.Message}", Level.Error);
            }
        }

        public static void TrimHistoryList(ListView listView, int maxCount)
        {
            while (listView.Items.Count >= maxCount)
            {
                listView.Items.RemoveAt(0); // 오래된 것 삭제
            }
        }

        public static void ReorderListViewItems(ListView listView)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                listView.Items[i].SubItems[0].Text = (i + 1).ToString();
            }
        }
    }
}
