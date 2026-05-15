using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static List<string[]> GetGridTableData(IWebDriver driver)
        {
            var allData = new List<string[]>();
            try
            {
                // 1. 데이터 영역(div)이 나타날 때까지 명시적 대기 (최대 10초)
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                string parentPath = "//*[@id='uncontrolled-tab-example-tabpane-WEB030102']/div/div[2]/div/div/div[3]/div/div/div[1]/div/div[2]/div/div/div[2]/div[1]";

                // 데이터가 들어있는 첫 번째 div가 보일 때까지 기다림
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"{parentPath}/div[3]")));

                // 2. 부모 컨테이너 아래의 모든 직접적인 자식 div들 가져오기
                var rows = driver.FindElements(By.XPath($"{parentPath}/div"));

                foreach (var row in rows)
                {
                    // 3. 행 내부의 셀들 추출 (상대 경로 ./div 사용)
                    var cells = row.FindElements(By.XPath("./div"));

                    // 데이터가 있는 행인지 검사 (열 개수가 5개 이상인 것만)
                    if (cells.Count >= 5)
                    {
                        string name = cells[0].Text.Trim();

                        // 헤더(Name, Value 등 제목)는 제외하고 실제 데이터만 수집
                        if (string.IsNullOrEmpty(name) || name == "Name" || name == "Value") continue;

                        string[] rowData = new string[5];
                        rowData[0] = name;
                        rowData[1] = cells[1].Text.Trim();
                        rowData[2] = cells[2].Text.Trim();
                        rowData[3] = cells[3].Text.Trim();
                        rowData[4] = cells[4].Text.Trim();

                        allData.Add(rowData);
                    }
                }
            }
            catch (Exception ex)
            {
                // 데이터가 하나도 없을 때 에러 로그
                Debug.WriteLine($"[Parsing Error] {ex.Message}");
            }
            return allData;
        }

        public static string ConvertTableToText(List<string[]> tableData)
        {
            if (tableData == null || tableData.Count == 0) return "수집된 데이터가 없습니다.";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"수집 시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"총 행수: {tableData.Count}");
            sb.AppendLine(new string('-', 50));

            // 헤더 예시 (필요시 추가)
            sb.AppendLine("Name\tAccess\tType\tValue\tDescription");
            sb.AppendLine(new string('-', 50));

            foreach (var row in tableData)
            {
                // 탭(\t)으로 구분하여 정렬
                sb.AppendLine(string.Join("\t", row));
            }

            return sb.ToString();
        }


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
