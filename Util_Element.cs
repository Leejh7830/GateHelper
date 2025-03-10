using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using SeleniumExtras.WaitHelpers;
using System.Text;

namespace GateBot
{
    class Util_Element
    {

        /// //////////////////////////////////////////////////////////////////////////////////////////////////

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


        public static void ShowAllElementXpaths(IWebDriver driver)
        {
            try
            {
                // 페이지 내 모든 요소 찾기
                var allElements = driver.FindElements(By.XPath("//*"));

                if (allElements.Count == 0)
                {
                    MessageBox.Show("페이지에 요소가 없습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // XPath 생성 및 출력 준비
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("페이지의 모든 XPath 목록:");

                foreach (var element in allElements)
                {
                    // 각 요소의 XPath를 생성
                    string xpath = GetElementXPath(driver, element);
                    sb.AppendLine(xpath);
                }

                // 결과 출력
                MessageBox.Show(sb.ToString(), "모든 XPath", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 요소의 XPath를 생성하는 함수
        private static string GetElementXPath(IWebDriver driver, IWebElement element)
        {
            string script = "function getElementXPath(element) {" +
                            "var paths = [];" +
                            "while (element.nodeType === Node.ELEMENT_NODE) {" +
                            "var index = 0;" +
                            "var sibling = element.previousSibling;" +
                            "while (sibling) {" +
                            "if (sibling.nodeType === Node.DOCUMENT_TYPE_NODE) {" +
                            "} else if (sibling.nodeName === element.nodeName) {" +
                            "index++;" +
                            "}" +
                            "sibling = sibling.previousSibling;" +
                            "}" +
                            "var tagName = element.nodeName.toLowerCase();" +
                            "var pathIndex = (index ? '[' + (index + 1) + ']' : '');" +
                            "paths.unshift(tagName + pathIndex);" +
                            "element = element.parentNode;" +
                            "}" +
                            "return paths.length ? '/' + paths.join('/') : null;" +
                            "}" +
                            "return getElementXPath(arguments[0]);";

            var xpath = (string)((IJavaScriptExecutor)driver).ExecuteScript(script, element);
            return xpath;
        }


        /// //////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
