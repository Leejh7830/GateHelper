using System;
using System.Collections.Generic;

namespace GateHelper.LogValidator.Models
{
    [Serializable]
    public class LogValidatorConfig
    {
        // 💡 공장 고유 식별자 복수 관리 (J1, J2, H1 등 대응 가능)
        public List<string> FactoryPrefixes { get; set; } = new List<string>();

        // 라인 구역 리ست (E, A, F, P)
        public List<string> LineZones { get; set; } = new List<string>();

        // 설비 타입 리스트 (STO, OHS, CNV)
        public List<string> EquipmentTypes { get; set; } = new List<string>();

        // 💡 [최종 조합 프로퍼티] 공장코드 x 구역 x 타입을 조합하여 최종 정규식용 리스트 도출
        // 예: J1 + E + STO = J1ESTO
        public List<string> CombinedEquipmentList
        {
            get
            {
                var combined = new List<string>();
                foreach (var factory in FactoryPrefixes)
                {
                    foreach (var zone in LineZones)
                    {
                        foreach (var type in EquipmentTypes)
                        {
                            combined.Add($"{factory}{zone}{type}");
                        }
                    }
                }
                return combined;
            }
        }

        public LogValidatorConfig()
        {
            // 완전 유연화된 마스터 기본값 초기 가드 세팅
            FactoryPrefixes.Add("J1");
            LineZones.AddRange(new[] { "E", "A", "F", "P" });
            EquipmentTypes.AddRange(new[] { "STO", "OHS", "CNV" });
        }
    }
}