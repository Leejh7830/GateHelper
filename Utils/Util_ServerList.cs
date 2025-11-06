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
using System.Text;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace GateHelper
{
    internal class Util_ServerList
    {
        // _meta 내부의 serverData.json 경로를 선언부에서 초기화
        private static readonly string _serverFilePath = Util.GetMetaPath("serverData.json");

        [Serializable]
        public class ServerInfo
        {
            public int No { get; set; }
            public string ServerName { get; set; }
            public DateTime LastConnected { get; set; }
            public string Memo { get; set; }
            public bool IsFavorite { get; set; }
        }

        public static void AddServerToListView(
            ObjectListView listView,
            string serverName,
            DateTime lastConnected,
            bool isDuplicateCheck,
            int maxCount = 100)
        {
            if (listView == null) return;

            // 이름을 먼저 정규화
            serverName = NormalizeServerName(serverName);

            // 현재 목록 스냅샷
            var items = listView.Objects?.OfType<ServerInfo>().ToList() ?? new List<ServerInfo>();

            // 중복제거, 기존 값도 정규화해서 비교(대/소문자 무시)
            var dupes = items
                .Where(s => string.Equals(NormalizeServerName(s.ServerName),
                                          serverName,
                                          StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (isDuplicateCheck && dupes.Count > 0)
            {
                // UPsert: 첫 항목은 살리고 최신 정보로 갱신, 나머지 중복은 삭제
                var keep = dupes[0];

                // 원본 표시명을 정규화된 값으로 덮어 표시 일관성 유지
                keep.ServerName = serverName;
                keep.LastConnected = lastConnected;
                // keep.Memo / keep.IsFavorite 는 그대로 유지

                // 중복 2개 이상이면 나머지 제거
                if (dupes.Count > 1)
                    listView.RemoveObjects(dupes.Skip(1).ToList());

                listView.RefreshObject(keep); // 화면 갱신
            }
            else
            {
                var serverInfo = new ServerInfo // 메모 빈칸, 즐겨찾기 기본 false 으로 추가
                {
                    ServerName = serverName,
                    LastConnected = lastConnected,
                    Memo = string.Empty,
                    IsFavorite = false
                };
                listView.AddObject(serverInfo);
            }

            if (maxCount > 0) // 초과분 제거(즐겨찾기 보호, 오래된 것부터 제거)
            {
                items = listView.Objects?.OfType<ServerInfo>().ToList() ?? new List<ServerInfo>();
                if (items.Count > maxCount)
                {
                    int over = items.Count - maxCount;

                    // 1) 즐겨찾기 아님(false) 우선(앞), 2) LastConnected 오래된 것부터
                    var victims = items
                        .OrderBy(s => s.IsFavorite)
                        .ThenBy(s => s.LastConnected)
                        .Take(over)
                        .ToList();

                    listView.RemoveObjects(victims);
                }
            }
            SortByLastConnected(listView); // 정렬
            // 필요하면 번호 재매김/정렬 갱신
            // listView.BuildList(true);
        }

        private static string NormalizeServerName(string s) // 정규화 작업
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var n = s.Normalize(NormalizationForm.FormKC);
            n = n.Replace('\u00A0', ' '); // NBSP
            n = Regex.Replace(n, @"[\u200B-\u200D\uFEFF]", "");
            n = Regex.Replace(n, @"\s+", " ").Trim();

            return n;
        }

        public static void SortByLastConnected(ObjectListView lv, bool newestAtBottom = true) // 리스트 정렬
        {
            if (lv == null) return;

            var col = lv.AllColumns.FirstOrDefault(c =>
                string.Equals(c.AspectName, "LastConnected", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Text, "LastConnected", StringComparison.OrdinalIgnoreCase));

            if (col == null) return;

            lv.PrimarySortColumn = col;
            lv.PrimarySortOrder = newestAtBottom ? SortOrder.Ascending : SortOrder.Descending;
            // Ascending = 오래된 ↑ → 최신 ↓ (최신이 맨 아래)

            lv.Sort();              // 정렬 실행
                                    // lv.BuildList(true);  // 필요시 강제 리빌드
        }


        public static void SaveServerDataToFile(ObjectListView listView)
        {
            try
            {
                List<ServerInfo> serverList = listView.Objects.Cast<ServerInfo>().ToList();

                // Ensure folder exists
                var dir = Path.GetDirectoryName(_serverFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

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
                // Ensure folder exists
                var dir = Path.GetDirectoryName(_serverFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(_serverFilePath))
                {
                    File.WriteAllText(_serverFilePath, "[]");
                    return;
                }

                string jsonString = File.ReadAllText(_serverFilePath);
                List<ServerInfo> serverList = JsonSerializer.Deserialize<List<ServerInfo>>(jsonString);
                
                listView.Items.Clear();

                listView.SetObjects(serverList);
                SortByLastConnected(listView); // 정렬

                LogMessage("Server Data loaded successfully.", Level.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to load Server Data: {ex.Message}", Level.Error);
            }
        }

        // 현재 로딩된 웹페이지에서 Server Name/IP 목록을 가져오고 메모리 저장
        public static List<(string ServerName, string ServerIP)> GetServerListFromWebPage(IWebDriver driver)
        {
            var serverList = new List<(string ServerName, string ServerIP)>();

            // 모든 tbody 순회
            var tbodys = driver.FindElements(By.XPath("//*[@id='seltable']/tbody"));
            foreach (var tbody in tbodys)
            {
                var rows = tbody.FindElements(By.TagName("tr"));
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 6)
                    {
                        string serverName = cells[3].Text.Trim(); // 4번째 td (index 3) = ServerName
                        string serverIP = cells[5].Text.Trim();   // 6번째 td (index 5) = ServerIP
                        serverList.Add((serverName, serverIP));
                    }
                }
            }
            return serverList;
        }

    }
}