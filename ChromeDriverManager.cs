using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;

namespace GateHelper
{
    class ChromeDriverManager
    {
        


        public static ChromeOptions ChromeDriverOptionSet(string chromePath)
        {
            // Chrome 옵션 설정
            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            return options;
        }



    }
}
