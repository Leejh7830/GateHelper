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
                return JsonConvert.DeserializeObject<WorkLogData>(json) ?? new WorkLogData();
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
            => Path.Combine(ImageDir, fileName);

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