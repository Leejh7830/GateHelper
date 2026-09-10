using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;
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
    <add key=""GateOneURL"" value="""" />
    
    <!-- 이메일 인증을 위한 Enportal URL -->
    <add key=""EnportalURL"" value="""" />

    <!-- 자동로그인 및 팝업제어용 GATEONE ID/PW -->
    <add key=""GateUserID"" value="""" />
    <add key=""GateUserPW"" value="""" />
    
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

    <!-- Manufacturing Managerment 접속 URL -->
    <add key=""ManagementUrl"" value="""" />
    
    <!-- Manufacturing Managerment ID/PW -->
    <add key=""ManagementUserID"" value="""" />
    <add key=""ManagementUserPW"" value="""" />

    <!-- Chrome 설치경로 -->
    <add key=""ChromePath"" value=""C:\Program Files\Google\Chrome\Application\chrome.exe"" />
  </appSettings>
</configuration>";

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(defaultAppSettings);

                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = "\r\n",
                        NewLineHandling = NewLineHandling.Replace
                    };

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

        private void LoadConfig()
        {
            try
            {
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = _configFilePath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                var missingFields = new System.Text.StringBuilder();

                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateOneURL"]?.Value)) missingFields.AppendLine("GateOneURL");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["EnportalURL"]?.Value)) missingFields.AppendLine("EnportalURL");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateUserID"]?.Value)) missingFields.AppendLine("GateUserID");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateUserPW"]?.Value)) missingFields.AppendLine("GateUserPW");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateName_A"]?.Value)) missingFields.AppendLine("GateName_A");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GateID_A"]?.Value)) missingFields.AppendLine("GateID_A");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["GatePW_A"]?.Value)) missingFields.AppendLine("GatePW_A");
                if (string.IsNullOrEmpty(config.AppSettings.Settings["ChromePath"]?.Value)) missingFields.AppendLine("ChromePath");

                if (missingFields.Length > 0)
                {
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
                    GateOneURL = config.AppSettings.Settings["GateOneURL"]?.Value ?? "",
                    EnportalURL = config.AppSettings.Settings["EnportalURL"]?.Value ?? "",
                    GateUserID = config.AppSettings.Settings["GateUserID"]?.Value ?? "",
                    GateUserPW = config.AppSettings.Settings["GateUserPW"]?.Value ?? "",
                    GateName_A = config.AppSettings.Settings["GateName_A"]?.Value ?? "",
                    GateID_A = config.AppSettings.Settings["GateID_A"]?.Value ?? "",
                    GatePW_A = config.AppSettings.Settings["GatePW_A"]?.Value ?? "",
                    GateName_B = config.AppSettings.Settings["GateName_B"]?.Value ?? "",
                    GateID_B = config.AppSettings.Settings["GateID_B"]?.Value ?? "",
                    GatePW_B = config.AppSettings.Settings["GatePW_B"]?.Value ?? "",
                    Fav1 = config.AppSettings.Settings["Favorite1"]?.Value ?? "",
                    Fav2 = config.AppSettings.Settings["Favorite2"]?.Value ?? "",
                    Fav3 = config.AppSettings.Settings["Favorite3"]?.Value ?? "",
                    ManagementUrl = config.AppSettings.Settings["ManagementUrl"]?.Value ?? "",
                    ManagementUserID = config.AppSettings.Settings["ManagementUserID"]?.Value ?? "",
                    ManagementUserPW = config.AppSettings.Settings["ManagementUserPW"]?.Value ?? "",
                    ChromePath = config.AppSettings.Settings["ChromePath"]?.Value ?? @"C:\Program Files\Google\Chrome\Application\chrome.exe"
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
                if (!File.Exists(_configFilePath))
                {
                    LogManager.LogMessage("Configuration file not found. Attempting to create...", Level.Info);
                    CreateConfigFiles();
                }

                if (File.Exists(_configFilePath))
                {
                    System.Diagnostics.Process.Start(_configFilePath);
                }
                else
                {
                    MessageBox.Show("설정 파일을 생성할 수 없거나 경로가 잘못되었습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, $"Failed to open configuration file: {_configFilePath}");
                MessageBox.Show($"Failed to open configuration file.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --------------------------------------------------------------
        // 새로 추가된 메서드: 평문 암호 검출 후 자동 암호화(마이그레이션)
        // 대상 키: GateUserPW, ManagementUserPW
        // autoProtect == true 이면 즉시 백업 후 암호화하여 덮어씀
        // autoProtect == false 이면 아무 작업 안함 (원하실 경우 사용자 묻기 로직 추가 가능)
        public void MigratePlainPasswords(bool autoProtect)
        {
            try
            {
                LogManager.LogMessage($"Credential migration: start (autoProtect={autoProtect})", Level.Info);

                var map = new ExeConfigurationFileMap { ExeConfigFilename = _configFilePath };
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

                string[] keys = new[] { "GateUserPW", "ManagementUserPW" };
                bool changed = false;
                int protectedCount = 0;

                foreach (var key in keys)
                {
                    try
                    {
                        var setting = configuration.AppSettings.Settings[key];
                        if (setting == null)
                        {
                            LogManager.LogMessage($"Config key '{key}' not found - skipped.", Level.Info);
                            continue;
                        }

                        string stored = setting.Value ?? string.Empty;
                        if (string.IsNullOrEmpty(stored))
                        {
                            LogManager.LogMessage($"Config key '{key}' is empty - skipped.", Level.Info);
                            continue;
                        }

                        LogManager.LogMessage($"Checking config key '{key}'...", Level.Info);

                        // 접두사 기반 판별
                        bool looksProtected = stored.StartsWith("DPAPI:");
                        if (looksProtected)
                        {
                            // 복호화 확인 (정상인지 체크)
                            try
                            {
                                var _ = CredentialHelper.Unprotect(stored);
                                LogManager.LogMessage($"Config key '{key}' already protected (DPAPI prefix + unprotect OK).", Level.Info);
                                continue;
                            }
                            catch
                            {
                                LogManager.LogMessage($"Config key '{key}' has DPAPI prefix but unprotect failed - leaving as-is.", Level.Warning);
                                continue;
                            }
                        }

                        // 접두사 없음 -> 평문일 가능성
                        LogManager.LogMessage($"Config key '{key}' appears to be plaintext.", Level.Info);

                        if (!autoProtect)
                        {
                            LogManager.LogMessage($"Auto-protect disabled - skipping encryption for '{key}'.", Level.Info);
                            continue;
                        }

                        // autoProtect == true 이면 암호화 시도
                        try
                        {
                            // 백업 생성(최소한의 안전장치)
                            try
                            {
                                var bakPath = _configFilePath + ".bak";
                                if (!File.Exists(bakPath))
                                    File.Copy(_configFilePath, bakPath);
                            }
                            catch
                            {
                                // best-effort backup - 실패해도 계속
                                LogManager.LogMessage($"Failed to create config backup (best-effort).", Level.Warning);
                            }

                            // 암호화하여 덮어쓰기
                            string protectedValue = CredentialHelper.Protect(stored);
                            setting.Value = protectedValue;
                            changed = true;
                            protectedCount++;
                            LogManager.LogMessage($"Config key '{key}' auto-protected (DPAPI saved).", Level.Info);
                        }
                        catch (Exception ex)
                        {
                            LogManager.LogException(ex, Level.Warning, $"Failed to protect key {key}");
                            // 실패하면 설정 파일 변경하지 않음
                        }
                    }
                    catch (Exception exKey)
                    {
                        LogManager.LogException(exKey, Level.Warning, $"Error while processing config key '{key}'");
                    }
                }

                if (changed)
                {
                    try
                    {
                        configuration.Save(ConfigurationSaveMode.Modified);
                        LogManager.LogMessage($"Credential migration: configuration saved. Protected keys: {protectedCount}", Level.Info);
                    }
                    catch (Exception ex)
                    {
                        LogManager.LogException(ex, Level.Error, "Failed to save protected config.");
                    }
                }
                else
                {
                    LogManager.LogMessage("Credential migration: no changes applied.", Level.Info);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, Level.Error, "MigratePlainPasswords overall failure");
            }
            finally
            {
                LogManager.LogMessage("Credential migration: finished.", Level.Info);
            }
        }
    }
}