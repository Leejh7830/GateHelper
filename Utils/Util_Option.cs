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

        // Alert 지속 감지 상태
        private static DateTime? _alertFirstSeenUtc = null;
        private static bool _alertNotified = false;

        // 그레이스 타임 (ms), 첫감지후 알림간격
        private static AppSettings _appSettings = new AppSettings();

        /////////////////////////////////////////////////////////////////////////////////////////

        public static async Task<bool> HandleWindows(IWebDriver driver, string mainHandle, Config config)
        {
            bool wasHandled = false;

            // 1) 팝업 처리: 즉시 닫지 않고 감지 후 5초 이상 지속 시에만 알림
            try
            {
                var handles = driver.WindowHandles;
                var now = DateTime.UtcNow;

                // 현재 살아있는 팝업(메인 제외) 스냅샷
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
                            LogMessage($"[Detect] Popup window handle detected: {h}", Level.Info);
                        }
                        else
                        {
                            var info = _seenWindows[h];
                            if (!info.Notified && (now - info.FirstSeenUtc).TotalMilliseconds >= _appSettings.PopupGraceMs)
                            {
                                try
                                {
                                    var sec = ToPrettySeconds(_appSettings.PopupGraceMs);
                                    MessageBox.Show($"팝업창이 {sec}초 이상 떠 있습니다.",
                                                    "알림", MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                    LogMessage($"MessageBox shown: popup alive >= {_appSettings.PopupGraceMs} ms ({sec}s)", Level.Info);
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"MessageBox error (popup >= {_appSettings.PopupGraceMs} ms): {ex.Message}", Level.Error);
                                }
                                info.Notified = true;
                            }
                        }
                    }
                }

                // 사라진 팝업 정리
                var toRemove = new List<string>();
                foreach (var kv in _seenWindows)
                {
                    if (!currentPopups.Contains(kv.Key))
                    {
                        // 5초 내 자연 종료 시 알림 없음, 로그만 선택적으로 남김
                        // LogMessage($"Popup disappeared: {kv.Key}", Level.Info);
                        toRemove.Add(kv.Key);
                    }
                }
                foreach (var key in toRemove)
                    _seenWindows.Remove(key);
            }
            catch (WebDriverException ex)
            {
                LogMessage($"Popup detection skipped: {ex.Message}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }

            // 2) 모달 처리 (비밀번호 입력은 필요 기능이므로 그대로 진행)
            try
            {
                if (IsLockModalPresent(driver))
                {
                    LogMessage("Detected a Lock screen Modal.", Level.Info);
                    bool ok = await EnterModalPassword(driver, config);

                    wasHandled |= ok; // 성공했을 때만 handled 처리

                    if (ok) LogMessage("Lock screen Modal was handled successfully.", Level.Info);
                    else LogMessage("Lock screen Modal remains visible after processing attempt.", Level.Error);
                }
                else if (IsUnknownModalPresent(driver))
                {
                    // lock_passwd가 아닌 다른 모달은 감지만, 닫지 않음
                    LogMessage("[Detect] Unknown modal present (not handled).", Level.Info);
                    // 아무 동작도 하지 않음
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

            // 3) 알림 처리: 존재만 감지, 5초 이상 지속 시에만 알림 (확인 누르지 않음)
            try
            {
                try
                {
                    var _ = driver.SwitchTo().Alert(); // 존재 여부만 확인
                    var now = DateTime.UtcNow;

                    if (_alertFirstSeenUtc == null)
                    {
                        _alertFirstSeenUtc = now;
                        _alertNotified = false;
                        LogMessage("[Detect] Alert present.", Level.Info);
                    }
                    else
                    {
                        if (!_alertNotified && (now - _alertFirstSeenUtc.Value).TotalMilliseconds >= _appSettings.PopupGraceMs)
                        {
                            try
                            {
                                MessageBox.Show("알림창이 5초 이상 떠 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                LogMessage("MessageBox shown: alert alive >= 5s", Level.Info);
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"MessageBox error (alert >=5s): {ex.Message}", Level.Error);
                            }
                            _alertNotified = true;
                        }
                    }
                }
                catch (NoAlertPresentException)
                {
                    _alertFirstSeenUtc = null;
                    _alertNotified = false;
                }
            }
            catch (WebDriverException ex)
            {
                LogMessage($"Alert detection skipped: {ex.Message}", Level.Info);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error);
            }

            // 4) 복귀 정책: 모달을 실제로 처리했을 때만 메인으로 복귀 (실패시 치명적 오류)
            try
            {
                if (!wasHandled)
                {
                    // 아무작업도 안했을 때
                    return false;
                }

                var handles = driver.WindowHandles;
                if (handles == null || !handles.Contains(mainHandle))
                {
                    LogMessage("FATAL: Main window handle is missing.", Level.Critical);
                    throw new NoSuchWindowException("Main window not found (fatal).");
                }

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
                // lock_passwd 모달이 있으면 여기선 false
                if (IsLockModalPresent(driver)) return false;

                // 흔한 모달 패턴(필요시 클래스/셀렉터 추가 가능)
                var any =
                    driver.FindElements(By.CssSelector("[role='dialog'], [aria-modal='true'], .modal, .dialog, .MuiDialog-root"));

                return any != null && any.Count > 0;
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

            // 페이지 안정화(선택)
            try
            {
                var js = driver as IJavaScriptExecutor;
                wait.Until(d => (js?.ExecuteScript("return document.readyState") as string) == "complete");
            }
            catch { /* 무시: 일부 페이지는 readyState 확인이 불가 */ }

            try
            {
                // 입력 박스가 보이고 상호작용 가능한 상태가 될 때까지 대기
                var input = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(byPwd));

                // 천천히, 확실히 입력
                input.Click();
                await Task.Delay(200);
                input.Clear();
                await Task.Delay(200);

                const int maxAttempts = 3;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    input.SendKeys(config.UserPW);
                    await Task.Delay(300);

                    // 입력 검증(비밀번호 필드는 value 조회가 제한될 수 있음 → 길이로만 점검)
                    try
                    {
                        var val = input.GetAttribute("value") ?? string.Empty;
                        if (val.Length >= Math.Min(1, config.UserPW.Length))
                            break; // 충분히 입력됨
                    }
                    catch { /* 일부 브라우저/보안설정에서 value 접근 불가 */ }

                    if (attempt < maxAttempts)
                    {
                        input.Clear();
                        await Task.Delay(250);
                    }
                }

                // Enter 전 소폭 대기 후 전송
                await Task.Delay(300);
                input.SendKeys(OpenQA.Selenium.Keys.Enter);

                // 모달이 사라질 때까지 대기(정확성 우선)
                bool closed = wait.Until(d =>
                {
                    var els = d.FindElements(byPwd);
                    if (els.Count == 0) return true;
                    try { return !els[0].Displayed; } catch { return true; }
                });

                return closed;
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

        /* 기존 코드
        private static async Task<bool> EnterModalPassword(IWebDriver driver, Config config)
        {
            if (config == null || string.IsNullOrEmpty(config.UserPW))
            {
                LogMessage("Config Value is Null", Level.Error);
                return false;
            }

            Thread.Sleep(1000);

            // 비밀번호 입력 / 정상 True, 비정상 False 반환
            if (!Util_Element.SendKeysToElement(driver, "//*[@id='lock_passwd']", config.UserPW))
            {
                return false;
            }

            // 비밀번호 입력창에 직접 엔터 키를 입력
            try
            {
                Thread.Sleep(2000);
                var passwordElement = driver.FindElement(By.XPath("//*[@id='lock_passwd']"));
                passwordElement.SendKeys(OpenQA.Selenium.Keys.Enter);
            }
            catch (Exception ex)
            {
                LogException(ex, Level.Error, "엔터 키 입력 오류: 요소를 찾을 수 없거나 상호작용할 수 없습니다.");
                return false;
            }

            await Task.Delay(1000); // 엔터 키 처리 대기
            return true;
        }
        */


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
