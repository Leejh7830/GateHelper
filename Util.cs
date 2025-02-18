using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace GateBot
{
    public static class Util
    {
        // ChromeDriver 초기화 메소드
        public static IWebDriver InitializeDriver()
        {
            try
            {
                // WebDriverManager를 사용하여 ChromeDriver 자동 업데이트
                new WebDriverManager.DriverManager().SetUpDriver(new WebDriverManager.DriverConfigs.Impl.ChromeConfig());

                // ChromeDriver 초기화
                return new ChromeDriver(); // 드라이버 서비스 경로 명시 없이 초기화
            }
            catch (Exception ex)
            {
                // 드라이버 초기화 실패 시 예외 처리
                Console.WriteLine($"드라이버 초기화 오류: {ex.Message}");
                throw; // 예외를 다시 던져서 호출한 곳에서 처리할 수 있도록 합니다.
            }
        }

        // ChromeDriver 종료 메소드
        public static void CloseDriver(IWebDriver driver)
        {
            try
            {
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"드라이버 종료 오류: {ex.Message}");
            }
        }

        // 열려 있는 탭에서 URL 확인하는 메소드
        public static void CheckOpenUrls(IWebDriver driver, string[] targetUrls)
        {
            bool isTargetUrl = false;

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
                        if (currentUrl.Contains(url))
                        {
                            Console.WriteLine($"열려 있는 탭에 {url}가 있습니다!");
                            isTargetUrl = true;
                        }
                    }
                }

                if (!isTargetUrl)
                {
                    Console.WriteLine("열려 있는 탭에 대상 URL이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"URL 확인 오류: {ex.Message}");
            }
        }
    }
}
