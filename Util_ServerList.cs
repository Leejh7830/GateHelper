using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using static GateHelper.LogManager;
using System.Text.Json;

internal class Util_ServerList
{
    private static readonly string _serverFilePath = Path.Combine(Application.StartupPath, "serverData.json");

    [Serializable]
    public class ServerInfo
    {
        public int No { get; set; }
        public string ServerName { get; set; }
        public DateTime LastConnected { get; set; }
        public string Memo { get; set; }
        public bool IsFavorite { get; set; }
    }

    public static void AddServerToListView(ListView listView, string serverName, DateTime lastConnected, bool isDuplicateCheck, int maxCount = 100)
    {
        // 중복 제거 로직은 Tag를 기반으로 수정해야 하지만, 일단 간단한 텍스트 기반으로 유지
        if (isDuplicateCheck)
        {
            for (int i = listView.Items.Count - 1; i >= 0; i--)
            {
                if (listView.Items[i].SubItems[1].Text.Equals(serverName, StringComparison.OrdinalIgnoreCase))
                {
                    listView.Items.RemoveAt(i);
                }
            }
        }

        TrimHistoryList(listView, maxCount);

        // ⭐ 새로운 ServerInfo 객체를 생성하여 ListViewItem에 연결
        var serverInfo = new ServerInfo
        {
            ServerName = serverName,
            LastConnected = lastConnected,
            Memo = ""
        };

        ListViewItem item = new ListViewItem(new[]
        {
            "TEMP",
            serverInfo.ServerName,
            serverInfo.LastConnected.ToString("yyyy-MM-dd HH:mm:ss"),
            serverInfo.Memo
        });

        // ⭐ Tag 속성에 객체 저장
        item.Tag = serverInfo;

        listView.Items.Add(item);
        ReorderListViewItems(listView);
    }

    public static void SaveServerDataToFile(ListView listView)
    {
        try
        {
            List<ServerInfo> serverList = new List<ServerInfo>();

            foreach (ListViewItem item in listView.Items)
            {
                // ⭐ Tag 속성에서 ServerInfo 객체를 가져와 리스트에 추가
                if (item.Tag is ServerInfo serverInfo)
                {
                    serverList.Add(serverInfo);
                }
            }

            string jsonString = JsonSerializer.Serialize(serverList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_serverFilePath, jsonString);
        }
        catch (Exception ex)
        {
            LogMessage($"Failed to save Server Data: {ex.Message}", Level.Error);
        }
    }

    public static void LoadServerDataFromFile(ListView listView)
    {
        try
        {
            if (!File.Exists(_serverFilePath))
            {
                File.WriteAllText(_serverFilePath, "[]");
                return;
            }

            string jsonString = File.ReadAllText(_serverFilePath);
            List<ServerInfo> serverList = JsonSerializer.Deserialize<List<ServerInfo>>(jsonString);

            listView.Items.Clear();

            foreach (var server in serverList)
            {
                ListViewItem item = new ListViewItem(new[]
                {
                    server.No.ToString(),
                    server.ServerName,
                    server.LastConnected.ToString("yyyy-MM-dd HH:mm:ss"),
                    server.Memo
                });

                if (server.IsFavorite)
                {
                    item.Font = new Font(listView.Font, FontStyle.Bold);
                }
                item.Tag = server;

                listView.Items.Add(item);
            }

            LogMessage("Server Data loaded successfully.", Level.Info);
        }
        catch (Exception ex)
        {
            LogMessage($"Failed to load Server Data: {ex.Message}", Level.Error);
        }
    }

    public static void TrimHistoryList(ListView listView, int maxCount)
    {
        while (listView.Items.Count >= maxCount)
        {
            listView.Items.RemoveAt(0);
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