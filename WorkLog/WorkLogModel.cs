using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GateHelper
{
    /// <summary>
    /// 개별 업무 로그 항목을 정의하는 모델 클래스
    /// </summary>
    [DataContract]
    public class WorkLogEntry
    {
        [DataMember] public int No { get; set; }
        [DataMember] public DateTime Date { get; set; } = DateTime.Now;
        [DataMember] public string Title { get; set; } = "";
        [DataMember] public string Content { get; set; } = "";
        [DataMember] public string Status { get; set; } = "OPEN";
        [DataMember] public string Priority { get; set; } = "NORMAL"; // HIGH, NORMAL, LOW
        [DataMember] public string Tags { get; set; } = "";
        [DataMember] public string Memo { get; set; } = "";
        [DataMember] public DateTime LastUpdated { get; set; } = DateTime.Now;
        [DataMember] public List<string> ImagePaths { get; set; } = new List<string>();

        // 이미지 존재 여부 확인 (UI 바인딩용)
        public bool HasImage => ImagePaths != null && ImagePaths.Count > 0;

        // 생성자: 기본값 초기화
        public WorkLogEntry()
        {
            Title = "";
            Content = "";
            Status = "OPEN";
            Priority = "NORMAL";
            Tags = "";
            Memo = "";
            ImagePaths = new List<string>();
        }

        // 수정 시간 갱신 로직
        public void Touch() => LastUpdated = DateTime.Now;

        // 상태 순환 변경 (OPEN -> ING.. -> DONE -> FIXED)
        public string CycleNextStatus()
        {
            switch (Status)
            {
                case "OPEN": Status = "ING.."; break;
                case "ING..": Status = "DONE"; break;
                case "DONE": Status = "FIXED"; break;
                case "FIXED": Status = "OPEN"; break;
                default: Status = "OPEN"; break;
            }
            Touch();
            return Status;
        }

        // 우선순위 순환 변경 (NORMAL -> HIGH -> LOW)
        public string CycleNextPriority()
        {
            switch (Priority)
            {
                case "NORMAL": Priority = "HIGH"; break;
                case "HIGH": Priority = "LOW"; break;
                case "LOW": Priority = "NORMAL"; break;
                default: Priority = "NORMAL"; break;
            }
            Touch();
            return Priority;
        }
    }

    /// <summary>
    /// 설정값과 항목 리스트를 포함하는 전체 데이터 구조
    /// </summary>
    public class WorkLogData
    {
        public float FontSize { get; set; } = 10f;
        public bool HideDone { get; set; } = false; // 사용자의 필터 설정 유지용
        public int SplitterDistance { get; set; } = 480; // 좌우 스플리터 위치 저장 (좌측 그리드 넉넉한 공간 확보)
        public int DetailSplitterDistance { get; set; } = 130; // 우측 상하 스플리터 위치 저장 (스크린샷 세로 높이 극대화)
        public string QuickFilter { get; set; } = "ALL"; // ALL, ING, TODAY, HIGH
        public List<WorkLogEntry> Items { get; set; } = new List<WorkLogEntry>();
    }
}