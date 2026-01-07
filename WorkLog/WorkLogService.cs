using System;
using System.Collections.Generic;
using System.Linq;
/*
namespace GateHelper
{
    public class WorkLogService
    {
        /// <summary>
        /// 설정된 필터 조건에 따라 리스트를 필터링합니다.
        /// </summary>
        public List<WorkLogEntry> FilterItems(List<WorkLogEntry> items, string query, bool hideDone)
        {
            if (items == null) return new List<WorkLogEntry>();

            return items.Where(entry => RowMatchesFilter(entry, query, hideDone)).ToList();
        }

        /// <summary>
        /// 개별 행이 필터 조건에 부합하는지 판별합니다. (핵심 로직)
        /// </summary>
        private bool RowMatchesFilter(WorkLogEntry entry, string query, bool hideDone)
        {
            // 1. 상태 필터 우선 적용
            if (hideDone && entry.Status == "DONE")
            {
                return false;
            }

            // 2. 검색어 필터 적용
            string q = query?.Trim().ToLower();
            if (string.IsNullOrEmpty(q))
            {
                return true;
            }

            return (entry.Title?.ToLower().Contains(q) ?? false) ||
                   (entry.Content?.ToLower().Contains(q) ?? false) ||
                   (entry.Tags?.ToLower().Contains(q) ?? false) ||
                   (entry.Memo?.ToLower().Contains(q) ?? false) ||
                   (entry.Status?.ToLower().Contains(q) ?? false);
        }

        /// <summary>
        /// 새 항목 추가 시 필요한 기본 데이터를 생성합니다.
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
        /// 폰트 크기 제한 로직 (비즈니스 정책)
        /// </summary>
        public float ClampFontSize(float currentSize, float delta)
        {
            return Math.Max(8f, Math.Min(24f, currentSize + delta));
        }
    }
}
*/