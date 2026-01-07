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
            Tags = "";
            Memo = "";
            ImagePaths = new List<string>();
        }

        // 수정 시간 갱신 로직
        public void Touch() => LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// 설정값과 항목 리스트를 포함하는 전체 데이터 구조
    /// </summary>
    public class WorkLogData
    {
        public float FontSize { get; set; } = 10f;
        public bool HideDone { get; set; } = false; // 사용자의 필터 설정 유지용
        public List<WorkLogEntry> Items { get; set; } = new List<WorkLogEntry>();
    }
}