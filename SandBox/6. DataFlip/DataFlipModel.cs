using System;
using System.Collections.Generic;
using System.Drawing;

namespace GateHelper.DataFlip
{
    public enum Owner
    {
        None = 0,
        Player = 1, // Cyan
        Ai = 2      // Crimson
    }

    public enum AiStrategyPersona
    {
        Rush = 0,   // 기동 침투형 (Raider & Vanguard 중심, 빈 라인 기습)
        Siege = 1,  // 원거리 요새형 (Turret & Wall 거점 확보 후 Colossus 포격)
        Tech = 2    // 하이테크형 (발전소 최대 가동 후 Titan & Dreadnought 진격)
    }

    public enum MapSizeType
    {
        Small = 0,    // 12 x 6 (소형: 좁은 전장, 빠른 난타전)
        Standard = 1, // 14 x 8 (표준: 정규 밸런스 전장)
        Large = 2     // 18 x 8 (대형: 가로 확장, 장거리 천궁/포탑/전진인프라 전략전)
    }

    public enum TileType
    {
        Plain = 0,     // 일반 타일
        Resource = 1,  // 자원 타일 (발전소 건설 시 2배 수입: +4G)
        Obstacle = 2   // 서버 랙 장애물 (통과 불가)
    }

    public enum BuildingType
    {
        None = 0,
        PowerPlant = 1, // 8G~, HP 4, 턴당 +1G (자원 타일 위: +3G)
        Turret = 2,     // 6G~, HP 6, 13칸 전방위 요격
        Wall = 3,       // 3G, HP 8, 이동 불가 바리케이드
        Factory = 4,    // 9G, HP 10, 전진 스폰 포인트 제공
        RepairDepot = 5,// 6G, HP 6, 턴마다 전장의 부상당한 아군 유닛 1기 체력 회복 +1
        PhaseGate = 6   // 5G, HP 6, 통과하는 아군 유닛 이동력 +1
    }

    public enum UnitType
    {
        None = 0,
        Vanguard = 1, // 2G, HP 3, 공 1, 이속 1칸 (기본 보병)
        Raider = 2,   // 4G, HP 3, 공 2, 이속 2칸 (고속 기동/기습)
        Titan = 3,    // 10G, HP 10, 공 3, 이속 1칸, 사거리 2칸 (중장갑 만능 공성 거신)
        Colossus = 4, // 14G, HP 8, 공 4, 이속 1칸, 사거리 2~3칸 (초장거리 공성 포격, 인접 1칸 근접 공격 불가)
        Dreadnought = 5, // 18G, HP 12, 공 3, 이속 1칸, 사거리 2칸 (십자 스플래시 방전, 보병/침투병 공격에 취약 +2뎀)
        Cheongung = 6 // 12G, HP 5, 공 3, 이속 1칸, 사거리 4칸 (초장거리 미사일 포대, 건물 공격 시 2배 6뎀, 인접 1칸 근접 불가, 시즈모드 없이 전진)
    }

    public enum UserSkillType
    {
        None = 0,
        OrbitalStrike = 1, // 8G: 지정 적 건물/서버랙 6뎀 일격 타격
        EmpJammer = 2,     // 5G: 지정 적 건물 및 3x3 범위 2턴간 기능 정지 (포탑 사격 정지, 발전소 수입 0)
        NanoRepair = 3,    // 4G: 아군 모든 건물 체력 +3 즉시 수리
        ReconScan = 4,     // 3G: 전장의 모든 은폐 침투병 은폐 해제 및 위치 노출
        HealingBeam = 5,   // 4G: 지정 행(Row) 아군 유닛 체력 +4 회복 (최대체력 초과 불가)
        FortifyBeam = 6    // 5G: 지정 행(Row) 아군 유닛 최대 체력 +2 (내구도 증가)
    }

    public class Unit
    {
        public int Id { get; set; }
        public Owner Owner { get; set; }
        public UnitType Type { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Atk { get; set; }
        public int Speed { get; set; }
        public int Range { get; set; }
        public int MinRange { get; set; } = 1; // 최소 사거리 (1: 근접 가능, 2: 인접 1칸 공격 불가)
        public int Col { get; set; }
        public int Row { get; set; }
        public bool HasActed { get; set; }
        public bool HasShield { get; set; } = false; // 보병 전술 방패 (1회 피해 무효화 실드)
        public bool IsCloaked { get; set; } = false; // 침투병 광학 스텔스 (근접 공격 전까지 원거리/포탑 타깃팅 면역)
        public bool HasBomb { get; set; } = false;   // 최후의 자폭 코어 (사망 시 상하좌우 1칸 적 2 데미지)

        // 부드러운 슬로우 이동 보간 및 피격 애니메이션용
        public float VisualCol { get; set; } = -1;
        public float TargetCol { get; set; } = -1;
        public float HitFlashAlpha { get; set; } = 0f;
        public float BumpOffset { get; set; } = 0f;
        public float ShakeOffset { get; set; } = 0f;

        public Unit Clone()
        {
            return new Unit
            {
                Id = this.Id,
                Owner = this.Owner,
                Type = this.Type,
                Hp = this.Hp,
                MaxHp = this.MaxHp,
                Atk = this.Atk,
                Speed = this.Speed,
                Range = this.Range,
                MinRange = this.MinRange,
                HasShield = this.HasShield,
                IsCloaked = this.IsCloaked,
                HasBomb = this.HasBomb,
                Col = this.Col,
                Row = this.Row,
                HasActed = this.HasActed,
                VisualCol = this.VisualCol,
                TargetCol = this.TargetCol,
                HitFlashAlpha = this.HitFlashAlpha,
                BumpOffset = this.BumpOffset,
                ShakeOffset = this.ShakeOffset
            };
        }
    }

    public class Building
    {
        public Owner Owner { get; set; }
        public BuildingType Type { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public int EmpTurns { get; set; } = 0; // EMP 기능 정지 잔여 턴수 (>0이면 사격/발전 불가)

        // 피격 시 번쩍임 및 흔들림 연출
        public float HitFlashAlpha { get; set; } = 0f;
        public float ShakeOffset { get; set; } = 0f;

        public Building Clone()
        {
            return new Building
            {
                Owner = this.Owner,
                Type = this.Type,
                Hp = this.Hp,
                MaxHp = this.MaxHp,
                Col = this.Col,
                Row = this.Row,
                EmpTurns = this.EmpTurns,
                HitFlashAlpha = this.HitFlashAlpha,
                ShakeOffset = this.ShakeOffset
            };
        }
    }

    public class CombatLaserBeam
    {
        public int StartCol { get; set; }
        public int StartRow { get; set; }
        public int EndCol { get; set; }
        public int EndRow { get; set; }
        public Color BeamColor { get; set; }
        public float Alpha { get; set; } = 1.0f;
    }

    public enum CardRarity
    {
        Common, // 일반 (출현율 50%)
        Rare,   // 희귀 (출현율 35%)
        Epic    // 에픽 (출현율 15%)
    }

    public class ProtocolCard
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public Color AccentColor { get; set; }
        public CardRarity Rarity { get; set; } = CardRarity.Common;
        public int MinTurn { get; set; } = 1; // 카드 풀 등장 최소 턴 (1: 턴 3부터, 6: 턴 6부터, 9: 턴 9부터, 12: 턴 12부터)
        public Action<CoreFrontGameState, Owner> ApplyAction { get; set; }
        public Func<CoreFrontGameState, Owner, bool> IsAvailable { get; set; } // 출현 가능 조건 (한도 도달 및 이미 해금된 고유기술 제외)
    }

    public class CombatEvent
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public bool IsCritical { get; set; }
    }

    public class ReplayUnitData
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public Owner Owner { get; set; }
        public UnitType Type { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public bool HasShield { get; set; }
        public bool IsCloaked { get; set; }
        public bool HasBomb { get; set; }
    }

    public class ReplayBuildingData
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public Owner Owner { get; set; }
        public BuildingType Type { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int EmpTurns { get; set; }
    }

