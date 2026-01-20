using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;

namespace GateHelper
{
    public static class ServerLoader
    {
        // 드라이버에서 서버명 목록만 추출하여 반환 (예외는 호출자에게 전달)
        public static List<string> GetServers(IWebDriver driver, int timeoutSeconds = 10)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath("//*[@id='seltable']//tr")));

            var serverList = new List<string>();
            int tbodyIndex = 1;

            while (true)
            {
                string xpath = $"//*[@id='seltable']/tbody[{tbodyIndex}]/tr/td[4]";
                var serverListElements = driver.FindElements(By.XPath(xpath));

                if (serverListElements == null || serverListElements.Count == 0)
                    break;

                foreach (var element in serverListElements)
                {
                    serverList.Add(element.Text);
                }
                tbodyIndex++;
            }

            return serverList;
        }
    }
}