using System;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using OpenQA.Selenium;
using Level = GateBot.LogManager.Level;

namespace GateBot
{
    internal class ConfigManager
    {
        private readonly string _configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        private readonly string _appSettingsFilePath;
        public Config LoadedConfig { get; private set; }

        public ConfigManager()
        {
            _appSettingsFilePath = Path.Combine(_configDirectory, "appsettings.config");
            try
            {
                CreateConfigDirectory();
                CreateConfigFiles();
                LoadConfig();
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
            }
        }

        private void CreateConfigDirectory()
        {
            if (!Directory.Exists(_configDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_configDirectory);
                    LogManager.LogMessage($"Create Config Directory: {_configDirectory}", Level.Info);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex, Level.Error, "Create Config Directory Fail");
                    throw;
                }
            }
        }

        private void CreateConfigFiles()
        {
            try
            {
                if (!File.Exists(_appSettingsFilePath))
                {
                    string defaultAppSettings = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <appSettings>
    <add key=""Url"" value="""" />
    <add key=""GateID"" value="""" />
    <add key=""GatePW"" value="""" />
    <add key=""ChromePath"" value=""C:\Program Files\Google\Chrome\Application\chrome.exe"" />
  </appSettings>
</configuration>";

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(defaultAppSettings);

                    // XmlWriter 설정
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true, // 들여쓰기 활성화
                        IndentChars = "  ", // 들여쓰기 문자 (2개의 공백)
                        NewLineChars = "\r\n", // 줄바꿈 문자
                        NewLineHandling = NewLineHandling.Replace // 줄바꿈 문자 처리
                    };

                    // XmlWriter를 사용하여 파일에 쓰기
                    using (XmlWriter writer = XmlWriter.Create(_appSettingsFilePath, settings))
                    {
                        xmlDoc.Save(writer);
                    }

                    MessageBox.Show($"Configuration file created. Please enter the information and restart the program.\nFile path: {_appSettingsFilePath}",
                        "Configuration file creation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LogManager.LogMessage($"Configuration file created successfully: {_appSettingsFilePath}", Level.Info);

                    LogManager.LogMessage("프로그램 종료", Level.Info);
                    Environment.Exit(0);
                }
            }
            catch (XmlException ex)
            {
                LogManager.LogException(ex, Level.Error, "Invalid XML format in default app settings.");
                throw;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, "Failed to create or load configuration files.");
                throw;
            }
        }

        // Config 파일 로드
        private void LoadConfig()
        {
            try
            {
                string configFilePath = _appSettingsFilePath;
                LoadedConfig = new Config
                {
                    Url = ConfigurationManager.AppSettings["Url"],
                    GateID = ConfigurationManager.AppSettings["GateID"],
                    GatePW = ConfigurationManager.AppSettings["GatePW"],
                    ChromePath = ConfigurationManager.AppSettings["ChromePath"]
                };

                // 필수 항목 검증
                var missingFields = new System.Text.StringBuilder();

                if (string.IsNullOrEmpty(LoadedConfig.Url)) missingFields.AppendLine("URL");
                if (string.IsNullOrEmpty(LoadedConfig.GateID)) missingFields.AppendLine("GateID");
                if (string.IsNullOrEmpty(LoadedConfig.GatePW)) missingFields.AppendLine("GatePW");
                if (string.IsNullOrEmpty(LoadedConfig.ChromePath)) missingFields.AppendLine("ChromePath");

                if (missingFields.Length > 0)
                {
                    MessageBox.Show($"The following items are missing from the configuration file:\n{missingFields}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadedConfig = null;
                    return;
                }

                LogManager.LogMessage($"Configuration file loaded successfully: {_appSettingsFilePath}", Level.Info);
            }
            catch (ConfigurationErrorsException ex)
            {
                LogManager.LogException(ex, Level.Error, $"Configuration file load error: {_appSettingsFilePath}");
                MessageBox.Show($"Configuration file load error. Check the configuration file. \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadedConfig = null;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"An unexpected error occurred while loading the configuration file: {_appSettingsFilePath}");
                MessageBox.Show($"An unexpected error occurred while loading the configuration file. \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadedConfig = null;
            }
        }
    }
}