    public class ReplayObstacleData
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public int Hp { get; set; }
    }

    public class ReplayTurnSnapshot
    {
        public int TurnNumber { get; set; }
        public Owner CurrentTurn { get; set; }
        public int PlayerCoreHp { get; set; }
        public int AiCoreHp { get; set; }
        public int PlayerCredit { get; set; }
        public int AiCredit { get; set; }
        public List<ReplayUnitData> Units { get; set; } = new List<ReplayUnitData>();
        public List<ReplayBuildingData> Buildings { get; set; } = new List<ReplayBuildingData>();
        public List<ReplayObstacleData> Obstacles { get; set; } = new List<ReplayObstacleData>();
    }

    public class CoreFrontGameState
    {
        public const int DefaultCols = 14;
        public const int DefaultRows = 8;
        public static int StaticCols => DefaultCols;
        public static int StaticRows => DefaultRows;

        public int Cols { get; set; } = DefaultCols;
        public int Rows { get; set; } = DefaultRows;
        public MapSizeType CurrentMapSize { get; set; } = MapSizeType.Standard;
        public const int BaseMaxHp = 25;

        // 맵 그리드
        public TileType[,] MapTiles { get; private set; }
        public int[,] ObstacleHp { get; private set; }
        public Building[,] Buildings { get; private set; }
        public List<Unit> Units { get; private set; }

        // 게임 종료 후 전투 통계 대시보드 (유저 흥미 & 전적 분석)
        public int StatsPlayerUnitsKilled { get; set; } = 0;
        public int StatsPlayerUnitsLost { get; set; } = 0;
        public int StatsPlayerBuildingsDestroyed { get; set; } = 0;
        public int StatsObstaclesDestroyed { get; set; } = 0;
        public int StatsTotalCreditEarned { get; set; } = 6;
        public int StatsDamageDealtToAiCore { get; set; } = 0;
        public int StatsTurnsElapsed => TurnNumber;

        // 초경량 턴별 전투 리플레이 스냅샷 기록
        public List<ReplayTurnSnapshot> ReplaySnapshots { get; set; } = new List<ReplayTurnSnapshot>();

        // 본진 체력
        public int PlayerCoreMaxHp { get; set; } = BaseMaxHp;
        public int AiCoreMaxHp { get; set; } = BaseMaxHp;
        public int PlayerCoreHp { get; set; } = BaseMaxHp;
        public int AiCoreHp { get; set; } = BaseMaxHp;

        // 자원 & 턴
        public int TurnNumber { get; set; } = 1;
        public Owner CurrentTurn { get; set; } = Owner.Player;
        public int PlayerCredit { get; set; } = 6;
        public int AiCredit { get; set; } = 6;

        // 상태
        public bool IsGameOver { get; set; } = false;
        public Owner Winner { get; set; } = Owner.None;
        public string StatusMessage { get; set; } = "당신의 턴 - 건물이나 유닛을 배치하고 [턴 종료]를 누르세요.";

        // 카드 선택 로그라이크 상태
        public bool IsCardSelectionActive { get; set; } = false;
        public List<ProtocolCard> CurrentOfferedCards { get; set; } = new List<ProtocolCard>();

        // 카드 선택 후 유닛 직접 배치 지정 대기 상태
        public UnitType PendingDropUnitType { get; set; } = UnitType.None;
        public int PendingDropRemainingCount { get; set; } = 0;
        public int PendingDropMinCol { get; set; } = 1;
        public int PendingDropMaxCol { get; set; } = 1;
        public int PendingDropCustomAtk { get; set; } = 0;   // 후반부 공수 강화: 투하되는 유닛에게만 부여되는 커스텀 공격력
        public bool PendingDropForceShield { get; set; } = false; // 후반부 공수 강화: 투하되는 유닛에게 부여되는 실드 여부

        // 1턴 첫 발전소 직접 배치 모드 (플레이어가 Col 1~2 원하는 곳에 무료 건설)
        public bool IsInitialBasePlacementActive { get; set; } = false;

        // 패시브 보너스 (무제한 누적 스택)
        public int PlayerIncomeBonus { get; set; } = 0;
        public int AiIncomeBonus { get; set; } = 0;
        public int PlayerUnitHpBonus { get; set; } = 0;
        public int AiUnitHpBonus { get; set; } = 0;
        public int PlayerUnitAtkBonus { get; set; } = 0;
        public int AiUnitAtkBonus { get; set; } = 0;
        public int PlayerCostDiscount { get; set; } = 0;
        public int TurretRangeBonus { get; set; } = 0;

        // 8대 유닛/인프라 고유 특수 기술 해금 상태 (프로토콜 카드로 해금)
        public bool PlayerHasSiegeAnchor { get; set; } = false;   // 콜로서스 시즈 앵커 (제자리 포격 Hold & Fire)
        public bool AiHasSiegeAnchor { get; set; } = false;

        public bool PlayerHasColossusMelee { get; set; } = false; // 콜로서스 근접 방어 발칸포 (1칸 근접 교전/반격 허용)
        public bool AiHasColossusMelee { get; set; } = false;

        public bool PlayerHasCheongungTrajectory { get; set; } = false; // 천궁 탄도 궤적 정밀 연산 (사거리 4->5, 최소사거리 2->3)
        public bool AiHasCheongungTrajectory { get; set; } = false;

        public bool PlayerHasPhalanxShield { get; set; } = false; // 보병 전술 방패 (스폰 시 1회 피해 무효화 실드)
        public bool AiHasPhalanxShield { get; set; } = false;

        public bool PlayerHasStealthCloak { get; set; } = false;  // 침투병 광학 스텔스 (근접 공격 전까지 원거리/포탑 타깃팅 면역)
        public bool AiHasStealthCloak { get; set; } = false;

        public bool PlayerHasTurretRadar { get; set; } = false;   // 포탑 양자 레이더 망 (은폐 유닛도 13칸 요격 가능)
        public bool AiHasTurretRadar { get; set; } = false;

        public bool PlayerHasKineticSlam { get; set; } = false;   // 타이탄 운동에너지 충격파 (1칸 넉백 및 연쇄 충돌 피해)
        public bool AiHasKineticSlam { get; set; } = false;

        public bool PlayerHasChainPlasma { get; set; } = false;   // 드레드노트 연쇄 플라즈마 (십자 4방향 2뎀 스플래시 방전)
        public bool AiHasChainPlasma { get; set; } = false;

        // AI 전술 페르소나 (판마다 다른 전략 구사)
        public AiStrategyPersona CurrentAiPersona { get; set; } = AiStrategyPersona.Tech;

        // 발전소 보유 한도 (기본 3기 -> 카드로 대폭 확장 또는 골드 투자로 점진 증설 가능)
        public int PlayerPlantLimit { get; set; } = 3;
        public int AiPlantLimit { get; set; } = 3;

        public int GetPlantLimit(Owner owner) => (owner == Owner.Player) ? PlayerPlantLimit : AiPlantLimit;

        public int GetGridExpandCost(Owner owner)
        {
            int currentLimit = GetPlantLimit(owner);
            // 3기->4기: 10G, 4기->5기: 14G, 5기->6기: 18G (점진적 가중치 상향)
            return 10 + Math.Max(0, currentLimit - 3) * 4;
        }

        public bool UpgradePowerGrid(Owner owner)
        {
            int cost = GetGridExpandCost(owner);
            int credit = (owner == Owner.Player) ? PlayerCredit : AiCredit;
            if (credit < cost) return false;

            if (owner == Owner.Player)
            {
                PlayerCredit -= cost;
                PlayerPlantLimit++;
            }
            else
            {
                AiCredit -= cost;
                AiPlantLimit++;
            }
            return true;
        }

        private int _unitIdCounter = 1;
        private Random _rand = new Random();

        public CoreFrontGameState()
        {
            MapTiles = new TileType[Rows, Cols];
            ObstacleHp = new int[Rows, Cols];
            Buildings = new Building[Rows, Cols];
            Units = new List<Unit>();
        }

        public void RecordReplaySnapshot()
        {
            var snap = new ReplayTurnSnapshot
            {
                TurnNumber = this.TurnNumber,
                CurrentTurn = this.CurrentTurn,
                PlayerCoreHp = this.PlayerCoreHp,
                AiCoreHp = this.AiCoreHp,
                PlayerCredit = this.PlayerCredit,
                AiCredit = this.AiCredit
            };

            foreach (var u in Units)
            {
                snap.Units.Add(new ReplayUnitData
                {
                    Col = u.Col,
                    Row = u.Row,
                    Owner = u.Owner,
                    Type = u.Type,
                    Hp = u.Hp,
                    MaxHp = u.MaxHp,
                    HasShield = u.HasShield,
                    IsCloaked = u.IsCloaked,
                    HasBomb = u.HasBomb
                });
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var b = Buildings[r, c];
                    if (b != null)
                    {
                        snap.Buildings.Add(new ReplayBuildingData
                        {
                            Col = c,
                            Row = r,
                            Owner = b.Owner,
                            Type = b.Type,
                            Hp = b.Hp,
                            MaxHp = b.MaxHp,
                            EmpTurns = b.EmpTurns
                        });
                    }
                    if (ObstacleHp[r, c] > 0)
                    {
                        snap.Obstacles.Add(new ReplayObstacleData
                        {
                            Col = c,
                            Row = r,
                            Hp = ObstacleHp[r, c]
                        });
                    }
                }
            }

            ReplaySnapshots.Add(snap);
        }

        public void InitializeMap(MapSizeType size = MapSizeType.Standard)
        {
            CurrentMapSize = size;
            switch (size)
            {
                case MapSizeType.Small:
                    Cols = 12;
                    Rows = 6;
                    break;
                case MapSizeType.Standard:
                    Cols = 14;
                    Rows = 8;
                    break;
                case MapSizeType.Large:
                    Cols = 18;
                    Rows = 8;
                    break;
            }

            MapTiles = new TileType[Rows, Cols];
            ObstacleHp = new int[Rows, Cols];
            Buildings = new Building[Rows, Cols];
            Units.Clear();
            _unitIdCounter = 1;

            PlayerCoreMaxHp = BaseMaxHp;
            AiCoreMaxHp = BaseMaxHp;
            PlayerCoreHp = BaseMaxHp;
            AiCoreHp = BaseMaxHp;
            TurnNumber = 1;
            CurrentTurn = Owner.Player;
            PlayerCredit = 6;
            AiCredit = 6;
            IsGameOver = false;
            Winner = Owner.None;
            IsCardSelectionActive = false;
            PendingDropUnitType = UnitType.None;
            PendingDropRemainingCount = 0;

            // 통계 및 리플레이 초기화
            StatsPlayerUnitsKilled = 0;
            StatsPlayerUnitsLost = 0;
            StatsPlayerBuildingsDestroyed = 0;
            StatsObstaclesDestroyed = 0;
            StatsTotalCreditEarned = 6;
            StatsDamageDealtToAiCore = 0;
            ReplaySnapshots.Clear();

            // 판마다 AI 전술 성향 무작위 배정 (Rush 33%, Siege 33%, Tech 34%)
            int personaRoll = _rand.Next(3);
            CurrentAiPersona = (personaRoll == 0) ? AiStrategyPersona.Rush :
                               (personaRoll == 1) ? AiStrategyPersona.Siege :
                                                    AiStrategyPersona.Tech;

            PlayerIncomeBonus = 0;
            AiIncomeBonus = 0;
            PlayerUnitHpBonus = 0;
            AiUnitHpBonus = 0;
            PlayerUnitAtkBonus = 0;
            AiUnitAtkBonus = 0;
            PlayerCostDiscount = 0;
            TurretRangeBonus = 0;

            PlayerHasSiegeAnchor = false;
            AiHasSiegeAnchor = false;
            PlayerHasColossusMelee = false;
            AiHasColossusMelee = false;
            PlayerHasCheongungTrajectory = false;
            AiHasCheongungTrajectory = false;
            PlayerHasPhalanxShield = false;
            AiHasPhalanxShield = false;
            PlayerHasStealthCloak = false;
            AiHasStealthCloak = false;
            PlayerHasTurretRadar = false;
            AiHasTurretRadar = false;
            PlayerHasKineticSlam = false;
            AiHasKineticSlam = false;
            PlayerHasChainPlasma = false;
            AiHasChainPlasma = false;

            PendingDropCustomAtk = 0;
            PendingDropForceShield = false;

            PlayerPlantLimit = 3;
            AiPlantLimit = 3;

            // 1. 기본 타일 초기화
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    MapTiles[r, c] = TileType.Plain;
                    ObstacleHp[r, c] = 0;
                    Buildings[r, c] = null;
                }
            }

            // 2. 전장 구역 대칭 분할 (공평한 중앙 장애물 존 & 좌우 자원 존)
            // 중앙 장애물 존: 맵 정중앙을 기준으로 좌우 대칭인 중립 격전 구역
            int midLeft;
            int midRight;
            if (Cols <= 12) // 소형 (12열: 0..11) -> 중앙 5, 6열 (2열)
            {
                midLeft = 5;
                midRight = 6;
            }
            else if (Cols <= 14) // 표준 (14열: 0..13) -> 중앙 5, 6, 7, 8열 (4열: 좌2, 우2)
            {
                midLeft = 5;
                midRight = 8;
            }
            else // 대형 (18열: 0..17) -> 중앙 7, 8, 9, 10열 (4열: 좌2, 우2)
            {
                midLeft = 7;
                midRight = 10;
            }

            // 좌측 자원 노드 2개: 플레이어 전진 구역 (중앙 장애물 존 왼쪽)
            int leftColStart = Math.Max(2, midLeft - 3);
            int leftColEnd = midLeft - 1;
            int leftPlaced = 0;
            int tryLeft = 0;
            while (leftPlaced < 2 && tryLeft < 100)
            {
                tryLeft++;
                int r = _rand.Next(Rows);
                int c = _rand.Next(leftColStart, leftColEnd + 1);
                if (MapTiles[r, c] == TileType.Plain && !(r == Rows / 2 && c <= 2))
                {
                    MapTiles[r, c] = TileType.Resource;
                    leftPlaced++;
                }
            }

            // 우측 자원 노드 2개: AI 전진 구역 (중앙 장애물 존 오른쪽)
            int rightColStart = midRight + 1;
            int rightColEnd = Math.Min(Cols - 3, midRight + 3);
            int rightPlaced = 0;
            int tryRight = 0;
            while (rightPlaced < 2 && tryRight < 100)
            {
                tryRight++;
                int r = _rand.Next(Rows);
                int c = _rand.Next(rightColStart, rightColEnd + 1);
                if (MapTiles[r, c] == TileType.Plain && !(r == Rows / 2 && c >= Cols - 2))
                {
                    MapTiles[r, c] = TileType.Resource;
                    rightPlaced++;
                }
            }

            // 3. 파괴 가능한 서버 랙 장애물 6개 공평 대칭 무작위 배치 (중앙 전장 존)
            // 좌측 중앙과 우측 중앙에 균등 분산하여 컴퓨터 쪽으로 치우치지 않게 완벽 분할
            int obsPlaced = 0;
            int[] rowObsCounts = new int[Rows];
            int attempts = 0;
            while (obsPlaced < 6 && attempts < 200)
            {
                attempts++;
                int r = _rand.Next(Rows);
                int cMin = (obsPlaced % 2 == 0) ? midLeft : (midLeft + midRight + 1) / 2;
                int cMax = (obsPlaced % 2 == 0) ? (midLeft + midRight) / 2 : midRight;
                int c = _rand.Next(cMin, cMax + 1);

                if (c >= midLeft && c <= midRight && MapTiles[r, c] == TileType.Plain && rowObsCounts[r] < 1)
                {
                    SetObstacle(r, c, 6);
                    rowObsCounts[r]++;
                    obsPlaced++;
                }
            }
            while (obsPlaced < 6)
            {
                int r = _rand.Next(Rows);
                int c = _rand.Next(midLeft, midRight + 1);
                if (MapTiles[r, c] == TileType.Plain)
                {
                    SetObstacle(r, c, 6);
                    obsPlaced++;
                }
            }

            // 4. 초기 기본 건물 배치 (AI는 고정/자원배치, 플레이어는 Col 1~2 직접 선택 배치 활성화)
            int aiPlantRow = Rows / 2;
            Buildings[aiPlantRow, Cols - 2] = new Building { Owner = Owner.Ai, Type = BuildingType.PowerPlant, Hp = 4, MaxHp = 4, Col = Cols - 2, Row = aiPlantRow };
            IsInitialBasePlacementActive = true;

            // 5. 1턴 초기 리플레이 스냅샷 1회 기록
            RecordReplaySnapshot();

            StatusMessage = "⚡ 작전 개시! 아군 본진 구역(Col 1~2)에 첫 무료 발전소를 배치할 위치를 클릭하십시오.";
        }

        public bool PlaceInitialPlayerBase(int col, int row)
        {
            if (!IsInitialBasePlacementActive) return false;
            if (col < 1 || col > 2 || row < 0 || row >= Rows) return false;
            if (MapTiles[row, col] == TileType.Obstacle) return false;
            if (Buildings[row, col] != null) return false;

            Buildings[row, col] = new Building
            {
                Owner = Owner.Player,
                Type = BuildingType.PowerPlant,
                Hp = 4,
                MaxHp = 4,
                Col = col,
                Row = row
            };

            IsInitialBasePlacementActive = false;
            StatusMessage = "기지 인프라 가동 개시! 유닛이나 방어 건물을 배치하고 [턴 종료]를 누르세요.";
            return true;
        }

        /// <summary>
        /// 턴 시작 시 건물 자체 수리 (2턴당 1회복) 및 EMP 무력화 잔여 턴수 차감
        /// </summary>
        public List<CombatEvent> ProcessTurnStartBuildingRegenAndEmp()
        {
            var events = new List<CombatEvent>();
            bool isRegenTurn = (TurnNumber % 2 == 0);

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var b = Buildings[r, c];
                    if (b != null && b.Hp > 0)
                    {
                        // EMP 무력화 턴수 감소
                        if (b.EmpTurns > 0)
                        {
                            b.EmpTurns--;
                        }

                        // 2턴당 체력 1 자체 수리 (최대 체력 한도 내)
                        if (isRegenTurn && b.Hp < b.MaxHp)
                        {
                            b.Hp = Math.Min(b.MaxHp, b.Hp + 1);
                            events.Add(new CombatEvent
                            {
                                Col = c,
                                Row = r,
                                Text = "+1 자가수리 🔧",
                                TextColor = (b.Owner == Owner.Player) ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 83, 80)
                            });
                        }

                        // 수리 격납고 (RepairDepot): 턴마다 전장에 있는 부상당한 아군 유닛 1기 체력 회복 +1
                        if (b.Type == BuildingType.RepairDepot && b.EmpTurns <= 0)
                        {
                            var woundedUnits = Units.FindAll(u => u.Owner == b.Owner && u.Hp > 0 && u.Hp < u.MaxHp);
                            if (woundedUnits.Count > 0)
                            {
                                var targetUnit = woundedUnits[0];
                                foreach (var wu in woundedUnits)
                                {
                                    if (wu.Hp < targetUnit.Hp) targetUnit = wu;
                                }
                                targetUnit.Hp = Math.Min(targetUnit.MaxHp, targetUnit.Hp + 1);
                                targetUnit.HitFlashAlpha = 0.8f;
                                events.Add(new CombatEvent
                                {
                                    Col = targetUnit.Col,
                                    Row = targetUnit.Row,
                                    Text = "+1 격납고 수리 🏥",
                                    TextColor = (targetUnit.Owner == Owner.Player) ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 83, 80)
                                });
                            }
                        }
                    }
                }
            }
            return events;
        }

        private void SetObstacle(int r, int c, int hp)
        {
            MapTiles[r, c] = TileType.Obstacle;
            ObstacleHp[r, c] = hp;
        }

        #region Cost Helpers

        public int GetBuildingCost(BuildingType type, Owner owner = Owner.Player)
        {
            int baseCost;
            switch (type)
            {
                case BuildingType.PowerPlant:
                    // 발전소 건설비 점진 가중치 대폭 상향: 기본 8G + 보유당 +4G (돈찍누 빌드 원천 차단!)
                    // 0기: 8G, 1기: 12G, 2기: 16G, 3기: 20G...
                    int plantCount = GetBuildingCount(BuildingType.PowerPlant, owner);
                    baseCost = 8 + (plantCount * 4);
                    break;
                case BuildingType.Turret:
                    // 포탑 건설비 점진 가중치: 기본 6G + 진영별 기존 보유 포탑 1기당 +2G (파괴 시 비용 복원)
                    int turretCount = GetBuildingCount(BuildingType.Turret, owner);
                    baseCost = 6 + (turretCount * 2);
                    break;
                case BuildingType.Wall: baseCost = 3; break;
                case BuildingType.Factory: baseCost = 9; break;
                case BuildingType.RepairDepot: baseCost = 6; break; // 6G, HP 6, 턴당 부상 아군 1기 +1 수리
                case BuildingType.PhaseGate: baseCost = 5; break;   // 5G, HP 6, 통과하는 유닛 이동력 +1
                default: baseCost = 0; break;
            }
            int discount = (owner == Owner.Player) ? PlayerCostDiscount : 0;
            return Math.Max(1, baseCost - discount);
        }

        public int GetBuildingCount(BuildingType type, Owner owner)
        {
            int count = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Buildings[r, c] != null && Buildings[r, c].Owner == owner && Buildings[r, c].Type == type)
                        count++;
                }
            }
            return count;
        }

        public int GetUnitCost(UnitType type)
        {
            int baseCost;
            switch (type)
            {
                case UnitType.Vanguard: baseCost = 2; break;
                case UnitType.Raider: baseCost = 4; break;
                case UnitType.Titan: baseCost = 10; break;
                case UnitType.Colossus: baseCost = 14; break;
                case UnitType.Dreadnought: baseCost = 18; break;
                case UnitType.Cheongung: baseCost = 12; break; // 신규: 12G (대건물/서버랙 6뎀 공성 결전 미사일 유닛)
                default: baseCost = 0; break;
            }
            return Math.Max(1, baseCost - PlayerCostDiscount);
        }

        public int GetSkillCost(UserSkillType skill)
        {
            switch (skill)
            {
                case UserSkillType.OrbitalStrike: return 8; // 8G: 단일 건물/서버랙 즉시 6뎀
                case UserSkillType.EmpJammer: return 5;     // 5G: 건물 및 3x3 범위 2턴간 기능 정지
                case UserSkillType.NanoRepair: return 4;    // 4G: 아군 모든 건물 체력 +3 HP 즉시 수리
                case UserSkillType.ReconScan: return 3;     // 3G: 전장의 모든 은폐 침투병 강제 해제
                case UserSkillType.HealingBeam: return 4;   // 4G: 지정 행(Row) 아군 유닛 체력 +4 회복 (최대체력 초과 불가)
                case UserSkillType.FortifyBeam: return 5;   // 5G: 지정 행(Row) 아군 유닛 최대 체력 +2 (내구도 증가)
                default: return 0;
            }
        }

        public bool CastSkill(UserSkillType skill, int targetCol = 0, int targetRow = 0, Owner caster = Owner.Player)
        {
            return CastSkill(skill, targetCol, targetRow, caster, out _, out _);
        }

        public bool CastSkill(UserSkillType skill, int targetCol, int targetRow, Owner caster, out List<CombatEvent> outEvents, out CombatLaserBeam outBeam)
        {
            outEvents = new List<CombatEvent>();
            outBeam = null;
            int cost = GetSkillCost(skill);
            int credit = (caster == Owner.Player) ? PlayerCredit : AiCredit;
            if (credit < cost) return false;

            Owner opponent = (caster == Owner.Player) ? Owner.Ai : Owner.Player;

            switch (skill)
            {
                case UserSkillType.NanoRepair:
                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;
                    for (int r = 0; r < Rows; r++)
                    {
                        for (int c = 0; c < Cols; c++)
                        {
                            var b = Buildings[r, c];
                            if (b != null && b.Owner == caster && b.Hp > 0)
                            {
                                b.Hp = Math.Min(b.MaxHp, b.Hp + 3);
                                b.HitFlashAlpha = 0.8f;
                                outEvents.Add(new CombatEvent { Col = c, Row = r, Text = "+3 나노수리 🔧", TextColor = Color.FromArgb(76, 175, 80) });
                            }
                        }
                    }
                    return true;

                case UserSkillType.ReconScan:
                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;
                    int revealed = 0;
                    foreach (var u in Units)
                    {
                        if (u.Owner == opponent && u.IsCloaked)
                        {
                            u.IsCloaked = false;
                            revealed++;
                            outEvents.Add(new CombatEvent { Col = u.Col, Row = u.Row, Text = "스텔스 노출! 📡", TextColor = Color.FromArgb(0, 229, 255) });
                        }
                    }
                    if (revealed == 0)
                    {
                        outEvents.Add(new CombatEvent { Col = Cols / 2, Row = Rows / 2, Text = "스캔 완료 (은폐 없음) 📡", TextColor = Color.FromArgb(0, 229, 255) });
                    }
                    return true;

                case UserSkillType.OrbitalStrike:
                    if (targetCol <= 0 || targetCol >= Cols - 1 || targetRow < 0 || targetRow >= Rows) return false;
                    var targetBldg = Buildings[targetRow, targetCol];
                    bool isObstacle = (MapTiles[targetRow, targetCol] == TileType.Obstacle);
                    if (targetBldg == null && !isObstacle) return false;
                    if (targetBldg != null && targetBldg.Owner != opponent) return false;

                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;

                    outBeam = new CombatLaserBeam
                    {
                        StartCol = targetCol,
                        StartRow = -1, // 하늘에서 내리꽂히는 궤도 레이저
                        EndCol = targetCol,
                        EndRow = targetRow,
                        BeamColor = Color.FromArgb(255, 60, 60),
                        Alpha = 1.0f
                    };

                    if (targetBldg != null)
                    {
                        targetBldg.Hp -= 6;
                        targetBldg.HitFlashAlpha = 1.0f;
                        targetBldg.ShakeOffset = 6f;
                        outEvents.Add(new CombatEvent { Col = targetCol, Row = targetRow, Text = "-6 궤도폭격! 💥", TextColor = Color.FromArgb(255, 23, 68), IsCritical = true });
                        if (targetBldg.Hp <= 0)
                        {
                            if (targetBldg.Owner == Owner.Ai) StatsPlayerBuildingsDestroyed++;
                            Buildings[targetRow, targetCol] = null;
                        }
                    }
                    else if (isObstacle)
                    {
                        ObstacleHp[targetRow, targetCol] -= 6;
                        outEvents.Add(new CombatEvent { Col = targetCol, Row = targetRow, Text = "-6 궤도폭격! 💥", TextColor = Color.FromArgb(255, 23, 68), IsCritical = true });
                        if (ObstacleHp[targetRow, targetCol] <= 0)
                        {
                            MapTiles[targetRow, targetCol] = TileType.Plain;
                            StatsObstaclesDestroyed++;
                            if (caster == Owner.Player)
                            {
                                PlayerCredit += 2;
                                StatsTotalCreditEarned += 2;
                            }
                            else AiCredit += 2;
                            string rewardText = (caster == Owner.Player) ? "아군 랙 완파! +2G 국고 ⚡" : "적 랙 완파! (적 +2G) ⚡";
                            Color rewardCol = (caster == Owner.Player) ? Color.FromArgb(255, 215, 0) : Color.FromArgb(255, 110, 64);
                            outEvents.Add(new CombatEvent { Col = targetCol, Row = targetRow, Text = rewardText, TextColor = rewardCol, IsCritical = true });
                        }
                    }
                    return true;

                case UserSkillType.EmpJammer:
                    if (targetCol <= 0 || targetCol >= Cols - 1 || targetRow < 0 || targetRow >= Rows) return false;

                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;

                    int empCount = 0;
                    for (int r = Math.Max(0, targetRow - 1); r <= Math.Min(Rows - 1, targetRow + 1); r++)
                    {
                        for (int c = Math.Max(1, targetCol - 1); c <= Math.Min(Cols - 2, targetCol + 1); c++)
                        {
                            var b = Buildings[r, c];
                            if (b != null && b.Owner == opponent && b.Hp > 0)
                            {
                                b.EmpTurns = Math.Max(b.EmpTurns, 2);
                                b.HitFlashAlpha = 1.0f;
                                empCount++;
                                outEvents.Add(new CombatEvent { Col = c, Row = r, Text = "⚡EMP 2T 정지!", TextColor = Color.FromArgb(0, 229, 255) });
                            }
                        }
                    }
                    if (empCount == 0)
                    {
                        outEvents.Add(new CombatEvent { Col = targetCol, Row = targetRow, Text = "⚡EMP 방전 (적 건물 없음)", TextColor = Color.FromArgb(0, 229, 255) });
                    }
                    return true;

                case UserSkillType.HealingBeam:
                    if (targetRow < 0 || targetRow >= Rows) return false;

                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;

                    outBeam = new CombatLaserBeam
                    {
                        StartCol = 0,
                        StartRow = targetRow,
                        EndCol = Cols - 1,
                        EndRow = targetRow,
                        BeamColor = Color.FromArgb(0, 230, 118),
                        Alpha = 1.0f
                    };

                    int healedCount = 0;
                    foreach (var u in Units)
                    {
                        if (u.Owner == caster && u.Row == targetRow && u.Hp > 0)
                        {
                            if (u.Hp < u.MaxHp)
                            {
                                u.Hp = Math.Min(u.MaxHp, u.Hp + 4);
                                u.HitFlashAlpha = 0.8f;
                                outEvents.Add(new CombatEvent { Col = u.Col, Row = u.Row, Text = "+4 치료 💚", TextColor = Color.FromArgb(0, 230, 118) });
                                healedCount++;
                            }
                        }
                    }
                    if (healedCount == 0)
                    {
                        outEvents.Add(new CombatEvent { Col = Cols / 2, Row = targetRow, Text = "치료 광선 조사 완료 💚", TextColor = Color.FromArgb(0, 230, 118) });
                    }
                    return true;

                case UserSkillType.FortifyBeam:
                    if (targetRow < 0 || targetRow >= Rows) return false;

                    if (caster == Owner.Player) PlayerCredit -= cost;
                    else AiCredit -= cost;

                    outBeam = new CombatLaserBeam
                    {
                        StartCol = 0,
                        StartRow = targetRow,
                        EndCol = Cols - 1,
                        EndRow = targetRow,
                        BeamColor = Color.FromArgb(0, 229, 255),
                        Alpha = 1.0f
                    };

                    int fortifyCount = 0;
                    foreach (var u in Units)
                    {
                        if (u.Owner == caster && u.Row == targetRow && u.Hp > 0)
                        {
                            u.MaxHp += 2;
                            u.Hp += 2;
                            u.HitFlashAlpha = 0.8f;
                            outEvents.Add(new CombatEvent { Col = u.Col, Row = u.Row, Text = "+2 구축 🛡️", TextColor = Color.FromArgb(0, 229, 255) });
                            fortifyCount++;
                        }
                    }
                    if (fortifyCount == 0)
                    {
                        outEvents.Add(new CombatEvent { Col = Cols / 2, Row = targetRow, Text = "구축 광선 조사 완료 🛡️", TextColor = Color.FromArgb(0, 229, 255) });
                    }
                    return true;
            }
            return false;
        }

        public int CalculateIncome(Owner owner)
        {
            int income = 2; // 기본 수입
            income += (owner == Owner.Player) ? PlayerIncomeBonus : AiIncomeBonus;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var b = Buildings[r, c];
                    // EMP 상태인 발전소는 크레딧을 생산하지 못함!
                    if (b != null && b.Owner == owner && b.Type == BuildingType.PowerPlant && b.EmpTurns <= 0)
                    {
                        // 자원 노드 위 발전소는 3G (일반의 3배), 일반 타일은 1G
                        income += (MapTiles[r, c] == TileType.Resource) ? 3 : 1;
                    }
                }
            }
            return income;
        }

        #endregion

        #region Placement Rules

        public bool IsValidBuildingPlacement(int col, int row, Owner owner)
        {
            if (col <= 0 || col >= Cols - 1 || row < 0 || row >= Rows) return false;
            if (MapTiles[row, col] == TileType.Obstacle) return false;
            if (Buildings[row, col] != null) return false;
            if (Units.Exists(u => u.Col == col && u.Row == row)) return false;

            // 건설 가능 영역: 아군 진영 베이스라인(Col 1~2 for Player, Col (Cols-3)~(Cols-2) for AI)
            // 또는 아군 기존 건물 인접 2칸 이내
            if (owner == Owner.Player)
            {
                if (col <= 2) return true;
            }
            else
            {
                if (col >= Cols - 3) return true;
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Buildings[r, c] != null && Buildings[r, c].Owner == owner)
                    {
                        int dist = Math.Abs(col - c) + Math.Abs(row - r);
                        if (dist <= 2) return true;
                    }
                }
            }

            return false;
        }

        public bool IsValidUnitSpawnPlacement(int col, int row, Owner owner)
        {
            if (col <= 0 || col >= Cols - 1 || row < 0 || row >= Rows) return false;
            if (MapTiles[row, col] == TileType.Obstacle) return false;
            if (Buildings[row, col] != null && Buildings[row, col].Type == BuildingType.Wall) return false;
            if (Units.Exists(u => u.Col == col && u.Row == row)) return false;

            // 스폰 가능 위치:
            // 1. 본진 바로 앞 라인 (Player: Col 1, AI: Col (Cols - 2))
            // 2. 또는 아군 공장(Factory)의 바로 앞 1칸
            if (owner == Owner.Player)
            {
                if (col == 1) return true;
            }
            else
            {
                if (col == Cols - 2) return true;
            }

            int factoryCheckCol = (owner == Owner.Player) ? col - 1 : col + 1;
            if (factoryCheckCol >= 1 && factoryCheckCol < Cols - 1)
            {
                var b = Buildings[row, factoryCheckCol];
                if (b != null && b.Owner == owner && b.Type == BuildingType.Factory)
                    return true;
            }

            return false;
        }

        public bool Build(int col, int row, BuildingType type, Owner owner)
        {
            // 발전소는 진영당 최대 한도(기본 3기, 업그레이드/카드로 확장 가능)로 제한
            if (type == BuildingType.PowerPlant && GetBuildingCount(BuildingType.PowerPlant, owner) >= GetPlantLimit(owner))
                return false;

            int cost = GetBuildingCost(type, owner);
            int credit = (owner == Owner.Player) ? PlayerCredit : AiCredit;
            if (credit < cost) return false;
            if (!IsValidBuildingPlacement(col, row, owner)) return false;

            if (owner == Owner.Player) PlayerCredit -= cost;
            else AiCredit -= cost;

            int hp;
            switch (type)
            {
                case BuildingType.PowerPlant: hp = 4; break;
                case BuildingType.Turret: hp = 6; break;
                case BuildingType.Wall: hp = 8; break;
                case BuildingType.Factory: hp = 10; break;
                case BuildingType.RepairDepot: hp = 6; break;
                case BuildingType.PhaseGate: hp = 6; break;
                default: hp = 5; break;
            }

            Buildings[row, col] = new Building
            {
                Owner = owner,
                Type = type,
                Hp = hp,
                MaxHp = hp,
                Col = col,
                Row = row
            };

            return true;
        }

        public bool SpawnUnit(int col, int row, UnitType type, Owner owner)
        {
            int cost = GetUnitCost(type);
            int credit = (owner == Owner.Player) ? PlayerCredit : AiCredit;
            if (credit < cost) return false;
            if (!IsValidUnitSpawnPlacement(col, row, owner)) return false;

            if (owner == Owner.Player) PlayerCredit -= cost;
            else AiCredit -= cost;

            int hp = 3;
            int atk = 1;
            int spd = 1;
            int rng = 1;
            int minRng = 1;

            switch (type)
            {
                case UnitType.Vanguard:
                    hp = 3 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 1 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 1;
                    minRng = 1;
                    break;
                case UnitType.Raider:
                    hp = 3 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 2 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 2; // 2칸 질주
                    rng = 1;
                    minRng = 1;
                    break;
                case UnitType.Titan:
                    hp = 10 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 2; // 2칸 원거리 공성 사격
                    minRng = 1;
                    break;
                case UnitType.Colossus:
                    hp = 8 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 4 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 3; // 3칸 초장거리 포격
                    // 근접 방어 발칸포 카드 해금 시 근접 1칸 사격 및 반격 허용!
                    minRng = ((owner == Owner.Player ? PlayerHasColossusMelee : AiHasColossusMelee) ? 1 : 2);
                    break;
                case UnitType.Dreadnought:
                    hp = 12 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 2; // 2칸 사거리 (십자 스플래시 방전)
                    minRng = 1;
                    break;
                case UnitType.Cheongung:
                    hp = 5 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    bool hasTraj = (owner == Owner.Player) ? PlayerHasCheongungTrajectory : AiHasCheongungTrajectory;
                    rng = hasTraj ? 5 : 4;
                    minRng = hasTraj ? 3 : 2;
                    break;
            }

            bool hasShield = (type == UnitType.Vanguard) && ((owner == Owner.Player) ? PlayerHasPhalanxShield : AiHasPhalanxShield);
            bool isCloaked = (type == UnitType.Raider) && ((owner == Owner.Player) ? PlayerHasStealthCloak : AiHasStealthCloak);

            Units.Add(new Unit
            {
                Id = _unitIdCounter++,
                Owner = owner,
                Type = type,
                Hp = hp,
                MaxHp = hp,
                Atk = atk,
                Speed = spd,
                Range = rng,
                MinRange = minRng,
                HasShield = hasShield,
                IsCloaked = isCloaked,
                Col = col,
                Row = row,
                VisualCol = col,
                TargetCol = col,
                HasActed = true // 소환된 턴에는 즉시 진격하지 않음
            });

            return true;
        }

        /// <summary>
        /// 프로토콜 카드(공수/증원 등)를 통한 무료 타일 직접 지정 스폰
        /// </summary>
        public bool SpawnUnitDirect(int col, int row, UnitType type, Owner owner)
        {
            if (col <= 0 || col >= Cols - 1 || row < 0 || row >= Rows) return false;
            if (MapTiles[row, col] == TileType.Obstacle) return false;
            if (Buildings[row, col] != null && Buildings[row, col].Type == BuildingType.Wall) return false;
            if (Units.Exists(u => u.Col == col && u.Row == row)) return false;

            int hp = 3;
            int atk = 1;
            int spd = 1;
            int rng = 1;
            int minRng = 1;

            switch (type)
            {
                case UnitType.Vanguard:
                    hp = 3 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 1 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    if (PendingDropCustomAtk > 0) atk = Math.Max(atk, PendingDropCustomAtk);
                    spd = 1;
                    rng = 1;
                    minRng = 1;
                    break;
                case UnitType.Raider:
                    hp = 3 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 2 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 2;
                    rng = 1;
                    minRng = 1;
                    break;
                case UnitType.Titan:
                    hp = 10 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 2;
                    minRng = 1;
                    break;
                case UnitType.Colossus:
                    hp = 8 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 4 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 3;
                    minRng = ((owner == Owner.Player ? PlayerHasColossusMelee : AiHasColossusMelee) ? 1 : 2);
                    break;
                case UnitType.Dreadnought:
                    hp = 12 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    rng = 2;
                    minRng = 1;
                    break;
                case UnitType.Cheongung:
                    hp = 5 + ((owner == Owner.Player) ? PlayerUnitHpBonus : AiUnitHpBonus);
                    atk = 3 + ((owner == Owner.Player) ? PlayerUnitAtkBonus : AiUnitAtkBonus);
                    spd = 1;
                    bool hasTraj = (owner == Owner.Player) ? PlayerHasCheongungTrajectory : AiHasCheongungTrajectory;
                    rng = hasTraj ? 5 : 4;
                    minRng = hasTraj ? 3 : 2;
                    break;
            }

            bool hasShield = (type == UnitType.Vanguard) && (((owner == Owner.Player) ? PlayerHasPhalanxShield : AiHasPhalanxShield) || PendingDropForceShield);
            bool isCloaked = (type == UnitType.Raider) && ((owner == Owner.Player) ? PlayerHasStealthCloak : AiHasStealthCloak);

            Units.Add(new Unit
            {
                Id = _unitIdCounter++,
                Owner = owner,
                Type = type,
                Hp = hp,
                MaxHp = hp,
                Atk = atk,
                Speed = spd,
                Range = rng,
                MinRange = minRng,
                HasShield = hasShield,
                IsCloaked = isCloaked,
                Col = col,
                Row = row,
                VisualCol = col,
                TargetCol = col,
                HasActed = false // 무료 공수 유닛은 즉시 전열 참여 가능
            });

            return true;
        }

        #endregion

        #region Step-by-Step Turn & Combat Engine (타격감 & 시각화 지원)

        /// <summary>
        /// 포탑의 전방위 요격 범위 판정 (인접 1칸 사각지대 제외, 거리 2~3칸 요격망)
        /// </summary>
        public static bool IsTileInTurretRange(int turretRow, int turretCol, int targetRow, int targetCol, int rangeBonus = 0)
        {
            int dr = Math.Abs(targetRow - turretRow);
            int dc = Math.Abs(targetCol - turretCol);

            // 포탑 사각지대: 바로 인접 1칸(상하좌우 및 본인 타일)은 포탑이 요격할 수 없음!
            if (dr + dc <= 1) return false;

            if (rangeBonus == 0)
            {
                // 기본 요격망: 3x3 대각 4칸(dr==1 && dc==1) + 상하좌우 십자 2칸(dr==2, dc==0 또는 dr==0, dc==2)
                if (dr <= 1 && dc <= 1) return true;
                if ((dr == 2 && dc == 0) || (dr == 0 && dc == 2)) return true;
                return false;
            }
            else
            {
                // 보너스 사거리(레일건 개조 카드) 적용 시
                int limitDist = 2 + rangeBonus;
                if (dr + dc <= limitDist) return true;
                if (dr <= 1 + rangeBonus && dc <= 1 + rangeBonus) return true;
                return false;
            }
        }

        private bool IsTurretLineBlockedByObstacle(int tr, int tc, int targetR, int targetC)
        {
            if (MapTiles[targetR, targetC] == TileType.Obstacle) return true;

            // 동일 행(Row)인 경우 직선 경로 사이 장애물 체크
            if (tr == targetR)
            {
                int minC = Math.Min(tc, targetC);
                int maxC = Math.Max(tc, targetC);
                for (int c = minC + 1; c < maxC; c++)
                {
                    if (MapTiles[tr, c] == TileType.Obstacle) return true;
                }
            }
            // 동일 열(Col)인 경우 직선 경로 사이 장애물 체크
            else if (tc == targetC)
            {
                int minR = Math.Min(tr, targetR);
                int maxR = Math.Max(tr, targetR);
                for (int r = minR + 1; r < maxR; r++)
                {
                    if (MapTiles[r, tc] == TileType.Obstacle) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 1. 포탑 사격 실행 (13칸 전방위 요격 & 가장 위협적인 단일 타깃 우선 포격)
        /// </summary>
        public List<CombatLaserBeam> ExecuteTurretAttacks(Owner actingOwner, List<CombatEvent> outEvents)
        {
            var beams = new List<CombatLaserBeam>();
            Owner opponent = (actingOwner == Owner.Player) ? Owner.Ai : Owner.Player;
            Color beamColor = (actingOwner == Owner.Player) ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 23, 68);

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var b = Buildings[r, c];
                    if (b != null && b.Owner == actingOwner && b.Type == BuildingType.Turret && b.Hp > 0 && b.EmpTurns <= 0)
                    {
                        int rangeBonus = (actingOwner == Owner.Player) ? TurretRangeBonus : 0;

                        // 13칸 범위 내 적 유닛들 중 가장 위협적인(본진에 가까운) 유닛 탐색
                        Unit bestTargetUnit = null;
                        int bestThreatScore = -9999;
                        bool hasRadar = (actingOwner == Owner.Player) ? PlayerHasTurretRadar : AiHasTurretRadar;

                        foreach (var u in Units)
                        {
                            if (u.Owner != opponent || u.Hp <= 0) continue;
                            if (!IsTileInTurretRange(r, c, u.Row, u.Col, rangeBonus)) continue;

                            // [은폐 감지 체크]: 침투병이 은폐 상태이고 포탑 진영에 레이더가 없으면 요격 불가!
                            if (u.IsCloaked && !hasRadar) continue;

                            // 직선 경로에 장애물 랙이 가로막고 있는지 확인 (직선 시야 차단)
                            if (IsTurretLineBlockedByObstacle(r, c, u.Row, u.Col)) continue;

                            // 위협도 점수:
                            // 플레이어 포탑: 적 유닛의 Col이 작을수록(플레이어 코어 쪽으로 더 깊이 침공할수록) 위협적
                            // AI 포탑: 적 유닛의 Col이 클수록(AI 코어 쪽으로 더 깊이 침공할수록) 위협적
                            int threatScore = (actingOwner == Owner.Player) ? (100 - u.Col * 5) : (u.Col * 5);
                            // 남은 체력이 적은 유닛 우선 마무리(+10점)
                            threatScore += (10 - Math.Min(10, u.Hp));

                            if (threatScore > bestThreatScore)
                            {
                                bestThreatScore = threatScore;
                                bestTargetUnit = u;
                            }
                        }

                        if (bestTargetUnit != null)
                        {
                            int dmg = 2;
                            if (bestTargetUnit.HasShield)
                            {
                                bestTargetUnit.HasShield = false;
                                dmg = 0;
                                outEvents.Add(new CombatEvent
                                {
                                    Col = bestTargetUnit.Col,
                                    Row = bestTargetUnit.Row,
                                    Text = "실드 방어! 🛡️",
                                    TextColor = Color.FromArgb(64, 196, 255)
                                });
                            }
                            else
                            {
                                bestTargetUnit.Hp -= dmg;
                                bestTargetUnit.HitFlashAlpha = 1.0f; // 피격 플래시!
                                bestTargetUnit.ShakeOffset = (actingOwner == Owner.Player) ? 4f : -4f;
                                outEvents.Add(new CombatEvent
                                {
                                    Col = bestTargetUnit.Col,
                                    Row = bestTargetUnit.Row,
                                    Text = $"-{dmg} ⚡",
                                    TextColor = beamColor
                                });
                            }

                            beams.Add(new CombatLaserBeam
                            {
                                StartCol = c,
                                StartRow = r,
                                EndCol = bestTargetUnit.Col,
                                EndRow = bestTargetUnit.Row,
                                BeamColor = beamColor,
                                Alpha = 1.0f
                            });

                            continue;
                        }

                        // 유닛이 없다면 13칸 범위 내 적 건물도 포격 가능
                        Building bestTargetBldg = null;
                        int targetBldgR = -1, targetBldgC = -1;

                        for (int br = 0; br < Rows; br++)
                        {
                            for (int bc = 0; bc < Cols; bc++)
                            {
                                var enemyBldg = Buildings[br, bc];
                                if (enemyBldg != null && enemyBldg.Owner == opponent && enemyBldg.Hp > 0)
                                {
                                    if (IsTileInTurretRange(r, c, br, bc, rangeBonus) && !IsTurretLineBlockedByObstacle(r, c, br, bc))
                                    {
                                        bestTargetBldg = enemyBldg;
                                        targetBldgR = br;
                                        targetBldgC = bc;
                                        break;
                                    }
                                }
                            }
                            if (bestTargetBldg != null) break;
                        }

                        if (bestTargetBldg != null)
                        {
                            int dmg = 2;
                            bestTargetBldg.Hp -= dmg;
                            bestTargetBldg.HitFlashAlpha = 1.0f;
                            bestTargetBldg.ShakeOffset = (actingOwner == Owner.Player) ? 4f : -4f;

                            beams.Add(new CombatLaserBeam
                            {
                                StartCol = c,
                                StartRow = r,
                                EndCol = targetBldgC,
                                EndRow = targetBldgR,
                                BeamColor = beamColor,
                                Alpha = 1.0f
                            });

                            outEvents.Add(new CombatEvent
                            {
                                Col = targetBldgC,
                                Row = targetBldgR,
                                Text = $"-{dmg} ⚡",
                                TextColor = beamColor
                            });

                            if (bestTargetBldg.Hp <= 0)
                            {
                                Buildings[targetBldgR, targetBldgC] = null;
                                if (bestTargetBldg.Owner == Owner.Ai) StatsPlayerBuildingsDestroyed++;
                            }
                        }
                    }
                }
            }

            var deadUnits = Units.FindAll(u => u.Hp <= 0);
            foreach (var dead in deadUnits)
            {
                if (dead.Owner == Owner.Ai) StatsPlayerUnitsKilled++;
                else StatsPlayerUnitsLost++;
                TriggerMartyrdomBombExplosion(dead, outEvents);
            }
            Units.RemoveAll(u => u.Hp <= 0);
            return beams;
        }

        /// <summary>
        /// 2. 유닛 이동 목표 계산 (슬라이드 애니메이션을 위해 TargetCol 설정)
        /// </summary>
        public void PrepareUnitMarchTargets(Owner actingOwner)
        {
            Owner opponent = (actingOwner == Owner.Player) ? Owner.Ai : Owner.Player;
            int stepDir = (actingOwner == Owner.Player) ? 1 : -1;

            // 기저 상태 동기화
            foreach (var u in Units)
            {
                if (u.TargetCol < 0) u.TargetCol = u.Col;
            }

            var actingUnits = Units.FindAll(u => u.Owner == actingOwner);
            // 전선에 가장 가까운 유닛부터 전진 계산 (충돌 및 병목 방지)
            actingUnits.Sort((a, b) => (actingOwner == Owner.Player) ? b.Col.CompareTo(a.Col) : a.Col.CompareTo(b.Col));

            foreach (var unit in actingUnits)
            {
                if (unit.Hp <= 0 || unit.HasActed) continue;

                int targetCol = unit.Col;

                // 가속 관문 (PhaseGate): 통과하거나 관문에서 출발하는 아군 유닛 이동력 +1
                int maxSteps = unit.Speed;
                bool gateBoosted = false;
                var currentBldg = Buildings[unit.Row, unit.Col];
                if (currentBldg != null && currentBldg.Owner == actingOwner && currentBldg.Type == BuildingType.PhaseGate && currentBldg.Hp > 0 && currentBldg.EmpTurns <= 0)
                {
                    gateBoosted = true;
                    maxSteps++;
                }

                for (int step = 0; step < maxSteps; step++)
                {
                    // [시즈 포격 규칙]: 
                    // 콜로서스는 '시즈 앵커(Siege Anchor)' 카드가 해금되었을 때만 제자리 포격(Hold & Fire) 발동!
                    // (카드가 없으면 일반 유닛처럼 전진하다가 충돌 시 정지)
                    bool canHoldAndFire = (unit.Type == UnitType.Colossus) && ((actingOwner == Owner.Player) ? PlayerHasSiegeAnchor : AiHasSiegeAnchor);
                    if (canHoldAndFire)
                    {
                        bool hasTargetInRange = false;
                        for (int dist = unit.MinRange; dist <= unit.Range; dist++)
                        {
                            int checkCol = targetCol + dist * stepDir;
                            if (checkCol <= 0 || checkCol >= Cols - 1) break;

                            // 은폐 상태인 유닛은 dist > 1일 때 시야에 잡히지 않음
                            if (Units.Exists(u => u.Hp > 0 && u.Col == checkCol && u.Row == unit.Row && u.Owner == opponent && (!u.IsCloaked || dist == 1)))
                            {
                                hasTargetInRange = true;
                                break;
                            }
                            if (Buildings[unit.Row, checkCol] != null && Buildings[unit.Row, checkCol].Owner == opponent && Buildings[unit.Row, checkCol].Hp > 0)
                            {
                                hasTargetInRange = true;
                                break;
                            }
                            if (MapTiles[unit.Row, checkCol] == TileType.Obstacle)
                            {
                                hasTargetInRange = true;
                                break;
                            }
                        }

                        if (hasTargetInRange)
                            break;
                    }

                    int nextCol = targetCol + stepDir;

                    // 1. 상대 본진 골라인 도달 (Core 진입)
                    if ((actingOwner == Owner.Player && nextCol >= Cols - 1) ||
                        (actingOwner == Owner.Ai && nextCol <= 0))
                    {
                        targetCol = nextCol;
                        break;
                    }

                    // 2. 장애물 랙에 막힘
                    if (MapTiles[unit.Row, nextCol] == TileType.Obstacle)
                        break;

                    // 3. 적 유닛 마주침: 적 칸으로 진입하지 않고 마주보는 바로 앞 칸에서 정지하여 저지선 형성!
                    if (Units.Exists(u => u.Hp > 0 && u.Col == nextCol && u.Row == unit.Row && u.Owner == opponent))
                        break;

                    // 4. 적 건물 마주침: 적 건물 칸으로 진입하지 않고 바로 앞 칸에서 정지하여 공성 대치!
                    if (Buildings[unit.Row, nextCol] != null && Buildings[unit.Row, nextCol].Owner == opponent && Buildings[unit.Row, nextCol].Hp > 0)
                        break;

                    // 5. 아군 방화벽 격벽(Wall) 마주침: 격벽은 통과 불가 방어벽이므로 멈춤
                    var friendlyBuilding = Buildings[unit.Row, nextCol];
                    if (friendlyBuilding != null && friendlyBuilding.Owner == actingOwner && friendlyBuilding.Type == BuildingType.Wall)
                        break;
                    // (규칙 반영: 아군 발전소, 포탑, 전진 공장은 아군 유닛이 자유롭게 통과하여 계속 전진 가능!)

                    // 6. 아군 유닛 충돌 방지: 이미 앞서 이동 결정을 마친 아군 유닛이 해당 칸을 차지했거나 정지해 있는 경우
                    bool friendlyUnitOccupied = Units.Exists(u => u.Hp > 0 && u != unit && u.Owner == actingOwner && u.Row == unit.Row &&
                        (Math.Abs(u.TargetCol - nextCol) < 0.1f || (u.HasActed && u.Col == nextCol)));
                    if (friendlyUnitOccupied)
                        break;

                    targetCol = nextCol;

                    // 이동 중 가속 관문 통과 시 추가 이동력 +1 (턴당 최대 1회 부스트)
                    if (!gateBoosted)
                    {
                        var steppedBldg = Buildings[unit.Row, nextCol];
                        if (steppedBldg != null && steppedBldg.Owner == actingOwner && steppedBldg.Type == BuildingType.PhaseGate && steppedBldg.Hp > 0 && steppedBldg.EmpTurns <= 0)
                        {
                            gateBoosted = true;
                            maxSteps++;
                        }
                    }
                }

                unit.TargetCol = targetCol;
            }
        }

        /// <summary>
        /// 3. 유닛 이동 완료 후 교전 판정 및 본진 타격 해결
        /// </summary>
        public List<CombatEvent> ResolveUnitCombatAndCoreStrikes(Owner actingOwner)
        {
            var events = new List<CombatEvent>();
            Owner opponent = (actingOwner == Owner.Player) ? Owner.Ai : Owner.Player;
            int stepDir = (actingOwner == Owner.Player) ? 1 : -1;

            var actingUnits = Units.FindAll(u => u.Owner == actingOwner);
            actingUnits.Sort((a, b) => (actingOwner == Owner.Player) ? b.Col.CompareTo(a.Col) : a.Col.CompareTo(b.Col));

            // 이동 애니메이션 완료 후 논리 좌표 동기화 (TargetCol이 설정된 경우에만 동기화)
            foreach (var u in actingUnits)
            {
                if (u.TargetCol >= 0)
                {
                    u.Col = (int)Math.Round(u.TargetCol);
                    u.VisualCol = u.Col;
                }
            }

            foreach (var unit in actingUnits)
            {
                if (unit.Hp <= 0) continue;
                if (unit.HasActed)
                {
                    unit.HasActed = false;
                    continue;
                }

                // 1. 상대 본진 골라인 도달 시 코어 폭발 타격
                if ((actingOwner == Owner.Player && unit.Col >= Cols - 1) ||
                    (actingOwner == Owner.Ai && unit.Col <= 0))
                {
                    if (unit.IsCloaked) unit.IsCloaked = false;
                    int dmg = unit.Atk * 2;
                    if (actingOwner == Owner.Player)
                    {
                        AiCoreHp = Math.Max(0, AiCoreHp - dmg);
                        StatsDamageDealtToAiCore += dmg;
                        events.Add(new CombatEvent { Col = Cols - 1, Row = unit.Row, Text = $"CORE HIT -{dmg}", TextColor = Color.FromArgb(0, 229, 255), IsCritical = true });
                    }
                    else
                    {
                        PlayerCoreHp = Math.Max(0, PlayerCoreHp - dmg);
                        events.Add(new CombatEvent { Col = 0, Row = unit.Row, Text = $"CORE HIT -{dmg}", TextColor = Color.FromArgb(255, 23, 68), IsCritical = true });
                    }
                    unit.Hp = 0;
                    continue;
                }

                // 2. 전방 사거리 내(Range) 적 유닛 또는 적 건물 탐색 및 공격 (MinRange 반영: 근접 불가 유닛 지원)
                for (int dist = unit.MinRange; dist <= unit.Range; dist++)
                {
                    int targetCol = unit.Col + dist * stepDir;
                    if (targetCol <= 0 || targetCol >= Cols - 1) break;

                    // (A) 적 유닛과의 교전
                    var enemyUnit = Units.Find(u => u.Col == targetCol && u.Row == unit.Row && u.Owner == opponent && u.Hp > 0);
                    // [은폐 회피]: 은폐 상태인 적 유닛은 dist > 1인 원거리에서는 타깃팅되지 않음!
                    if (enemyUnit != null && enemyUnit.IsCloaked && dist > 1)
                    {
                        enemyUnit = null;
                    }

                    if (enemyUnit != null)
                    {
                        // 공격자 침투병은 첫 공격 개시 시 은폐 해제!
                        if (unit.IsCloaked)
                        {
                            unit.IsCloaked = false;
                            events.Add(new CombatEvent { Col = unit.Col, Row = unit.Row, Text = "은폐 해제! ⚡", TextColor = Color.FromArgb(0, 229, 255) });
                        }

                        unit.BumpOffset = stepDir * 6f;
                        enemyUnit.HitFlashAlpha = 1.0f;
                        enemyUnit.ShakeOffset = stepDir * 4f;

                        // 선공 타격 (드레드노트 취약성 체크: 보병/침투병에게 맞을 시 +2뎀)
                        int atkDmg = unit.Atk;
                        bool isVuln = (enemyUnit.Type == UnitType.Dreadnought && (unit.Type == UnitType.Vanguard || unit.Type == UnitType.Raider));
                        if (isVuln) atkDmg += 2;

                        // [보병 전술 방패]: 첫 1회 피해를 완전히 무효화!
                        if (enemyUnit.HasShield)
                        {
                            enemyUnit.HasShield = false;
                            events.Add(new CombatEvent { Col = targetCol, Row = unit.Row, Text = "실드 방어! 🛡️", TextColor = Color.FromArgb(64, 196, 255) });
                        }
                        else
                        {
                            enemyUnit.Hp -= atkDmg;
                            string hitText = isVuln ? $"-{atkDmg} 취약!" : $"-{atkDmg}";
                            events.Add(new CombatEvent { Col = targetCol, Row = unit.Row, Text = hitText, TextColor = isVuln ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 193, 7) });
                        }

                        // 타이탄 특수능력: [운동에너지 충격파] - 1칸 넉백 및 연쇄 충돌 피해
                        bool hasKineticSlam = (actingOwner == Owner.Player) ? PlayerHasKineticSlam : AiHasKineticSlam;
                        if (unit.Type == UnitType.Titan && hasKineticSlam && enemyUnit.Hp > 0)
                        {
                            int behindCol = enemyUnit.Col + stepDir;
                            bool isBlocked = false;

                            if (behindCol <= 0 || behindCol >= Cols - 1)
                            {
                                isBlocked = true;
                                if (behindCol <= 0) PlayerCoreHp = Math.Max(0, PlayerCoreHp - 1);
                                else
                                {
                                    AiCoreHp = Math.Max(0, AiCoreHp - 1);
                                    StatsDamageDealtToAiCore += 1;
                                }
                            }
                            else if (MapTiles[unit.Row, behindCol] == TileType.Obstacle)
                            {
                                isBlocked = true;
                                ObstacleHp[unit.Row, behindCol] -= 1;
                                if (ObstacleHp[unit.Row, behindCol] <= 0)
                                {
                                    MapTiles[unit.Row, behindCol] = TileType.Plain;
                                    StatsObstaclesDestroyed++;
                                    if (actingOwner == Owner.Player) { PlayerCredit += 2; StatsTotalCreditEarned += 2; }
                                    else AiCredit += 2;
                                    string rewardText = (actingOwner == Owner.Player) ? "아군 랙 돌파! +2G ⚡" : "적 랙 돌파! (적 +2G) ⚡";
                                    Color rewardCol = (actingOwner == Owner.Player) ? Color.FromArgb(255, 215, 0) : Color.FromArgb(255, 110, 64);
                                    events.Add(new CombatEvent { Col = behindCol, Row = unit.Row, Text = rewardText, TextColor = rewardCol, IsCritical = true });
                                }
                            }
                            else if (Buildings[unit.Row, behindCol] != null && Buildings[unit.Row, behindCol].Hp > 0)
                            {
                                isBlocked = true;
                                Buildings[unit.Row, behindCol].Hp -= 1;
                                if (Buildings[unit.Row, behindCol].Hp <= 0)
                                {
                                    if (Buildings[unit.Row, behindCol].Owner == Owner.Ai) StatsPlayerBuildingsDestroyed++;
                                    Buildings[unit.Row, behindCol] = null;
                                }
                            }
                            else
                            {
                                var behindUnit = Units.Find(u => u.Col == behindCol && u.Row == unit.Row && u.Hp > 0);
                                if (behindUnit != null)
                                {
                                    isBlocked = true;
                                    behindUnit.Hp -= 1;
                                    behindUnit.HitFlashAlpha = 0.8f;
                                    events.Add(new CombatEvent { Col = behindCol, Row = unit.Row, Text = "연쇄충돌! -1 💥", TextColor = Color.FromArgb(255, 87, 34) });
                                }
                            }

                            if (isBlocked)
                            {
                                enemyUnit.Hp -= 1;
                                enemyUnit.HitFlashAlpha = 1.0f;
                                events.Add(new CombatEvent { Col = enemyUnit.Col, Row = unit.Row, Text = "충돌! -1 💥", TextColor = Color.FromArgb(255, 60, 60) });
                            }
                            else
                            {
                                enemyUnit.Col = behindCol;
                                enemyUnit.VisualCol = behindCol;
                                enemyUnit.TargetCol = behindCol;
                                events.Add(new CombatEvent { Col = behindCol, Row = unit.Row, Text = "넉백! 💥", TextColor = Color.FromArgb(255, 152, 0) });
                            }
                        }

                        // 드레드노트 특수능력: [연쇄 플라즈마 방전] 카드 해금 시에만 십자 스플래시 발동
                        bool hasChainPlasma = (actingOwner == Owner.Player) ? PlayerHasChainPlasma : AiHasChainPlasma;
                        if (unit.Type == UnitType.Dreadnought && hasChainPlasma)
                        {
                            ApplyCrossSplash(targetCol, unit.Row, opponent, actingOwner, events);
                        }

                        // 근접 맞대결(dist == 1)인 경우 방어 유닛도 반격 (단, 방어 유닛이 근접 불가 유닛인 경우 반격 불가!)
                        if (dist == 1 && enemyUnit.Hp > 0 && enemyUnit.MinRange <= 1)
                        {
                            int retDmg = enemyUnit.Atk;
                            bool isUnitVuln = (unit.Type == UnitType.Dreadnought && (enemyUnit.Type == UnitType.Vanguard || enemyUnit.Type == UnitType.Raider));
                            if (isUnitVuln) retDmg += 2;

                            if (unit.HasShield)
                            {
                                unit.HasShield = false;
                                events.Add(new CombatEvent { Col = unit.Col, Row = unit.Row, Text = "실드 방어! 🛡️", TextColor = Color.FromArgb(64, 196, 255) });
                            }
                            else
                            {
                                unit.Hp -= retDmg;
                                unit.HitFlashAlpha = 0.8f;
                                unit.ShakeOffset = -stepDir * 3f;
                                string retText = isUnitVuln ? $"-{retDmg} 취약!" : $"-{retDmg}";
                                events.Add(new CombatEvent { Col = unit.Col, Row = unit.Row, Text = retText, TextColor = isUnitVuln ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 87, 34) });
                            }
                        }

                        if (enemyUnit.Hp <= 0)
                        {
                            if (enemyUnit.Owner == Owner.Ai) StatsPlayerUnitsKilled++;
                            else StatsPlayerUnitsLost++;
                            TriggerMartyrdomBombExplosion(enemyUnit, events);
                            Units.Remove(enemyUnit);
                        }
                        break;
                    }

                    // (B) 적 건물과의 공성 공격
                    var enemyBuilding = Buildings[unit.Row, targetCol];
                    if (enemyBuilding != null && enemyBuilding.Owner == opponent && enemyBuilding.Hp > 0)
                    {
                        if (unit.IsCloaked) unit.IsCloaked = false;
                        unit.BumpOffset = stepDir * 6f;
                        enemyBuilding.HitFlashAlpha = 1.0f;
                        enemyBuilding.ShakeOffset = stepDir * 4f;

                        // 천궁(Cheongung) 특성: 건물 공격 시 2배 피해(시즈버스터)!
                        int bldgDmg = (unit.Type == UnitType.Cheongung) ? unit.Atk * 2 : unit.Atk;
                        enemyBuilding.Hp -= bldgDmg;
                        events.Add(new CombatEvent
                        {
                            Col = targetCol,
                            Row = unit.Row,
                            Text = (unit.Type == UnitType.Cheongung) ? $"-{bldgDmg} 시즈버스터! 💥" : $"-{unit.Atk} 🧱",
                            TextColor = (unit.Type == UnitType.Cheongung) ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 152, 0),
                            IsCritical = (unit.Type == UnitType.Cheongung)
                        });

                        // 드레드노트 십자 스플래시 (해금 시)
                        bool hasChainPlasma = (actingOwner == Owner.Player) ? PlayerHasChainPlasma : AiHasChainPlasma;
                        if (unit.Type == UnitType.Dreadnought && hasChainPlasma)
                        {
                            ApplyCrossSplash(targetCol, unit.Row, opponent, actingOwner, events);
                        }

                        if (enemyBuilding.Hp <= 0)
                        {
                            if (enemyBuilding.Owner == Owner.Ai) StatsPlayerBuildingsDestroyed++;
                            Buildings[unit.Row, targetCol] = null;
                        }
                        break;
                    }

                    // (C) 중립 장애물 랙 파괴 공격!
                    if (MapTiles[unit.Row, targetCol] == TileType.Obstacle)
                    {
                        if (unit.IsCloaked) unit.IsCloaked = false;
                        unit.BumpOffset = stepDir * 6f;

                        // 천궁 특성: 장애물 랙에도 2배 피해(시즈버스터)!
                        int obsDmg = (unit.Type == UnitType.Cheongung) ? unit.Atk * 2 : unit.Atk;
                        ObstacleHp[unit.Row, targetCol] -= obsDmg;
                        events.Add(new CombatEvent
                        {
                            Col = targetCol,
                            Row = unit.Row,
                            Text = (unit.Type == UnitType.Cheongung) ? $"-{obsDmg} 시즈버스터! 💥" : $"-{unit.Atk} 랙 타격",
                            TextColor = (unit.Type == UnitType.Cheongung) ? Color.FromArgb(255, 87, 34) : Color.FromArgb(200, 210, 225),
                            IsCritical = (unit.Type == UnitType.Cheongung)
                        });

                        // 드레드노트 십자 스플래시 (해금 시)
                        bool hasChainPlasma = (actingOwner == Owner.Player) ? PlayerHasChainPlasma : AiHasChainPlasma;
                        if (unit.Type == UnitType.Dreadnought && hasChainPlasma)
                        {
                            ApplyCrossSplash(targetCol, unit.Row, opponent, actingOwner, events);
                        }

                        if (ObstacleHp[unit.Row, targetCol] <= 0)
                        {
                            MapTiles[unit.Row, targetCol] = TileType.Plain;
                            StatsObstaclesDestroyed++;
                            if (actingOwner == Owner.Player)
                            {
                                PlayerCredit += 2;
                                StatsTotalCreditEarned += 2;
                            }
                            else AiCredit += 2;

                            string rewardText = (actingOwner == Owner.Player) ? "아군 랙 돌파! +2G ⚡" : "적 랙 돌파! (적 +2G) ⚡";
                            Color rewardCol = (actingOwner == Owner.Player) ? Color.FromArgb(255, 215, 0) : Color.FromArgb(255, 110, 64);
                            events.Add(new CombatEvent
                            {
                                Col = targetCol,
                                Row = unit.Row,
                                Text = rewardText,
                                TextColor = rewardCol,
                                IsCritical = true
                            });
                        }
                        break;
                    }
                }
            }

            var combatDeadUnits = Units.FindAll(u => u.Hp <= 0);
            foreach (var dead in combatDeadUnits)
            {
                if (dead.Owner == Owner.Ai) StatsPlayerUnitsKilled++;
                else StatsPlayerUnitsLost++;
                TriggerMartyrdomBombExplosion(dead, events);
            }
            Units.RemoveAll(u => u.Hp <= 0);

            // 승리 체크
            if (AiCoreHp <= 0)
            {
                IsGameOver = true;
                Winner = Owner.Player;
                StatusMessage = "🎉 승리! 적 메인프레임 코어를 완파했습니다!";
                RecordReplaySnapshot();
            }
            else if (PlayerCoreHp <= 0)
            {
                IsGameOver = true;
                Winner = Owner.Ai;
                StatusMessage = "💀 패배! 아군 코어가 파괴되었습니다.";
                RecordReplaySnapshot();
            }

            return events;
        }

        /// <summary>
        /// 최후의 자폭 코어 기폭: 사망 시 상하좌우 4방향 적 기물/코어에 2 데미지 (아군 피해 없음)
        /// </summary>
        private void TriggerMartyrdomBombExplosion(Unit dead, List<CombatEvent> events)
        {
            if (!dead.HasBomb) return;
            dead.HasBomb = false; // 1회 기폭 보장

            Owner opponent = (dead.Owner == Owner.Player) ? Owner.Ai : Owner.Player;
            events.Add(new CombatEvent
            {
                Col = dead.Col,
                Row = dead.Row,
                Text = "💣 자폭 코어 기폭!",
                TextColor = Color.FromArgb(255, 23, 68),
                IsCritical = true
            });

            int[] dR = { -1, 1, 0, 0 };
            int[] dC = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nr = dead.Row + dR[i];
                int nc = dead.Col + dC[i];

                // 코어 도달 체크
                if (dead.Owner == Owner.Player && nc >= Cols - 1 && nr >= 0 && nr < Rows)
                {
                    AiCoreHp = Math.Max(0, AiCoreHp - 2);
                    StatsDamageDealtToAiCore += 2;
                    events.Add(new CombatEvent { Col = Cols - 1, Row = nr, Text = "💣 적 코어 -2", TextColor = Color.FromArgb(255, 23, 68), IsCritical = true });
                    continue;
                }
                else if (dead.Owner == Owner.Ai && nc <= 0 && nr >= 0 && nr < Rows)
                {
                    PlayerCoreHp = Math.Max(0, PlayerCoreHp - 2);
                    events.Add(new CombatEvent { Col = 0, Row = nr, Text = "💣 아군 코어 -2", TextColor = Color.FromArgb(255, 23, 68), IsCritical = true });
                    continue;
                }

                if (nr < 0 || nr >= Rows || nc <= 0 || nc >= Cols - 1) continue;

                // 1. 적 유닛만 타격
                var enemyUnit = Units.Find(u => u.Col == nc && u.Row == nr && u.Owner == opponent && u.Hp > 0);
                if (enemyUnit != null)
                {
                    if (enemyUnit.HasShield)
                    {
                        enemyUnit.HasShield = false;
                        events.Add(new CombatEvent { Col = nc, Row = nr, Text = "실드 방어! 🛡️", TextColor = Color.FromArgb(64, 196, 255) });
                    }
                    else
                    {
                        enemyUnit.Hp -= 2;
                        enemyUnit.HitFlashAlpha = 1.0f;
                        events.Add(new CombatEvent { Col = nc, Row = nr, Text = "-2 자폭! 💣", TextColor = Color.FromArgb(255, 23, 68) });
                        if (enemyUnit.Hp <= 0 && enemyUnit.HasBomb)
                        {
                            TriggerMartyrdomBombExplosion(enemyUnit, events);
                        }
                    }
                }

                // 2. 적 건물만 타격
                var enemyBldg = Buildings[nr, nc];
                if (enemyBldg != null && enemyBldg.Owner == opponent && enemyBldg.Hp > 0)
                {
                    enemyBldg.Hp -= 2;
                    enemyBldg.HitFlashAlpha = 1.0f;
                    events.Add(new CombatEvent { Col = nc, Row = nr, Text = "-2 자폭! 💣", TextColor = Color.FromArgb(255, 23, 68) });
                    if (enemyBldg.Hp <= 0)
                    {
                        if (enemyBldg.Owner == Owner.Ai) StatsPlayerBuildingsDestroyed++;
                        Buildings[nr, nc] = null;
                    }
                }

                // 3. 서버 랙 장애물 타격
                if (MapTiles[nr, nc] == TileType.Obstacle && ObstacleHp[nr, nc] > 0)
                {
                    ObstacleHp[nr, nc] -= 2;
                    events.Add(new CombatEvent { Col = nc, Row = nr, Text = "-2 랙 폭발! 💣", TextColor = Color.FromArgb(255, 110, 64) });
                    if (ObstacleHp[nr, nc] <= 0)
                    {
                        MapTiles[nr, nc] = TileType.Plain;
                        StatsObstaclesDestroyed++;
                        if (dead.Owner == Owner.Player)
                        {
                            PlayerCredit += 2;
                            StatsTotalCreditEarned += 2;
                        }
                        else AiCredit += 2;
                        string rewardText = (dead.Owner == Owner.Player) ? "아군 랙 돌파! +2G ⚡" : "적 랙 돌파! (적 +2G) ⚡";
                        Color rewardCol = (dead.Owner == Owner.Player) ? Color.FromArgb(255, 215, 0) : Color.FromArgb(255, 110, 64);
                        events.Add(new CombatEvent { Col = nc, Row = nr, Text = rewardText, TextColor = rewardCol, IsCritical = true });
                    }
                }
            }
        }

        /// <summary>
        /// 플라즈마 드레드노트 특수 능력: 목표 지점 기준 상/하/좌/우 십자 4방향 스플래시 방전 데미지 (2뎀)
        /// </summary>
        private void ApplyCrossSplash(int centerCol, int centerRow, Owner opponent, Owner actingOwner, List<CombatEvent> events)
        {
            int splashDmg = 2;
            int[] dRow = { -1, 1, 0, 0 };
            int[] dCol = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int sc = centerCol + dCol[i];
                int sr = centerRow + dRow[i];
                if (sc <= 0 || sc >= Cols - 1 || sr < 0 || sr >= Rows) continue;

                // 1. 십자 범위 내 적 유닛
                var splashUnit = Units.Find(u => u.Col == sc && u.Row == sr && u.Owner == opponent && u.Hp > 0);
                if (splashUnit != null)
                {
                    splashUnit.Hp -= splashDmg;
                    splashUnit.HitFlashAlpha = 0.8f;
                    events.Add(new CombatEvent { Col = sc, Row = sr, Text = $"-{splashDmg} ⚡", TextColor = Color.FromArgb(0, 229, 255) });
                }

                // 2. 십자 범위 내 적 건물
                var splashBldg = Buildings[sr, sc];
                if (splashBldg != null && splashBldg.Owner == opponent && splashBldg.Hp > 0)
                {
                    splashBldg.Hp -= splashDmg;
                    splashBldg.HitFlashAlpha = 0.8f;
                    events.Add(new CombatEvent { Col = sc, Row = sr, Text = $"-{splashDmg} ⚡", TextColor = Color.FromArgb(0, 229, 255) });
                    if (splashBldg.Hp <= 0)
                    {
                        Buildings[sr, sc] = null;
                    }
                }

                // 3. 십자 범위 내 중립 장애물 랙
                if (MapTiles[sr, sc] == TileType.Obstacle)
                {
                    ObstacleHp[sr, sc] -= splashDmg;
                    events.Add(new CombatEvent { Col = sc, Row = sr, Text = $"-{splashDmg} ⚡", TextColor = Color.FromArgb(180, 200, 220) });
                    if (ObstacleHp[sr, sc] <= 0)
                    {
                        MapTiles[sr, sc] = TileType.Plain;
                        if (actingOwner == Owner.Player) PlayerCredit += 2;
                        else AiCredit += 2;

                        string rewardText = (actingOwner == Owner.Player) ? "아군 랙 돌파! +2G ⚡" : "적 랙 돌파! (적 +2G) ⚡";
                        Color rewardCol = (actingOwner == Owner.Player) ? Color.FromArgb(255, 215, 0) : Color.FromArgb(255, 110, 64);
                        events.Add(new CombatEvent
                        {
                            Col = sc,
                            Row = sr,
                            Text = rewardText,
                            TextColor = rewardCol,
                            IsCritical = true
                        });
                    }
                }
            }
        }

        #endregion

        #region Roguelike Protocol Card System (영구 누적 & 신규 유닛 적용)

        public void TriggerRoguelikeCardSelection()
        {
            IsCardSelectionActive = true;
            CurrentOfferedCards = GenerateRandomCards(3, Owner.Player);
            StatusMessage = "⚡ 전술 프로토콜 보급 도착! 3개 중 하나를 선택하십시오.";
        }

        public void SelectProtocolCard(ProtocolCard card)
        {
            if (card != null)
            {
                card.ApplyAction?.Invoke(this, Owner.Player);

                // AI도 랜덤하게 1개 버프 획득 (공정한 게임 밸런스)
                var aiPool = GenerateRandomCards(3, Owner.Ai);
                if (aiPool.Count > 0)
                {
                    aiPool[_rand.Next(aiPool.Count)].ApplyAction?.Invoke(this, Owner.Ai);
                }
            }

            IsCardSelectionActive = false;
            CurrentOfferedCards.Clear();
            StatusMessage = $"프로토콜 '{card?.Title}' 적용 완료! 턴 {TurnNumber}을 이어가세요.";
        }

        public List<ProtocolCard> GetAllProtocolCardTemplates()
        {
            // [카드 동적 스케일링 방안 A]: 턴 수에 따라 카드의 가치와 효과 스케일링 강화
            int ocCredit = (this.TurnNumber >= 12) ? 14 : 8;
            string ocTitle = (this.TurnNumber >= 12) ? "⚡ 초대형 오버클럭 펀딩" : "⚡ 오버클럭 펀딩";
            string ocDesc = $"국방 예산 크레딧 +{ocCredit}G를\n즉시 긴급 수급합니다.";

            int vCount = (this.TurnNumber >= 12) ? 4 : (this.TurnNumber >= 7 ? 3 : 2);
            int vAtk = (this.TurnNumber >= 12) ? 2 : 1;
            bool vShield = (this.TurnNumber >= 7);
            string vTitle = (this.TurnNumber >= 12) ? "👥 정예 보병 코만도 강습" : (this.TurnNumber >= 7 ? "👥 보병 강화 증원" : "👥 보병 기동 증원");
            string vSub = (this.TurnNumber >= 12) ? $"정예 보병 4기 (공{vAtk}, 실드)" : (this.TurnNumber >= 7 ? "보병 3기 (실드 장착)" : "기본 보병 2기 지정 배치");
            string vDesc = (this.TurnNumber >= 12)
                ? $"본진 진영(Col 1~3)에 정예 코만도 4기를\n즉시 무료 공수 투하합니다.\n• 공격력 2, 1회 실드 장착\n• 투하 정예 유닛에만 공격력 2 적용"
                : (this.TurnNumber >= 7
                    ? "본진 진영(Col 1~3)에 방패를 장착한\n보병 3기를 즉시 무료 공수합니다.\n• 소환 시 1회 방패 장착"
                    : "본진 진영(Col 1~3) 원하는 타일에\n기본 보병 2기를 즉시 무료 공수합니다.\n• 카드 선택 후 원하는 타일 클릭");

            int crHeal = (this.TurnNumber >= 12) ? 10 : 6;
            string crTitle = (this.TurnNumber >= 12) ? "🔧 대규모 코어 오버홀" : "🔧 긴급 코어 리부트";
            string crSub = $"본진 코어 HP +{crHeal} 즉시 수리";
            string crDesc = $"손상된 아군 메인프레임 코어를 복구하여\n본진 체력을 즉시 +{crHeal} 회복합니다.\n(최대 체력 한도 내 안전 수리)";

            int adCount = (this.TurnNumber >= 12) ? 3 : 2;
            int adMaxCol = (this.TurnNumber >= 12) ? 9 : 8;
            string adTitle = (this.TurnNumber >= 12) ? "🚀 대규모 스텔스 침투 공수" : "🚀 스텔스 침투 공수";
            string adSub = (this.TurnNumber >= 12) ? "전장 중앙/후방 3기 기습 투하" : "전장 중앙 기습 투하 (지정 배치)";
            string adDesc = $"전장 침투선(Col 4~{adMaxCol})에 고속 침투병\n{adCount}기를 원하는 위치에 공수 투하합니다.\n• 카드 선택 후 원하는 타일 클릭";

            return new List<ProtocolCard>
            {
                // ================= [COMMON: 일반 등급 (50%)] =================
                new ProtocolCard
                {
                    Id = "OVERCLOCK",
                    Title = ocTitle,
                    Subtitle = $"즉시 긴급 군자금 +{ocCredit}G",
                    Description = ocDesc,
                    AccentColor = Color.FromArgb(200, 215, 230),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerCredit += ocCredit;
                        else state.AiCredit += ocCredit;
                    }
                },
                new ProtocolCard
                {
                    Id = "VANGUARD_CALL",
                    Title = vTitle,
                    Subtitle = vSub,
                    Description = vDesc,
                    AccentColor = Color.FromArgb(100, 210, 255),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player)
                        {
                            state.PendingDropUnitType = UnitType.Vanguard;
                            state.PendingDropRemainingCount = vCount;
                            state.PendingDropMinCol = 1;
                            state.PendingDropMaxCol = 3;
                            state.PendingDropCustomAtk = (state.TurnNumber >= 12) ? 2 : 0;
                            state.PendingDropForceShield = vShield;
                            state.StatusMessage = $"⚡ 보병 {vCount}기를 배치할 위치를 클릭하십시오 (Col 1~3)";
                        }
                        else
                        {
                            int c = Cols - 2;
                            int spawnedCount = 0;
                            state.PendingDropCustomAtk = (state.TurnNumber >= 12) ? 2 : 0;
                            state.PendingDropForceShield = vShield;
                            for (int r = 0; r < Rows && spawnedCount < vCount; r++)
                            {
                                if (!state.Units.Exists(u => u.Col == c && u.Row == r) && state.Buildings[r, c]?.Type != BuildingType.Wall)
                                {
                                    state.SpawnUnitDirect(c, r, UnitType.Vanguard, Owner.Ai);
                                    spawnedCount++;
                                }
                            }
                            state.PendingDropCustomAtk = 0;
                            state.PendingDropForceShield = false;
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "CORE_REPAIR",
                    Title = crTitle,
                    Subtitle = crSub,
                    Description = crDesc,
                    AccentColor = Color.FromArgb(76, 175, 80),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerCoreHp = Math.Min(state.PlayerCoreMaxHp, state.PlayerCoreHp + crHeal);
                        else state.AiCoreHp = Math.Min(state.AiCoreMaxHp, state.AiCoreHp + crHeal);
                    }
                },
                new ProtocolCard
                {
                    Id = "FIELD_REPAIR",
                    Title = "🔧 긴급 나노 수리",
                    Subtitle = "전장 전원 체력 +3 회복",
                    Description = "현재 전장에 배치된 모든 아군 유닛과\n건물의 체력을 즉시 +3 수리합니다.",
                    AccentColor = Color.FromArgb(64, 196, 255),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        state.Units.FindAll(u => u.Owner == owner).ForEach(u => {
                            u.Hp = Math.Min(u.MaxHp, u.Hp + 3);
                            u.HitFlashAlpha = 0.6f;
                        });
                        for (int r = 0; r < Rows; r++)
                        {
                            for (int c = 0; c < Cols; c++)
                            {
                                var b = state.Buildings[r, c];
                                if (b != null && b.Owner == owner)
                                {
                                    b.Hp = Math.Min(b.MaxHp, b.Hp + 3);
                                    b.HitFlashAlpha = 0.6f;
                                }
                            }
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "GREEN_ENERGY",
                    Title = "🔋 초전도 국고 예산",
                    Subtitle = "턴당 기본 수입 영구 +1G",
                    Description = "매 턴 국고로 지급되는 기본 크레딧이\n영구적으로 +1G 증가합니다.\n(발전소 수와 무관하게 턴마다 영구 지급)",
                    AccentColor = Color.FromArgb(76, 175, 80),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerIncomeBonus += 1;
                        else state.AiIncomeBonus += 1;
                    }
                },
                new ProtocolCard
                {
                    Id = "BARRICADE_AIRDROP",
                    Title = "🧱 긴급 격벽 투하",
                    Subtitle = "전진 방화벽 즉시 공수",
                    Description = "전선 길목(Col 3)에 HP 8 방화벽을\n즉시 1기 무료 공수 건설합니다.",
                    AccentColor = Color.FromArgb(160, 180, 205),
                    Rarity = CardRarity.Common,
                    ApplyAction = (state, owner) =>
                    {
                        int dropCol = (owner == Owner.Player) ? 3 : Cols - 4;
                        for (int r = 2; r < Rows - 2; r++)
                        {
                            if (state.Buildings[r, dropCol] == null && state.MapTiles[r, dropCol] != TileType.Obstacle)
                            {
                                state.Buildings[r, dropCol] = new Building
                                {
                                    Owner = owner,
                                    Type = BuildingType.Wall,
                                    Hp = 8,
                                    MaxHp = 8,
                                    Col = dropCol,
                                    Row = r
                                };
                                break;
                            }
                        }
                    }
                },

                // ================= [RARE: 희귀 등급 (35%)] =================
                new ProtocolCard
                {
                    Id = "CHEONGUNG_TRAJECTORY",
                    Title = "🔭 탄도 궤적 정밀 연산",
                    Subtitle = "사거리 4->5 & 사각 3칸 (고유)",
                    Description = "천궁 정밀 탄도 연산 컴퓨터를 동기화합니다.\n• 최대 사거리 +1 (총 5칸 초장거리 포격)\n• 안전 최소 사거리 +1 (3칸 이내 근접 불가)",
                    AccentColor = Color.FromArgb(255, 87, 34),
                    Rarity = CardRarity.Rare,
                    MinTurn = 9,
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasCheongungTrajectory : !state.AiHasCheongungTrajectory),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasCheongungTrajectory = true;
                        else state.AiHasCheongungTrajectory = true;
                        state.Units.FindAll(u => u.Owner == owner && u.Type == UnitType.Cheongung).ForEach(u => {
                            u.Range = 5;
                            u.MinRange = 3;
                        });
                    }
                },
                new ProtocolCard
                {
                    Id = "CORE_EXPANSION",
                    Title = "🛡️ 메인프레임 코어 증설",
                    Subtitle = "코어 최대 HP +5 & 즉시 수리",
                    Description = "아군 메인프레임 코어 내구도를 증설합니다.\n• 최대 체력 영구 +5 확장\n• 현재 코어 HP +5 즉시 긴급 수리",
                    AccentColor = Color.FromArgb(0, 230, 118),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player)
                        {
                            state.PlayerCoreMaxHp += 5;
                            state.PlayerCoreHp = Math.Min(state.PlayerCoreMaxHp, state.PlayerCoreHp + 5);
                        }
                        else
                        {
                            state.AiCoreMaxHp += 5;
                            state.AiCoreHp = Math.Min(state.AiCoreMaxHp, state.AiCoreHp + 5);
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "NANO_ARMOR",
                    Title = "🛡️ 나노 합금 연구",
                    Subtitle = "생산 유닛 체력 영구 +2 (누적)",
                    Description = "향후 새로 생산되는 모든 아군 유닛의\n최대 체력이 영구적으로 +2 증가합니다.",
                    AccentColor = Color.FromArgb(0, 230, 118),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerUnitHpBonus += 2;
                        else state.AiUnitHpBonus += 2;
                    }
                },
                new ProtocolCard
                {
                    Id = "CYBER_DDOS",
                    Title = "👾 DDoS 사이버 교란",
                    Subtitle = "상대 수입 -1G & 자금 3G 강탈",
                    Description = "적 통신망을 마비시켜 수입을 억제합니다.\n• 상대 턴당 수입 영구 -1G 차감\n• 적 국고에서 3G 즉시 강탈",
                    AccentColor = Color.FromArgb(255, 61, 0),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        Owner enemy = (owner == Owner.Player) ? Owner.Ai : Owner.Player;
                        if (enemy == Owner.Ai)
                        {
                            state.AiIncomeBonus = Math.Max(-2, state.AiIncomeBonus - 1);
                            int stolen = Math.Min(3, state.AiCredit);
                            state.AiCredit -= stolen;
                            state.PlayerCredit += stolen;
                        }
                        else
                        {
                            state.PlayerIncomeBonus = Math.Max(-2, state.PlayerIncomeBonus - 1);
                            int stolen = Math.Min(3, state.PlayerCredit);
                            state.PlayerCredit -= stolen;
                            state.AiCredit += stolen;
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "RAILGUN_BATTERY",
                    Title = "🎯 레일건 포탑 개조",
                    Subtitle = "포탑 사거리 +1칸 (최대 2회)",
                    Description = "모든 포탑의 요격 사거리가\n+1칸(최대 4칸) 증가합니다.",
                    AccentColor = Color.FromArgb(0, 229, 255),
                    Rarity = CardRarity.Rare,
                    IsAvailable = (state, owner) => state.TurretRangeBonus < 2, // 최대 2회 도달 시 풀에서 제외
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.TurretRangeBonus += 1;
                    }
                },
                new ProtocolCard
                {
                    Id = "TURRET_RADAR",
                    Title = "📡 양자 레이더 망",
                    Subtitle = "포탑 은폐 감지 센서 (고유)",
                    Description = "모든 포탑에 양자 레이더를 장착하여\n은폐 침투병을 원거리에서 요격합니다.",
                    AccentColor = Color.FromArgb(0, 200, 255),
                    Rarity = CardRarity.Rare,
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasTurretRadar : !state.AiHasTurretRadar),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasTurretRadar = true;
                        else state.AiHasTurretRadar = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "PHALANX_SHIELD",
                    Title = "🛡️ 전술 방패 결진",
                    Subtitle = "보병 1회 피해 무효화 실드 (고유)",
                    Description = "향후 생산/증원되는 모든 보병이\n첫 1회 피해 무효화 실드를 장착합니다.",
                    AccentColor = Color.FromArgb(64, 196, 255),
                    Rarity = CardRarity.Rare,
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasPhalanxShield : !state.AiHasPhalanxShield),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasPhalanxShield = true;
                        else state.AiHasPhalanxShield = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "STEALTH_CAMOUFLAGE",
                    Title = "⚡ 광학 스텔스 클로킹",
                    Subtitle = "침투병 첫 공격 시까지 은폐 (고유)",
                    Description = "침투병이 적 앞 칸에 도달할 때까지\n원거리 포격 및 포탑 타깃팅을 회피합니다.",
                    AccentColor = Color.FromArgb(38, 198, 218),
                    Rarity = CardRarity.Rare,
                    MinTurn = 6, // 2차 카드(턴 6)부터 풀에 합류!
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasStealthCloak : !state.AiHasStealthCloak),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasStealthCloak = true;
                        else state.AiHasStealthCloak = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "COLOSSUS_SIEGE",
                    Title = "🔭 시즈 앵커 프로토콜",
                    Subtitle = "콜로서스 제자리 포격 체제 (고유)",
                    Description = "사거리(2~3칸) 내 타깃이 존재하면\n전진을 멈추고 제자리에서 포격합니다.",
                    AccentColor = Color.FromArgb(186, 104, 200),
                    Rarity = CardRarity.Rare,
                    MinTurn = 12, // 4차 카드(턴 12)부터 풀에 합류!
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasSiegeAnchor : !state.AiHasSiegeAnchor),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasSiegeAnchor = true;
                        else state.AiHasSiegeAnchor = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "COLOSSUS_MELEE",
                    Title = "⚔️ 근접 방어 발칸포",
                    Subtitle = "콜로서스 사각지대 해제 (고유)",
                    Description = "콜로서스에 근접 자동 발칸포를 탑재하여\n인접 1칸 근접 공격 및 반격이 가능해집니다.\n(최소 사거리 1칸으로 완화)",
                    AccentColor = Color.FromArgb(255, 179, 0),
                    Rarity = CardRarity.Rare,
                    MinTurn = 12, // 4차 카드(턴 12)부터 풀에 합류!
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasColossusMelee : !state.AiHasColossusMelee),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasColossusMelee = true;
                        else state.AiHasColossusMelee = true;
                        state.Units.FindAll(u => u.Owner == owner && u.Type == UnitType.Colossus).ForEach(u => u.MinRange = 1);
                    }
                },

                // ================= [EPIC: 에픽 등급 (15%)] =================
                new ProtocolCard
                {
                    Id = "GRID_EXPANSION",
                    Title = "⚡ 전력망 그리드 확장",
                    Subtitle = "발전소 한도 +2기 & 3G 환급",
                    Description = "발전소 보유 한도가 영구 +2기 확장되며\n즉시 3G 보조금을 지급받습니다.",
                    AccentColor = Color.FromArgb(255, 215, 0),
                    Rarity = CardRarity.Epic,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player)
                        {
                            state.PlayerPlantLimit += 2;
                            state.PlayerCredit += 3;
                        }
                        else
                        {
                            state.AiPlantLimit += 2;
                            state.AiCredit += 3;
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "AIR_DROP",
                    Title = adTitle,
                    Subtitle = adSub,
                    Description = adDesc,
                    AccentColor = Color.FromArgb(0, 229, 255),
                    Rarity = CardRarity.Epic,
                    MinTurn = 6, // 2차 카드(턴 6)부터 풀에 합류!
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player)
                        {
                            state.PendingDropUnitType = UnitType.Raider;
                            state.PendingDropRemainingCount = adCount;
                            state.PendingDropMinCol = 4;
                            state.PendingDropMaxCol = adMaxCol;
                            state.StatusMessage = $"🚀 침투병 {adCount}기를 공수 투하할 위치를 클릭하십시오 (Col 4~{adMaxCol})";
                        }
                        else
                        {
                            int targetCol = (state.TurnNumber >= 12) ? 6 : 7;
                            int spawnedCount = 0;
                            for (int r = 1; r < Rows && spawnedCount < adCount; r += 2)
                            {
                                if (!state.Units.Exists(u => u.Col == targetCol && u.Row == r) && state.MapTiles[r, targetCol] != TileType.Obstacle)
                                {
                                    state.SpawnUnitDirect(targetCol, r, UnitType.Raider, Owner.Ai);
                                    spawnedCount++;
                                }
                            }
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "ORBITAL_EMP",
                    Title = "💥 궤도 EMP 폭격",
                    Subtitle = "적 전선 타격 & 코어 수리",
                    Description = "모든 적 유닛에게 2 피해를 입히고\n아군 본진 코어를 +3 수리합니다.",
                    AccentColor = Color.FromArgb(255, 23, 68),
                    Rarity = CardRarity.Epic,
                    ApplyAction = (state, owner) =>
                    {
                        Owner enemy = (owner == Owner.Player) ? Owner.Ai : Owner.Player;
                        state.Units.FindAll(u => u.Owner == enemy).ForEach(u => {
                            if (u.HasShield)
                            {
                                u.HasShield = false;
                            }
                            else
                            {
                                u.Hp -= 2;
                            }
                            u.HitFlashAlpha = 1.0f;
                        });
                        state.Units.RemoveAll(u => u.Hp <= 0);
                        if (owner == Owner.Player) state.PlayerCoreHp = Math.Min(state.PlayerCoreMaxHp, state.PlayerCoreHp + 3);
                        else state.AiCoreHp = Math.Min(state.AiCoreMaxHp, state.AiCoreHp + 3);
                    }
                },
                new ProtocolCard
                {
                    Id = "TITAN_PROTOCOL",
                    Title = "⚙️ 타이탄 기동 명령",
                    Subtitle = "최종 결전 병기 무료 소환 (지정 배치)",
                    Description = "중장갑 결전 병기 '타이탄' 1기를\n원하는 본진 타일에 즉시 무료 소환합니다.\n(카드 선택 후 원하는 타일 클릭)",
                    AccentColor = Color.FromArgb(255, 110, 64),
                    Rarity = CardRarity.Epic,
                    MinTurn = 9, // 3차 카드(턴 9)부터 풀에 합류!
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player)
                        {
                            state.PendingDropUnitType = UnitType.Titan;
                            state.PendingDropRemainingCount = 1;
                            state.PendingDropMinCol = 1;
                            state.PendingDropMaxCol = 3;
                            state.StatusMessage = "⚙️ 타이탄 1기를 배치할 위치를 클릭하십시오 (Col 1~3)";
                        }
                        else
                        {
                            int c = Cols - 2;
                            for (int r = 0; r < Rows; r++)
                            {
                                if (!state.Units.Exists(u => u.Col == c && u.Row == r))
                                {
                                    state.SpawnUnitDirect(c, r, UnitType.Titan, Owner.Ai);
                                    break;
                                }
                            }
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "INFANTRY_ASSAULT",
                    Title = "🌊 전면 보병 상륙작전",
                    Subtitle = "코어 앞 전 행 보병 동시 상륙",
                    Description = "본진 제1열(Col 1)의 모든 빈 타일에\n정예 보병을 일제히 동시 상륙 배치합니다!\n• 보유 중인 보병 특성(HP/공/실드) 온전 적용",
                    AccentColor = Color.FromArgb(0, 229, 255),
                    Rarity = CardRarity.Epic,
                    MinTurn = 6, // 턴 6부터 등장
                    ApplyAction = (state, owner) =>
                    {
                        int targetCol = (owner == Owner.Player) ? 1 : state.Cols - 2;
                        for (int r = 0; r < state.Rows; r++)
                        {
                            if (state.ObstacleHp[r, targetCol] <= 0 && state.Buildings[r, targetCol] == null && !state.Units.Exists(u => u.Col == targetCol && u.Row == r))
                            {
                                state.SpawnUnitDirect(targetCol, r, UnitType.Vanguard, owner);
                            }
                        }
                    }
                },
                new ProtocolCard
                {
                    Id = "KINETIC_SLAM",
                    Title = "💥 운동에너지 충격파",
                    Subtitle = "타이탄 1칸 넉백 & 충돌 피해 (고유)",
                    Description = "타이탄 포격 적중 시 목표를 뒤로 1칸 밀쳐냅니다. 뒤에 유닛/건물/벽이 있어 밀려나지 못하면 양쪽 모두에게 충돌 데미지 1을 가합니다.",
                    AccentColor = Color.FromArgb(255, 138, 101),
                    Rarity = CardRarity.Epic,
                    MinTurn = 9, // 3차 카드(턴 9)부터 풀에 합류!
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasKineticSlam : !state.AiHasKineticSlam),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasKineticSlam = true;
                        else state.AiHasKineticSlam = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "CHAIN_PLASMA",
                    Title = "✦ 연쇄 플라즈마 방전",
                    Subtitle = "드레드노트 십자 4방향 스플래시 (고유)",
                    Description = "플라즈마 드레드노트 주 타격 시 상/하/좌/우 십자 4방향에 2 데미지 연쇄 방전 피해를 입힙니다.",
                    AccentColor = Color.FromArgb(0, 229, 255),
                    Rarity = CardRarity.Epic,
                    MinTurn = 12, // 4차 카드(턴 12)부터 풀에 합류!
                    IsAvailable = (state, owner) => (owner == Owner.Player ? !state.PlayerHasChainPlasma : !state.AiHasChainPlasma),
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerHasChainPlasma = true;
                        else state.AiHasChainPlasma = true;
                    }
                },
                new ProtocolCard
                {
                    Id = "QUANTUM_OVERCLOCK",
                    Title = "🔮 퀀텀 뉴럴 오버클럭",
                    Subtitle = "생산 유닛 공격력 영구 +1 (누적)",
                    Description = "향후 새로 생산되는 모든 아군 유닛의 공격력이 영구적으로 +1 증가합니다.",
                    AccentColor = Color.FromArgb(215, 60, 255),
                    Rarity = CardRarity.Epic,
                    ApplyAction = (state, owner) =>
                    {
                        if (owner == Owner.Player) state.PlayerUnitAtkBonus += 1;
                        else state.AiUnitAtkBonus += 1;
                    }
                },
                new ProtocolCard
                {
                    Id = "MARTYRDOM_CORE",
                    Title = "💣 최후의 자폭 코어",
                    Subtitle = "현재 배치된 아군 유닛 전원 자폭 장착",
                    Description = "현재 전장에 배치된 모든 아군 유닛에게\n자폭 폭탄을 장착합니다.\n• 유닛 사망 시 상하좌우 4방향 적에게 2 데미지\n• 아군 피해 없음 (적 기물/코어만 타격)",
                    AccentColor = Color.FromArgb(255, 23, 68),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        state.Units.FindAll(u => u.Owner == owner && u.Hp > 0).ForEach(u => u.HasBomb = true);
                    }
                },
                new ProtocolCard
                {
                    Id = "OVERCLOCK_REACTOR",
                    Title = "⚡ 전력과부하 리액터",
                    Subtitle = "발전소 1기 희생 ➔ 전 유닛 데미지 +1",
                    Description = "과부하 에너지를 모든 기동 병기에 주입합니다.\n• 보유 발전소 1기 무작위 폭파\n• 현재/향후 모든 아군 유닛 공격력 +1 영구 증가",
                    AccentColor = Color.FromArgb(255, 152, 0),
                    Rarity = CardRarity.Epic,
                    IsAvailable = (state, owner) => state.GetBuildingCount(BuildingType.PowerPlant, owner) >= 3,
                    ApplyAction = (state, owner) =>
                    {
                        var plants = new List<Building>();
                        for (int r = 0; r < state.Rows; r++)
                        {
                            for (int c = 0; c < state.Cols; c++)
                            {
                                var b = state.Buildings[r, c];
                                if (b != null && b.Owner == owner && b.Type == BuildingType.PowerPlant)
                                    plants.Add(b);
                            }
                        }
                        if (plants.Count > 0)
                        {
                            var targetPlant = plants[state._rand.Next(plants.Count)];
                            state.Buildings[targetPlant.Row, targetPlant.Col] = null;
                        }
                        if (owner == Owner.Player) state.PlayerUnitAtkBonus += 1;
                        else state.AiUnitAtkBonus += 1;
                        state.Units.FindAll(u => u.Owner == owner).ForEach(u => u.Atk += 1);
                    }
                },
                new ProtocolCard
                {
                    Id = "HEALING_BEAM_PROTOCOL",
                    Title = "💚 전술 치료 광선",
                    Subtitle = "위기 행 아군 유닛 체력 +4 회복",
                    Description = "고출력 나노 회복 빔을 투사하여\n가장 큰 피해를 입은 행의 모든 아군 유닛을 +4 수리합니다.\n(최대 체력 초과 불가)",
                    AccentColor = Color.FromArgb(0, 230, 118),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        int bestRow = 0;
                        int maxWounded = -1;
                        for (int r = 0; r < state.Rows; r++)
                        {
                            int count = state.Units.FindAll(u => u.Owner == owner && u.Row == r && u.Hp < u.MaxHp).Count;
                            if (count > maxWounded)
                            {
                                maxWounded = count;
                                bestRow = r;
                            }
                        }
                        state.CastSkill(UserSkillType.HealingBeam, 0, bestRow, owner);
                    }
                },
                new ProtocolCard
                {
                    Id = "FORTIFY_BEAM_PROTOCOL",
                    Title = "🛡️ 전술 구축 광선",
                    Subtitle = "주력 행 아군 유닛 최대 체력 +2 & 회복 +2",
                    Description = "영구 방호 장갑 필드를 전개하여\n주력 행의 모든 아군 유닛 내구도를 증강합니다.\n• 최대 체력 +2 & 현재 체력 +2",
                    AccentColor = Color.FromArgb(0, 229, 255),
                    Rarity = CardRarity.Rare,
                    ApplyAction = (state, owner) =>
                    {
                        int bestRow = 0;
                        int maxUnits = -1;
                        for (int r = 0; r < state.Rows; r++)
                        {
                            int count = state.Units.FindAll(u => u.Owner == owner && u.Row == r && u.Hp > 0).Count;
                            if (count > maxUnits)
                            {
                                maxUnits = count;
                                bestRow = r;
                            }
                        }
                        state.CastSkill(UserSkillType.FortifyBeam, 0, bestRow, owner);
                    }
                }
            };
        }

        public List<ProtocolCard> GenerateRandomCardsDirect(int count, Owner targetOwner = Owner.Player) => GenerateRandomCards(count, targetOwner);

        private List<ProtocolCard> GenerateRandomCards(int count, Owner targetOwner = Owner.Player)
        {
            var allCards = GetAllProtocolCardTemplates();
            // 획득 가능 조건 만족 카드만 1차 필터링:
            // 1) 턴 수에 따른 유닛 티어 단계적 해금 (보병: 턴3~, 침투병: 턴6~, 타이탄: 턴9~, 콜로서스/드레드노트: 턴12~)
            // 2) 한도 도달 및 이미 해금된 고유 기술 제외
            var availableCards = allCards.FindAll(c => 
                this.TurnNumber >= c.MinTurn && 
                (c.IsAvailable == null || c.IsAvailable(this, targetOwner))
            );
            var result = new List<ProtocolCard>();

            // 가중치 확률 추첨: Common 50%, Rare 35%, Epic 15%
            for (int i = 0; i < count; i++)
            {
                int roll = _rand.Next(100); // 0 ~ 99
                CardRarity targetRarity;
                if (roll < 50) targetRarity = CardRarity.Common;       // 50%
                else if (roll < 85) targetRarity = CardRarity.Rare;    // 35%
                else targetRarity = CardRarity.Epic;                   // 15%

                // 후보군 필터링 (이미 선택지에 추가된 카드 중복 제외)
                var candidates = availableCards.FindAll(c => c.Rarity == targetRarity && !result.Exists(r => r.Id == c.Id));

                // 만약 해당 등급 풀이 고갈되었으면 다른 등급의 사용 가능한 미선택 카드에서 대체
                if (candidates.Count == 0)
                {
                    candidates = availableCards.FindAll(c => !result.Exists(r => r.Id == c.Id));
                }

                if (candidates.Count > 0)
                {
                    int pickIdx = _rand.Next(candidates.Count);
                    result.Add(candidates[pickIdx]);
                }
            }

            return result;
        }

        #endregion

        #region AI Strategic Decisions (3대 전술 페르소나 & 인간적인 확률적 지능)

        public void ExecuteAiDecisions()
        {
            // [전황 분석]
            var playerUnits = Units.FindAll(u => u.Owner == Owner.Player && u.Hp > 0);
            int dangerRow = -1;
            int maxPlayerCol = -1;

            foreach (var pu in playerUnits)
            {
                if (pu.Col > maxPlayerCol)
                {
                    maxPlayerCol = pu.Col;
                    dangerRow = pu.Row;
                }
            }

            // 플레이어가 유닛을 하나도 두지 않은 빈 라인 탐색 (기습 침투 대상)
            var emptyRows = new List<int>();
            for (int r = 0; r < Rows; r++)
            {
                if (!playerUnits.Exists(u => u.Row == r))
                {
                    emptyRows.Add(r);
                }
            }

            bool hasPlayerDreadnought = playerUnits.Exists(u => u.Type == UnitType.Dreadnought);
            bool threatDetected = maxPlayerCol >= Cols - 6; // 아군 진영 접근 위협

            // 1. [내정 체크] AI 페르소나별 발전소 투자 비중
            int aiPlantCount = GetBuildingCount(BuildingType.PowerPlant, Owner.Ai);
            int aiPlantLimit = GetPlantLimit(Owner.Ai);
            int plantCost = GetBuildingCost(BuildingType.PowerPlant, Owner.Ai);

            int targetPlants = (CurrentAiPersona == AiStrategyPersona.Rush) ? 2 :
                               (CurrentAiPersona == AiStrategyPersona.Siege) ? 3 :
                               aiPlantLimit;

            // 자금 넉넉 시 전력망 증설 (Tech형은 적극적, Rush형은 자제)
            if (CurrentAiPersona == AiStrategyPersona.Tech && aiPlantCount >= aiPlantLimit && AiCredit >= GetGridExpandCost(Owner.Ai) + 6)
            {
                UpgradePowerGrid(Owner.Ai);
                aiPlantLimit = GetPlantLimit(Owner.Ai);
                targetPlants = aiPlantLimit;
            }

            if (aiPlantCount < targetPlants && AiCredit >= plantCost)
            {
                // 실제 무작위 배치된 아군 진영 부근(Col 8~10) 중립 자원 노드 선점 시도 (+3G 수입 획득)
                bool builtOnResource = false;
                for (int c = Cols - 4; c >= 8; c--)
                {
                    for (int r = 0; r < Rows; r++)
                    {
                        if (MapTiles[r, c] == TileType.Resource && IsValidBuildingPlacement(c, r, Owner.Ai))
                        {
                            Build(c, r, BuildingType.PowerPlant, Owner.Ai);
                            builtOnResource = true;
                            break;
                        }
                    }
                    if (builtOnResource) break;
                }

                // 자원 노드가 없거나 건설 불가 시 안전한 아군 베이스라인에 배치
                if (!builtOnResource)
                {
                    for (int c = Cols - 2; c >= Cols - 3; c--)
                    {
                        for (int r = 0; r < Rows; r++)
                        {
                            if (IsValidBuildingPlacement(c, r, Owner.Ai))
                            {
                                Build(c, r, BuildingType.PowerPlant, Owner.Ai);
                                break;
                            }
                        }
                        if (GetBuildingCount(BuildingType.PowerPlant, Owner.Ai) >= targetPlants) break;
                    }
                }
            }

            // 2. [수비 체크] 13칸 포탑 요새화 (인간미 있는 확률적 수비)
            int turretCost = GetBuildingCost(BuildingType.Turret, Owner.Ai);
            if (threatDetected && AiCredit >= turretCost)
            {
                // 65% 확률로 수비 거점(포탑) 건설, 35%는 유닛 맞대결이나 다른 행동 선택
                int defRoll = _rand.Next(100);
                if (defRoll < 65)
                {
                    int defRow = (dangerRow >= 0) ? dangerRow : _rand.Next(Rows);
                    // dangerRow 또는 바로 옆 줄(±1)에 포탑 배치하여 13칸 방어망 형성
                    int[] rowCandidates = { defRow, Math.Max(0, defRow - 1), Math.Min(Rows - 1, defRow + 1) };
                    foreach (int r in rowCandidates)
                    {
                        if (IsValidBuildingPlacement(Cols - 3, r, Owner.Ai))
                        {
                            Build(Cols - 3, r, BuildingType.Turret, Owner.Ai);
                            break;
                        }
                    }
                }
            }

            // 3. [병력 생산 & 라인 배치] 상성 카운터 + 페르소나 + 확률적 실수
            while (AiCredit >= 2)
            {
                UnitType spawnType = UnitType.Vanguard;
                int pickRoll = _rand.Next(100);

                // (A) 상성 카운터: 플레이어가 드레드노트를 꺼냈을 때 (소형 유닛 카운터)
                if (hasPlayerDreadnought && pickRoll < 45)
                {
                    // 45% 확률로 드레드노트의 약점인 침투병(4G)이나 보병(2G)으로 다굴 픽
                    spawnType = (AiCredit >= 4 && _rand.Next(100) < 60) ? UnitType.Raider : UnitType.Vanguard;
                }
                // (B) 엉뚱한 실수 (15% 확률): 돈이 많은데도 보병 1기만 뽑고 끝내거나 침투병 엉뚱 소환
                else if (pickRoll < 15)
                {
                    spawnType = (AiCredit >= 4 && _rand.Next(100) < 50) ? UnitType.Raider : UnitType.Vanguard;
                }
                // (C) AI 페르소나 주력 병력
                else
                {
                    switch (CurrentAiPersona)
                    {
                        case AiStrategyPersona.Rush:
                            if (AiCredit >= 10 && _rand.Next(100) < 25) spawnType = UnitType.Titan;
                            else if (AiCredit >= 4 && _rand.Next(100) < 65) spawnType = UnitType.Raider;
                            else spawnType = UnitType.Vanguard;
                            break;

                        case AiStrategyPersona.Siege:
                            if (AiCredit >= 14 && _rand.Next(100) < 35) spawnType = UnitType.Colossus;
                            else if (AiCredit >= 12 && _rand.Next(100) < 35) spawnType = UnitType.Cheongung;
                            else if (AiCredit >= 10 && _rand.Next(100) < 35) spawnType = UnitType.Titan;
                            else if (AiCredit >= 4 && _rand.Next(100) < 40) spawnType = UnitType.Raider;
                            else spawnType = UnitType.Vanguard;
                            break;

                        case AiStrategyPersona.Tech:
                        default:
                            if (AiCredit >= 18 && _rand.Next(100) < 30) spawnType = UnitType.Dreadnought;
                            else if (AiCredit >= 14 && _rand.Next(100) < 30) spawnType = UnitType.Colossus;
                            else if (AiCredit >= 12 && _rand.Next(100) < 25) spawnType = UnitType.Cheongung;
                            else if (AiCredit >= 10 && _rand.Next(100) < 45) spawnType = UnitType.Titan;
                            else if (AiCredit >= 4 && _rand.Next(100) < 40) spawnType = UnitType.Raider;
                            else spawnType = UnitType.Vanguard;
                            break;
                    }
                }

                int cost = GetUnitCost(spawnType);
                if (AiCredit < cost)
                {
                    // 만약 비싼 걸 뽑으려다 돈이 부족하면 보병으로 대체
                    spawnType = UnitType.Vanguard;
                    cost = GetUnitCost(spawnType);
                    if (AiCredit < cost) break;
                }

                // (D) 소환 라인(Row) 선택: 맞대응(60%) vs 빈집기습(25%) vs 무작위(15%)
                int lineChoiceRoll = _rand.Next(100);
                List<int> preferredRows = new List<int>();

                if (lineChoiceRoll < 60 && dangerRow >= 0)
                {
                    // 1순위: 적 주력이 오는 라인 및 양옆 라인 (정면 저지선 형성)
                    preferredRows.Add(dangerRow);
                    if (dangerRow - 1 >= 0) preferredRows.Add(dangerRow - 1);
                    if (dangerRow + 1 < Rows) preferredRows.Add(dangerRow + 1);
                }
                else if (lineChoiceRoll < 85 && emptyRows.Count > 0)
                {
                    // 2순위: 적이 비워둔 라인으로 기습 돌파
                    preferredRows.AddRange(emptyRows);
                }

                // 나머지 행들도 후순위 추가
                for (int r = 0; r < Rows; r++)
                {
                    if (!preferredRows.Contains(r)) preferredRows.Add(r);
                }

                bool spawned = false;
                foreach (int r in preferredRows)
                {
                    if (IsValidUnitSpawnPlacement(Cols - 2, r, Owner.Ai))
                    {
                        spawned = SpawnUnit(Cols - 2, r, spawnType, Owner.Ai);
                        if (spawned) break;
                    }
                    else if (IsValidUnitSpawnPlacement(Cols - 3, r, Owner.Ai))
                    {
                        spawned = SpawnUnit(Cols - 3, r, spawnType, Owner.Ai);
                        if (spawned) break;
                    }
                }

                if (!spawned) break;
            }
        }

        #endregion
    }
}
