using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SeleniumExtras.WaitHelpers;
using System.Text;
using static GateHelper.LogManager;
using System.Collections.Generic;
using System.Linq;

namespace GateHelper
{
    class Util_Element
    {
        public static bool ClickElementByXPath(IWebDriver driver, string xpath)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
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
                // 요소를 찾았지만 5초 안에 클릭 가능한 상태가 되지 않은 경우
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

        // 요소에 값을 입력하는 메서드
        public static bool SendKeysToElement(IWebDriver driver, string xpath, string value)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
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
