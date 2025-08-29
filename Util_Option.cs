using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    class Util_Option
    {

        /////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config)
        {
            bool wasHandled = false;

            // 1) 팝업 처리.
            try
            {
                var windowHandles = new List<string>(driver.WindowHandles);

                foreach (var handle in windowHandles)
                {
                    if (handle == mainHandle) continue;

                    LogMessage("Detected a new browser window (popup).", Level.Info);

                    try { driver.SwitchTo().Window(handle); }
                    catch (WebDriverException ex)
                    {
                        LogMessage($"Popup disappeared before switch. Skip. ({ex.Message})", Level.Info);
                        continue;
                    }

                    try
                    {
                        // await Task.Delay(200); // 필요 시
                        driver.Close(); // 현재(팝업)만 닫음
                        LogMessage("Closed the new browser window.", Level.Info);
                        wasHandled = true;
                    }
                    catch (WebDriverException ex)
                    {
                        LogMessage($"Popup disappeared while closing. Skip. ({ex.Message})", Level.Info);
                    }
                }
            }
            catch (WebDriverException ex)
            {
                LogMessage($"Popup enumeration skipped: {ex.Message}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }

            // 2) 모달 처리
            try
            {
                if (IsModalPresent(driver))
                {
                    LogMessage("Detected a Lock screen Modal.", Level.Info);
                    bool ok = await EnterModalPassword(driver, config);

                    wasHandled |= ok; // 성공했을 때만 handled 처리

                    if (ok) LogMessage("Lock screen Modal was handled successfully.", Level.Info);
                    else LogMessage("Lock screen Modal remains visible after processing attempt.", Level.Error);
                }
            }
            catch (WebDriverException ex)
            {
                LogMessage($"Modal handling skipped: {ex.Message}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }

            // 3) 알림 처리
            try
            {
                var alert = driver.SwitchTo().Alert();
                string text = alert.Text;
                alert.Accept();
                LogMessage($"Closed Alert: '{text}'", Level.Info);
                wasHandled = true;
            }
            catch (NoAlertPresentException) { }
            catch (WebDriverException ex)
            {
                LogMessage($"Alert handling skipped: {ex.Message}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }

            // 4) 복귀 정책
            try
            {
                if (!wasHandled)
                {
                    // 아무작업도 안했을 때
                    return false;
                }

                // 무언가 처리했을 때만 메인으로 복귀 (실패시 치명적 오류)
                var handles = driver.WindowHandles;
                if (handles == null || !handles.Contains(mainHandle))
                {
                    LogMessage("FATAL: Main window handle is missing.", Level.Critical);
                    throw new NoSuchWindowException("Main window not found (fatal).");
                }

                // 이미 메인이라면 전환 생략
                if (driver.CurrentWindowHandle != mainHandle)
                    driver.SwitchTo().Window(mainHandle);
            }
            catch (WebDriverException ex)
            {
                LogMessage($"FATAL: Failed to restore window. {ex.Message}", Level.Critical);
                throw; // 호출부에서 Driver OFF 처리
            }

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
            if (!Util_Element.SendKeysToElement(driver, "//*[@id='lock_passwd']", config.EnportalPW))
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

        public static void UpdatePopupStatus(Label popupStatusLabel, bool isPopupEnabled, int popupCount)
        {
            string newPopupStatus = isPopupEnabled ? "ON" : "OFF";

            popupStatusLabel.Text = $"Detect {newPopupStatus} ({popupCount})";

            Color onColor = Color.Red;
            Color offColor = Color.Green;

            popupStatusLabel.BackColor = isPopupEnabled ? onColor : offColor;
            popupStatusLabel.ForeColor = Color.White;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
