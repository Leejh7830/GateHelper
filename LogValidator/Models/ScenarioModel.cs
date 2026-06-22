using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateHelper.LogValidator.Models
{
    // 💡 [2단계: 우측 상단 대기소 유닛 모델]
    // 템플릿 형태로 쟁여두고 재사용할 순수 유닛 블록입니다.
    public class UnitTemplateModel
    {
        public string EventName { get; set; }        // 예: EVT_POPUP_DETECT
        public string MaskingPattern { get; set; }   // RichTextBox에서 최종 정제된 노란 마스킹 문자열
    }

    // 💡 [3단계: 우측 하단 사다리 시퀀스 모델]
    // 사다리 틀에 딱 맞춰 순번(Step)을 달고 실제로 배치된 인스턴스입니다.
    public class ScenarioStepModel
    {
        public int StepNo { get; set; }             // 사다리 칸 순번 (1, 2, 3...)
        public string EventName { get; set; }        // 할당된 유닛의 이름
        public string MaskingPattern { get; set; }   // 검증에 사용될 최종 패턴 규칙
    }
}