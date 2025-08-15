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
            bool wasHandled = false; // 팝업 처리 여부를 저장할 변수

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
                        wasHandled = true; // 팝업 처리됨
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, Level.Error);
                    }
                }
            }

            // 모달 창 처리
            if (IsModalPresent(driver))
            {
                try
                {
                    LogMessage("Lock screen Modal Detected", Level.Error);
                        MessageBox.Show(
                            "화면 잠금 모달창이 감지되었습니다. 비밀번호를 입력하려면 확인을 눌러주세요.",
                            "모달 감지",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    await EnterModalPassword(driver, config);
                    LogMessage("Lock screen Modal Closed successfully.", Level.Info);
                }
                catch (Exception ex)
                {
                    LogException(ex, Level.Error);
                }
            }

            // 경고창 처리
            try
            {
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept(); // 또는 alert.Dismiss();
                LogMessage("Closed Alert", Level.Info);
                return true; // 경고창 처리 성공
            }
            catch (NoAlertPresentException)
            {
                // 경고창이 없는 경우
                return false; // 경고창 처리 실패 또는 없음
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                return false; // 경고창 처리 실패 또는 없음
            }
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

        private static async Task EnterModalPassword(IWebDriver driver, Config config)
        {
            if (config == null || string.IsNullOrEmpty(config.GatePW))
            {
                throw new ArgumentException("Config 객체 또는 GatePW 속성이 유효하지 않습니다.");
            }

            // 헬퍼 메서드를 사용하여 비밀번호 입력
            SendKeysToElement(driver, "//*[@id='lock_passwd']", config.GatePW);

            // 텍스트 입력 후 클릭 전 잠시 대기
            await Task.Delay(500);

            // 헬퍼 메서드를 사용하여 확인 버튼 클릭
            ClickElementByXPath(driver, "//*[@id='pop_container']/div[2]/a[1]");

            // 클릭 후 모달이 닫힐 때까지 잠시 대기
            await Task.Delay(1000);
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
