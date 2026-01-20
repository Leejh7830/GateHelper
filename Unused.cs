namespace GateHelper
{
    class Unused
    {
        #region Unused Method
        /*
        // 열려 있는 탭에서 URL 확인하는 메소드
        public static void CheckOpenUrls(IWebDriver driver)
        {
            bool isTargetUrl = false;

            // 확인할 URL 목록을 메서드 내부에서 정의
            string[] targetUrls =
            {
                "https://naver.com",
                "https://google.com",
                "https://example.com"
            };

            try
            {
                foreach (var handle in driver.WindowHandles)
                {
                    // 각 창/탭으로 전환
                    driver.SwitchTo().Window(handle);

                    // 현재 URL 확인
                    string currentUrl = driver.Url;

                    // 대상 URL 중 하나와 일치하는지 확인
                    foreach (string url in targetUrls)
                    {
                        if (currentUrl.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show($"열려 있는 탭에 {url}가 있습니다!", "URL 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            isTargetUrl = true;
                            break;
                        }
                    }
                }

                // 대상 URL이 하나도 열려 있지 않은 경우
                if (!isTargetUrl)
                {
                    MessageBox.Show("열려 있는 탭에 대상 URL이 없습니다.", "URL 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // URL 확인 중 오류 발생 시
                MessageBox.Show($"URL 확인 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */
        #endregion
    }
}
