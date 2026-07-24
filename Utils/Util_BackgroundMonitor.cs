using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GateHelper.LogManager;

namespace GateHelper
{
    internal class Util_BackgroundMonitor
    {
        // 상태 플래그
        public bool IsManagementActive { get; private set; } = false;
        public string ManagementHandle { get; private set; } = null;
        private HashSet<string> _ignoredHandles = new HashSet<string>();
        
        // 캐싱 (브라우저 부하 최소화를 위함)
        private int _lastWindowCount = -1;
        
        // 파싱 최적화된 타겟 키워드
        private string _targetKeyword = "";

        // MainUI로부터 전달받는 의존성들
        private Func<IWebDriver> _getDriver;
        private Func<string> _getMainHandle;
        private Action<string, Level> _logMessage;
        
        // 동시성 잠금
        private bool _isTickRunning = false;

        public Util_BackgroundMonitor(string managementUrl, Func<IWebDriver> getDriver, Func<string> getMainHandle, Action<string, Level> logMessage)
        {
            _getDriver = getDriver;
            _getMainHandle = getMainHandle;
            _logMessage = logMessage;
            
            UpdateManagementUrl(managementUrl);
        }

        public void UpdateManagementUrl(string managementUrl)
        {
            if (string.IsNullOrEmpty(managementUrl))
            {
                _targetKeyword = "";
                return;
            }

            // 💡 1. URL 파싱 최적화: 매 틱마다 파싱하지 않고 필요시(초기화/설정변경) 1회만 처리
            _targetKeyword = managementUrl
                .Replace("http://", "")
                .Replace("https://", "")
                .Replace("www.", "");

            if (_targetKeyword.Contains("?"))
            {
                _targetKeyword = _targetKeyword.Split('?')[0]; // 쿼리스트링 제거
            }
            
            _targetKeyword = _targetKeyword.TrimEnd('/');
        }

        public void SetManagementActiveManually(string handle)
        {
            ManagementHandle = handle;
            IsManagementActive = true;
            _lastWindowCount = -1; // 강제 업데이트 유도
        }

        public void Reset()
        {
            IsManagementActive = false;
            ManagementHandle = null;
            _ignoredHandles.Clear();
            _lastWindowCount = -1;
            _isTickRunning = false;
        }

