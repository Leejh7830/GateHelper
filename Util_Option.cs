using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Level = GateHelper.LogManager.Level;
using static GateHelper.LogManager;
using static GateHelper.Util_Control;

namespace GateHelper
{
    class Util_Option
    {

        /////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config)
        {
            bool wasHandled = false;

            // 팝업 창 처리 (새 창 핸들을 가진 팝업)
            List<string> windowHandles = new List<string>(driver.WindowHandles);
            foreach (string handle in windowHandles)
            {
                if (handle != mainHandle)
                {
                    try
                    {
                        LogMessage("Popup Window Detected", Level.Info);
                        driver.SwitchTo().Window(handle);
                        await Task.Delay(3000);
                        driver.Close();
                        LogMessage("Closed Popup Window", Level.Info);
                        wasHandled = true;
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, Level.Error);
                    }
                }
            }

            // 모달 창 처리 (HTML 요소 기반의 팝업)
            // 2. 모달 창 처리 (HTML 요소 기반의 팝업)
            if (IsModalPresent(driver))
            {
                try
                {
                    LogMessage("Lock screen Modal Detected", Level.Error);
                    await Task.Delay(3000);

                    // 비밀번호 입력 및 확인 버튼 클릭 로직을 시도
                    bool passwordEntered = await EnterModalPassword(driver, config);

                    // 자동화가 성공했는지 여부와 상관없이,
                    // 모달창이 더 이상 보이지 않으면 성공으로 간주
                    if (!IsModalPresent(driver))
                    {
                        LogMessage("Lock screen Modal Closed successfully.", Level.Info);
                        wasHandled = true;
                    }
                    else
                    {
                        LogMessage("Lock screen Modal remains visible after processing attempt.", Level.Critical);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                }
            }

            // 경고창 처리 (브라우저 자체의 Alert)
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                alert.Accept();
                LogMessage($"Closed Alert: '{alertText}'", Level.Info);
                wasHandled = true;
            }
            catch (NoAlertPresentException)
            {
                // 경고창이 없는 경우
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }
            // 모든 처리 후 메인 핸들로 다시 전환
            // driver.SwitchTo().Window(mainHandle);

            return wasHandled;
        }

        private static bool IsModalPresent(IWebDriver driver)
        {
            try
            {
                return driver.FindElement(By.XPath("//*[@id='lock_passwd']")).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private static async Task<bool> EnterModalPassword(IWebDriver driver, Config config)
        {
            if (config == null || string.IsNullOrEmpty(config.EnportalPW))
            {
                LogMessage("Enportal 값 오류", Level.Error);
                return false;
            }

            // 비밀번호 입력
            if (!SendKeysToElement(driver, "//*[@id='lock_passwd']", config.EnportalPW))
            {
                return false;
            }

            // 비밀번호 입력창에 직접 엔터 키를 입력
            try
            {
                var passwordElement = driver.FindElement(By.XPath("//*[@id='lock_passwd']"));
                passwordElement.SendKeys(OpenQA.Selenium.Keys.Enter);
                LogMessage("비밀번호 입력 후 엔터 키 입력 성공.", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "엔터 키 입력 오류: 요소를 찾을 수 없거나 상호작용할 수 없습니다.");
                return false;
            }

            await Task.Delay(1000); // 엔터 키 처리 대기

            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
