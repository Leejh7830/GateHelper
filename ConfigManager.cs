﻿using System;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using Level = GateBot.LogManager.Level;

namespace GateBot
{
    internal class ConfigManager
    {
        private readonly string _appSettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.config");
        public Config LoadedConfig { get; private set; }

        public ConfigManager()
        {
            try
            {
                CreateConfigFiles(); // Create
                LoadConfig(); // Load
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
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
    <add key=""Favorite1"" value=""Fav1"" />
    <add key=""Favorite2"" value=""Fav2"" />
    <add key=""Favorite3"" value=""Fav3"" />
    <!-- Favorite 으로 검색 -->
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
                    LoadedConfig = null;
                }
            }
            catch (XmlException ex)
            {
                LogManager.LogException(ex, Level.Error, "Invalid XML format in default app settings.");
                MessageBox.Show($"Invalid XML format in default app settings.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadedConfig = null;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"Failed to create or load configuration files: {_appSettingsFilePath}");
                MessageBox.Show($"Failed to create or load configuration files.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadedConfig = null;
            }
        }

        public void LoadConfig()
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = _appSettingsFilePath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var missingFields = new System.Text.StringBuilder(); // 필수 항목 검증

                if (string.IsNullOrEmpty(config.AppSettings.Settings["Url"].Value)) missingFields.AppendLine("URL");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateID"].Value)) missingFields.AppendLine("GateID");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GatePW"].Value)) missingFields.AppendLine("GatePW");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["ChromePath"].Value)) missingFields.AppendLine("ChromePath");

                if (missingFields.Length > 0)
                {
                    string errorMessage = $"The following required items are missing from the configuration file:\n{missingFields}\n\nDo you want to open the configuration file to edit?";
                    LogManager.LogMessage(errorMessage, Level.Error);
                    if (MessageBox.Show(errorMessage, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                    {
                        // 2025.03.12 추가
                        // 설정 파일 열기 & 초기화
                        try
                        {
                            System.Diagnostics.Process.Start(_appSettingsFilePath);
                        }
                        catch (Exception ex)
                        {
                            LogManager.LogException(ex, Level.Error, $"Failed to open configuration file: {_appSettingsFilePath}");
                            MessageBox.Show($"Failed to open configuration file.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    LoadedConfig = null;
                    return;
                }

                LoadedConfig = new Config
                {
                    Url = config.AppSettings.Settings["Url"].Value,
                    GateID = config.AppSettings.Settings["GateID"].Value,
                    GatePW = config.AppSettings.Settings["GatePW"].Value,
                    ChromePath = config.AppSettings.Settings["ChromePath"].Value,
                    Fav1 = config.AppSettings.Settings["Favorite1"]?.Value,
                    Fav2 = config.AppSettings.Settings["Favorite2"]?.Value,
                    Fav3 = config.AppSettings.Settings["Favorite3"]?.Value
                };

                LogManager.LogMessage($"Configuration file loaded successfully: {_appSettingsFilePath}", Level.Info);
            }
            catch (ConfigurationErrorsException ex)
            {
                LogManager.LogException(ex, Level.Error, $"Configuration file load error: {_appSettingsFilePath}");
                MessageBox.Show($"Configuration file load error. Check the configuration file. \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"An unexpected error occurred while loading the configuration file: {_appSettingsFilePath}");
                MessageBox.Show($"An unexpected error occurred while loading the configuration file. \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}