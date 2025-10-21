using System;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using Level = GateHelper.LogManager.Level;

namespace GateHelper
{
    internal class ConfigManager
    {
        private readonly string _configFilePath;
        public Config LoadedConfig { get; private set; }

        public ConfigManager()
        {
            try
            {
                // _meta 폴더 생성 및 경로 지정
                _configFilePath = Util.GetMetaPath("settings.config");

                // 루트에 설정 파일이 있고 meta에 없으면 이동(또는 복사) 시도
                string rootConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.config");
                if (File.Exists(rootConfigPath) && !File.Exists(_configFilePath))
                {
                    try
                    {
                        File.Move(rootConfigPath, _configFilePath);
                        LogManager.LogMessage($"Moved root settings.config to {_configFilePath}", Level.Info);
                    }
                    catch (Exception exMove)
                    {
                        LogManager.LogException(exMove, Level.Error, "Move root settings.config to _meta failed. Trying copy.");
                        try
                        {
                            File.Copy(rootConfigPath, _configFilePath);
                            try { File.Delete(rootConfigPath); } catch { /* 무시 */ }
                            LogManager.LogMessage($"Copied root settings.config to {_configFilePath}", Level.Info);
                        }
                        catch (Exception exCopy)
                        {
                            LogManager.LogException(exCopy, Level.Error, "Copy root settings.config to _meta failed.");
                            // 최종적으로는 계속해서 CreateConfigFiles()가 _configFilePath 위치에 파일을 생성하게 함
                        }
                    }
                }

                CreateConfigFiles(); // Create 또는 Exist 확인
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error);
            }
        }

        // 25.08.15 Added - Enportal ID/PW 추가
        // 25.10.21 Added - Gate Preset(A/B) 추가
        private void CreateConfigFiles()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    string defaultAppSettings = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <appSettings>
    <!-- Value 부분에 값을 입력하세요 -->
    <!-- GateOne 접속 URL -->
    <add key=""Url"" value="""" />

    <!-- 자동로그인 및 팝업제어용 EnPortal ID/PW -->
    <add key=""EnportalID"" value="""" />
    <add key=""EnportalPW"" value="""" />
    
    <!-- VM접속용 ID/PW Preset A -->
    <add key=""GateName_A"" value="""" />
    <add key=""GateID_A"" value="""" />
    <add key=""GatePW_A"" value="""" />

    <!-- VM접속용 ID/PW Preset B (선택) -->
    <add key=""GateName_B"" value="""" />
    <add key=""GateID_B"" value="""" />
    <add key=""GatePW_B"" value="""" />
        
    <!-- Favorite 해당 값으로 검색 -->
    <add key=""Favorite1"" value=""Fav1"" />
    <add key=""Favorite2"" value=""Fav2"" />
    <add key=""Favorite3"" value=""Fav3"" />

    <!-- Chrome 설치경로 -->
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
                    using (XmlWriter writer = XmlWriter.Create(_configFilePath, settings))
                    {
                        xmlDoc.Save(writer);
                    }

                    MessageBox.Show($"Configuration file created. Please enter the information and restart the program.\nFile path: {_configFilePath}",
                        "Configuration file creation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LogManager.LogMessage($"Configuration file created successfully: {_configFilePath}", Level.Info);
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
                LogManager.LogException(ex, Level.Error, $"Failed to create or load configuration files: {_configFilePath}");
                MessageBox.Show($"Failed to create or load configuration files.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadedConfig = null;
            }
        }
        public void ReloadConfig()
        {
            LoadConfig(); // 설정 파일 로드
        }

        // 25.08.15 Added - Enportal ID/PW 추가
        private void LoadConfig()
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = _configFilePath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var missingFields = new System.Text.StringBuilder(); // 25.03.12 필수 항목 검증

                if (string.IsNullOrEmpty(config.AppSettings.Settings["Url"].Value)) missingFields.AppendLine("URL");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["EnportalID"].Value)) missingFields.AppendLine("EnportalID");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["EnportalPW"].Value)) missingFields.AppendLine("EnportalPW");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateName_A"].Value)) missingFields.AppendLine("GateName_A");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateID_A"].Value)) missingFields.AppendLine("GateID_A");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GatePW_A"].Value)) missingFields.AppendLine("GatePW_A");
                // if (string.IsNullOrEmpty(config.AppSettings.Settings["GateName_B"].Value)) missingFields.AppendLine("GateName_B");
                // if (string.IsNullOrEmpty(config.AppSettings.Settings["GateID_B"].Value)) missingFields.AppendLine("GateID_B");
                // if (string.IsNullOrEmpty(config.AppSettings.Settings["GatePW_B"].Value)) missingFields.AppendLine("GatePW_B");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["ChromePath"].Value)) missingFields.AppendLine("ChromePath");

                if (missingFields.Length > 0)
                {
                    // 한글 메시지로 변경
                    string missingList = missingFields.ToString().Trim();
                    string errorMessage = $"설정 파일에 다음 필수 항목이 비어있습니다:\n{missingList}\n\n설정 파일을 열어 수정하시겠습니까?";
                    LogManager.LogMessage(errorMessage, Level.Error);

                    if (MessageBox.Show(errorMessage, "설정 파일 오류", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        OpenConfigFile(); // 설정 파일 열기
                    }

                    LoadedConfig = null;
                    return;
                }

                LoadedConfig = new Config
                {
                    Url = config.AppSettings.Settings["Url"].Value,
                    EnportalID = config.AppSettings.Settings["EnportalID"].Value,
                    EnportalPW = config.AppSettings.Settings["EnportalPW"].Value,
                    GateName_A = config.AppSettings.Settings["GateName_A"].Value,
                    GateID_A = config.AppSettings.Settings["GateID_A"].Value,
                    GatePW_A = config.AppSettings.Settings["GatePW_A"].Value,
                    GateName_B = config.AppSettings.Settings["GateName_B"].Value,
                    GateID_B = config.AppSettings.Settings["GateID_B"].Value,
                    GatePW_B = config.AppSettings.Settings["GatePW_B"].Value,
                    ChromePath = config.AppSettings.Settings["ChromePath"].Value,
                    Fav1 = config.AppSettings.Settings["Favorite1"]?.Value,
                    Fav2 = config.AppSettings.Settings["Favorite2"]?.Value,
                    Fav3 = config.AppSettings.Settings["Favorite3"]?.Value
                };

                LogManager.LogMessage($"Configuration file loaded successfully: {_configFilePath}", Level.Info);
            }
            catch (ConfigurationErrorsException ex)
            {
                LogManager.LogException(ex, Level.Error, $"Configuration file load error: {_configFilePath}");
                MessageBox.Show($"설정 파일 로드 중 오류가 발생했습니다. 설정 파일을 확인하세요.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"An unexpected error occurred while loading the configuration file: {_configFilePath}");
                MessageBox.Show($"설정 파일을 불러오는 중 예상치 못한 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        public void OpenConfigFile()
        {
            try
            {
                System.Diagnostics.Process.Start(_configFilePath);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"Failed to open configuration file: {_configFilePath}");
                MessageBox.Show($"Failed to open configuration file.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}