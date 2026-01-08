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
        // 팝업 지속 감지용 상태 캐시 (핸들별 첫 감지 시각/알림 여부)
        private class SeenInfo
        {
            public DateTime FirstSeenUtc;
            public bool Notified;
        }

        // 팝업 핸들 상태
        private static readonly Dictionary<string, SeenInfo> _seenWindows = new Dictionary<string, SeenInfo>();

        // 그레이스 타임 (ms), 첫감지후 알림간격
        private static AppSettings _appSettings = new AppSettings();

        /////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config)
        {
            bool wasHandled = false;

            // 1) 팝업 처리 로직 (WindowHandles 감지)
            try
            {
                var handles = driver.WindowHandles;
                var now = DateTime.UtcNow;
                var currentPopups = new HashSet<string>();

                if (handles != null)
                {
                    foreach (var h in handles)
                    {
                        if (h == mainHandle) continue;
                        currentPopups.Add(h);

                        if (!_seenWindows.ContainsKey(h))
                        {
                            _seenWindows[h] = new SeenInfo { FirstSeenUtc = now, Notified = false };
                        }
                        else
                        {
                            var info = _seenWindows[h];
                            if (!info.Notified && (now - info.FirstSeenUtc).TotalMilliseconds >= _appSettings.PopupGraceMs)
                            {
                                string msg = $"팝업창이 {ToPrettySeconds(_appSettings.PopupGraceMs)}초 이상 떠 있습니다.";
                                if (Application.OpenForms["MainUI"] is MainUI mainUI)
                                {
                                    mainUI.Invoke(new Action(() => mainUI.ShowTrayNotification("알림", msg, ToolTipIcon.Warning)));
                                }
                                info.Notified = true;
                                wasHandled = true;
                            }
                        }
                    }
                }

                var toRemove = new List<string>();
                foreach (var kv in _seenWindows) if (!currentPopups.Contains(kv.Key)) toRemove.Add(kv.Key);
                foreach (var key in toRemove) _seenWindows.Remove(key);
            }
            catch (Exception ex) { LogException(ex, Level.Error, "Popup detection error"); }

            // 2) 모달 및 알럿 통합 처리
            try
            {
                // A. 잠금 화면(비밀번호) 모달 감지 및 처리
                if (IsLockModalPresent(driver))
                {
                    LogMessage("Detected a Lock screen Modal.", Level.Info);

                    try
                    {
                        bool ok = await EnterModalPassword(driver, config);
                        wasHandled |= ok;
                        if (ok) LogMessage("Lock screen Modal was handled successfully.", Level.Info);
                    }
                    catch (UnhandledAlertException ex)
                    {
                        // [복구 로직] 비밀번호 입력 중 Base64 알럿 발생 시
                        LogMessage($"[HandleWindows] 보안 알럿 감지: {ex.AlertText}", Level.Error);

                        // 알럿 닫기 (대기 시간 0.5초 포함된 메서드)
                        await HandleUnexpectedAlert(driver);

                        // 알럿 제거 후 처음부터 다시 입력 시도 (가장 확실한 복구 방법)
                        LogMessage("알럿 해결 후 비밀번호 재입력을 시도합니다.", Level.Info);
                        bool retryOk = await EnterModalPassword(driver, config);
                        wasHandled |= retryOk;
                    }
                }
                // B. 그 외 알 수 없는 일반 모달 감지 (사용자님의 CssSelector 로직 활용)
                else if (IsUnknownModalPresent(driver))
                {
                    LogMessage("[Detect] Unknown modal present (not handled).", Level.Info);
                    // 필요 시 여기에 트레이 알림 추가 가능
                }
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "Modal handling error");
            }

            // 3) 복귀 정책 (작업이 수행된 경우에만 메인 핸들로 전환)
            try
            {
                if (wasHandled)
                {
                    if (driver.WindowHandles.Contains(mainHandle))
                    {
                        driver.SwitchTo().Window(mainHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Restore window failed: {ex.Message}", Level.Critical);
            }

            return wasHandled;
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

                if (Application.OpenForms["MainUI"] is MainUI mainUI)
                    mainUI.Invoke(new Action(() => mainUI.ShowTrayNotification("자동 복구", "보안 알럿을 해제했습니다.", ToolTipIcon.Info)));

                await Task.Delay(500); // 확인 후 안정화 대기
            }
            catch (NoAlertPresentException) { }
        }

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

        // lock_passwd가 아닌 '다른' 모달 간단 감지(닫지 않음, 로그만)
        private static bool IsUnknownModalPresent(IWebDriver driver)
        {
            try
            {
                // [보안] 알럿이 이미 떠 있으면 DOM 조회 시 예외가 발생하므로 우선 체크
                if (IsAlertPresent(driver, out _)) return false;

                // 1. lock_passwd(비밀번호창) 모달이 있으면 여기선 false (역할 분리)
                if (IsLockModalPresent(driver)) return false;

                // 2. 사용자님의 기존 CSS 패턴 (매우 우수함: role, aria, 다양한 클래스 대응)
                var any = driver.FindElements(By.CssSelector("[role='dialog'], [aria-modal='true'], .modal, .dialog, .MuiDialog-root"));

                // 3. 요소가 존재하고 실제로 화면에 보이는지(Displayed)까지 체크하면 더 정확합니다.
                return any != null && any.Count > 0 && any[0].Displayed;
            }
            catch
            {
                return false;
            }
        }


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



        public static void SetPopupGraceMs(int ms)
        {
            _appSettings.PopupGraceMs = ms < 0 ? 0 : ms;
            LogMessage($"Popup grace set: {_appSettings.PopupGraceMs} ms", Level.Info);
        }

        private static string ToPrettySeconds(int ms)
        {
            return (ms % 1000 == 0) ? (ms / 1000).ToString() : (ms / 1000.0).ToString("0.###");
        }

        // 팝업 감지 상태 라벨 업데이트
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
