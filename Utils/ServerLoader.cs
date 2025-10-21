using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace GateHelper
{
    public static class ServerLoader
    {
        // ����̹����� ������ ��ϸ� �����Ͽ� ��ȯ (���ܴ� ȣ���ڿ��� ����)
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