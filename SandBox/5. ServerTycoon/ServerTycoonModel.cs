using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace GateHelper.ServerTycoon
{
    public class ServerItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubTitle { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public double BaseCost { get; set; }
        public double BaseRps { get; set; }
        public double PowerWatts { get; set; } // 대당 소비 전력 (W)
        public double HeatOutput { get; set; } // 대당 발열량
        public string AccentColorHex { get; set; }

        [JsonIgnore]
        public Color AccentColor => ColorTranslator.FromHtml(AccentColorHex ?? "#00E5FF");

        public double GetNextCost(int buyCount = 1)
        {
            if (buyCount <= 1)
            {
                return Math.Floor(BaseCost * Math.Pow(1.18, Level));
            }

            double total = 0;
            for (int i = 0; i < buyCount; i++)
            {
                total += Math.Floor(BaseCost * Math.Pow(1.18, Level + i));
            }
            return total;
        }

        public int GetMaxAffordable(double money)
        {
            int count = 0;
            double cost = 0;
            while (count < 1000)
            {
                double nextOne = Math.Floor(BaseCost * Math.Pow(1.18, Level + count));
                if (cost + nextOne > money) break;
                cost += nextOne;
                count++;
            }
            return Math.Max(1, count);
        }

        public double GetTotalRps(double multiplier = 1.0)
        {
            return Level * BaseRps * multiplier;
        }

        public double GetTotalPowerDemand(double efficiencyMult = 1.0)
        {
            return Level * PowerWatts * efficiencyMult;
        }

        public double GetTotalHeatOutput()
        {
            return Level * HeatOutput;
        }
    }

    public class PowerItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubTitle { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public double BaseCost { get; set; }
        public double SupplyWatts { get; set; }
        public int OfflineBonusMinutes { get; set; }
        public string AccentColorHex { get; set; }

        [JsonIgnore]
        public Color AccentColor => ColorTranslator.FromHtml(AccentColorHex ?? "#FFA726");

        public double GetNextCost(int buyCount = 1)
        {
            if (buyCount <= 1)
            {
                return Math.Floor(BaseCost * Math.Pow(1.22, Level));
            }
            double total = 0;
            for (int i = 0; i < buyCount; i++)
            {
                total += Math.Floor(BaseCost * Math.Pow(1.22, Level + i));
            }
            return total;
        }

        public int GetMaxAffordable(double money)
        {
            int count = 0;
            double cost = 0;
            while (count < 1000)
            {
                double nextOne = Math.Floor(BaseCost * Math.Pow(1.22, Level + count));
                if (cost + nextOne > money) break;
                cost += nextOne;
                count++;
            }
            return Math.Max(1, count);
        }

        public double GetTotalSupply()
        {
            return Level * SupplyWatts;
        }
    }

    public class CoolingItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubTitle { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public double BaseCost { get; set; }
        public double CoolingCapacity { get; set; }
        public string AccentColorHex { get; set; }

        [JsonIgnore]
        public Color AccentColor => ColorTranslator.FromHtml(AccentColorHex ?? "#00E5FF");

        public double GetNextCost(int buyCount = 1)
        {
            if (buyCount <= 1)
            {
                return Math.Floor(BaseCost * Math.Pow(1.22, Level));
            }
            double total = 0;
            for (int i = 0; i < buyCount; i++)
            {
                total += Math.Floor(BaseCost * Math.Pow(1.22, Level + i));
            }
            return total;
        }

        public int GetMaxAffordable(double money)
        {
            int count = 0;
            double cost = 0;
            while (count < 1000)
            {
                double nextOne = Math.Floor(BaseCost * Math.Pow(1.22, Level + count));
                if (cost + nextOne > money) break;
                cost += nextOne;
                count++;
            }
            return Math.Max(1, count);
        }

        public double GetTotalCooling(double efficiencyMult = 1.0)
        {
            return Level * CoolingCapacity * efficiencyMult;
        }
    }

    public class SkillNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int MaxLevel { get; set; } = 5;
        public int[] LevelCosts { get; set; } = new int[] { 1, 2, 4, 8, 15 };

        public int GetCostForLevel(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            if (LevelCosts != null && currentLevel < LevelCosts.Length)
            {
                return LevelCosts[currentLevel];
            }
            return (currentLevel + 1) * 3;
        }

        public string GetLevelDescription(int currentLevel)
        {
            switch (Id)
            {
                case "skill_power_eff":
                    return currentLevel >= MaxLevel 
                        ? "서버 전력 -30% 절감 (MAX)" 
                        : $"서버 전력 -{currentLevel * 6}% ➔ -{(currentLevel + 1) * 6}% 절감";
                case "skill_cooling_boost":
                    return currentLevel >= MaxLevel 
                        ? "냉각 효율 +50% 증가 (MAX)" 
                        : $"냉각 효율 +{currentLevel * 10}% ➔ +{(currentLevel + 1) * 10}% 증가";
                case "skill_backbone":
                    return currentLevel >= MaxLevel 
                        ? "글로벌 RPS +60% 증가 (MAX)" 
                        : $"글로벌 RPS +{currentLevel * 12}% ➔ +{(currentLevel + 1) * 12}% 증가";
                case "skill_battery_max":
                    return currentLevel >= MaxLevel 
                        ? "오프라인 상한 +3h 20m (MAX)" 
                        : $"오프라인 상한 +{currentLevel * 40}분 ➔ +{(currentLevel + 1) * 40}분";
                case "skill_crit_packet":
                    return currentLevel >= MaxLevel 
                        ? "크릿 25% · 잭팟 8배 (MAX)" 
                        : $"크릿 {5 + currentLevel * 4}% (x{3 + currentLevel}) ➔ {5 + (currentLevel + 1) * 4}% (x{4 + currentLevel})";
                case "skill_ai_devops":
                    return currentLevel >= MaxLevel 
                        ? "장애 보상 2.5배 & 자동 진압 (MAX)" 
                        : (currentLevel >= 2 ? $"장애 보상 +{(currentLevel + 1) * 30}% & 자동 진압" : $"장애 보상 +{(currentLevel + 1) * 30}% (Lv.3시 드론 자동진압)");
                default:
                    return Description;
            }
        }
    }

    public enum IncidentType
    {
        None,
        DdosAttack,
        Overheat,
        MemoryLeak
    }

    public class TycoonIncident
    {
        public IncidentType Type { get; set; }
        public int TargetServerIndex { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public double RewardCash { get; set; }
        public float RemainingSeconds { get; set; }

        public static TycoonIncident GenerateRandom(int maxUnlockedServerIndex, Random rand, double currentRps)
        {
            int target = rand.Next(0, Math.Max(1, maxUnlockedServerIndex + 1));
            int roll = rand.Next(0, 3);
            double baseReward = Math.Max(30.0, currentRps * 12.0);

            switch (roll)
            {
                case 0:
                    return new TycoonIncident
                    {
                        Type = IncidentType.DdosAttack,
                        TargetServerIndex = target,
                        Title = "⚡ DDoS 공격 감지!",
                        Message = "대량 트래픽 유입! 긴급 패킷 방어 클릭!",
                        RewardCash = Math.Round(baseReward * 1.5, 0),
                        RemainingSeconds = 12f
                    };
                case 1:
                    return new TycoonIncident
                    {
                        Type = IncidentType.Overheat,
                        TargetServerIndex = target,
                        Title = "▲ 랙 발열 경보!",
                        Message = "서버 랙 온도 85°C 초과! 쿨러 가동 클릭!",
                        RewardCash = Math.Round(baseReward * 1.2, 0),
                        RemainingSeconds = 10f
                    };
                default:
                    return new TycoonIncident
                    {
                        Type = IncidentType.MemoryLeak,
                        TargetServerIndex = target,
                        Title = "◈ 메모리 누수 발생!",
                        Message = "좀비 프로세스 누적! 긴급 GC 회수 클릭!",
                        RewardCash = Math.Round(baseReward * 1.0, 0),
                        RemainingSeconds = 14f
                    };
            }
        }
    }

    public class FloatingText
    {
        public string Text;
        public float X;
        public float Y;
        public float VelocityY;
        public float Alpha;
        public Color TextColor;
        public bool IsCrit;
    }

    public class PacketParticle
    {
        public float X;
        public float Y;
        public float StartX;
        public float StartY;
        public float TargetX;
        public float TargetY;
        public float Progress;
        public float Speed;
        public Color ParticleColor;
        public int ServerIndex;
    }

    public class Shockwave
    {
        public float X;
        public float Y;
        public float Radius;
        public float MaxRadius;
        public float Alpha;
        public Color WaveColor;
    }

    #region 1. S/W Studio Models
    public class SoftwareCategoryDef
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SubTitle { get; set; }
        public string Icon { get; set; }
        public double UnlockCost { get; set; }
        public double RequiredPoints { get; set; }
        public double BaseRoyaltyPerSec { get; set; }
        public int OptimalUx { get; set; }
        public int OptimalAlgo { get; set; }
        public int OptimalSec { get; set; }
        public string AccentColorHex { get; set; }
    }

    public class ActiveSoftwareProject
    {
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public double TargetPoints { get; set; }
        public double CurrentPoints { get; set; }
        public int FocusUx { get; set; } = 40;
        public int FocusAlgo { get; set; } = 40;
        public int FocusSec { get; set; } = 20;

        [JsonIgnore]
        public bool IsCompleted => CurrentPoints >= TargetPoints;

        [JsonIgnore]
        public double ProgressPercent => TargetPoints > 0 ? Math.Min(100.0, (CurrentPoints / TargetPoints) * 100.0) : 0;
    }

    public class ReleasedSoftware
    {
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public double Rating { get; set; } // 1.0 ~ 10.0
        public string ReviewSummary { get; set; }
        public double RoyaltyPerSec { get; set; }
        public int RemainingSecs { get; set; }
        public double TotalEarned { get; set; }
    }

    public class SoftwareStudioState
    {
        public ActiveSoftwareProject CurrentProject { get; set; }
        public List<ReleasedSoftware> ReleasedProducts { get; set; } = new List<ReleasedSoftware>();
        public int SelectedCategoryIndex { get; set; } = 0;
        public int TotalSoftwareShipped { get; set; } = 0;

        public static List<SoftwareCategoryDef> GetCategories()
        {
            return new List<SoftwareCategoryDef>
            {
                new SoftwareCategoryDef
                {
                    Id = 0,
                    Name = "인디 모바일 게임",
                    SubTitle = "대중적 접근성, 빠른 제작 및 초기 폭발적 판매",
                    Icon = "sw_game",
                    UnlockCost = 2500.0,
                    RequiredPoints = 4000.0,
                    BaseRoyaltyPerSec = 22.0,
                    OptimalUx = 55, OptimalAlgo = 25, OptimalSec = 20,
                    AccentColorHex = "#FF4081"
                },
                new SoftwareCategoryDef
                {
                    Id = 1,
                    Name = "B2B 전사 ERP",
                    SubTitle = "기업 맞춤형 회계·물류 시스템, 높은 정기 라이선스",
                    Icon = "sw_erp",
                    UnlockCost = 35000.0,
                    RequiredPoints = 35000.0,
                    BaseRoyaltyPerSec = 260.0,
                    OptimalUx = 20, OptimalAlgo = 45, OptimalSec = 35,
                    AccentColorHex = "#448AFF"
                },
                new SoftwareCategoryDef
                {
                    Id = 2,
                    Name = "생성형 AI 모델",
                    SubTitle = "멀티모달 딥러닝 인공지능, 전 세계 구독 모델",
                    Icon = "sw_ai",
                    UnlockCost = 350000.0,
                    RequiredPoints = 250000.0,
                    BaseRoyaltyPerSec = 2500.0,
                    OptimalUx = 25, OptimalAlgo = 60, OptimalSec = 15,
                    AccentColorHex = "#B388FF"
                },
                new SoftwareCategoryDef
                {
                    Id = 3,
                    Name = "제로트러스트 백신",
                    SubTitle = "침입 탐지 및 랜섬웨어 방어, 서버 돌발 장애 발생 억제",
                    Icon = "sw_sec",
                    UnlockCost = 2800000.0,
                    RequiredPoints = 1800000.0,
                    BaseRoyaltyPerSec = 16000.0,
                    OptimalUx = 15, OptimalAlgo = 35, OptimalSec = 50,
                    AccentColorHex = "#00E676"
                },
                new SoftwareCategoryDef
                {
                    Id = 4,
                    Name = "분산 클라우드 OS",
                    SubTitle = "글로벌 데이터센터 통합 커널, 엔드게임급 글로벌 플랫폼",
                    Icon = "sw_os",
                    UnlockCost = 35000000.0,
                    RequiredPoints = 18000000.0,
                    BaseRoyaltyPerSec = 150000.0,
                    OptimalUx = 25, OptimalAlgo = 45, OptimalSec = 30,
                    AccentColorHex = "#FFD700"
                }
            };
        }
    }
    #endregion

    #region 2. B2B Contracts Models
    public enum SlaRequirementType
    {
        PowerUptime = 0,    // 전력 부족 정전 금지
        ThermalStrict = 1,  // 온도 45°C 이하 저지연 유지
        HighBandwidth = 2   // 고대역폭 안정 유지 (RPS 100% 가동률)
    }

    public class B2BContract
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public string Title { get; set; }
        public string TierBadge { get; set; } // D, C, B, A, S
        public double RequiredRps { get; set; }
        public int DurationSecs { get; set; }
        public double RewardCash { get; set; }
        public double PenaltyCash { get; set; }
        public SlaRequirementType SlaType { get; set; }
        public string SlaRuleDescription { get; set; }
        public int RemainingSecs { get; set; }
        public int ViolationsCount { get; set; }
        public string ClientColorHex { get; set; }

        public static B2BContract GenerateRandom(string repTier, Random rand, double currentRps)
        {
            string[] clients = { "스타트업 핀포인트", "네오페이 결제망", "빅글로벌 엔터테인먼트", "하이퍼스케일 AI 연구단", "국방 우주 클라우드" };
            string[] titles = { "실시간 마이크로서비스 백엔드 분산 호스팅", "초당 3만건 결제 트래픽 릴레이", "e스포츠 결승전 8K 스트리밍 인프라", "LLM 파운데이션 모델 텐서 분산 연산 노드", "양자 암호화 군집 위성 관제 통신망" };
            SlaRequirementType[] slaTypes = { SlaRequirementType.PowerUptime, SlaRequirementType.ThermalStrict, SlaRequirementType.HighBandwidth };
            string[] slaDescs = { "무정전 100% 전력 공급 필수 (정전 누적 시 위반)", "저지연 45°C 이하 쿨링 엄수 (과열 누적 시 위반)", "100% 최대 가동률 납기 (스로틀링 불가)" };
            string[] colors = { "#00E5FF", "#76FF03", "#FFD54F", "#FF4081", "#E040FB" };

            int tierIdx = 0;
            switch (repTier)
            {
                case "C": tierIdx = rand.Next(0, 2); break;
                case "B": tierIdx = rand.Next(1, 3); break;
                case "A": tierIdx = rand.Next(2, 4); break;
                case "S": tierIdx = rand.Next(3, 5); break;
                default: tierIdx = 0; break;
            }

            int slaIdx = rand.Next(0, 3);
            double baseReqRps = Math.Max(25.0, Math.Pow(6.8, tierIdx + 1) * 5.0);
            double reward = Math.Round(baseReqRps * 130.0 * (1.0 + rand.NextDouble() * 0.4), 0);
            double penalty = Math.Round(reward * 0.35, 0);

            return new B2BContract
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                ClientName = clients[tierIdx],
                Title = titles[tierIdx],
                TierBadge = (tierIdx == 0 ? "D" : (tierIdx == 1 ? "C" : (tierIdx == 2 ? "B" : (tierIdx == 3 ? "A" : "S")))),
                RequiredRps = Math.Round(baseReqRps, 0),
                DurationSecs = 35 + (tierIdx * 8),
                RewardCash = reward,
                PenaltyCash = penalty,
                SlaType = slaTypes[slaIdx],
                SlaRuleDescription = slaDescs[slaIdx],
                ClientColorHex = colors[tierIdx],
                RemainingSecs = 35 + (tierIdx * 8),
                ViolationsCount = 0
            };
        }
    }

    public class B2BMarketState
    {
        public string ReputationTier { get; set; } = "D"; // D, C, B, A, S
        public int ReputationScore { get; set; } = 20;
        public int SuccessfulContractsCount { get; set; } = 0;
        public B2BContract ActiveContract { get; set; }
        public List<B2BContract> AvailableOffers { get; set; } = new List<B2BContract>();
        public int RefreshCooldownSecs { get; set; } = 30;

        public void RefreshOffers(Random rand, double currentRps)
        {
            if (AvailableOffers == null) AvailableOffers = new List<B2BContract>();
            AvailableOffers.Clear();
            for (int i = 0; i < 3; i++)
            {
                AvailableOffers.Add(B2BContract.GenerateRandom(ReputationTier, rand, currentRps));
            }
            RefreshCooldownSecs = 35;
        }

        public void AddReputation(int delta)
        {
            ReputationScore = Math.Max(0, Math.Min(1000, ReputationScore + delta));
            if (ReputationScore >= 450) ReputationTier = "S";
            else if (ReputationScore >= 250) ReputationTier = "A";
            else if (ReputationScore >= 120) ReputationTier = "B";
            else if (ReputationScore >= 50) ReputationTier = "C";
            else ReputationTier = "D";
        }
    }
    #endregion

    #region 3. Crypto Mine & Trading Models
    public class CryptoMarketState
    {
        public string CoinSymbol { get; set; } = "BYT";
        public string CoinName { get; set; } = "ByteCoin";
        public double CurrentPrice { get; set; } = 120.0;
        public double TargetPrice { get; set; } = 120.0;
        public double PriceChangePercent { get; set; } = 0.0;
        public List<double> PriceHistory { get; set; } = new List<double>();
        public int MiningHashPercent { get; set; } = 0; // 0, 25, 50, 75, 100
        public double MinedCoins { get; set; } = 0.0;
        public double TotalMinedCoins { get; set; } = 0.0;
        public string MarketNews { get; set; } = "글로벌 핀테크, 차세대 분산 원장 노드 인프라 구축 추진 발표.";
        public int NewsCooldownSecs { get; set; } = 18;

        public void InitHistory()
        {
            if (PriceHistory == null || PriceHistory.Count == 0)
            {
                PriceHistory = new List<double>();
                double p = CurrentPrice > 0 ? CurrentPrice : 120.0;
                for (int i = 0; i < 35; i++) PriceHistory.Add(p);
            }
        }
    }
    #endregion

    public class TycoonGameState
    {
        public double Money { get; set; } = 50.0;
        public double CurrentRunEarned { get; set; } = 50.0; // 현재 회차 누적 자금 (환생 조건 계산용)
        public double TotalEarned { get; set; } = 50.0;     // 역대 총 누적 자금
        public long TotalClicks { get; set; } = 0;
        public int PrestigeCount { get; set; } = 0;
        public int ResearchTokens { get; set; } = 0;
        public int TotalTokensEarned { get; set; } = 0;
        public List<string> UnlockedSkills { get; set; } = new List<string>();
        public Dictionary<string, int> SkillLevels { get; set; } = new Dictionary<string, int>();
        public DateTime LastSaveTimeUtc { get; set; } = DateTime.UtcNow;

        // 신규 3대 게임 시스템 상태
        public SoftwareStudioState SoftwareStudio { get; set; } = new SoftwareStudioState();
        public B2BMarketState B2BMarket { get; set; } = new B2BMarketState();
        public CryptoMarketState CryptoMarket { get; set; } = new CryptoMarketState();

        public int GetSkillLevel(string skillId)
        {
            if (SkillLevels != null && SkillLevels.TryGetValue(skillId, out int lvl))
            {
                return lvl;
            }
            if (UnlockedSkills != null && UnlockedSkills.Contains(skillId))
            {
                return 1;
            }
            return 0;
        }

        public bool HasSkill(string skillId)
        {
            return GetSkillLevel(skillId) > 0;
        }

        public void SetSkillLevel(string skillId, int level)
        {
            if (SkillLevels == null) SkillLevels = new Dictionary<string, int>();
            SkillLevels[skillId] = level;
            if (UnlockedSkills == null) UnlockedSkills = new List<string>();
            if (!UnlockedSkills.Contains(skillId)) UnlockedSkills.Add(skillId);
        }

        // 3대 인프라 자산 리스트
        public List<ServerItem> Servers { get; set; } = new List<ServerItem>();
        public List<PowerItem> PowerFacilities { get; set; } = new List<PowerItem>();
        public List<CoolingItem> CoolingFacilities { get; set; } = new List<CoolingItem>();

        // 런타임 전용 상태 (저장 제외)
        [JsonIgnore] public float FeverMeter = 0f;
        [JsonIgnore] public bool IsFeverActive = false;
        [JsonIgnore] public float FeverRemainingSeconds = 0f;
        [JsonIgnore] public TycoonIncident CurrentIncident = null;

        public static List<ServerItem> CreateDefaultServers()
        {
            return new List<ServerItem>
            {
                new ServerItem
                {
                    Id = "rpi",
                    Name = "라즈베리 파이 4B",
                    SubTitle = "초경량 가성비 쿼드코어 노드",
                    Icon = "🍓",
                    Level = 1,
                    BaseCost = 15.0,
                    BaseRps = 1.0,
                    PowerWatts = 15.0,
                    HeatOutput = 0.4,
                    AccentColorHex = "#FF5376"
                },
                new ServerItem
                {
                    Id = "tower",
                    Name = "오피스 타워 PC",
                    SubTitle = "창고 구형 데스크탑의 부활",
                    Icon = "🖥️",
                    Level = 0,
                    BaseCost = 320.0,
                    BaseRps = 7.5,
                    PowerWatts = 90.0,
                    HeatOutput = 2.2,
                    AccentColorHex = "#FFA726"
                },
                new ServerItem
                {
                    Id = "blade1u",
                    Name = "1U 블레이드 랙",
                    SubTitle = "엔터프라이즈 IDC 입문 섀시",
                    Icon = "🗄️",
                    Level = 0,
                    BaseCost = 6800.0,
                    BaseRps = 60.0,
                    PowerWatts = 450.0,
                    HeatOutput = 9.0,
                    AccentColorHex = "#00E676"
                },
                new ServerItem
                {
                    Id = "xeon4u",
                    Name = "4U 제온 워크스테이션",
                    SubTitle = "64코어 대역폭 연산 몬스터",
                    Icon = "⚡",
                    Level = 0,
                    BaseCost = 125000.0,
                    BaseRps = 500.0,
                    PowerWatts = 2000.0,
                    HeatOutput = 24.0,
                    AccentColorHex = "#00E5FF"
                },
                new ServerItem
                {
                    Id = "gpu_h100",
                    Name = "NVIDIA H100 GPU 랙",
                    SubTitle = "초거대 LLM AI 모델 분산 가속",
                    Icon = "🔮",
                    Level = 0,
                    BaseCost = 3500000.0,
                    BaseRps = 5500.0,
                    PowerWatts = 11000.0,
                    HeatOutput = 70.0,
                    AccentColorHex = "#B388FF"
                },
                new ServerItem
                {
                    Id = "quantum",
                    Name = "양자 초전도 큐비트 코어",
                    SubTitle = "시공간 초월 극저온 양자 연산기",
                    Icon = "⚛️",
                    Level = 0,
                    BaseCost = 150000000.0,
                    BaseRps = 70000.0,
                    PowerWatts = 60000.0,
                    HeatOutput = 180.0,
                    AccentColorHex = "#69F0AE"
                }
            };
        }

        public static List<PowerItem> CreateDefaultPower()
        {
            return new List<PowerItem>
            {
                new PowerItem
                {
                    Id = "outlet",
                    Name = "가정용 벽면 단상 콘센트",
                    SubTitle = "220V 16A 기본 차단기 전력",
                    Icon = "🔌",
                    Level = 1, // 기본 3kW 제공
                    BaseCost = 0,
                    SupplyWatts = 3000.0,
                    OfflineBonusMinutes = 0,
                    AccentColorHex = "#FFA726"
                },
                new PowerItem
                {
                    Id = "multitab",
                    Name = "과부하 방지 서지 멀티탭",
                    SubTitle = "접지 보호 6구 고용량 멀티탭",
                    Icon = "🔋",
                    Level = 0,
                    BaseCost = 150.0,
                    SupplyWatts = 1500.0,
                    OfflineBonusMinutes = 10,
                    AccentColorHex = "#FFB74D"
                },
                new PowerItem
                {
                    Id = "ups_mini",
                    Name = "UPS 백업 배터리 랙",
                    SubTitle = "정전 방지 및 오프라인 배터리 공급",
                    Icon = "⚡",
                    Level = 0,
                    BaseCost = 3500.0,
                    SupplyWatts = 5000.0,
                    OfflineBonusMinutes = 20, // 오프라인 상한 +20분
                    AccentColorHex = "#4DD0E1"
                },
                new PowerItem
                {
                    Id = "high_voltage",
                    Name = "산업용 3상 380V 고압 인입",
                    SubTitle = "한전 고압 수전실 분전반 증설",
                    Icon = "🏭",
                    Level = 0,
                    BaseCost = 75000.0,
                    SupplyWatts = 35000.0,
                    OfflineBonusMinutes = 40, // 오프라인 상한 +40분
                    AccentColorHex = "#00E5FF"
                },
                new PowerItem
                {
                    Id = "solar_array",
                    Name = "루프탑 하이브리드 태양광",
                    SubTitle = "옥상 고효율 단결정 태양광 패널",
                    Icon = "☀️",
                    Level = 0,
                    BaseCost = 2000000.0,
                    SupplyWatts = 180000.0,
                    OfflineBonusMinutes = 60,
                    AccentColorHex = "#FFD54F"
                },
                new PowerItem
                {
                    Id = "smr_reactor",
                    Name = "마이크로 SMR 소형 원자로",
                    SubTitle = "차세대 무한 자가발전 원자력 팩",
                    Icon = "☢️",
                    Level = 0,
                    BaseCost = 80000000.0,
                    SupplyWatts = 2000000.0,
                    OfflineBonusMinutes = 90, // 오프라인 상한 +90분
                    AccentColorHex = "#76FF03"
                }
            };
        }

        public static List<CoolingItem> CreateDefaultCooling()
        {
            return new List<CoolingItem>
            {
                new CoolingItem
                {
                    Id = "passive",
                    Name = "자연 대류 섀시 통풍",
                    SubTitle = "기본 알루미늄 방열판 통풍구",
                    Icon = "🍃",
                    Level = 1,
                    BaseCost = 0,
                    CoolingCapacity = 15.0,
                    AccentColorHex = "#81C784"
                },
                new CoolingItem
                {
                    Id = "desk_fan",
                    Name = "탁상용 회전 선풍기",
                    SubTitle = "발열 랙 집중 송풍 쿨러",
                    Icon = "💨",
                    Level = 0,
                    BaseCost = 120.0,
                    CoolingCapacity = 20.0,
                    AccentColorHex = "#4FC3F7"
                },
                new CoolingItem
                {
                    Id = "crac_unit",
                    Name = "항온항습기(CRAC) 시스템",
                    SubTitle = "정밀 18°C 항온 공조 제어",
                    Icon = "❄️",
                    Level = 0,
                    BaseCost = 4200.0,
                    CoolingCapacity = 95.0,
                    AccentColorHex = "#00E5FF"
                },
                new CoolingItem
                {
                    Id = "water_loop",
                    Name = "커스텀 수냉 펌프 루프",
                    SubTitle = "구리 핀 라디에이터 냉각수 순환",
                    Icon = "💧",
                    Level = 0,
                    BaseCost = 90000.0,
                    CoolingCapacity = 680.0,
                    AccentColorHex = "#2979FF"
                },
                new CoolingItem
                {
                    Id = "immersion",
                    Name = "극저온 액침 냉각 챔버",
                    SubTitle = "비전도성 불소계 냉매 전면 침수",
                    Icon = "🧊",
                    Level = 0,
                    BaseCost = 2400000.0,
                    CoolingCapacity = 6000.0,
                    AccentColorHex = "#B388FF"
                },
                new CoolingItem
                {
                    Id = "quantum_cryo",
                    Name = "0.015K 극저온 헬륨 희석 냉동기",
                    SubTitle = "우주 절대영도 수준 극저온 냉각",
                    Icon = "🌌",
                    Level = 0,
                    BaseCost = 90000000.0,
                    CoolingCapacity = 55000.0,
                    AccentColorHex = "#69F0AE"
                }
            };
        }

        public static List<SkillNode> GetSkillDefinitions()
        {
            return new List<SkillNode>
            {
                new SkillNode
                {
                    Id = "skill_power_eff",
                    Name = "고효율 전원 회로",
                    Description = "서버 소비 전력 절감 (Lv당 -6%, 최대 -30%)",
                    Icon = "⚡",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 1, 2, 4, 8, 15 }
                },
                new SkillNode
                {
                    Id = "skill_cooling_boost",
                    Name = "나노 히트파이프 코팅",
                    Description = "냉각 장치 쿨링 용량 강화 (Lv당 +10%, 최대 +50%)",
                    Icon = "❄",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 1, 2, 4, 8, 15 }
                },
                new SkillNode
                {
                    Id = "skill_backbone",
                    Name = "100G 광대역 백본",
                    Description = "모든 서버 초당 수입(RPS) 증폭 (Lv당 +12%, 최대 +60%)",
                    Icon = "🌐",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 2, 4, 8, 15, 25 }
                },
                new SkillNode
                {
                    Id = "skill_battery_max",
                    Name = "리튬-황 대용량 배터리",
                    Description = "오프라인 방치 상한 확장 (Lv당 +40분, 최대 +3.3시간)",
                    Icon = "🔋",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 2, 4, 8, 15, 25 }
                },
                new SkillNode
                {
                    Id = "skill_crit_packet",
                    Name = "황금 패킷 라우팅",
                    Description = "클릭 크리티컬 확률 및 잭팟 배율 강화 (최대 25% 확률 8배)",
                    Icon = "★",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 3, 6, 12, 22, 35 }
                },
                new SkillNode
                {
                    Id = "skill_ai_devops",
                    Name = "AI 자동 복구 드론",
                    Description = "장애 보상 증폭(Lv당 +30%) 및 Lv.3 이상 자동 복구 드론",
                    Icon = "🤖",
                    MaxLevel = 5,
                    LevelCosts = new int[] { 3, 6, 12, 22, 35 }
                }
            };
        }

        #region Infrastructure & Physics Calculations

        public double GetTotalPowerSupply()
        {
            double sum = 0;
            if (PowerFacilities != null)
            {
                foreach (var p in PowerFacilities) sum += p.GetTotalSupply();
            }
            return Math.Max(3000.0, sum);
        }

        public double GetTotalPowerDemand()
        {
            int lvl = GetSkillLevel("skill_power_eff");
            double eff = Math.Max(0.70, 1.0 - (lvl * 0.06));
            double sum = 0;
            if (Servers != null)
            {
                foreach (var s in Servers) sum += s.GetTotalPowerDemand(eff);
            }
            return sum;
        }

        public double GetPowerEfficiencyRatio()
        {
            double supply = GetTotalPowerSupply();
            double demand = GetTotalPowerDemand();
            if (demand <= supply) return 1.0;
            if (demand <= 0) return 1.0;
            // 전력 부족 시 공급 비율만큼만 가동 (최소 20% 보장)
            return Math.Max(0.20, supply / demand);
        }

        public double GetTotalCoolingCapacity()
        {
            int lvl = GetSkillLevel("skill_cooling_boost");
            double eff = 1.0 + (lvl * 0.10);
            double sum = 0;
            if (CoolingFacilities != null)
            {
                foreach (var c in CoolingFacilities) sum += c.GetTotalCooling(eff);
            }
            return Math.Max(15.0, sum);
        }

        public double GetTotalHeatGenerated()
        {
            double sum = 0;
            if (Servers != null)
            {
                foreach (var s in Servers) sum += s.GetTotalHeatOutput();
            }
            return sum;
        }

        public double GetCurrentTemperature()
        {
            double heat = GetTotalHeatGenerated();
            double cooling = GetTotalCoolingCapacity();
            double diff = heat - cooling;

            // 쿨링이 남으면 실온 22°C까지 냉각, 쿨링이 부족하면 95°C까지 과열
            if (diff <= 0)
            {
                return Math.Max(22.0, 28.0 + (diff * 0.05));
            }
            else
            {
                return Math.Min(95.0, 28.0 + (diff * 0.25));
            }
        }

        public double GetTemperatureMultiplier()
        {
            double temp = GetCurrentTemperature();
            if (temp <= 35.0)
            {
                return 1.20; // 쾌적 쿨링 오버클럭 보너스 +20%!
            }
            else if (temp <= 60.0)
            {
                return 1.0; // 정상 온도 100%
            }
            else
            {
                return 0.65; // 고온 스로틀링 감속 페널티 -35%
            }
        }

        public double GetOfflineCapSeconds()
        {
            // 기본 2시간 (7,200초)
            int extraMinutes = 0;
            if (PowerFacilities != null)
            {
                foreach (var p in PowerFacilities)
                {
                    extraMinutes += p.Level * p.OfflineBonusMinutes;
                }
            }
            int batLvl = GetSkillLevel("skill_battery_max");
            extraMinutes += batLvl * 40;

            // 최대 8시간(28,800초)까지만 확장 가능 (인플레이션 방어선)
            double totalSecs = (120 + extraMinutes) * 60.0;
            return Math.Min(28800.0, totalSecs);
        }

        public double GetPrestigeMultiplier()
        {
            // 각 환생당 +15% 기본 생산 배율, 획득한 총 토큰당 +1.5% 미세 패시브 보너스
            return 1.0 + (PrestigeCount * 0.15) + (TotalTokensEarned * 0.015);
        }

        public double GetFeverMultiplier()
        {
            return IsFeverActive ? 2.5 : 1.0;
        }

        public double CalculateTotalRps()
        {
            double sum = 0;
            double mult = GetPrestigeMultiplier() * GetFeverMultiplier();
            int backboneLvl = GetSkillLevel("skill_backbone");
            if (backboneLvl > 0) mult *= (1.0 + backboneLvl * 0.12);

            // 전력 효율과 온도 보너스/페널티 동시 적용
            mult *= GetPowerEfficiencyRatio();
            mult *= GetTemperatureMultiplier();

            foreach (var s in Servers)
            {
                sum += s.GetTotalRps(mult);
            }
            return sum;
        }

        public double GetSoftwareRoyaltyRps()
        {
            if (SoftwareStudio?.ReleasedProducts == null) return 0;
            double sum = 0;
            foreach (var p in SoftwareStudio.ReleasedProducts)
            {
                if (p.RemainingSecs > 0) sum += p.RoyaltyPerSec;
            }
            return sum;
        }

        public double GetEffectiveCashRps()
        {
            double serverRps = CalculateTotalRps();
            double hashShare = (CryptoMarket != null) ? (CryptoMarket.MiningHashPercent / 100.0) : 0.0;
            double cashFromServers = serverRps * (1.0 - hashShare);
            return cashFromServers + GetSoftwareRoyaltyRps();
        }

        public double GetClickPower()
        {
            double rpsBonus = CalculateTotalRps() * 0.04;
            double mult = GetPrestigeMultiplier() * (IsFeverActive ? 3.0 : 1.0);
            return Math.Max(1.0, (1.0 + rpsBonus) * mult);
        }

        public int GetTotalServerCount()
        {
            int total = 0;
            if (Servers != null)
            {
                foreach (var s in Servers) total += s.Level;
            }
            return total;
        }

        public int CalculatePrestigeTokens()
        {
            // 이번 회차에서 최소 $1,000,000 ($1M) 이상 누적되어야 첫 토큰 획득 가능
            if (CurrentRunEarned < 1000000.0) return 0;

            // 지수 감쇄 곡선 (Power 0.38): $1M -> 1개, $8M -> 2개, $30M -> 3개, $80M -> 5개, $250M -> 8개, $1B -> 14개
            int tokens = (int)Math.Floor(Math.Pow(CurrentRunEarned / 1000000.0, 0.38));
            return Math.Max(0, tokens);
        }

        public int CalculateClaimableTokens()
        {
            return CalculatePrestigeTokens();
        }

        #endregion

        #region Save / Load & Offline Progress

        private static string GetSaveFilePath()
        {
            try
            {
                string dir = Path.Combine(Application.StartupPath, "_meta", "sandbox", "serverfarmtycoon");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return Path.Combine(dir, "server_tycoon.json");
            }
            catch
            {
                string fallbackDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_meta", "sandbox", "serverfarmtycoon");
                if (!Directory.Exists(fallbackDir))
                {
                    Directory.CreateDirectory(fallbackDir);
                }
                return Path.Combine(fallbackDir, "server_tycoon.json");
            }
        }

        public static TycoonGameState LoadOrCreate()
        {
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var state = JsonConvert.DeserializeObject<TycoonGameState>(json);
                    if (state != null)
                    {
                        state.EnsureDefaults();
                        return state;
                    }
                }
                catch { }
            }

            var fresh = new TycoonGameState();
            fresh.EnsureDefaults();
            fresh.Save();
            return fresh;
        }

        public void EnsureDefaults()
        {
            var defServers = CreateDefaultServers();
            if (Servers == null || Servers.Count == 0)
            {
                Servers = defServers;
            }
            else
            {
                foreach (var def in defServers)
                {
                    var existing = Servers.Find(s => s.Id == def.Id);
                    if (existing == null)
                    {
                        Servers.Add(def);
                    }
                    else
                    {
                        // 기존 세이브 파일에 누락된 전력, 발열, 최신 비용/RPS 등 강제 동기화 (보유 레벨은 보존)
                        existing.Name = def.Name;
                        existing.SubTitle = def.SubTitle;
                        existing.Icon = def.Icon;
                        existing.BaseCost = def.BaseCost;
                        existing.BaseRps = def.BaseRps;
                        existing.PowerWatts = def.PowerWatts;
                        existing.HeatOutput = def.HeatOutput;
                        existing.AccentColorHex = def.AccentColorHex;
                    }
                }
            }

            var defPower = CreateDefaultPower();
            if (PowerFacilities == null || PowerFacilities.Count == 0)
            {
                PowerFacilities = defPower;
            }
            else
            {
                foreach (var def in defPower)
                {
                    var existing = PowerFacilities.Find(p => p.Id == def.Id);
                    if (existing == null)
                    {
                        PowerFacilities.Add(def);
                    }
                    else
                    {
                        existing.Name = def.Name;
                        existing.SubTitle = def.SubTitle;
                        existing.Icon = def.Icon;
                        existing.BaseCost = def.BaseCost;
                        existing.SupplyWatts = def.SupplyWatts;
                        existing.OfflineBonusMinutes = def.OfflineBonusMinutes;
                        existing.AccentColorHex = def.AccentColorHex;
                    }
                }
            }

            var defCooling = CreateDefaultCooling();
            if (CoolingFacilities == null || CoolingFacilities.Count == 0)
            {
                CoolingFacilities = defCooling;
            }
            else
            {
                foreach (var def in defCooling)
                {
                    var existing = CoolingFacilities.Find(c => c.Id == def.Id);
                    if (existing == null)
                    {
                        CoolingFacilities.Add(def);
                    }
                    else
                    {
                        existing.Name = def.Name;
                        existing.SubTitle = def.SubTitle;
                        existing.Icon = def.Icon;
                        existing.BaseCost = def.BaseCost;
                        existing.CoolingCapacity = def.CoolingCapacity;
                        existing.AccentColorHex = def.AccentColorHex;
                    }
                }
            }

            if (UnlockedSkills == null)
                UnlockedSkills = new List<string>();

            if (SkillLevels == null)
                SkillLevels = new Dictionary<string, int>();

            foreach (var s in UnlockedSkills)
            {
                if (!SkillLevels.ContainsKey(s))
                {
                    SkillLevels[s] = 1;
                }
            }

            if (CurrentRunEarned <= 0)
            {
                CurrentRunEarned = Math.Max(50.0, Money);
            }

            // 과거 버그로 과도하게 쌓인 연구 토큰 상한 보정 (성장 재미 보존)
            if (ResearchTokens > 25)
            {
                ResearchTokens = 25;
            }
            if (TotalTokensEarned > 35)
            {
                TotalTokensEarned = 35;
            }

            // 신규 3대 시스템 기본값 보장
            if (SoftwareStudio == null) SoftwareStudio = new SoftwareStudioState();
            if (SoftwareStudio.ReleasedProducts == null) SoftwareStudio.ReleasedProducts = new List<ReleasedSoftware>();

            if (B2BMarket == null) B2BMarket = new B2BMarketState();
            if (B2BMarket.AvailableOffers == null || B2BMarket.AvailableOffers.Count == 0)
            {
                B2BMarket.RefreshOffers(new Random(), CalculateTotalRps());
            }

            if (CryptoMarket == null) CryptoMarket = new CryptoMarketState();
            CryptoMarket.InitHistory();
        }

        public void Save()
        {
            try
            {
                LastSaveTimeUtc = DateTime.UtcNow;
                string path = GetSaveFilePath();
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch { }
        }

        public double ProcessOfflineProgress(out double elapsedSeconds)
        {
            elapsedSeconds = 0;
            try
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan span = now - LastSaveTimeUtc;
                if (span.TotalSeconds < 10) return 0;

                // [핵심 밸런스]: 기본 2시간 상한 (설비에 따라 최대 8시간까지 확장)
                double maxCapSecs = GetOfflineCapSeconds();
                double secs = Math.Min(span.TotalSeconds, maxCapSecs);
                elapsedSeconds = secs;

                double rps = CalculateTotalRps();
                if (rps <= 0) return 0;

                // 방치 효율 75% 적용
                double earned = rps * secs * 0.75;
                Money += earned;
                CurrentRunEarned += earned;
                TotalEarned += earned;
                LastSaveTimeUtc = now;
                Save();
                return earned;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
