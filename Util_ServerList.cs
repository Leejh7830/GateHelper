using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace GateBot
{
    internal class Util_ServerList
    {
        // 웹 페이지 HTML 가져오기
        public static string GetHtmlFromWeb(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        // HTML 파싱
        public static List<string> ParseServerNamesFromHtml(string html)
        {
            var serverNames = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // tr 태그 찾기
            var trNodes = doc.DocumentNode.SelectNodes("//tr");
            if (trNodes != null)
            {
                foreach (var trNode in trNodes)
                {
                    // 4번째 td 태그 찾기 (인덱스는 0부터 시작)
                    var tdNode = trNode.SelectSingleNode(".//td[4]");
                    if (tdNode != null)
                    {
                        // td 태그의 텍스트 내용 가져오기
                        string serverName = tdNode.InnerText.Trim();
                        serverNames.Add(serverName);
                    }
                }
            }
            return serverNames;
        }

        // 콤보박스에 서버 이름 추가
        public static void AddServersToComboBox(ComboBox comboBox, List<string> serverNames)
        {
            comboBox.Items.Clear(); // 콤보박스 초기화
            comboBox.Items.AddRange(serverNames.ToArray()); // 콤보박스에 서버 이름 추가
        }
    }
}