        public async Task ExecuteTickAsync(Func<Task> screenLockCallback)
        {
            // 💡 2. 동시성 제어 방어 위치 수정: 가장 바깥으로 빼서 완벽히 락을 검
            if (_isTickRunning) return;
            _isTickRunning = true;
            
            try
            {
                var driver = _getDriver();
                var mainHandle = _getMainHandle();

                if (driver == null || string.IsNullOrEmpty(mainHandle)) return;

                var currentHandles = driver.WindowHandles;

                // 💡 3. 무지성 순회 방지 (캐싱): 탭 개수가 변했을 때만 정밀 검사 수행
                if (currentHandles.Count != _lastWindowCount)
                {
                    _lastWindowCount = currentHandles.Count;

                    // 💡 4. 닫힌 탭 청소(Cleanup) 최적화: 탭 갯수가 1개로 줄어도 정상적으로 청소됨
                    if (_ignoredHandles.Count > 0)
                    {
                        _ignoredHandles.RemoveWhere(h => !currentHandles.Contains(h));
                    }

                    // [탭 열림 자동 감지]
                    if (currentHandles.Count > 1)
                    {
                        if (string.IsNullOrEmpty(ManagementHandle) || !currentHandles.Contains(ManagementHandle))
                        {
                            bool isLoading = false;
                            string originalFocus = "";
                            try { originalFocus = driver.CurrentWindowHandle; } catch { }

                            foreach (var handle in currentHandles)
                            {
                                if (handle != mainHandle && !_ignoredHandles.Contains(handle))
                                {
                                    driver.SwitchTo().Window(handle);
                                    string currentUrl = driver.Url.ToLower();

                                    if (!string.IsNullOrEmpty(_targetKeyword) && currentUrl.Contains(_targetKeyword.ToLower()))
                                    {
                                        ManagementHandle = handle;
                                        IsManagementActive = true;
                                        _logMessage($"[플래그 ON] MGMT탭({_targetKeyword}) 열림 감지", Level.Info);
                                        break;
                                    }
                                    else if (currentUrl.Contains("about:blank") || string.IsNullOrEmpty(currentUrl))
                                    {
                                        _logMessage("[로딩 대기] 새 탭이 아직 로딩 중입니다.", Level.Info);
                                        isLoading = true;
                                    }
                                    else
                                    {
                                        // _logMessage($"[플래그 실패] 타겟 키워드: '{_targetKeyword}', 실제 URL: '{currentUrl}'", Level.Error); // 정상적인 필터링 과정이므로 혼란 방지를 위해 로그 제외
                                        _ignoredHandles.Add(handle);
                                    }
                                }
                            }

                            // 원래 포커스로 복구
                            if (!IsManagementActive)
                            {
                                // 💡 [수정] 공지사항 팝업 등이 떴을 때, 원래 탭(mainHandle)으로 강제로 되돌아가면 
                                //    팝업이 뒤로 밀려나는 불편함이 있으므로 포커스 복귀 로직을 제거(주석 처리)합니다.
                                /*
                                try 
                                { 
                                    if (!string.IsNullOrEmpty(originalFocus) && driver.WindowHandles.Contains(originalFocus))
                                        driver.SwitchTo().Window(originalFocus);
                                    else
                                        driver.SwitchTo().Window(mainHandle); 
                                } catch { }
                                */
                            }

                            // 로딩 중이면 하위 로직 건너뜀
                            if (isLoading && !IsManagementActive) return;
                        }
                    }
                }

                // [탭 닫힘 자동 감지]
                if (IsManagementActive && !string.IsNullOrEmpty(ManagementHandle))
                {
                    if (!currentHandles.Contains(ManagementHandle))
                    {
                        IsManagementActive = false;
                        ManagementHandle = null;

                        if (currentHandles.Contains(mainHandle))
                        {
                            driver.SwitchTo().Window(mainHandle);
                        }
                        _logMessage("[플래그 OFF] MGMT탭 종료 감지 -> 팝업감지 재개", Level.Info);
                    }
                    else
                    {
                        // 보조 사이트가 살아있으면 화면 잠금 감지(스크린 락) 건너뜀
                        return;
                    }
                }

                // 화면 잠금 해제 실행
                if (screenLockCallback != null)
                {
                    await screenLockCallback();
                }
            }
            catch (WebDriverException ex)
            {
                IsManagementActive = false;
                ManagementHandle = null;
                _lastWindowCount = -1;

                if (ex.Message.ToLower().Contains("invalid session id") ||
                    ex.Message.ToLower().Contains("not reachable") ||
                    ex.Message.ToLower().Contains("disconnected"))
                {
                    _logMessage("[세션 종료 감지] 클라우드 환경에 의해 크롬 브라우저가 닫혔습니다. 연결을 초기화합니다.", Level.Warning);
                    throw new InvalidOperationException("SessionDisconnected"); 
                }
                else
                {
                    _logMessage($"[인터락 예외] 탭 검사 중 통신 예외 발생: {ex.Message}", Level.Error);
                }
            }
            catch (Exception ex)
            {
                IsManagementActive = false;
                ManagementHandle = null;
                _lastWindowCount = -1;
                
                if (ex is InvalidOperationException && ex.Message == "SessionDisconnected")
                {
                    throw; // 위에서 던진 세션 종료 예외는 그대로 패스
                }
                
                _logMessage($"[인터락 예외] 탭 검사 중 알 수 없는 브라우저 예외 발생: {ex.Message}", Level.Error);
            }
            finally
            {
                _isTickRunning = false;
            }
        }
    }
}
