using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Level = GateBot.LogManager.Level;

namespace GateBot
{
    class Util_Option
    {

        /////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config)
        {
            // 팝업 창 처리
            List<string> windowHandles = new List<string>(driver.WindowHandles);
            foreach (string handle in windowHandles)
            {
                if (handle != mainHandle)
                {
                    try
                    {
                        LogManager.LogMessage("Popup Detected", Level.Info);
                        driver.SwitchTo().Window(handle);
                        await Task.Delay(3000); // 3초 대기 후 닫기
                        driver.Close();
                        LogManager.LogMessage("Closed Popup", Level.Info);
                    }
                    catch (Exception ex)
                    {
                        LogManager.LogException(ex, Level.Error);
                    }
                }
            }

            // 모달 창 처리
            if (IsModalPresent(driver))
            {
                try
                {
                    await EnterModalPassword(driver, config);
                    LogManager.LogMessage("Closed Modal", Level.Info);
                }
                catch (NoSuchElementException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (ArgumentException ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, Level.Error);
                }
            }

            // 경고창 처리
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept(); // 또는 alert.Dismiss();
                LogManager.LogMessage("Closed Alert", Level.Info);
                return true; // 경고창 처리 성공
            }
            catch (NoAlertPresentException)
            {
                // 경고창이 없는 경우
                return false; // 경고창 처리 실패 또는 없음
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
                return false; // 경고창 처리 실패 또는 없음
            }
        }

        private static bool IsModalPresent(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.XPath("//*[@id='lock_passwd']")); // 해당Xpath가 존재하면 모달창 확인
                LogManager.LogMessage("Modal Detected", Level.Info);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private static async Task EnterModalPassword(IWebDriver driver, Config config)
        {
            if (config == null || string.IsNullOrEmpty(config.GatePW))
            {
                throw new ArgumentException("Config 객체 또는 GatePW 속성이 유효하지 않습니다.");
            }

            try
            {
                Util_Control.SendKeysToElement(driver, "//*[@id='lock_passwd']", config.GatePW);
                await Task.Delay(1000);
                Util_Control.ClickElementByXPath(driver, "//*[@id='pop_container']/div[2]/a[1]");
            }
            catch (NoSuchElementException ex)
            {
                throw new NoSuchElementException("비밀번호 입력 요소를 찾을 수 없습니다. XPath: //*[@id='lock_passwd']", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"모달 창 비밀번호 입력 오류. XPath: //*[@id='lock_passwd'], 오류: {ex.Message}", ex);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
