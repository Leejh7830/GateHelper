using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace GateHelper
{
    public class WorkLogRepository
    {
        private readonly string _dataPath;
        public string ImageDir { get; }  // 경로 중복 제거: 한 곳에서만 관리

        public WorkLogRepository()
        {
            _dataPath = Util.GetMetaPath("WorkLog.json");
            ImageDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            if (!Directory.Exists(ImageDir))
                Directory.CreateDirectory(ImageDir);
        }

        // --- 데이터 I/O ---

        public WorkLogData Load()
        {
            try
            {
                if (!File.Exists(_dataPath)) return new WorkLogData();

                string json = File.ReadAllText(_dataPath);
                if (string.IsNullOrWhiteSpace(json)) return new WorkLogData();

                WorkLogData data = null;
                string trimmed = json.TrimStart();
                if (trimmed.StartsWith("["))
                {
                    // [하위 호환성] 과거 구버전(JSON 배열 형태) 데이터 마이그레이션 지원
                    var items = JsonConvert.DeserializeObject<List<WorkLogEntry>>(json);
                    data = new WorkLogData { Items = items ?? new List<WorkLogEntry>() };
                }
                else
                {
                    data = JsonConvert.DeserializeObject<WorkLogData>(json);
                }

                if (data == null) data = new WorkLogData();
                if (data.Items == null) data.Items = new List<WorkLogEntry>();

                // 과거 데이터 무결성 보정 및 표준화 (전 버전 하위 호환성 보장)
                foreach (var entry in data.Items)
                {
                    if (entry == null) continue;

                    // 1. Status 표준화 (대소문자 및 구버전 값 대응: Open -> OPEN, Done -> DONE, Blocked -> OPEN)
                    if (string.IsNullOrWhiteSpace(entry.Status))
                    {
                        entry.Status = "OPEN";
                    }
                    else
                    {
                        string s = entry.Status.Trim();
                        if (string.Equals(s, "ING..", StringComparison.OrdinalIgnoreCase)) entry.Status = "ING..";
                        else if (string.Equals(s, "DONE", StringComparison.OrdinalIgnoreCase)) entry.Status = "DONE";
                        else if (string.Equals(s, "FIXED", StringComparison.OrdinalIgnoreCase)) entry.Status = "FIXED";
                        else if (string.Equals(s, "OPEN", StringComparison.OrdinalIgnoreCase)) entry.Status = "OPEN";
                        else if (string.Equals(s, "BLOCKED", StringComparison.OrdinalIgnoreCase)) entry.Status = "OPEN";
                        else entry.Status = s.ToUpperInvariant();
                    }

                    // 2. Priority 누락 시 기본값 설정
                    if (string.IsNullOrWhiteSpace(entry.Priority))
                    {
                        entry.Priority = "NORMAL";
                    }
                    else
                    {
                        string p = entry.Priority.Trim().ToUpperInvariant();
                        if (p == "HIGH" || p == "LOW" || p == "NORMAL") entry.Priority = p;
                        else entry.Priority = "NORMAL";
                    }

                    // 3. 컬렉션 및 문자열 null 가드
                    if (entry.ImagePaths == null) entry.ImagePaths = new List<string>();
                    if (entry.Title == null) entry.Title = "";
                    if (entry.Content == null) entry.Content = "";
                    if (entry.Memo == null) entry.Memo = "";
                    if (entry.Tags == null) entry.Tags = "";
                }

                return data;
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Error);
                return new WorkLogData();
            }
        }

        /// <summary>
        /// 임시 파일에 먼저 쓴 후 교체 → 저장 중 예외 발생 시 기존 데이터 보호
        /// </summary>
        public void Save(WorkLogData data)
        {
            string tmpPath = _dataPath + ".tmp";
            string bakPath = _dataPath + ".bak";
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(tmpPath, json);

                // tmp → 실제 파일로 원자적 교체 (bak으로 이전 버전 보존)
                if (File.Exists(_dataPath))
                    File.Replace(tmpPath, _dataPath, bakPath);
                else
                    File.Move(tmpPath, _dataPath);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Error);
                // tmp 찌꺼기 정리
                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
            }
        }

        // --- 이미지 관리 ---

        public string GetFullImagePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
            if (Path.IsPathRooted(fileName) && File.Exists(fileName))
                return fileName;
            return Path.Combine(ImageDir, Path.GetFileName(fileName));
        }

        /// <summary>
        /// 안전한 Bitmap 복사본을 받아 백그라운드에서 JPEG로 저장합니다.
        /// 호출 전 UI 스레드에서 new Bitmap(clipboardImage)로 복사본을 만들어 넘겨야 합니다.
        /// </summary>
        public async Task<string> SaveImageAsync(Bitmap bmp, int entryNo, int nextIndex)
        {
            string timePart = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{entryNo}_{timePart}_{nextIndex}.jpg";
            string fullPath = Path.Combine(ImageDir, fileName);

            // 중복 파일명 안전 처리
            int safetyCopy = 1;
            while (File.Exists(fullPath))
            {
                fileName = $"{entryNo}_{timePart}_{nextIndex}_{safetyCopy++}.jpg";
                fullPath = Path.Combine(ImageDir, fileName);
            }

            // 저장만 백그라운드 처리 (Bitmap은 이미 UI 스레드에서 안전하게 생성된 복사본)
            await Task.Run(() => bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg));

            return File.Exists(fullPath) ? fileName : null;
        }

        /// <summary>
        /// 항목에 연결된 이미지 파일을 물리적으로 삭제합니다.
        /// </summary>
        public List<string> DeleteImages(List<string> fileNames)
        {
            var failedFiles = new List<string>();
            foreach (var fileName in fileNames)
            {
                try
                {
                    string fullPath = Path.Combine(ImageDir, fileName);
                    if (File.Exists(fullPath)) File.Delete(fullPath);
                }
                catch
                {
                    failedFiles.Add(fileName);
                }
            }
            return failedFiles;
        }
    }
}