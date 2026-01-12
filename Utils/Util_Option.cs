using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GateHelper.LogManager;

namespace GateHelper
{
    class Util_Option
    {
        #region Fields & Properties

        // 그레이스 타임 (ms), 첫감지후 알림간격
        private static AppSettings _appSettings = new AppSettings();

        // 비밀번호 모달을 성공적으로 처리한 누적 횟수
        private static int _lockHandledCount = 0;

        // 이전 상태 저장
        private static bool? _lastLoggedEnabled = null;

        // 현재 카운트를 외부(MainUI)에서 가져갈 수 있게 Getter 추가
        public static int GetLockHandledCount() => _lockHandledCount;

        #endregion


        #region Public Methods (HandleWindows)
        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config, Label popupStatusLabel, bool isFeatureEnabled)
        {
            bool wasHandled = false;

            // 1. UI 상태 업데이트 (진입 시점에 현재 상태 표시)
            // 숫자는 내부 변수인 _lockHandledCount를 사용합니다.
            UpdatePopupStatus(popupStatusLabel, isFeatureEnabled, _lockHandledCount);

            // 옵션이 꺼져 있으면 로직을 타지 않고 바로 리턴
            if (!isFeatureEnabled) return false;

            try
            {
                // 2. 잠금 화면(비밀번호) 모달 처리
                if (IsLockModalPresent(driver))
                {
                    LogMessage("Detected a Lock screen Modal. Processing...", Level.Info);
                    try
                    {
                        // 비밀번호 입력 시도
                        if (await EnterModalPassword(driver, config))
                        {
                            _lockHandledCount++;
                            wasHandled = true;

                            // 성공 시 카운트가 올라갔으므로 UI 즉시 업데이트
                            UpdatePopupStatus(popupStatusLabel, isFeatureEnabled, _lockHandledCount);
                        }
                    }
                    catch (UnhandledAlertException ex)
                    {
                        // [보안 복구] 입력 중 브라우저 보안 알럿(alert) 발생 시
                        LogMessage($"[Action] Security alert detected: {ex.AlertText}", Level.Error);

                        // 알럿을 닫고 다시 시도
                        await HandleUnexpectedAlert(driver);

                        if (await EnterModalPassword(driver, config))
                        {
                            _lockHandledCount++;
                            wasHandled = true;
                            UpdatePopupStatus(popupStatusLabel, isFeatureEnabled, _lockHandledCount);
                        }
                    }
                }
                // 3. 비밀번호 모달은 없지만, 다른 '알 수 없는 모달'이 떠 있는 경우
                else if (IsUnknownModalPresent(driver))
                {
                    // 다른 작업 없이 로그로만 기록 (사용자 확인용)
                    LogMessage("[Detect] Unknown modal or dialog is currently displayed.", Level.Info);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "Modal handling error in HandleWindows");
            }

            // 4. 복귀 정책 (작업이 수행되었거나 창 전환이 일어났을 경우 안전하게 메인으로 복귀)
            if (wasHandled)
            {
                try
                {
                    // 현재 열려있는 핸들 목록에 메인 핸들이 있는지 확인 후 전환
                    if (driver.WindowHandles.Contains(mainHandle))
                    {
                        driver.SwitchTo().Window(mainHandle);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Failed to switch back to main handle: {ex.Message}", Level.Info);
                }
            }

            return wasHandled;
        }
        #endregion


        #region Element Checkers

        // lock_passwd 모달만 판별(알림이 있으면 DOM 접근 스킵)
        private static bool IsLockModalPresent(IWebDriver driver)
        {
            try
            {
                // 알림이 있으면 DOM 접근 시 Unhandled Prompt 문제가 생김 → 스킵
                if (IsAlertPresent(driver, out _))
                    return false;

                var elems = driver.FindElements(By.XPath("//*[@id='lock_passwd']"));
                if (elems == null || elems.Count == 0)
                    return false;

                try { return elems[0].Displayed; }
                catch { return true; } // 표시 여부 접근 실패시 존재만 true
            }
            catch (WebDriverException ex)
            {
                LogMessage($"IsLockModalPresent skipped (WebDriver): {ex.Message}", Level.Info);
                return false;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
                return false;
            }
        }

        private static bool IsUnknownModalPresent(IWebDriver driver)
        {
            try
            {
                // 브라우저 알럿이 떠 있으면 DOM 조회가 불가능하므로 스킵
                if (IsAlertPresent(driver, out _)) return false;

                // 비밀번호 모달이 아닐 때만 체크
                // (IsLockModalPresent에서 이미 체크하므로 HandleWindows 흐름상 안전하지만 중복 방어)

                // 범용적인 모달/다이얼로그 패턴 (role, aria, common classes)
                var any = driver.FindElements(By.CssSelector("[role='dialog'], [aria-modal='true'], .modal, .dialog, .MuiDialog-root"));

                // 요소가 존재하고 화면에 보이는 상태인지 확인
                return any != null && any.Count > 0 && any[0].Displayed;
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region Private Logic

        private static async Task<bool> EnterModalPassword(IWebDriver driver, Config config)
        {
            if (config == null || string.IsNullOrEmpty(config.UserPW))
            {
                LogMessage("Config Value is Null", Level.Error);
                return false;
            }

            var byPwd = By.XPath("//*[@id='lock_passwd']");
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(20));

            // 페이지 안정화
            try
            {
                var js = driver as IJavaScriptExecutor;
                wait.Until(d => (js?.ExecuteScript("return document.readyState") as string) == "complete");
            }
            catch { }

            try
            {
                // 입력 박스 대기
                var input = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(byPwd));

                input.Click();
                await Task.Delay(200);
                input.Clear();
                await Task.Delay(200);

                // 비밀번호 입력 (최대 3회 검증 로직 유지)
                const int maxAttempts = 3;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    input.SendKeys(config.UserPW);
                    await Task.Delay(300);

                    try
                    {
                        var val = input.GetAttribute("value") ?? string.Empty;
                        if (val.Length >= Math.Min(1, config.UserPW.Length)) break;
                    }
                    catch { }

                    if (attempt < maxAttempts)
                    {
                        input.Clear();
                        await Task.Delay(250);
                    }
                }

                await Task.Delay(300);
                input.SendKeys(OpenQA.Selenium.Keys.Enter); // 엔터 전송

                // 모달이 사라질 때까지 대기
                // ※ 이 과정에서 브라우저 알럿이 뜨면 UnhandledAlertException이 발생하여 catch 블록으로 이동합니다.
                bool closed = wait.Until(d =>
                {
                    var els = d.FindElements(byPwd);
                    if (els.Count == 0) return true;
                    try { return !els[0].Displayed; } catch { return true; }
                });

                return closed;
            }
            catch (UnhandledAlertException)
            {
                // 알럿 발생 시 상위 HandleWindows에서 처리하도록 예외를 던짐
                throw;
            }
            catch (WebDriverTimeoutException ex)
            {
                LogException(ex, Level.Error, "잠금 모달 처리 타임아웃");
                return false;
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "잠금 모달 처리 오류");
                return false;
            }
        }

        // 알럿 처리 전용 보조 메서드
        private static async Task HandleUnexpectedAlert(IWebDriver driver)
        {
            try
            {
                await Task.Delay(500); // 확인 전 대기
                IAlert alert = driver.SwitchTo().Alert();
                alert.Accept();
                LogMessage("보안 알럿을 확인하고 닫았습니다.", Level.Info);

                /*
                if (Application.OpenForms["MainUI"] is MainUI mainUI)
                    mainUI.Invoke(new Action(() => mainUI.ShowTrayNotification("자동 복구", "보안 알럿을 해제했습니다.", ToolTipIcon.Info)));
                */
                await Task.Delay(500); // 확인 후 안정화 대기
            }
            catch (NoAlertPresentException) { }
        }

        private static bool IsAlertPresent(IWebDriver driver, out string alertText)
        {
            alertText = string.Empty;
            try
            {
                var alert = driver.SwitchTo().Alert();
                try { alertText = alert.Text; } catch { /* 무시 */ }
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
            catch (WebDriverException)
            {
                // 드문 환경에서 Alert 조회 자체가 예외일 수 있음 → 알림 있다고 단정하지 않음
                return false;
            }
        }

        #endregion

        // 팝업 감지 상태 라벨 업데이트
        public static void UpdatePopupStatus(Label popupStatusLabel, bool isPopupEnabled, int successCount)
        {
            if (popupStatusLabel == null) return;

            // 1. 상태가 변했을 때만 로그 출력 (중앙 관리)
            if (_lastLoggedEnabled != isPopupEnabled)
            {
                string statusText = isPopupEnabled ? "ON" : "OFF";
                LogMessage($"[Status Change] Popup Detect {statusText}", Level.Info);

                // 현재 상태 저장
                _lastLoggedEnabled = isPopupEnabled;
            }

            // 2. UI 업데이트 (기존 로직)
            if (popupStatusLabel.InvokeRequired)
            {
                popupStatusLabel.Invoke(new Action(() => UpdatePopupStatus(popupStatusLabel, isPopupEnabled, successCount)));
                return;
            }

            Color onColor = ColorTranslator.FromHtml("#4CAF50");
            Color offColor = ColorTranslator.FromHtml("#F44336");

            popupStatusLabel.Text = $"Detect {(isPopupEnabled ? "ON" : "OFF")} ({successCount})";
            popupStatusLabel.BackColor = isPopupEnabled ? onColor : offColor;
            popupStatusLabel.ForeColor = Color.White;
        }

        public static void SetPopupGraceMs(int ms)
        {
            _appSettings.PopupGraceMs = ms < 0 ? 0 : ms;
            LogMessage($"Popup grace set: {_appSettings.PopupGraceMs} ms", Level.Info);
        }

        private static string ToPrettySeconds(int ms)
        {
            return (ms % 1000 == 0) ? (ms / 1000).ToString() : (ms / 1000.0).ToString("0.###");
        }

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
