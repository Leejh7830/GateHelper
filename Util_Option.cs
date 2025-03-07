using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateBot
{
    class Util_Option
    {

        /////////////////////////////////////////////////////////////////////////////////////////
        private static int popupCount = 0;

        public static async Task<int> ControlPopupTimerTick(IWebDriver driver, string mainHandle, Config config)
        {
            return await CloseOtherWindowsAfterDelay(driver, mainHandle, config);
        }

        private static async Task<int> CloseOtherWindowsAfterDelay(IWebDriver _driver, string mainHandle, Config config) // Config 추가
        {
            List<string> windowHandles = new List<string>(_driver.WindowHandles);

            foreach (string handle in windowHandles)
            {
                if (handle != mainHandle)
                {
                    try
                    {
                        _driver.SwitchTo().Window(handle);
                        await Task.Delay(3000); // 3초 대기 후 닫기
                        _driver.Close();
                        popupCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"팝업 창 닫기 오류: {ex.Message}");
                    }
                }
            }

            if (IsModalPresent(_driver)) // 모달 창 존재 여부 확인
            {
                await EnterModalPassword(_driver, config);
                popupCount++;
            }
            return popupCount;
        }

        private static bool IsModalPresent(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.XPath("//*[@id='lock_passwd']")); // 해당Xpath가 존재하면 모달창 확인
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private static async Task EnterModalPassword(IWebDriver _driver, Config config) // Config 추가
        {
            try
            {
                Util.SendKeysToElement(_driver, "//*[@id='lock_passwd']", config.GatePW);
                await Task.Delay(1000); // 1초 대기 (필요에 따라 조정)
                Util.ClickElementByXPath(_driver, "//*[@id='pop_container']/div[2]/a[1]");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("비밀번호 입력 요소를 찾을 수 없습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"모달 창 비밀번호 입력 오류: {ex.Message}");
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
