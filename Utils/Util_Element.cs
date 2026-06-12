using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    class Util_Element
    {
        // [MGMT] 이전 복사본을 기억하여 고스트 카피를 판별하기 위한 해시 메모리
        private static string _lastCopiedHash = string.Empty;

        public static bool ClickElementByXPath(IWebDriver driver, string xpath)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                IWebElement element = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(xpath)));

                element.Click();
                return true;
            }
            catch (NoSuchElementException ex)
            {
                // XPath에 해당하는 요소를 찾을 수 없는 경우
                LogException(ex, Level.Error, $"클릭 오류: XPath '{xpath}' - 요소를 찾을 수 없습니다.");
                return false;
            }
            catch (WebDriverTimeoutException ex)
            {
                // 요소를 찾았지만 클릭 가능한 상태가 되지 않은 경우
                LogException(ex, Level.Error, $"클릭 오류: XPath '{xpath}' - 요소가 클릭 가능한 상태가 아닙니다.");
                return false;
            }
            catch (Exception ex)
            {
                // 그 외 예상치 못한 모든 오류
                LogException(ex, Level.Error, $"클릭 오류: XPath '{xpath}' - 예상치 못한 오류 발생.");
                return false;
            }
        }

        /// <summary>
        /// Xpath 요소에 값을 입력하는 메서드
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xpath"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SendKeysToElement(IWebDriver driver, string xpath, string value)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                element.Clear();
                element.SendKeys(value);
                return true;
            }
            catch (WebDriverTimeoutException ex)
            {
                // 설정시간안에 요소를 찾지 못했거나 보이지 않는 경우
                LogException(ex, Level.Error, $"키 입력 오류: XPath '{xpath}' - 요소를 찾지 못했습니다.");
                return false;
            }
            catch (ElementNotInteractableException ex)
            {
                // 요소는 찾았지만, 입력할 수 없는 상태인 경우
                LogException(ex, Level.Error, $"키 입력 오류: XPath '{xpath}' - 요소가 상호작용 불가능한 상태입니다.");
                return false;
            }
            catch (Exception ex)
            {
                // 그 외 예상치 못한 모든 오류
                LogException(ex, Level.Error, $"키 입력 오류: XPath '{xpath}' - 예상치 못한 오류 발생.");
                return false;
            }
        }

        public static void WaitForElementLoadByXPath(IWebDriver driver, string xpath, int timeoutSeconds = 30)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));
            }
            catch (WebDriverTimeoutException ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"XPath 요소 로딩 시간 초과 (제한 시간: {timeoutSeconds}초)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 예외 다시 던지기
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                MessageBox.Show($"XPath 요소 로딩 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 예외 다시 던지기
            }
        }

        public static bool ClickElementByKeyword(IWebDriver driver, string keyword)
        {
            try
            {
                string xpath = $"//span[@class='wj-node-text' and contains(text(), '{keyword}')]";
                return ClickElementByXPath(driver, xpath);
            }
            catch (Exception ex)
            {
                // 유틸리티 클래스 상단에 'using static GateHelper.LogManager;'가 있다면 바로 호출 가능합니다.
                LogMessage($"[Keyword Click Error] 키워드 '{keyword}' 클릭 중 오류: {ex.Message}", Level.Error);
                return false;
            }
        }

        ////////////////////////////////////////////////////통합모니터링(Management) 전용 시작/////////////////////////////////////////////////////////////////

        /// <summary>
        /// (1) 최종 호출 메서드: 클립보드에서 텍스트를 추출한 후 엑셀 저장을 위한 List<string[]> 형태로 파싱합니다.
        /// </summary>
        public static async Task<List<string[]>> GetTableDataByClipboardFast(IWebDriver driver)
        {
            List<string[]> parsedData = new List<string[]>();

            // 정확한 데이터 표 컨테이너 지정
            string gridXPath = "//*[@id='uncontrolled-tab-example-tabpane-WEB030102']/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]";

            // 클립보드 하이재킹 모듈 실행
            string rawText = await ExtractDataViaClipboardAsync(driver, gridXPath);

            if (string.IsNullOrWhiteSpace(rawText)) return parsedData;

            // 복사된 텍스트 파싱 (\r\n 또는 \n 으로 엔터키 기준 행 분리)
            string[] rows = rawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string row in rows)
            {
                // 탭(\t) 기호로 열 분리
                string[] columns = row.Split('\t');

                // 배열 앞뒤 공백 및 찌꺼기 문자 정제
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i] = columns[i].Trim().Trim('"');
                }

                // 헤더(Name, Value 등)가 포함되었다면 제외
                if (columns.Length > 0 && (columns[0] == "Name" || columns[0] == "Value"))
                    continue;

                parsedData.Add(columns);
            }
            // [메모리 최적화] 거대 문자열 변수들의 연결을 끊고 가비지 컬렉터(GC)를 통해 즉시 RAM에서 강제 소각
            rawText = null;
            rows = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return parsedData;
        }


        /// <summary>
        /// (2) 엔진 메서드: 그리드를 클릭하고 Ctrl+A, Ctrl+C를 보내 클립보드 텍스트를 탈취합니다.
        /// </summary>
        private static async Task<string> ExtractDataViaClipboardAsync(IWebDriver driver, string gridXPath)
        {
            try
            {
                var gridElement = driver.FindElement(By.XPath(gridXPath));
                string rawData = string.Empty;
                int maxAttempts = 3; // 로딩 지연 시 최대 3회 재시도

                // 💡 [방어 1] 데이터 로딩 지연을 극복하기 위한 재시도 루프
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    await ScrollAndClickAsync(driver, gridElement, 500);

                    RunInSTA(() => Clipboard.Clear());

                    // 단축키 전송 (전체 선택 -> 복사)
                    new OpenQA.Selenium.Interactions.Actions(driver)
                        .KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("a").KeyUp(OpenQA.Selenium.Keys.Control)
                        .KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("c").KeyUp(OpenQA.Selenium.Keys.Control)
                        .Perform();

                    // 클립보드에 데이터가 찰 때까지 최대 3초 대기
                    int waitCount = 0;
                    bool hasText = false;
                    while (waitCount < 15)
                    {
                        await Task.Delay(200);
                        RunInSTA(() => hasText = Clipboard.ContainsText());
                        if (hasText) break;
                        waitCount++;
                    }

                    if (hasText)
                    {
                        RunInSTA(() => {
                            rawData = Clipboard.GetText();
                            Clipboard.Clear();
                        });
                    }

                    // 💡 [방어 2] 속도 저하 차단: ESC 키를 전송하여 브라우저의 파란색 블록(전체 선택) 강제 해제
                    new OpenQA.Selenium.Interactions.Actions(driver).SendKeys(OpenQA.Selenium.Keys.Escape).Perform();

                    // 💡 [방어 3] 중복 데이터 차단: 지금 복사한 내용이 방금 전 호기의 데이터와 100% 동일한지 검증
                    if (attempt < maxAttempts && !string.IsNullOrEmpty(rawData) && rawData == _lastCopiedHash)
                    {
                        LogManager.LogMessage($"[데이터 동기화 지연] 이전 표의 데이터가 복사되었습니다. 웹 로딩을 대기하고 재시도합니다. ({attempt}/{maxAttempts})", LogManager.Level.Warning);
                        await Task.Delay(1500); // 1.5초 서버 응답 대기 후 다시 시도
                        continue;
                    }
                    else
                    {
                        _lastCopiedHash = rawData; // 검증 통과 시 새 데이터로 메모리 갱신
                        break; // 정상 수집 루프 탈출
                    }
                }

                // C# 메모리 릭(가비지 컬렉터 부하) 방지
                GC.Collect();

                return rawData;
            }
            catch (Exception ex)
            {
                LogManager.LogMessage($"클립보드 데이터 추출 실패: {ex.Message}", LogManager.Level.Error);
                return string.Empty;
            }
        }


        /// <summary>
        /// (3) 시스템 방어 메서드: C#의 비동기 스레드에서 클립보드에 접근할 때 발생하는 
        /// ThreadStateException을 원천 차단하기 위해 STA(단일 스레드) 아파트를 강제 생성합니다.
        /// </summary>
        private static void RunInSTA(Action action)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                try { action(); } catch { }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        /// <summary>
        /// (4) 3단 방어 스크롤 & 클릭: 스크롤, 일반 클릭, JS 강제 클릭을 혼합하여 재시도합니다.
        /// </summary>
        public static async Task<bool> ScrollAndClickAsync(IWebDriver driver, IWebElement element, int delayAfterClickMs = 1000, int maxRetries = 3)
        {
            if (element == null || !element.Displayed) return false;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    // 1단계: 무조건 화면 중앙으로 스크롤하여 시야에 확보
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                    await Task.Delay(300); // UI 스크롤 안정화 대기

                    try
                    {
                        // 2단계: 표준 클릭 시도
                        element.Click();
                    }
                    catch (ElementClickInterceptedException)
                    {
                        // 3단계: 다른 팝업이나 레이어에 가려졌다면 JS 강제 클릭으로 돌파
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                    }

                    // 클릭이 성공했으므로 지정된 딜레이만큼 대기 후 탈출
                    await Task.Delay(delayAfterClickMs);
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    LogManager.LogMessage("ScrollAndClickAsync: 요소가 DOM에서 유실됨", LogManager.Level.Warning);
                    return false;
                }
                catch (Exception ex)
                {
                    LogManager.LogMessage($"ScrollAndClickAsync 재시도 {i + 1}/{maxRetries} 실패: {ex.Message}", LogManager.Level.Warning);
                }

                await Task.Delay(500); // 쿨타임
            }

            return false;
        }

        ////////////////////////////////////////////////////통합모니터링(Management) 전용 끝/////////////////////////////////////////////////////////////////

    }
}
