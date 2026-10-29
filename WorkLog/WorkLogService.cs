using System;
using System.Collections.Generic;
using System.Linq;

namespace GateHelper
{
    public class WorkLogStats
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int Ing { get; set; }
        public int Done { get; set; }
        public int Fixed { get; set; }
        public int HighPriority { get; set; }
        public int TodayUpdated { get; set; }
        public double DonePercent => Total == 0 ? 0 : Math.Round((double)(Done + Fixed) / Total * 100.0, 1);
    }

    public class WorkLogService
    {
        /// <summary>
        /// 필터 조건에 따라 리스트를 필터링합니다 (텍스트 검색 + 완료 숨김 + 퀵 필터)
        /// </summary>
        public List<WorkLogEntry> FilterItems(List<WorkLogEntry> items, string query, bool hideDone, string quickFilter = "ALL")
        {
            if (items == null) return new List<WorkLogEntry>();
            return items.Where(entry => RowMatchesFilter(entry, query, hideDone, quickFilter)).ToList();
        }

        private bool RowMatchesFilter(WorkLogEntry entry, string query, bool hideDone, string quickFilter)
        {
            // 1. 완료 숨김 필터
            if (hideDone && (entry.Status == "DONE" || entry.Status == "FIXED"))
                return false;

            // 2. 퀵 필터 분기
            if (!string.IsNullOrEmpty(quickFilter) && quickFilter != "ALL")
            {
                switch (quickFilter)
                {
                    case "ING":
                        if (entry.Status != "ING..") return false;
                        break;
                    case "OPEN":
                        if (entry.Status != "OPEN") return false;
                        break;
                    case "HIGH":
                        if (entry.Priority != "HIGH") return false;
                        break;
                    case "TODAY":
                        var today = DateTime.Today;
                        if (entry.Date.Date != today && entry.LastUpdated.Date != today) return false;
                        break;
                }
            }

            // 3. 텍스트 검색 쿼리
            string q = query?.Trim().ToLower();
            if (string.IsNullOrEmpty(q)) return true;

            return (entry.No.ToString().Contains(q)) ||
                   (entry.Title?.ToLower().Contains(q) ?? false) ||
                   (entry.Content?.ToLower().Contains(q) ?? false) ||
                   (entry.Tags?.ToLower().Contains(q) ?? false) ||
                   (entry.Memo?.ToLower().Contains(q) ?? false) ||
                   (entry.Status?.ToLower().Contains(q) ?? false) ||
                   (entry.Priority?.ToLower().Contains(q) ?? false);
        }

        /// <summary>
        /// 상태 및 업무 진행 현황 통계를 계산합니다.
        /// </summary>
        public WorkLogStats GetStats(List<WorkLogEntry> items)
        {
            var stats = new WorkLogStats();
            if (items == null || items.Count == 0) return stats;

            stats.Total = items.Count;
            var today = DateTime.Today;

            foreach (var item in items)
            {
                switch (item.Status)
                {
                    case "OPEN": stats.Open++; break;
                    case "ING..": stats.Ing++; break;
                    case "DONE": stats.Done++; break;
                    case "FIXED": stats.Fixed++; break;
                    default: stats.Open++; break;
                }

                if (item.Priority == "HIGH")
                    stats.HighPriority++;

                if (item.Date.Date == today || item.LastUpdated.Date == today)
                    stats.TodayUpdated++;
            }

            return stats;
        }

        /// <summary>
        /// 오늘/특정 날짜의 업무 보고 텍스트를 마크다운 포맷으로 깔끔하게 생성합니다.
        /// 메신저(Slack, Teams, 잔디) 및 메일 보고에 즉시 붙여넣을 수 있습니다.
        /// </summary>
        public string GenerateDailyReport(List<WorkLogEntry> items, DateTime? targetDate = null)
        {
            if (items == null || items.Count == 0)
                return "기록된 업무가 없습니다.";

            DateTime date = targetDate ?? DateTime.Today;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[ {date:yyyy-MM-dd} 일일 업무 보고 ]");
            sb.AppendLine("──────────────────────────────────────────");

            // 1. 오늘 완료/해결된 업무
            var doneItems = items.Where(x => (x.Status == "DONE" || x.Status == "FIXED") && x.LastUpdated.Date == date.Date).ToList();
            sb.AppendLine($"✅ 오늘 완료 ({doneItems.Count}건)");
            if (doneItems.Count == 0)
            {
                sb.AppendLine("   (해당 사항 없음)");
            }
            else
            {
                foreach (var item in doneItems)
                {
                    string tagText = string.IsNullOrWhiteSpace(item.Tags) ? "" : $" [{item.Tags}]";
                    sb.AppendLine($"   • [#{item.No}] {item.Title}{tagText}");
                }
            }
            sb.AppendLine();

            // 2. 현재 진행 중인 업무
            var ingItems = items.Where(x => x.Status == "ING..").OrderByDescending(x => x.Priority == "HIGH").ToList();
            sb.AppendLine($"🔄 진행 중인 작업 ({ingItems.Count}건)");
            if (ingItems.Count == 0)
            {
                sb.AppendLine("   (진행 중인 작업 없음)");
            }
            else
            {
                foreach (var item in ingItems)
                {
                    string pTag = item.Priority == "HIGH" ? "[긴급] " : "";
                    string tagText = string.IsNullOrWhiteSpace(item.Tags) ? "" : $" [{item.Tags}]";
                    sb.AppendLine($"   • [#{item.No}] {pTag}{item.Title}{tagText}");
                }
            }
            sb.AppendLine();

            // 3. 대기 중 / 오늘 신규 등록된 업무
            var openItems = items.Where(x => x.Status == "OPEN" && (x.Priority == "HIGH" || x.Date.Date == date.Date)).ToList();
            if (openItems.Count > 0)
            {
                sb.AppendLine($"⏳ 주요 대기/신규 ({openItems.Count}건)");
                foreach (var item in openItems)
                {
                    string pTag = item.Priority == "HIGH" ? "[긴급] " : "";
                    sb.AppendLine($"   • [#{item.No}] {pTag}{item.Title}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("──────────────────────────────────────────");
            return sb.ToString();
        }

        /// <summary>
        /// 새 항목 생성 (No 자동 채번)
        /// </summary>
        public WorkLogEntry CreateNewEntry(List<WorkLogEntry> currentItems)
        {
            int nextNo = (currentItems == null || currentItems.Count == 0)
                         ? 1
                         : currentItems.Max(x => x.No) + 1;

            return new WorkLogEntry
            {
                No = nextNo,
                Date = DateTime.Now,
                Title = "새 업무 항목",
                Status = "OPEN",
                Priority = "NORMAL"
            };
        }

        /// <summary>
        /// 폰트 크기 범위 제한 (8~24pt)
        /// </summary>
        public float ClampFontSize(float currentSize, float delta)
            => Math.Max(8f, Math.Min(24f, currentSize + delta));
    }
}