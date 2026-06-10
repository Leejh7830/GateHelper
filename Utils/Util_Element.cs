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
        /// 우측 데이터 영역에 정밀 포커스를 주어 0.5초 만에 전체 데이터를 클립보드로 덤프 후 파싱합니다.
        /// </summary>
        public static List<string[]> GetTableDataByClipboardFast(IWebDriver driver)
        {
            var uniqueKeys = new HashSet<string>();
            var allData = new List<string[]>();

            try
            {
                // 1. Lee님이 완벽하게 검증하신 데이터 영역의 정확한 부모 컨테이너 지정
                string dataAreaPath = "//*[@id='uncontrolled-tab-example-tabpane-WEB030102']/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]";
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var dataArea = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(dataAreaPath)));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                // 진짜 물리적 스크롤바 컨테이너 추출
                IWebElement scrollContainer = (IWebElement)js.ExecuteScript(
                    "let el = arguments[0]; " +
                    "while (el && el.tagName !== 'BODY') { " +
                    "   if (el.scrollHeight > el.clientHeight && window.getComputedStyle(el).overflowY !== 'visible') { " +
                    "       return el; " +
                    "   } " +
                    "   el = el.parentElement; " +
                    "} " +
                    "return arguments[0].parentElement;", dataArea);


                string rowPath = $"{dataAreaPath}/div[contains(@class, 'wj-row') or @role='row']";
                long lastScrollTop = -1;
                int sameCount = 0;

                // 최대 25회 PageDown 루프로 완주하도록 가드 설정
                for (int step = 0; step < 25; step++)
                {
                    // 현재 화면(가상 뷰포트)에 바인딩된 데이터 행 확보
                    var rows = driver.FindElements(By.XPath(rowPath));
                    if (rows.Count == 0)
                    {
                        rows = driver.FindElements(By.XPath("//*[@id='uncontrolled-tab-example-tabpane-WEB030102']//div[contains(@class, 'wj-row')]"));
                    }

                    int previousCount = allData.Count;

                    foreach (var row in rows)
                    {
                        var cells = row.FindElements(By.XPath("./div"));
                        if (cells.Count >= 5)
                        {
                            string name = cells[0].Text.Trim();
                            if (string.IsNullOrEmpty(name) || name == "Name" || name == "Value") continue;

                            if (!uniqueKeys.Contains(name))
                            {
                                uniqueKeys.Add(name);
                                allData.Add(new string[] {
                            name,
                            cells[1].Text.Trim(),
                            cells[2].Text.Trim(),
                            cells[3].Text.Trim(),
                            cells[4].Text.Trim()
                        });
                            }
                        }
                    }

                    // 물리적 최하단 바닥 검증 (가상화 오류로 인한 조기 종료 방지)
                    long currentScrollTop = (long)js.ExecuteScript("return Math.ceil(arguments[0].scrollTop);", scrollContainer);
                    long scrollHeight = (long)js.ExecuteScript("return arguments[0].scrollHeight;", scrollContainer);
                    long clientHeight = (long)js.ExecuteScript("return arguments[0].clientHeight;", scrollContainer);

                    if (currentScrollTop == lastScrollTop || (currentScrollTop + clientHeight >= scrollHeight - 3))
                    {
                        sameCount++;
                        if (sameCount >= 2) break; // 진짜 바닥 안착 시 즉시 루프 탈출
                    }
                    else
                    {
                        lastScrollTop = currentScrollTop;
                    }

                    // 가상화 엔진이 공식 지원하는 PageDown 단축키 이벤트를 주입하여 한 페이지 단위 초고속 점프
                    js.ExecuteScript("arguments[0].focus();", scrollContainer);
                    js.ExecuteScript(
                        "var e = new KeyboardEvent('keydown', { key: 'PageDown', keyCode: 34, bubbles: true }); " +
                        "arguments[0].dispatchEvent(e);", scrollContainer);

                    // 💡 속도 극대화 튜닝: 대기 시간을 가상화가 깨지지 않는 최저 마진인 0.3초(300ms)로 단축
                    Thread.Sleep(300);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"[스크롤 에러] 데이터 수집 중 치명적 예외 발생: {ex.Message}", Level.Error);
            }

            return allData;
        }


        /// <summary>
        /// 요소를 화면 중앙으로 스크롤한 뒤 클릭합니다. ElementNotInteractable 예외를 원천 차단합니다.
        /// </summary>
        public static async Task<bool> ScrollAndClickAsync(IWebDriver driver, IWebElement element, int delayAfterClickMs = 1000)
        {
            if (element == null || !element.Displayed) return false;
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                await Task.Delay(300); // UI 스크롤 안정화 대기
                element.Click();
                await Task.Delay(delayAfterClickMs);
                return true;
            }
            catch
            {
                return false;
            }
        }


        ////////////////////////////////////////////////////통합모니터링(Management) 전용 끝/////////////////////////////////////////////////////////////////


        /// //////////////////////////////////////////////////////////////////////////////////////////////////
        public static void FindAndAlertElement(IWebDriver driver, string xpath)
        {
            try
            {
                // 최대 5초 동안 요소가 화면에 나타날 때까지 기다립니다.
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

                // ElementIsVisible은 요소가 존재하고, 화면에 표시될 때까지 기다립니다.
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpath)));

                // 성공적으로 기다렸다면, 요소가 발견된 것입니다.
                string message = $"XPath '{xpath}'에 해당하는 요소를 발견했습니다.";
                LogManager.LogMessage(message, Level.Info);
                MessageBox.Show(message, "요소 발견", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (WebDriverTimeoutException)
            {
                // 5초 안에 요소를 찾지 못하면 이 예외가 발생합니다.
                string message = $"XPath '{xpath}'에 해당하는 요소를 찾을 수 없습니다.";
                LogManager.LogMessage(message, Level.Error);
                MessageBox.Show(message, "요소 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // 그 외 예상치 못한 오류가 발생한 경우
                LogManager.LogException(ex, Level.Error, $"요소 검색 중 예상치 못한 오류 발생: '{xpath}'");
                MessageBox.Show($"오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void FindElementAndShowMessage(IWebDriver driver, string query)
        {
            try
            {
                // XPath인지 CSS Selector인지 자동 판별
                By by = query.StartsWith("/") || query.StartsWith(".") ? By.XPath(query) : By.CssSelector(query);

                // 현재 페이지에서 검색
                if (FindElementInCurrentContext(driver, by))
                {
                    MessageBox.Show($"요소를 찾았습니다: {query}", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 모든 iframe을 순회하면서 검색
                var allFrames = driver.FindElements(By.TagName("iframe"));
                foreach (var frame in allFrames)
                {
                    try
                    {
                        driver.SwitchTo().Frame(frame);
                        if (FindElementInCurrentContext(driver, by))
                        {
                            MessageBox.Show($"요소를 찾았습니다 (iframe 내부): {query}", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"iframe 전환 실패: {ex.Message}");
                    }
                    finally
                    {
                        driver.SwitchTo().DefaultContent();
                    }
                }

                // 요소를 찾지 못한 경우
                MessageBox.Show($"요소를 찾을 수 없습니다: {query}", "실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 현재 컨텍스트에서 요소를 찾는 함수
        private static bool FindElementInCurrentContext(IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElements(by).Count > 0;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// //////////////////////////////////////////////////////////////////////////////////////////////////

    }
}
