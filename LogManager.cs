using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace GateBot
{
    internal class LogManager
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        private static string _currentLogFilePath;

        public enum Level
        {
            Info,
            Error,
            Critical
        }


        public static void InitializeLogFile()
        {
            CreateLogDirectoryIfNotExists(); // 폴더 생성

            _currentLogFilePath = GetLogFilePath();
            CreateLogFileIfNotExists(_currentLogFilePath); // 로그파일 생성
        }

        // Message 로그 기록
        public static void LogMessage(string message, Level level, [CallerMemberName] string memberName = "")
        {
            CreateLogIfNecessary();

            try
            {
                using (StreamWriter writer = File.AppendText(_currentLogFilePath))
                {
                    writer.WriteLine($"[{DateTime.Now}] [{GetLogLevelString(level)}] 메서드: {memberName}, 메시지: {message}");
                    writer.WriteLine("----------------------------------------");
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"로그 기록 오류: {logEx.Message}");
            }
        }

        public static void LogException(Exception ex, Level level, [CallerMemberName] string memberName = "")
        {
            CreateLogIfNecessary();

            try
            {
                using (StreamWriter writer = File.AppendText(_currentLogFilePath))
                {
                    writer.WriteLine($"[{DateTime.Now}] [{GetLogLevelString(level)}] 메서드: {memberName}, 오류: {ex.Message}");
                    writer.WriteLine($"예외 타입: {ex.GetType()}");
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine("------------------------------------------------------------------------");
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"로그 기록 오류: {logEx.Message}");
            }
        }

        private static void CreateLogIfNecessary()
        {
            string newLogFilePath = GetLogFilePath();
            if (newLogFilePath != _currentLogFilePath)
            {
                _currentLogFilePath = newLogFilePath;
                CreateLogFileIfNotExists(_currentLogFilePath);
                LogMessage("Create Log", Level.Info);
            }
        }

        private static string GetLogFilePath()
        {
            return Path.Combine(LogDirectory, $"Trace_{DateTime.Now:yyyyMMdd}.log");
        }

        // 로그 폴더 생성
        private static void CreateLogDirectoryIfNotExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                try
                {
                    Directory.CreateDirectory(LogDirectory);
                    Console.WriteLine($"로그 디렉토리 생성: {LogDirectory}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"로그 디렉토리 생성 오류: {ex.Message}");
                }
            }
        }

        // 로그 파일 생성
        private static void CreateLogFileIfNotExists(string logFilePath)
        {
            if (!File.Exists(logFilePath))
            {
                try
                {
                    File.Create(logFilePath).Dispose();
                    Console.WriteLine($"로그 파일 생성: {logFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"로그 파일 생성 오류: {ex.Message}");
                }
            }
        }

        private static string GetLogLevelString(Level level)
        {
            switch (level)
            {
                case Level.Info: return "INFO";
                case Level.Error: return "ERROR";
                case Level.Critical: return "CRITICAL";
                default: return "UNKNOWN";
            }
        }

        internal static void LogException(OpenQA.Selenium.NoSuchElementException ex, object error)
        {
            throw new NotImplementedException();
        }

        internal static void LogException(OpenQA.Selenium.NoAlertPresentException ex, OpenQA.Selenium.LogLevel warning)
        {
            throw new NotImplementedException();
        }

        internal static void LogException(OpenQA.Selenium.NoSuchWindowException ex, OpenQA.Selenium.LogLevel warning)
        {
            throw new NotImplementedException();
        }
    }
}
