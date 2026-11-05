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

            string noStr = entry.No.ToString();
            string hashNo = "#" + noStr;

            return noStr.Contains(q) ||
                   hashNo.Contains(q) ||
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
            var doneItems = items.Where(x => (x.Status == "DONE" || x.Status == "FIXED") && (x.LastUpdated.Date == date.Date || x.Date.Date == date.Date)).ToList();
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
        /// 최초 사용자 또는 데이터가 비어있을 때 시스템 구조를 한눈에 파악할 수 있는 고품질 샘플/가이드 데이터를 생성합니다.
        /// </summary>
        public List<WorkLogEntry> CreateSampleEntries()
        {
            var now = DateTime.Now;
            return new List<WorkLogEntry>
            {
                new WorkLogEntry
                {
                    No = 1,
                    Date = now.AddHours(-2),
                    Title = "[오류 접수] MES 생산라인 게이트웨이 통신 지연",
                    Status = "OPEN",
                    Priority = "HIGH",
                    Content = "• 발생 위치: 생산 AP-03 라인 메인 게이트웨이\r\n• 현상: 10분 간격으로 소켓 응답 지연 발생 (HTTP 504)\r\n• 조치 필요: 방화벽 세션 풀 및 스위치 포트 트래픽 확인 요망",
                    Memo = "금일 14:00 설비 담당자 미팅 및 패킷 덤프 분석 예정",
                    LastUpdated = now.AddHours(-2)
                },
                new WorkLogEntry
                {
                    No = 2,
                    Date = now.AddHours(-4),
                    Title = "[정기 점검] 11월 1주차 DB 백업 및 로그 파일 압축",
                    Status = "ING..",
                    Priority = "NORMAL",
                    Content = "• 대상 서버: DB-01, DB-02 마스터/슬레이브\r\n• 작업 내용: 트랜잭션 로그 백업 및 30일 경과 아카이브 압축\r\n• 진행률: DB-01 완료, DB-02 압축 진행 중 (약 75%)",
                    Memo = "작업 완료 후 디스크 여유 공간 150GB 이상 확보 확인 필요",
                    LastUpdated = now.AddMinutes(-20)
                },
                new WorkLogEntry
                {
                    No = 3,
                    Date = now.AddDays(-1),
                    Title = "[UI 개선] WorkLog Studio 가독성 및 네온 디자인 통일",
                    Status = "DONE",
                    Priority = "LOW",
                    Content = "• 폰트 크기 확대: 제목 12pt Bold, 본문/메모 11.5pt 적용\r\n• 단일행 제목 입력창 개편 및 좌측 📌 제목 뱃지 배치\r\n• 액션 버튼 1px 네온 테두리 디자인 일관화\r\n• 날짜 연-월-일(yyyy-MM-dd) 표시 및 상태 드롭다운 전환",
                    Memo = "사용자 피드백 반영 완료 및 릴리즈 빌드 무결성 검증 성공",
                    LastUpdated = now.AddHours(-1)
                },
                new WorkLogEntry
                {
                    No = 4,
                    Date = now.AddDays(-2),
                    Title = "[버그 수정] 스크린샷 팝아웃 새창 듀얼 모니터 잔류 현상",
                    Status = "FIXED",
                    Priority = "HIGH",
                    Content = "• 문제 현상: 새 업무 등록 직후 팝아웃 버튼 클릭 시 이전 이미지 캐시 잔류\r\n• 원인 분석: 뷰어 이미지 인덱스 초기화 누락\r\n• 해결 조치: ClearImage 및 null 가드 보강",
                    Memo = "패치 적용 후 이미지 없는 항목 팝아웃 시 정상 안내 메시지 출력 확인 완료",
                    LastUpdated = now.AddDays(-1)
                },
                new WorkLogEntry
                {
                    No = 5,
                    Date = now.AddDays(-3),
                    Title = "[기능 검토] 주간 일일보고 엑셀(XLSX) 자동 변환 연동",
                    Status = "OPEN",
                    Priority = "NORMAL",
                    Content = "• 부서 보고용 주간 단위 업무 로그 ClosedXML 내보내기 기능 검토\r\n• 완료/진행/대기 건별 시트 분리 또는 필터 서식 적용 방안 설계",
                    Memo = "ClosedXML 라이브러리 활용하여 제로 리소스 방식으로 추가 예정",
                    LastUpdated = now.AddDays(-3)
                }
            };
        }

        /// <summary>
        /// 폰트 크기 범위 제한 (8~24pt)
        /// </summary>
        public float ClampFontSize(float currentSize, float delta)
            => Math.Max(8f, Math.Min(24f, currentSize + delta));
    }
}