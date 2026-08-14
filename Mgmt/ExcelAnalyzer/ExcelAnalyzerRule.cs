using System;
using System.Collections.Generic;

namespace GateHelper.Mgmt.ExcelAnalyzer
{
    /// <summary>
    /// 단일 검증 시나리오(규칙) 데이터를 담는 클래스
    /// </summary>
    public class RuleProfile
    {
        public string RuleName { get; set; } = "New Rule";
        
        /// <summary>
        /// 이 규칙이 적용될 설비(호기) 이름 리스트 (예: J1FSTO11301)
        /// 1:1 고정 매칭을 위해 와일드카드 대신 정확한 이름을 사용합니다.
        /// </summary>
        public List<string> MappedMachines { get; set; } = new List<string>();

        /// <summary>
        /// 대상 아이템 (옵션, 예: StockerSEM)
        /// </summary>
        public string TargetItem { get; set; } = "";

        /// <summary>
        /// 값이 호기마다 반드시 달라야 하는 고유값 변수명 리스트 (예: HostID, IP 등)
        /// </summary>
        public List<string> UniqueVariables { get; set; } = new List<string>();

        /// <summary>
        /// 값이 시나리오 내 호기들끼리 무조건 같아야 하는 공통값 변수명 리스트 (예: Timeout 등)
        /// </summary>
        public List<string> CommonVariables { get; set; } = new List<string>();

        /// <summary>
        /// 예외 처리(검사 무시)로 지정된 변수명 리스트
        /// </summary>
        public List<string> ExceptionVariables { get; set; } = new List<string>();

        /// <summary>
        /// 특별 관리(강조)로 지정된 변수명 리스트
        /// </summary>
        public List<string> HighlightVariables { get; set; } = new List<string>();
    }

    /// <summary>
    /// _meta/excel_rules.json 등에 저장/로드할 때 최상위 루트로 사용되는 래퍼 클래스
    /// </summary>
    public class ExcelAnalyzerConfig
    {
        public List<RuleProfile> Profiles { get; set; } = new List<RuleProfile>();
        
        /// <summary>
        /// 마지막으로 사용된 엑셀 컬럼 매핑 정보를 기억하기 위한 설정
        /// </summary>
        public string LastMappedMachineColumn { get; set; } = "";
        public string LastMappedUnitColumn { get; set; } = "";
        public string LastMappedNameColumn { get; set; } = "";
        public string LastMappedValueColumn { get; set; } = "";
        public string LastMappedDescColumn { get; set; } = "";
    }

    /// <summary>
    /// 엑셀에서 추출한 단일 행 데이터 모델
    /// </summary>
    public class ExcelRowData
    {
        public string MachineName { get; set; }
        public string VariableName { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public int RowIndex { get; set; }
    }

    /// <summary>
    /// 검증 엔진이 반환할 에러 상세 정보
    /// </summary>
    public class ValidationError
    {
        public string MachineName { get; set; }
        public string RuleName { get; set; }
        public string VariableName { get; set; }
        
        /// <summary>
        /// UniqueViolation (중복 에러) 또는 CommonViolation (불일치 에러)
        /// </summary>
        public string ErrorType { get; set; } 
        
        public string Description { get; set; }
        public string ExpectedValue { get; set; }
        public string ActualValue { get; set; }
    }
}
