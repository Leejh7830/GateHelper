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
using System.Linq;

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

    public static void AddServerToListView(ObjectListView listView, string serverName, DateTime lastConnected, bool isDuplicateCheck, int maxCount = 100)
    {
        if (isDuplicateCheck)
        {
            var existingObject = listView.Objects.Cast<ServerInfo>()
                                        .FirstOrDefault(s => s.ServerName.Equals(serverName, StringComparison.OrdinalIgnoreCase));
            if (existingObject != null)
            {
                listView.RemoveObject(existingObject);
            }
        }

        var serverInfo = new ServerInfo
        {
            ServerName = serverName,
            LastConnected = lastConnected,
            Memo = ""
        };

        listView.AddObject(serverInfo);
    }

    public static void SaveServerDataToFile(ObjectListView listView)
    {
        try
        {
            List<ServerInfo> serverList = listView.Objects.Cast<ServerInfo>().ToList();

            string jsonString = JsonSerializer.Serialize(serverList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_serverFilePath, jsonString);
        }
        catch (Exception ex)
        {
            LogMessage($"Failed to save Server Data: {ex.Message}", Level.Error);
        }
    }

    public static void LoadServerDataFromFile(ObjectListView listView)
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

            listView.SetObjects(serverList);

            LogMessage("Server Data loaded successfully.", Level.Info);
        }
        catch (Exception ex)
        {
            LogMessage($"Failed to load Server Data: {ex.Message}", Level.Error);
        }
    }

}