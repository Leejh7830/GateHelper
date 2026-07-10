using System;
using System.Collections.Generic;
using System.Linq;

namespace GateHelper
{
    public class WorkLogService
    {
        /// <summary>
        /// 필터 조건에 따라 리스트를 필터링합니다.
        /// </summary>
        public List<WorkLogEntry> FilterItems(List<WorkLogEntry> items, string query, bool hideDone)
        {
            if (items == null) return new List<WorkLogEntry>();
            return items.Where(entry => RowMatchesFilter(entry, query, hideDone)).ToList();
        }

        private bool RowMatchesFilter(WorkLogEntry entry, string query, bool hideDone)
        {
            if (hideDone && entry.Status == "DONE") return false;

            string q = query?.Trim().ToLower();
            if (string.IsNullOrEmpty(q)) return true;

            return (entry.Title?.ToLower().Contains(q) ?? false) ||
                   (entry.Content?.ToLower().Contains(q) ?? false) ||
                   (entry.Tags?.ToLower().Contains(q) ?? false) ||
                   (entry.Memo?.ToLower().Contains(q) ?? false) ||
                   (entry.Status?.ToLower().Contains(q) ?? false);
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
                Date = DateTime.Now
            };
        }

        /// <summary>
        /// 폰트 크기 범위 제한 (8~24pt)
        /// </summary>
        public float ClampFontSize(float currentSize, float delta)
            => Math.Max(8f, Math.Min(24f, currentSize + delta));
    }
}