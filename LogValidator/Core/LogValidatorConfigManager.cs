using System;
using System.IO;
using System.Collections.Generic;
using GateHelper.LogValidator.Models;

namespace GateHelper.LogValidator.Core
{
    // 💡 GateHelper 프로젝트 마스터 유틸리티와 연동되는 검증기 전용 설정 매니저
    public static class LogValidatorConfigManager
    {
        private static readonly string ConfigPath = Util.GetMetaPath("log_validator_config.dat");

        public static LogValidatorConfig Current { get; private set; } = new LogValidatorConfig();

        public static void Load()
        {
            try
            {
                // Util.MetaFolder 프로퍼티 호출 자체로 _meta 폴더의 생존이 100% 보장됩니다.
                string baseMetaPath = Util.MetaFolder;

                if (!File.Exists(ConfigPath))
                {
                    Current = new LogValidatorConfig();
                    Save();
                    return;
                }

                string[] lines = File.ReadAllLines(ConfigPath);
                if (lines.Length >= 3)
                {
                    var config = new LogValidatorConfig();

                    // 1번째 줄: 공장코드 파싱
                    config.FactoryPrefixes.Clear();
                    string[] factories = lines[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in factories) config.FactoryPrefixes.Add(f.Trim().ToUpper());

                    // 2번째 줄: 구역 파싱
                    config.LineZones.Clear();
                    string[] zones = lines[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var zone in zones) config.LineZones.Add(zone.Trim().ToUpper());

                    // 3번째 줄: 설비타입 파싱
                    config.EquipmentTypes.Clear();
                    string[] types = lines[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var type in types) config.EquipmentTypes.Add(type.Trim().ToUpper());

                    Current = config;
                }
            }
            catch (Exception ex)
            {
                // Util.cs와 동일한 LogManager 인터페이스로 예외 로그 기록 위임
                LogManager.LogException(ex, LogManager.Level.Error, "LogValidatorConfig 로드 실패");
                Current = new LogValidatorConfig();
            }
        }

        public static void Save()
        {
            try
            {
                // 저장 전 폴더 생존 보장 유도
                string baseMetaPath = Util.MetaFolder;

                string factoriesStr = string.Join(",", Current.FactoryPrefixes);
                string zonesStr = string.Join(",", Current.LineZones);
                string typesStr = string.Join(",", Current.EquipmentTypes);

                List<string> contents = new List<string>
                {
                    factoriesStr,
                    zonesStr,
                    typesStr
                };

                File.WriteAllLines(ConfigPath, contents);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Error, "LogValidatorConfig 저장 실패");
            }
        }
    }
}