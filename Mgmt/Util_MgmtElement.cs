using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper.Mgmt
{
    /// <summary>
    /// MGMT(통합모니터링) 전용 DOM 탐색, 스크롤, 클립보드 데이터 수집 메서드 모음.
    /// 범용 메서드는 Util_Element.cs에 유지.
    /// </summary>
    public static class Util_MgmtElement
    {
        // 이전 복사본을 기억하여 고스트 카피를 판별하기 위한 해시 메모리
        private static string _lastCopiedHash = string.Empty;

        // MGMT 그리드 XPath 상수 (절대경로 중복 제거)
        public const string GridXPath =
            "//*[@id='uncontrolled-tab-example-tabpane-WEB030102']" +
            "/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]";


        // ─────────────────────────────────────────────────────────────
        // 스크롤 방식 데이터 수집
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// JS 가상 스크롤로 그리드 전체 데이터를 수집합니다.
        /// Dictionary 기반 중복 제거로 스크롤 오버랩 데이터를 안전하게 처리합니다.
        /// </summary>
        public static async Task<List<string[]>> GetTableDataBySmartScrollAsync(IWebDriver driver, string targetXPath = null)
        {
            targetXPath = targetXPath ?? GridXPath;
            try
            {
                var jsExecutor = (IJavaScriptExecutor)driver;
                var targetElement = driver.FindElement(By.XPath(targetXPath));

                // Key: 1열(Name), Value: 줄 전체 데이터 — 중복 덮어쓰기로 무결성 보장
                var rowDict = new Dictionary<string, string>();
                var orderedKeys = new List<string>();

                bool isBottom = false;
                int maxAttempts = 100;
                int attempt = 0;

                // 스크롤 초기화
                jsExecutor.ExecuteScript(@"
                    var c = arguments[0].querySelector('[wj-part=""root""]') || arguments[0];
                    c.scrollTop = 0;
                ", targetElement);
                await Task.Delay(400);

                while (!isBottom && attempt < maxAttempts)
                {
                    attempt++;

                    // 현재 뷰포트 데이터 추출
                    string chunkData = (string)jsExecutor.ExecuteScript(@"
                        var grid = arguments[0];
                        var rows = grid.querySelectorAll('.wj-row:not(.wj-header), tbody tr');
                        var res = '';
                        for (var i = 0; i < rows.length; i++) {
                            var cells = rows[i].querySelectorAll('.wj-cell:not(.wj-header), td');
                            if (cells.length === 0) continue;
                            var rd = [];
                            for (var j = 0; j < cells.length; j++) {
                                rd.push(cells[j].innerText.replace(/\n/g, ' ').trim());
                            }
                            if (rd.length > 0) res += rd.join('\t') + '\n';
                        }
                        return res.trim();
                    ", targetElement);

                    if (!string.IsNullOrWhiteSpace(chunkData))
                    {
                        foreach (var line in chunkData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string cleanLine = line.Trim();
                            if (string.IsNullOrEmpty(cleanLine)) continue;

                            string[] cols = cleanLine.Split('\t');
                            if (cols.Length == 0) continue;

                            string pk = cols[0].Trim();
                            if (string.IsNullOrEmpty(pk)) continue;

                            // 헤더 행 제거
                            if (pk.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                                pk.Equals("Value", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (!rowDict.ContainsKey(pk))
                                orderedKeys.Add(pk);

                            rowDict[pk] = cleanLine; // 최신 데이터로 덮어쓰기
                        }
                    }

                    // 스크롤 이동 (clientHeight × 1.5 — 이동량 증가로 횟수 감소)
                    isBottom = (bool)jsExecutor.ExecuteScript(@"
                        var c = arguments[0].querySelector('[wj-part=""root""]') || arguments[0];
                        var before = Math.ceil(c.scrollTop);
                        c.scrollTop += (c.clientHeight * 1.5);
                        var after = Math.ceil(c.scrollTop);
                        return before === after;
                    ", targetElement);

                    // 데이터가 새로 추가됐을 때만 딜레이 (불필요한 고정 대기 제거)
                    if (!isBottom) await Task.Delay(150);
                }

                var parsedData = orderedKeys
                    .Select(k => rowDict[k].Split('\t'))
                    .ToList();

                LogManager.LogMessage(
                    $"[스마트 스크롤 완료] {orderedKeys.Count}줄 추출 (스크롤 {attempt}회)",
                    LogManager.Level.Info);

                return parsedData;
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"스마트 스크롤 실패: {ex.Message}", LogManager.Level.Error);
                return new List<string[]>();
            }
        }


        // ─────────────────────────────────────────────────────────────
        // 클립보드 방식 데이터 수집 (보조)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Ctrl+A → Ctrl+C 클립보드 방식으로 그리드 데이터를 수집합니다.
        /// 가상 스크롤 그리드에서는 뷰포트 데이터만 복사되므로 보조 수단으로 사용합니다.
        /// </summary>
        public static async Task<List<string[]>> GetTableDataByClipboardFast(IWebDriver driver, string targetXPath = null)
        {
            targetXPath = targetXPath ?? GridXPath;
            var parsedData = new List<string[]>();

            string rawText = await ExtractDataViaClipboardAsync(driver, targetXPath);
            if (string.IsNullOrWhiteSpace(rawText)) return parsedData;

            foreach (var row in rawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] columns = row.Split('\t');
                for (int i = 0; i < columns.Length; i++)
                    columns[i] = columns[i].Trim().Trim('"');

                if (columns.Length > 0 &&
                    (columns[0] == "Name" || columns[0] == "Value"))
                    continue;

                parsedData.Add(columns);
            }

            return parsedData;
        }

        private static async Task<string> ExtractDataViaClipboardAsync(IWebDriver driver, string gridXPath)
        {
            try
            {
                var gridElement = driver.FindElement(By.XPath(gridXPath));
                string rawData = string.Empty;

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    await Util_Element.ScrollAndClickAsync(driver, gridElement, 500);

                    RunInSTA(() => Clipboard.Clear());

                    new OpenQA.Selenium.Interactions.Actions(driver)
                        .KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("a").KeyUp(OpenQA.Selenium.Keys.Control)
                        .KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("c").KeyUp(OpenQA.Selenium.Keys.Control)
                        .Perform();

                    // 클립보드에 데이터가 찰 때까지 최대 3초 대기
                    bool hasText = false;
                    for (int w = 0; w < 15; w++)
                    {
                        await Task.Delay(200);
                        RunInSTA(() => hasText = Clipboard.ContainsText());
                        if (hasText) break;
                    }

                    if (hasText)
                        RunInSTA(() => { rawData = Clipboard.GetText(); Clipboard.Clear(); });

                    new OpenQA.Selenium.Interactions.Actions(driver)
                        .SendKeys(OpenQA.Selenium.Keys.Escape).Perform();

                    // 이전 호기와 동일 데이터면 재시도
                    if (attempt < 3 && !string.IsNullOrEmpty(rawData) && rawData == _lastCopiedHash)
                    {
                        LogManager.LogMessage(
                            $"[클립보드] 이전 데이터와 동일 — 재시도 ({attempt}/3)",
                            LogManager.Level.Warning);
                        await Task.Delay(1500);
                        continue;
                    }

                    _lastCopiedHash = rawData;
                    break;
                }

                return rawData;
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"클립보드 추출 실패: {ex.Message}", LogManager.Level.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// 비동기 스레드에서 클립보드 접근 시 ThreadStateException 방지용 STA 래퍼.
        /// </summary>
        private static void RunInSTA(Action action)
        {
            var thread = new System.Threading.Thread(() =>
            {
                try { action(); } catch { }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }


        // ─────────────────────────────────────────────────────────────
        // 설비 스캔
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// JS querySelectorAll로 설비명 전체 리스트를 반환합니다.
        /// 영문+숫자 혼합이고 4자리 이상 숫자로 끝나는 패턴을 설비로 간주합니다.
        /// 예: J1FSTO12815 ✅  MonitoringSystem ❌  ESHD ❌
        /// </summary>
        public static List<string> ScanMachineList(IWebDriver driver)
        {
            try
            {
                var jsExecutor = (IJavaScriptExecutor)driver;
                // 설비명 패턴: J1FSTO12815, J1FCNV12303 등
                // 영문자 + 숫자 혼합이고 끝부분에 5자리 이상 숫자로 끝나는 노드만 설비로 간주
                // MonitoringSystem, ESHD, Form, Module 등 그룹명 제외
                string jsScript = @"
                    var nodes = document.querySelectorAll('.wj-node-text');
                    var names = [];
                    var pattern = /^[A-Za-z0-9]+\d{4,}$/;
                    for(var i = 0; i < nodes.length; i++) {
                        var text = nodes[i].innerText.trim();
                        if(pattern.test(text)) {
                            names.push(text);
                        }
                    }
                    return names.join(',');
                ";

                string result = (string)jsExecutor.ExecuteScript(jsScript);

                if (string.IsNullOrWhiteSpace(result))
                {
                    LogManager.LogMessage("설비 스캔: 결과 없음 (트리가 펼쳐져 있는지 확인 필요)", LogManager.Level.Warning);
                    return new List<string>();
                }

                var list = result
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                LogManager.LogMessage($"설비 스캔 완료: {list.Count}대 발견", LogManager.Level.Info);
                return list;
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"설비 리스트 스캔 실패: {ex.Message}", LogManager.Level.Error);
                return new List<string>();
            }
        }

        /// <summary>
        /// 설비 타입만 추출합니다. (레거시 호환용 — 현재 미사용, 신규 코드는 ScanMachineList 사용)
        /// Task.Run 안에서 ExecuteScript를 호출하면 스레드 안전하지 않으므로 동기 방식으로 유지.
        /// </summary>
        public static List<string> ScanEquipmentTypes(IWebDriver driver)
        {
            try
            {
                var jsExecutor = (IJavaScriptExecutor)driver;
                string result = (string)jsExecutor.ExecuteScript(@"
                    var nodes = document.querySelectorAll('.wj-node-text');
                    var types = new Set();
                    for(var i = 0; i < nodes.length; i++) {
                        var text = nodes[i].innerText.trim();
                        var pattern = /^[A-Za-z0-9]+\d{4,}$/;
                        if(pattern.test(text)) {
                            var m = text.match(/[A-Za-z]+/g);
                            if(m) types.add(m[m.length-1]);
                        }
                    }
                    return Array.from(types).join(',');
                ");

                if (string.IsNullOrWhiteSpace(result)) return new List<string>();

                return result
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().ToUpper())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"설비 타입 스캔 실패: {ex.Message}", LogManager.Level.Error);
                return new List<string>();
            }
        }


        // ─────────────────────────────────────────────────────────────
        // 클릭 유틸 (MGMT 전용 래퍼 — 범용은 Util_Element.ScrollAndClickAsync)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 이름 기반 XPath로 노드를 직접 찾아 클릭합니다.
        /// </summary>
        public static async Task<IWebElement> FindAndClickNodeByNameAsync(
            IWebDriver driver, string nodeName, int delayMs = 1000)
        {
            string xpath = $"//span[contains(@class,'wj-node-text') and text()='{nodeName}']";
            var element = driver.FindElements(By.XPath(xpath))
                                 .Where(el => el.Displayed)
                                 .FirstOrDefault();

            if (element == null)
            {
                LogManager.LogMessage($"노드 미발견: {nodeName}", LogManager.Level.Warning);
                return null;
            }

            bool clicked = await Util_Element.ScrollAndClickAsync(driver, element, delayMs);
            return clicked ? element : null;
        }
    }
}