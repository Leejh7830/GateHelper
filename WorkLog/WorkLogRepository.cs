/*
 * using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GateHelper
{
    public class WorkLogRepository
    {
        private readonly string _dataPath;
        private readonly string _imageDir;

        public WorkLogRepository()
        {
            // 데이터 경로 및 이미지 폴더 경로 설정 (Util 클래스 의존성 유지)
            _dataPath = Util.GetMetaPath("WorkLog.json");
            _imageDir = Path.Combine(Path.GetDirectoryName(_dataPath), "WorkLog_Images");

            if (!Directory.Exists(_imageDir))
            {
                Directory.CreateDirectory(_imageDir);
            }
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

        public void Save(WorkLogData data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex, LogManager.Level.Error);
            }
        }

        // --- 이미지 관리 ---

        public string GetFullImagePath(string fileName)
        {
            return Path.Combine(_imageDir, fileName);
        }

        /// <summary>
        /// 이미지를 비동기로 저장하고 생성된 파일명을 반환합니다.
        /// </summary>
        public async Task<string> SaveImageAsync(Image img)
        {
            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{DateTime.Now.Ticks}.jpg";
            string fullPath = Path.Combine(_imageDir, fileName);

            using (Bitmap bmp = new Bitmap(img))
            {
                await Task.Run(() => bmp.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg));
            }

            return File.Exists(fullPath) ? fileName : null;
        }

        /// <summary>
        /// 특정 항목에 연결된 이미지 파일들을 물리적으로 삭제합니다.
        /// </summary>
        public List<string> DeleteImages(List<string> fileNames)
        {
            List<string> failedFiles = new List<string>();
            foreach (var fileName in fileNames)
            {
                try
                {
                    string fullPath = Path.Combine(_imageDir, fileName);
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
*/