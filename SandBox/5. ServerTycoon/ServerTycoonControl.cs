using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace GateHelper.ServerTycoon
{
    public partial class ServerTycoonControl : UserControl
    {
        private TycoonGameState _state;
        private Timer _renderTimer;
        private Timer _logicTimer;
        private Random _rand = new Random();

        // 렌더링 파티클 및 애니메이션 리스트
        private List<FloatingText> _floatingTexts = new List<FloatingText>();
        private List<PacketParticle> _particles = new List<PacketParticle>();
        private List<Shockwave> _shockwaves = new List<Shockwave>();

        private float _fanAngle = 0f;
        private int _frameCounter = 0;
        private int _incidentSpawnCooldown = 20;
        private int _buyMultiplier = 1; // 1, 10, -1(MAX)

        // 7대 하단 서브 탭: 0=서버, 1=전력, 2=냉각, 3=S/W개발, 4=B2B수주, 5=코인거래, 6=연구/환생
        private int _activeSubTab = 0;
        private Rectangle[] _subTabBounds = new Rectangle[7];

        // UI 영역 캐시
        private Rectangle[] _rackBounds = new Rectangle[6];
        private Rectangle _deployButtonRect;
        private Rectangle _feverBarRect;
        private Rectangle _buy1Rect, _buy10Rect, _buyMaxRect;
        private Rectangle _backButtonRect;

        // 아이템 카드 및 구매 버튼 캐시
        private Rectangle[] _cardBounds = new Rectangle[6];
        private Rectangle[] _cardBuyButtonBounds = new Rectangle[6];
        private Rectangle _prestigeActionBtnRect;

        // S/W 스튜디오 인터랙션 영역
        private Rectangle[] _swCategoryBounds = new Rectangle[5];
        private Rectangle _swUxMinusRect, _swUxPlusRect;
        private Rectangle _swAlgoMinusRect, _swAlgoPlusRect;
        private Rectangle _swSecMinusRect, _swSecPlusRect;
        private Rectangle _swStartDevBtnRect;
        private Rectangle _swCrunchBtnRect;
        private Rectangle _swLaunchBtnRect;

        // B2B 수주 인터랙션 영역
        private Rectangle[] _b2bOfferBounds = new Rectangle[3];
        private Rectangle[] _b2bAcceptBtnBounds = new Rectangle[3];
        private Rectangle _b2bCancelBtnRect;
        private Rectangle _b2bRefreshBtnRect;

        // 코인 거래소 인터랙션 영역
        private Rectangle _cryptoHash0Rect, _cryptoHash25Rect, _cryptoHash50Rect, _cryptoHash100Rect;
        private Rectangle _cryptoSell50BtnRect, _cryptoSellAllBtnRect;
        private Rectangle _cryptoBuy1kBtnRect, _cryptoBuyMaxBtnRect;

        // 마우스 호버 인덱스
        private int _hoveredRackIndex = -1;
        private int _hoveredCardIndex = -1;
        private bool _isDeployHovered = false;

        // 오프라인 보상 배너 상태
        private bool _showOfflineBanner = false;
        private string _offlineBannerText = "";
        private Rectangle _offlineBannerRect;
        private Rectangle _offlineClaimButtonRect;

        public ServerTycoonControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);

            _state = TycoonGameState.LoadOrCreate();

            // 1. 오프라인 방치 보상 계산 (기본 2시간 상한 적용)
            double offlineEarned = _state.ProcessOfflineProgress(out double elapsedSecs);
            if (offlineEarned > 0)
            {
                TimeSpan span = TimeSpan.FromSeconds(elapsedSecs);
                string timeStr = span.TotalHours >= 1 
                    ? $"{(int)span.TotalHours}시간 {span.Minutes}분" 
                    : $"{span.Minutes}분 {span.Seconds}초";
                double capHours = _state.GetOfflineCapSeconds() / 3600.0;
                _offlineBannerText = $"[부재 중 서버 운영 보고] 자리 비운 시간: {timeStr} (최대 {capHours:F1}시간 인정) -> +${offlineEarned:N1} 획득!";
                _showOfflineBanner = true;
            }

            // 2. 렌더링 타이머 (30ms, 약 33fps)
            _renderTimer = new Timer { Interval = 30 };
            _renderTimer.Tick += RenderTimer_Tick;

            // 3. 로직 타이머 (1초 틱)
            _logicTimer = new Timer { Interval = 1000 };
            _logicTimer.Tick += LogicTimer_Tick;

            _renderTimer.Start();
            _logicTimer.Start();
        }

        public void OnTabVisibilityChanged(bool isVisible)
        {
            if (isVisible)
            {
                if (!_renderTimer.Enabled) _renderTimer.Start();
                if (!_logicTimer.Enabled) _logicTimer.Start();
                Invalidate();
            }
            else
            {
                _renderTimer.Stop();
                _state.Save();
            }
        }

        #region Game Loops (Logic & Render)

        private void LogicTimer_Tick(object sender, EventArgs e)
        {
            // 1. 초당 자동 수입 적립 (서버 수익 + S/W 로열티, 코인 채굴 비중 차감)
            double rps = _state.CalculateTotalRps();
            double hashRatio = (_state.CryptoMarket != null) ? (_state.CryptoMarket.MiningHashPercent / 100.0) : 0.0;
            double cashFromServers = rps * (1.0 - hashRatio);
            double royalties = _state.GetSoftwareRoyaltyRps();
            double totalIncome = cashFromServers + royalties;

            if (totalIncome > 0)
            {
                _state.Money += totalIncome;
                _state.CurrentRunEarned += totalIncome;
                _state.TotalEarned += totalIncome;
            }

            // 가상 코인 채굴
            if (hashRatio > 0 && rps > 0 && _state.CryptoMarket != null)
            {
                double mined = (rps * hashRatio) / Math.Max(20.0, _state.CryptoMarket.CurrentPrice * 2.2);
                _state.CryptoMarket.MinedCoins += mined;
                _state.CryptoMarket.TotalMinedCoins += mined;
            }

            // S/W 스튜디오 개발 진척 및 라이선스 감쇄
            if (_state.SoftwareStudio?.CurrentProject != null)
            {
                var prj = _state.SoftwareStudio.CurrentProject;
                if (!prj.IsCompleted)
                {
                    double devSpeed = Math.Max(25.0, rps * 0.45);
                    prj.CurrentPoints = Math.Min(prj.TargetPoints, prj.CurrentPoints + devSpeed);
                }
            }
            if (_state.SoftwareStudio?.ReleasedProducts != null)
            {
                for (int i = _state.SoftwareStudio.ReleasedProducts.Count - 1; i >= 0; i--)
                {
                    var rel = _state.SoftwareStudio.ReleasedProducts[i];
                    rel.RemainingSecs--;
                    rel.TotalEarned += rel.RoyaltyPerSec;
                    if (rel.RemainingSecs <= 0)
                    {
                        _state.SoftwareStudio.ReleasedProducts.RemoveAt(i);
                    }
                }
            }

            // B2B 수주 계약 이행 및 SLA 검증
            if (_state.B2BMarket != null)
            {
                if (_state.B2BMarket.ActiveContract != null)
                {
                    var contract = _state.B2BMarket.ActiveContract;
                    contract.RemainingSecs--;

                    bool violation = false;
                    if (contract.SlaType == SlaRequirementType.ThermalStrict && _state.GetCurrentTemperature() > 45.0)
                        violation = true;
                    else if (contract.SlaType == SlaRequirementType.PowerUptime && _state.GetTotalPowerDemand() > _state.GetTotalPowerSupply())
                        violation = true;
                    else if (contract.SlaType == SlaRequirementType.HighBandwidth && rps < contract.RequiredRps * 0.75)
                        violation = true;

                    if (violation) contract.ViolationsCount++;

                    if (contract.RemainingSecs <= 0)
                    {
                        if (contract.ViolationsCount <= 3)
                        {
                            _state.Money += contract.RewardCash;
                            _state.CurrentRunEarned += contract.RewardCash;
                            _state.TotalEarned += contract.RewardCash;
                            _state.B2BMarket.SuccessfulContractsCount++;
                            _state.B2BMarket.AddReputation(15);
                            AddFloatingText($"★ B2B 납기 완수! +${contract.RewardCash:N0} (신용도 ↑)", Width / 2f, 130, Color.FromArgb(0, 255, 128), true);
                        }
                        else
                        {
                            double penalty = Math.Min(_state.Money, contract.PenaltyCash);
                            _state.Money -= penalty;
                            _state.B2BMarket.AddReputation(-10);
                            AddFloatingText($"▲ SLA 위반 계약 파기! -${penalty:N0}", Width / 2f, 130, Color.FromArgb(255, 82, 82), true);
                        }
                        _state.B2BMarket.ActiveContract = null;
                        _state.B2BMarket.RefreshOffers(_rand, rps);
                    }
                }
                else
                {
                    _state.B2BMarket.RefreshCooldownSecs--;
                    if (_state.B2BMarket.RefreshCooldownSecs <= 0)
                    {
                        _state.B2BMarket.RefreshOffers(_rand, rps);
                    }
                }
            }

            // 가상 코인 시세 및 뉴스 틱
            if (_state.CryptoMarket != null)
            {
                var crypto = _state.CryptoMarket;
                crypto.InitHistory();

                crypto.NewsCooldownSecs--;
                if (crypto.NewsCooldownSecs <= 0)
                {
                    crypto.NewsCooldownSecs = _rand.Next(15, 30);
                    int newsRoll = _rand.Next(0, 5);
                    switch (newsRoll)
                    {
                        case 0:
                            crypto.MarketNews = "[속보] 대형 기관 투자자, ByteCoin 전략 매수 유입!";
                            crypto.TargetPrice = Math.Min(3500.0, crypto.CurrentPrice * (1.20 + _rand.NextDouble() * 0.15));
                            break;
                        case 1:
                            crypto.MarketNews = "[경보] 해외 거래소 점검 루머로 차익 실현 매물 출회!";
                            crypto.TargetPrice = Math.Max(25.0, crypto.CurrentPrice * (0.82 - _rand.NextDouble() * 0.08));
                            break;
                        case 2:
                            crypto.MarketNews = "[호재] 글로벌 클라우드 IDC 협의체, 공식 결제 화폐 채택!";
                            crypto.TargetPrice = Math.Min(3500.0, crypto.CurrentPrice * (1.15 + _rand.NextDouble() * 0.12));
                            break;
                        case 3:
                            crypto.MarketNews = "[조정] 급등세 이후 횡보 및 매물 소화 국면.";
                            crypto.TargetPrice = Math.Max(25.0, crypto.CurrentPrice * 0.92);
                            break;
                        default:
                            crypto.MarketNews = "[동향] 온체인 채굴 해시레이트 견조 유지 중.";
                            crypto.TargetPrice = crypto.CurrentPrice * (0.97 + _rand.NextDouble() * 0.06);
                            break;
                    }
                }

                double diff = crypto.TargetPrice - crypto.CurrentPrice;
                double step = diff * 0.18 + (_rand.NextDouble() - 0.5) * (crypto.CurrentPrice * 0.025);
                crypto.CurrentPrice = Math.Max(15.0, Math.Round(crypto.CurrentPrice + step, 2));

                crypto.PriceHistory.Add(crypto.CurrentPrice);
                if (crypto.PriceHistory.Count > 35) crypto.PriceHistory.RemoveAt(0);

                double firstP = crypto.PriceHistory[0];
                crypto.PriceChangePercent = firstP > 0 ? ((crypto.CurrentPrice - firstP) / firstP) * 100.0 : 0.0;
            }

            // 2. 피버 타이머 감쇄
            if (_state.IsFeverActive)
            {
                _state.FeverRemainingSeconds -= 1.0f;
                if (_state.FeverRemainingSeconds <= 0)
                {
                    _state.IsFeverActive = false;
                    _state.FeverMeter = 0f;
                    AddFloatingText("FEVER 종료", Width / 2f, 180, Color.FromArgb(255, 170, 0), false);
                }
            }
            else if (_state.FeverMeter > 0)
            {
                _state.FeverMeter = Math.Max(0f, _state.FeverMeter - 2.5f);
            }

            // 3. 돌발 장애 이벤트 관리 (AI DevOps Lv.3 이상 시 드론 자동 진압)
            if (_state.CurrentIncident != null)
            {
                _state.CurrentIncident.RemainingSeconds -= 1.0f;

                int aiLvl = _state.GetSkillLevel("skill_ai_devops");
                if (aiLvl >= 3 && _state.CurrentIncident.RemainingSeconds <= 8f)
                {
                    // AI 자동 복구 드론 발동
                    double rewardMult = 1.0 + (aiLvl * 0.30);
                    double reward = Math.Round(_state.CurrentIncident.RewardCash * rewardMult, 0);
                    _state.Money += reward;
                    _state.CurrentRunEarned += reward;
                    _state.TotalEarned += reward;
                    AddFloatingText($"[AI DevOps] 드론 자동 복구! +${reward:N0}", Width / 2f, 120, Color.FromArgb(0, 229, 255), true);
                    _state.CurrentIncident = null;
                }
                else if (_state.CurrentIncident.RemainingSeconds <= 0)
                {
                    _state.CurrentIncident = null;
                }
            }
            else
            {
                _incidentSpawnCooldown--;
                if (_incidentSpawnCooldown <= 0)
                {
                    _incidentSpawnCooldown = _rand.Next(25, 55);
                    int maxUnlocked = 0;
                    for (int i = 0; i < _state.Servers.Count; i++)
                    {
                        if (_state.Servers[i].Level > 0) maxUnlocked = i;
                    }
                    if (_state.GetTotalServerCount() > 0)
                    {
                        _state.CurrentIncident = TycoonIncident.GenerateRandom(maxUnlocked, _rand, rps);
                    }
                }
            }

            // 4. 10초마다 자동 영구 저장
            if (_frameCounter % 10 == 0)
            {
                _state.Save();
            }
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            _frameCounter++;
            _fanAngle = (_fanAngle + 12f) % 360f;

            // 1. 패킷 파티클 업데이트
            UpdateParticles();

            // 2. 플로팅 텍스트 업데이트
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var ft = _floatingTexts[i];
                ft.Y += ft.VelocityY;
                ft.Alpha -= 0.035f;
                if (ft.Alpha <= 0f)
                {
                    _floatingTexts.RemoveAt(i);
                }
            }

            // 3. 충격파 링 업데이트
            for (int i = _shockwaves.Count - 1; i >= 0; i--)
            {
                var sw = _shockwaves[i];
                sw.Radius += 3.5f;
                sw.Alpha -= 0.05f;
                if (sw.Alpha <= 0f || sw.Radius >= sw.MaxRadius)
                {
                    _shockwaves.RemoveAt(i);
                }
            }

            Invalidate();
        }

        private void UpdateParticles()
        {
            if (_frameCounter % 5 == 0)
            {
                float hubX = Width / 2f;
                float hubY = 195f;

                for (int i = 0; i < _state.Servers.Count; i++)
                {
                    var s = _state.Servers[i];
                    if (s.Level > 0 && i < _rackBounds.Length)
                    {
                        var rect = _rackBounds[i];
                        float startX = rect.X + rect.Width / 2f;
                        float startY = rect.Bottom - 6;

                        if (_rand.Next(0, 100) < Math.Min(95, 30 + s.Level * 4))
                        {
                            _particles.Add(new PacketParticle
                            {
                                StartX = startX,
                                StartY = startY,
                                X = startX,
                                Y = startY,
                                TargetX = hubX,
                                TargetY = hubY,
                                Progress = 0f,
                                Speed = 0.035f + (float)_rand.NextDouble() * 0.025f,
                                ParticleColor = s.AccentColor,
                                ServerIndex = i
                            });
                        }
                    }
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Progress += p.Speed;
                p.X = p.StartX + (p.TargetX - p.StartX) * p.Progress;
                p.Y = p.StartY + (p.TargetY - p.StartY) * p.Progress;

                if (p.Progress >= 1f)
                {
                    _particles.RemoveAt(i);
                }
            }

            if (_particles.Count > 100)
            {
                _particles.RemoveRange(0, _particles.Count - 100);
            }
        }

        #endregion

        #region Rendering (GDI+ Canvas)

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;

            // 1. 전체 배경
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(15, 17, 23)))
            {
                g.FillRectangle(bgBrush, 0, 0, w, h);
            }

            // 2. 상단 서버실 모니터링 뷰 (Y: 0 ~ 205)
            DrawServerRoom(g, w, 205);

            // 3. 중앙 조작부 & HUD (Y: 205 ~ 320)
            DrawCenterConsole(g, w, 205, 115);

            // 4. 하단 4대 인프라 서브 탭 바 (Y: 320 ~ 348)
            DrawSubTabBar(g, w, 320, 28);

            // 5. 하단 탭별 상점 및 연구 뷰 (Y: 348 ~ h)
            DrawActiveTabContent(g, w, 348, h - 348);

            // 6. 오프라인 방치 보상 배너
            if (_showOfflineBanner)
            {
                DrawOfflineBanner(g, w);
            }

            // 7. 플로팅 텍스트
            DrawFloatingTexts(g);
        }

        private void DrawServerRoom(Graphics g, int w, int roomHeight)
        {
            using (LinearGradientBrush roomBg = new LinearGradientBrush(
                new Point(0, 0), new Point(0, roomHeight),
                Color.FromArgb(20, 24, 34), Color.FromArgb(12, 14, 20)))
            {
                g.FillRectangle(roomBg, 0, 0, w, roomHeight);
            }

            using (Pen borderPen = new Pen(Color.FromArgb(40, 48, 64), 1))
            {
                g.DrawLine(borderPen, 0, roomHeight, w, roomHeight);
            }

            // 바닥 그리드
            using (Pen floorPen = new Pen(Color.FromArgb(22, 28, 38), 1))
            {
                int floorY = 150;
                for (int x = -100; x < w + 100; x += 45)
                {
                    g.DrawLine(floorPen, x, floorY, x + (x - w / 2) * 2, roomHeight);
                }
                g.DrawLine(floorPen, 0, floorY, w, floorY);
            }

            // 상단 타이틀
            using (Font titleFont = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 215, 235)))
            {
                string prestigeTag = _state.PrestigeCount > 0 ? $" [환생 +{_state.PrestigeCount}회]" : "";
                g.DrawString($"▣ 내 서버실 통합 현황판{prestigeTag}", titleFont, titleBrush, 12, 10);
            }

            // 상단 미니 실시간 텔레메트리 (전력 & 온도)
            double supply = _state.GetTotalPowerSupply();
            double demand = _state.GetTotalPowerDemand();
            double temp = _state.GetCurrentTemperature();
            bool isOverheated = temp > 60.0;
            bool isOptimal = temp <= 35.0;

            using (Font teleFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (SolidBrush pwrBrush = new SolidBrush(demand > supply ? Color.FromArgb(255, 82, 82) : Color.FromArgb(255, 167, 38)))
            using (SolidBrush tempBrush = new SolidBrush(isOverheated ? Color.FromArgb(255, 82, 82) : (isOptimal ? Color.FromArgb(0, 229, 255) : Color.FromArgb(76, 175, 80))))
            {
                string pwrText = $"⚡ {demand:N0}W / {supply:N0}W";
                string tempText = isOptimal ? $"❄ {temp:F1}°C (쾌적 +20%)" : (isOverheated ? $"▲ {temp:F1}°C (과열 -35%)" : $"● {temp:F1}°C");
                g.DrawString($"{pwrText}  |  {tempText}", teleFont, tempBrush, 14, 30);
            }

            // 뒤로가기 버튼 [◀ SandBox]
            _backButtonRect = new Rectangle(w - 95, 8, 85, 24);
            using (SolidBrush backBtnBg = new SolidBrush(Color.FromArgb(32, 38, 52)))
            using (Pen backBtnPen = new Pen(Color.FromArgb(60, 72, 96), 1))
            using (Font btnFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(170, 190, 215)))
            {
                g.FillRectangle(backBtnBg, _backButtonRect);
                g.DrawRectangle(backBtnPen, _backButtonRect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("◀ SandBox", btnFont, textBrush, _backButtonRect, sf);
            }

            // 중앙 허브
            float hubX = w / 2f;
            float hubY = 195f;
            using (SolidBrush hubBrush = new SolidBrush(Color.FromArgb(30, 42, 60)))
            using (Pen hubPen = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
            {
                g.FillEllipse(hubBrush, hubX - 16, hubY - 8, 32, 16);
                g.DrawEllipse(hubPen, hubX - 16, hubY - 8, 32, 16);
            }

            // 6개 서버 랙 및 케이블
            int rackWidth = 84;
            int rackHeight = 120;
            int gap = 12;
            int totalRacksWidth = 6 * rackWidth + 5 * gap;
            int startX = (w - totalRacksWidth) / 2;
            int rackY = 48;

            for (int i = 0; i < 6; i++)
            {
                int rx = startX + i * (rackWidth + gap);
                _rackBounds[i] = new Rectangle(rx, rackY, rackWidth, rackHeight);

                var server = i < _state.Servers.Count ? _state.Servers[i] : null;
                bool isUnlocked = server != null && server.Level > 0;

                if (isUnlocked)
                {
                    using (Pen cablePen = new Pen(Color.FromArgb(45, server.AccentColor), 1.5f))
                    {
                        float rackBottomCenter = rx + rackWidth / 2f;
                        g.DrawLine(cablePen, rackBottomCenter, rackY + rackHeight, hubX, hubY);
                    }
                }
            }

            foreach (var p in _particles)
            {
                using (SolidBrush pBrush = new SolidBrush(p.ParticleColor))
                {
                    g.FillEllipse(pBrush, p.X - 2.5f, p.Y - 2.5f, 5, 5);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                DrawSingleRack(g, _rackBounds[i], i);
            }

            if (_state.CurrentIncident != null)
            {
                DrawIncidentBanner(g, _state.CurrentIncident);
            }
        }

        private void DrawSingleRack(Graphics g, Rectangle rect, int index)
        {
            var server = index < _state.Servers.Count ? _state.Servers[index] : null;
            if (server == null) return;

            bool isUnlocked = server.Level > 0;
            bool isHovered = _hoveredRackIndex == index;

            Color chassisColor = isUnlocked ? Color.FromArgb(24, 28, 38) : Color.FromArgb(16, 18, 24);
            Color borderColor = isUnlocked 
                ? (isHovered ? server.AccentColor : Color.FromArgb(48, 58, 76)) 
                : Color.FromArgb(32, 36, 46);

            using (SolidBrush chassisBrush = new SolidBrush(chassisColor))
            using (Pen borderPen = new Pen(borderColor, isHovered ? 2f : 1f))
            {
                g.FillRectangle(chassisBrush, rect);
                g.DrawRectangle(borderPen, rect);
            }

            Rectangle headerRect = new Rectangle(rect.X, rect.Y, rect.Width, 20);
            using (SolidBrush headerBg = new SolidBrush(isUnlocked ? Color.FromArgb(32, 38, 52) : Color.FromArgb(20, 22, 28)))
            using (Font headerFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush headerText = new SolidBrush(isUnlocked ? Color.FromArgb(220, 235, 255) : Color.FromArgb(100, 110, 125)))
            {
                g.FillRectangle(headerBg, headerRect);
                using (SolidBrush dotBrush = new SolidBrush(isUnlocked ? server.AccentColor : Color.FromArgb(70, 80, 95)))
                {
                    g.FillEllipse(dotBrush, headerRect.X + 6, headerRect.Y + 7, 6, 6);
                }
                StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(GetShortName(server.Id), headerFont, headerText, 
                    new Rectangle(headerRect.X + 16, headerRect.Y, headerRect.Width - 18, headerRect.Height), sf);
            }

            if (!isUnlocked)
            {
                int lx = rect.X + rect.Width / 2;
                int ly = rect.Y + 36;
                using (Pen lockPen = new Pen(Color.FromArgb(90, 105, 125), 1.5f))
                using (SolidBrush lockBody = new SolidBrush(Color.FromArgb(36, 42, 54)))
                using (Font subFont = new Font("Segoe UI", 7f, FontStyle.Bold))
                using (SolidBrush lockBrush = new SolidBrush(Color.FromArgb(120, 135, 155)))
                {
                    // 자물쇠 고리 및 몸통
                    g.DrawArc(lockPen, lx - 5, ly - 9, 10, 10, 180, 180);
                    g.FillRectangle(lockBody, lx - 7, ly - 3, 14, 11);
                    g.DrawRectangle(lockPen, lx - 7, ly - 3, 14, 11);

                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"${FormatCompactNumber(server.BaseCost)}", subFont, lockBrush, new Rectangle(rect.X, rect.Y + 54, rect.Width, 18), sf);
                }
                return;
            }

            // 블레이드 슬롯 디테일
            int slotY = rect.Y + 24;
            int slotHeight = 16;
            int slotGap = 4;
            int filledSlots = Math.Min(4, Math.Max(1, (server.Level + 1) / 2));

            for (int s = 0; s < 4; s++)
            {
                int sy = slotY + s * (slotHeight + slotGap);
                Rectangle slotRect = new Rectangle(rect.X + 4, sy, rect.Width - 8, slotHeight);
                bool isSlotFilled = s < filledSlots;

                using (SolidBrush slotBrush = new SolidBrush(isSlotFilled ? Color.FromArgb(36, 42, 56) : Color.FromArgb(18, 20, 26)))
                using (Pen slotPen = new Pen(Color.FromArgb(44, 52, 68), 1))
                {
                    g.FillRectangle(slotBrush, slotRect);
                    g.DrawRectangle(slotPen, slotRect);
                }

                if (isSlotFilled)
                {
                    bool ledOn = ((_frameCounter + s * 4 + index * 3) % 18) < 11;
                    Color ledColor = ledOn ? server.AccentColor : Color.FromArgb(30, 45, 55);
                    using (SolidBrush ledBrush = new SolidBrush(ledColor))
                    {
                        g.FillEllipse(ledBrush, slotRect.X + 4, slotRect.Y + 5, 5, 5);
                    }
                }
            }

            // 하단 쿨링 팬 & 레벨 뱃지
            int fanSize = 20;
            int fanX = rect.X + 6;
            int fanY = rect.Bottom - fanSize - 4;
            DrawCoolingFan(g, fanX, fanY, fanSize, _fanAngle, server.AccentColor);

            using (SolidBrush badgeBg = new SolidBrush(Color.FromArgb(40, server.AccentColor)))
            using (Pen badgeBorder = new Pen(server.AccentColor, 1))
            using (Font badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush badgeText = new SolidBrush(Color.White))
            {
                Rectangle badgeRect = new Rectangle(rect.Right - 46, rect.Bottom - 18, 40, 15);
                g.FillRectangle(badgeBg, badgeRect);
                g.DrawRectangle(badgeBorder, badgeRect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString($"Lv.{server.Level}", badgeFont, badgeText, badgeRect, sf);
            }
        }

        private void DrawCoolingFan(Graphics g, int x, int y, int size, float angle, Color accent)
        {
            Rectangle rect = new Rectangle(x, y, size, size);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(18, 22, 30)))
            using (Pen border = new Pen(Color.FromArgb(50, 60, 80), 1))
            {
                g.FillEllipse(bg, rect);
                g.DrawEllipse(border, rect);
            }

            float cx = x + size / 2f;
            float cy = y + size / 2f;
            float r = size / 2f - 3;

            using (Pen fanPen = new Pen(Color.FromArgb(160, accent), 1.5f))
            {
                for (int b = 0; b < 4; b++)
                {
                    float a = (float)((angle + b * 90) * Math.PI / 180.0);
                    float ex = cx + (float)Math.Cos(a) * r;
                    float ey = cy + (float)Math.Sin(a) * r;
                    g.DrawLine(fanPen, cx, cy, ex, ey);
                }
            }
        }

        private void DrawIncidentBanner(Graphics g, TycoonIncident incident)
        {
            int targetIndex = incident.TargetServerIndex;
            if (targetIndex < 0 || targetIndex >= _rackBounds.Length) return;

            var rack = _rackBounds[targetIndex];
            int bw = 110;
            int bh = 32;
            int bx = rack.X + (rack.Width - bw) / 2;
            int by = Math.Max(4, rack.Y - bh - 6);

            bool flash = (_frameCounter % 10) < 5;
            Color bannerBg = flash ? Color.FromArgb(180, 20, 30) : Color.FromArgb(120, 15, 20);
            Color textCol = flash ? Color.Yellow : Color.White;

            Rectangle bRect = new Rectangle(bx, by, bw, bh);
            using (SolidBrush bBg = new SolidBrush(bannerBg))
            using (Pen bPen = new Pen(Color.FromArgb(255, 60, 80), 2))
            using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush tBrush = new SolidBrush(textCol))
            {
                g.FillRectangle(bBg, bRect);
                g.DrawRectangle(bPen, bRect);

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString($"{incident.Title}\n[클릭 퇴치! +${incident.RewardCash:N0}]", bFont, tBrush, bRect, sf);
            }
        }

        private void DrawCenterConsole(Graphics g, int w, int topY, int height)
        {
            Rectangle consoleRect = new Rectangle(10, topY + 4, w - 20, height - 8);
            using (SolidBrush panelBg = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (Pen borderPen = new Pen(Color.FromArgb(38, 46, 62), 1))
            {
                g.FillRectangle(panelBg, consoleRect);
                g.DrawRectangle(borderPen, consoleRect);
            }

            int leftPad = 24;
            using (Font moneyFont = new Font("Segoe UI", 17f, FontStyle.Bold))
            using (SolidBrush moneyBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
            {
                g.DrawString($"$ {FormatCompactNumber(_state.Money)}", moneyFont, moneyBrush, leftPad, topY + 12);
            }

            double totalRps = _state.CalculateTotalRps();
            double clickPower = _state.GetClickPower();
            using (Font statFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush rpsBrush = new SolidBrush(Color.FromArgb(0, 229, 255)))
            using (SolidBrush nodeBrush = new SolidBrush(Color.FromArgb(160, 180, 205)))
            {
                string feverTag = _state.IsFeverActive ? " [★ 2.5x FEVER]" : "";
                g.DrawString($"⚡ +${totalRps:N1}/sec{feverTag}", statFont, rpsBrush, leftPad, topY + 44);
                g.DrawString($"[가동 서버] {_state.GetTotalServerCount()}대  |  [클릭당] +${clickPower:N1}", statFont, nodeBrush, leftPad, topY + 63);
            }

            // 피버 바
            _feverBarRect = new Rectangle(leftPad, topY + 84, 230, 13);
            using (SolidBrush fBg = new SolidBrush(Color.FromArgb(30, 36, 48)))
            using (Pen fPen = new Pen(Color.FromArgb(50, 60, 80), 1))
            {
                g.FillRectangle(fBg, _feverBarRect);
                g.DrawRectangle(fPen, _feverBarRect);
            }

            float fillWidth = _feverBarRect.Width * (_state.FeverMeter / 100f);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(_feverBarRect.X, _feverBarRect.Y, (int)fillWidth, _feverBarRect.Height);
                Color fillCol = _state.IsFeverActive ? Color.FromArgb(255, 60, 0) : Color.FromArgb(255, 150, 0);
                using (SolidBrush fillBrush = new SolidBrush(fillCol))
                {
                    g.FillRectangle(fillBrush, fillRect);
                }
            }

            using (Font fFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string feverText = _state.IsFeverActive 
                    ? $"★ FEVER ACTIVE! ({_state.FeverRemainingSeconds:F0}s)" 
                    : $"연타 시 피버 발동! ({_state.FeverMeter:F0}%)";
                g.DrawString(feverText, fFont, textBrush, _feverBarRect, sf);
            }

            // 우측 [ ⚡ PING / DEPLOY ] 버튼
            int btnWidth = 210;
            int btnHeight = 72;
            int btnX = consoleRect.Right - btnWidth - 14;
            int btnY = topY + 14;
            _deployButtonRect = new Rectangle(btnX, btnY, btnWidth, btnHeight);

            Color btnBgCol = _isDeployHovered ? Color.FromArgb(0, 180, 215) : Color.FromArgb(0, 145, 180);
            if (_state.IsFeverActive)
            {
                btnBgCol = _isDeployHovered ? Color.FromArgb(255, 90, 30) : Color.FromArgb(220, 60, 20);
            }

            using (SolidBrush btnBrush = new SolidBrush(btnBgCol))
            using (Pen btnPen = new Pen(Color.FromArgb(120, 230, 255), _isDeployHovered ? 2f : 1f))
            {
                g.FillRectangle(btnBrush, _deployButtonRect);
                g.DrawRectangle(btnPen, _deployButtonRect);
            }

            using (Font btnTitleFont = new Font("Segoe UI", 11.5f, FontStyle.Bold))
            using (Font btnSubFont = new Font("Segoe UI", 8f, FontStyle.Regular))
            using (SolidBrush btnTextBrush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                Rectangle tRect = new Rectangle(_deployButtonRect.X, _deployButtonRect.Y + 10, _deployButtonRect.Width, 24);
                Rectangle sRect = new Rectangle(_deployButtonRect.X, _deployButtonRect.Y + 36, _deployButtonRect.Width, 20);

                g.DrawString("⚡ 데이터 전송 클릭!", btnTitleFont, btnTextBrush, tRect, sf);
                g.DrawString($"트래픽 처리 수익 (클릭당 +${clickPower:N1})", btnSubFont, btnTextBrush, sRect, sf);
            }

            foreach (var sw in _shockwaves)
            {
                using (Pen swPen = new Pen(Color.FromArgb((int)(sw.Alpha * 255), sw.WaveColor), 2f))
                {
                    g.DrawEllipse(swPen, sw.X - sw.Radius, sw.Y - sw.Radius, sw.Radius * 2, sw.Radius * 2);
                }
            }
        }

        private void DrawSubTabBar(Graphics g, int w, int topY, int height)
        {
            string[] tabNames = { "▣ 서버실", "⚡ 전력실", "❄ 냉각실", "◈ S/W 개발", "★ 외주 계약", "▲ 코인 거래", "✦ 연구/환생" };
            int tabCount = tabNames.Length;
            int tabGap = 3;
            int tabW = (w - 20 - (tabCount - 1) * tabGap) / tabCount;
            int tabH = height;

            for (int i = 0; i < tabCount; i++)
            {
                int tx = 10 + i * (tabW + tabGap);
                _subTabBounds[i] = new Rectangle(tx, topY, tabW, tabH);
                bool isActive = _activeSubTab == i;

                Color bg = isActive ? Color.FromArgb(32, 42, 60) : Color.FromArgb(20, 24, 32);
                Color border = isActive ? Color.FromArgb(0, 229, 255) : Color.FromArgb(38, 46, 60);
                Color textCol = isActive ? Color.White : Color.FromArgb(150, 165, 185);

                using (SolidBrush brush = new SolidBrush(bg))
                using (Pen pen = new Pen(border, isActive ? 1.5f : 1f))
                using (Font f = new Font("Segoe UI", 8.0f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(textCol))
                {
                    g.FillRectangle(brush, _subTabBounds[i]);
                    g.DrawRectangle(pen, _subTabBounds[i]);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(tabNames[i], f, tBrush, _subTabBounds[i], sf);
                }
            }
        }

        private void DrawActiveTabContent(Graphics g, int w, int topY, int height)
        {
            // 상단 구매 수량 토글 버튼 (탭 0, 1, 2 서버/전력/냉각 공통)
            if (_activeSubTab < 3)
            {
                int toggleX = w - 165;
                _buy1Rect = new Rectangle(toggleX, topY + 4, 42, 20);
                _buy10Rect = new Rectangle(toggleX + 46, topY + 4, 42, 20);
                _buyMaxRect = new Rectangle(toggleX + 92, topY + 4, 46, 20);

                DrawToggleButton(g, _buy1Rect, "x1", _buyMultiplier == 1);
                DrawToggleButton(g, _buy10Rect, "x10", _buyMultiplier == 10);
                DrawToggleButton(g, _buyMaxRect, "MAX", _buyMultiplier == -1);
            }

            switch (_activeSubTab)
            {
                case 0:
                    DrawServersTab(g, w, topY);
                    break;
                case 1:
                    DrawPowerTab(g, w, topY);
                    break;
                case 2:
                    DrawCoolingTab(g, w, topY);
                    break;
                case 3:
                    DrawSoftwareStudioTab(g, w, topY);
                    break;
                case 4:
                    DrawB2BContractsTab(g, w, topY);
                    break;
                case 5:
                    DrawCryptoMarketTab(g, w, topY);
                    break;
                case 6:
                    DrawTechTreeTab(g, w, topY);
                    break;
            }
        }

        private void DrawServersTab(Graphics g, int w, int topY)
        {
            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 220, 245)))
            {
                g.DrawString("▣ 서버 컴퓨터 구매 및 업그레이드 (수익 증가 | 전기·발열 발생)", titleFont, titleBrush, 14, topY + 6);
            }

            int cardPad = 8;
            int cardW = (w - 20 - cardPad) / 2;
            int cardH = 66;
            int startCardY = topY + 28;

            for (int i = 0; i < 6; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int cx = 10 + col * (cardW + cardPad);
                int cy = startCardY + row * (cardH + 5);

                _cardBounds[i] = new Rectangle(cx, cy, cardW, cardH);
                DrawServerCard(g, _cardBounds[i], i);
            }
        }

        private void DrawServerCard(Graphics g, Rectangle rect, int index)
        {
            var server = index < _state.Servers.Count ? _state.Servers[index] : null;
            if (server == null) return;

            int buyCount = _buyMultiplier == -1 ? server.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = server.GetNextCost(buyCount);
            bool canAfford = _state.Money >= cost;
            bool isHovered = _hoveredCardIndex == index;

            using (SolidBrush cBrush = new SolidBrush(Color.FromArgb(22, 26, 36)))
            using (Pen cPen = new Pen(isHovered ? server.AccentColor : Color.FromArgb(42, 50, 66), isHovered ? 1.5f : 1f))
            {
                g.FillRectangle(cBrush, rect);
                g.DrawRectangle(cPen, rect);
            }

            // 고유 벡터 아이콘 뱃지 렌더링 (네모 박스 문제 원천 제거)
            DrawUnitVectorIcon(g, new Rectangle(rect.X + 6, rect.Y + (rect.Height - 36) / 2, 36, 36), server.Id, server.AccentColor);

            using (Font nameFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
            using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(230, 240, 255)))
            using (SolidBrush levelBrush = new SolidBrush(Color.FromArgb(160, 180, 205)))
            using (SolidBrush rpsBrush = new SolidBrush(server.AccentColor))
            {
                g.DrawString(server.Name, nameFont, nameBrush, rect.X + 48, rect.Y + 6);
                g.DrawString($"Lv.{server.Level} | +${server.GetTotalRps():N1}/s", subFont, levelBrush, rect.X + 48, rect.Y + 24);
                g.DrawString($"소비: {server.PowerWatts:N0}W, 열: +{server.HeatOutput:F1}°", subFont, new SolidBrush(Color.FromArgb(120, 135, 155)), rect.X + 48, rect.Y + 42);
            }

            int btnW = 74;
            int btnH = 44;
            int bx = rect.Right - btnW - 8;
            int by = rect.Y + (rect.Height - btnH) / 2;
            Rectangle buyBtnRect = new Rectangle(bx, by, btnW, btnH);
            _cardBuyButtonBounds[index] = buyBtnRect;

            Color btnBg = canAfford ? Color.FromArgb(35, 140, 90) : Color.FromArgb(38, 44, 56);
            Color btnBorder = canAfford ? Color.FromArgb(0, 230, 118) : Color.FromArgb(50, 60, 76);
            Color textCol = canAfford ? Color.White : Color.FromArgb(130, 140, 155);

            using (SolidBrush bBg = new SolidBrush(btnBg))
            using (Pen bPen = new Pen(btnBorder, canAfford ? 1.5f : 1f))
            using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (Font countFont = new Font("Segoe UI", 7f))
            using (SolidBrush tBrush = new SolidBrush(textCol))
            {
                g.FillRectangle(bBg, buyBtnRect);
                g.DrawRectangle(bPen, buyBtnRect);

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string countStr = buyCount > 1 ? $"(+{buyCount})" : "";
                Rectangle tRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 3, buyBtnRect.Width, 18);
                Rectangle cRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 21, buyBtnRect.Width, 18);

                g.DrawString($"${FormatCompactNumber(cost)}", bFont, tBrush, tRect, sf);
                g.DrawString($"구매 {countStr}", countFont, tBrush, cRect, sf);
            }
        }

        private void DrawPowerTab(Graphics g, int w, int topY)
        {
            double supply = _state.GetTotalPowerSupply();
            double demand = _state.GetTotalPowerDemand();
            double capHours = _state.GetOfflineCapSeconds() / 3600.0;

            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(255, 167, 38)))
            {
                g.DrawString($"⚡ 전력 공급: 현재 공급 {supply:N0}W / 필요 전력 {demand:N0}W  |  [부재 중 축전 시간] {capHours:F1}시간", titleFont, titleBrush, 14, topY + 6);
            }

            int cardPad = 8;
            int cardW = (w - 20 - cardPad) / 2;
            int cardH = 66;
            int startCardY = topY + 28;

            for (int i = 0; i < 6; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int cx = 10 + col * (cardW + cardPad);
                int cy = startCardY + row * (cardH + 5);

                _cardBounds[i] = new Rectangle(cx, cy, cardW, cardH);
                DrawPowerCard(g, _cardBounds[i], i);
            }
        }

        private void DrawPowerCard(Graphics g, Rectangle rect, int index)
        {
            var power = index < _state.PowerFacilities.Count ? _state.PowerFacilities[index] : null;
            if (power == null) return;

            int buyCount = _buyMultiplier == -1 ? power.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = power.GetNextCost(buyCount);
            bool canAfford = _state.Money >= cost && power.BaseCost > 0;
            bool isHovered = _hoveredCardIndex == index;

            using (SolidBrush cBrush = new SolidBrush(Color.FromArgb(24, 26, 34)))
            using (Pen cPen = new Pen(isHovered ? power.AccentColor : Color.FromArgb(46, 50, 64), isHovered ? 1.5f : 1f))
            {
                g.FillRectangle(cBrush, rect);
                g.DrawRectangle(cPen, rect);
            }

            // 고유 벡터 아이콘 뱃지 렌더링
            DrawUnitVectorIcon(g, new Rectangle(rect.X + 6, rect.Y + (rect.Height - 36) / 2, 36, 36), power.Id, power.AccentColor);

            using (Font nameFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
            using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(230, 240, 255)))
            using (SolidBrush levelBrush = new SolidBrush(Color.FromArgb(255, 183, 77)))
            {
                g.DrawString(power.Name, nameFont, nameBrush, rect.X + 48, rect.Y + 6);
                string bonusStr = power.OfflineBonusMinutes > 0 ? $" | +{power.OfflineBonusMinutes}분 배터리" : "";
                g.DrawString($"Lv.{power.Level} (+{power.GetTotalSupply():N0}W){bonusStr}", subFont, levelBrush, rect.X + 48, rect.Y + 24);
                g.DrawString(power.SubTitle, subFont, new SolidBrush(Color.FromArgb(120, 135, 155)), rect.X + 48, rect.Y + 42);
            }

            int btnW = 74;
            int btnH = 44;
            int bx = rect.Right - btnW - 8;
            int by = rect.Y + (rect.Height - btnH) / 2;
            Rectangle buyBtnRect = new Rectangle(bx, by, btnW, btnH);
            _cardBuyButtonBounds[index] = buyBtnRect;

            if (power.BaseCost <= 0)
            {
                // 기본 제공 설비
                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(36, 42, 54)))
                using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(140, 160, 180)))
                {
                    g.FillRectangle(bBg, buyBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("기본 제공", bFont, tBrush, buyBtnRect, sf);
                }
                return;
            }

            Color btnBg = canAfford ? Color.FromArgb(160, 100, 20) : Color.FromArgb(38, 44, 56);
            Color btnBorder = canAfford ? Color.FromArgb(255, 167, 38) : Color.FromArgb(50, 60, 76);
            Color textCol = canAfford ? Color.White : Color.FromArgb(130, 140, 155);

            using (SolidBrush bBg = new SolidBrush(btnBg))
            using (Pen bPen = new Pen(btnBorder, canAfford ? 1.5f : 1f))
            using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (Font countFont = new Font("Segoe UI", 7f))
            using (SolidBrush tBrush = new SolidBrush(textCol))
            {
                g.FillRectangle(bBg, buyBtnRect);
                g.DrawRectangle(bPen, buyBtnRect);

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string countStr = buyCount > 1 ? $"(+{buyCount})" : "";
                Rectangle tRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 3, buyBtnRect.Width, 18);
                Rectangle cRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 21, buyBtnRect.Width, 18);

                g.DrawString($"${FormatCompactNumber(cost)}", bFont, tBrush, tRect, sf);
                g.DrawString($"증설 {countStr}", countFont, tBrush, cRect, sf);
            }
        }

        private void DrawCoolingTab(Graphics g, int w, int topY)
        {
            double cooling = _state.GetTotalCoolingCapacity();
            double heat = _state.GetTotalHeatGenerated();
            double temp = _state.GetCurrentTemperature();
            string status = temp <= 35.0 ? "[쾌적 보너스 +20%]" : (temp > 60.0 ? "[과열 성능 저하 -35%]" : "[안정 가동]");

            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(0, 229, 255)))
            {
                g.DrawString($"❄ 냉각 설비: 냉각 능력 {cooling:N0} / 발생 열기 {heat:N0}  |  온도: {temp:F1}°C {status}", titleFont, titleBrush, 14, topY + 6);
            }

            int cardPad = 8;
            int cardW = (w - 20 - cardPad) / 2;
            int cardH = 66;
            int startCardY = topY + 28;

            for (int i = 0; i < 6; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int cx = 10 + col * (cardW + cardPad);
                int cy = startCardY + row * (cardH + 5);

                _cardBounds[i] = new Rectangle(cx, cy, cardW, cardH);
                DrawCoolingCard(g, _cardBounds[i], i);
            }
        }

        private void DrawCoolingCard(Graphics g, Rectangle rect, int index)
        {
            var cooling = index < _state.CoolingFacilities.Count ? _state.CoolingFacilities[index] : null;
            if (cooling == null) return;

            int buyCount = _buyMultiplier == -1 ? cooling.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = cooling.GetNextCost(buyCount);
            bool canAfford = _state.Money >= cost && cooling.BaseCost > 0;
            bool isHovered = _hoveredCardIndex == index;

            using (SolidBrush cBrush = new SolidBrush(Color.FromArgb(20, 26, 36)))
            using (Pen cPen = new Pen(isHovered ? cooling.AccentColor : Color.FromArgb(40, 52, 68), isHovered ? 1.5f : 1f))
            {
                g.FillRectangle(cBrush, rect);
                g.DrawRectangle(cPen, rect);
            }

            // 고유 벡터 아이콘 뱃지 렌더링
            DrawUnitVectorIcon(g, new Rectangle(rect.X + 6, rect.Y + (rect.Height - 36) / 2, 36, 36), cooling.Id, cooling.AccentColor);

            using (Font nameFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 7.5f, FontStyle.Regular))
            using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(230, 240, 255)))
            using (SolidBrush levelBrush = new SolidBrush(Color.FromArgb(0, 229, 255)))
            {
                g.DrawString(cooling.Name, nameFont, nameBrush, rect.X + 48, rect.Y + 6);
                g.DrawString($"Lv.{cooling.Level} (냉각 용량: +{cooling.GetTotalCooling():N0})", subFont, levelBrush, rect.X + 48, rect.Y + 24);
                g.DrawString(cooling.SubTitle, subFont, new SolidBrush(Color.FromArgb(120, 135, 155)), rect.X + 48, rect.Y + 42);
            }

            int btnW = 74;
            int btnH = 44;
            int bx = rect.Right - btnW - 8;
            int by = rect.Y + (rect.Height - btnH) / 2;
            Rectangle buyBtnRect = new Rectangle(bx, by, btnW, btnH);
            _cardBuyButtonBounds[index] = buyBtnRect;

            if (cooling.BaseCost <= 0)
            {
                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(36, 42, 54)))
                using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(140, 160, 180)))
                {
                    g.FillRectangle(bBg, buyBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("기본 제공", bFont, tBrush, buyBtnRect, sf);
                }
                return;
            }

            Color btnBg = canAfford ? Color.FromArgb(20, 110, 140) : Color.FromArgb(38, 44, 56);
            Color btnBorder = canAfford ? Color.FromArgb(0, 229, 255) : Color.FromArgb(50, 60, 76);
            Color textCol = canAfford ? Color.White : Color.FromArgb(130, 140, 155);

            using (SolidBrush bBg = new SolidBrush(btnBg))
            using (Pen bPen = new Pen(btnBorder, canAfford ? 1.5f : 1f))
            using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (Font countFont = new Font("Segoe UI", 7f))
            using (SolidBrush tBrush = new SolidBrush(textCol))
            {
                g.FillRectangle(bBg, buyBtnRect);
                g.DrawRectangle(bPen, buyBtnRect);

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string countStr = buyCount > 1 ? $"(+{buyCount})" : "";
                Rectangle tRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 3, buyBtnRect.Width, 18);
                Rectangle cRect = new Rectangle(buyBtnRect.X, buyBtnRect.Y + 21, buyBtnRect.Width, 18);

                g.DrawString($"${FormatCompactNumber(cost)}", bFont, tBrush, tRect, sf);
                g.DrawString($"설치 {countStr}", countFont, tBrush, cRect, sf);
            }
        }

        private void DrawTechTreeTab(Graphics g, int w, int topY)
        {
            // 상단 환생 배너 & 액션 버튼
            int claimable = _state.CalculatePrestigeTokens();
            Rectangle presBannerRect = new Rectangle(10, topY + 4, w - 20, 44);
            using (SolidBrush bannerBg = new SolidBrush(Color.FromArgb(28, 22, 42)))
            using (Pen bannerPen = new Pen(Color.FromArgb(179, 136, 255), 1))
            {
                g.FillRectangle(bannerBg, presBannerRect);
                g.DrawRectangle(bannerPen, presBannerRect);
            }

            using (Font tFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush tokenBrush = new SolidBrush(Color.FromArgb(225, 205, 255)))
            {
                string statusInfo;
                if (claimable > 0)
                {
                    statusInfo = $"회차 수익: ${FormatCompactNumber(_state.CurrentRunEarned)} ➔ ◈ +{claimable}개 토큰 수령 가능!";
                }
                else
                {
                    double remain = Math.Max(0, 1000000.0 - _state.CurrentRunEarned);
                    statusInfo = $"회차 수익: ${FormatCompactNumber(_state.CurrentRunEarned)} (첫 토큰까지: ${FormatCompactNumber(remain)})";
                }

                string info = $"◈ 보유 연구 토큰: {_state.ResearchTokens}개 (환생 배율: x{_state.GetPrestigeMultiplier():F2})\n{statusInfo}";
                g.DrawString(info, tFont, tokenBrush, 22, topY + 7);
            }

            // [☁ 마이그레이션 실행] 버튼
            int pBtnW = 145;
            int pBtnH = 32;
            int px = presBannerRect.Right - pBtnW - 6;
            int py = presBannerRect.Y + 6;
            _prestigeActionBtnRect = new Rectangle(px, py, pBtnW, pBtnH);

            bool canPrestige = claimable >= 1;
            Color pBg = canPrestige ? Color.FromArgb(120, 60, 200) : Color.FromArgb(36, 32, 46);
            Color pBorder = canPrestige ? Color.FromArgb(220, 180, 255) : Color.FromArgb(50, 46, 62);

            using (SolidBrush pBrush = new SolidBrush(pBg))
            using (Pen pPen = new Pen(pBorder, canPrestige ? 1.5f : 1f))
            using (Font bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush tBrush = new SolidBrush(canPrestige ? Color.White : Color.FromArgb(120, 115, 135)))
            {
                g.FillRectangle(pBrush, _prestigeActionBtnRect);
                g.DrawRectangle(pPen, _prestigeActionBtnRect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string btnText = canPrestige ? $"☁ 환생 (+{claimable})" : "수익 부족 (환생 불가)";
                g.DrawString(btnText, bFont, tBrush, _prestigeActionBtnRect, sf);
            }

            // 6종 영구 스킬 카드 2열 3행
            var skills = TycoonGameState.GetSkillDefinitions();
            int cardPad = 8;
            int cardW = (w - 20 - cardPad) / 2;
            int cardH = 58;
            int startCardY = topY + 52;

            for (int i = 0; i < skills.Count; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int cx = 10 + col * (cardW + cardPad);
                int cy = startCardY + row * (cardH + 4);

                _cardBounds[i] = new Rectangle(cx, cy, cardW, cardH);
                DrawSkillCard(g, _cardBounds[i], skills[i], i);
            }
        }

        private void DrawSkillCard(Graphics g, Rectangle rect, SkillNode skill, int index)
        {
            int currentLvl = _state.GetSkillLevel(skill.Id);
            bool isMax = currentLvl >= skill.MaxLevel;
            int nextCost = isMax ? 0 : skill.GetCostForLevel(currentLvl);
            bool canUpgrade = !isMax && _state.ResearchTokens >= nextCost;
            bool isHovered = _hoveredCardIndex == index;

            Color cBg = currentLvl > 0 ? Color.FromArgb(26, 32, 42) : Color.FromArgb(20, 22, 30);
            Color cBorder = isMax ? Color.FromArgb(0, 230, 118) : (isHovered ? Color.FromArgb(179, 136, 255) : Color.FromArgb(44, 48, 62));

            using (SolidBrush brush = new SolidBrush(cBg))
            using (Pen pen = new Pen(cBorder, isMax ? 1.5f : (isHovered ? 1.5f : 1f)))
            {
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
            }

            // 고유 벡터 아이콘 뱃지 렌더링
            DrawUnitVectorIcon(g, new Rectangle(rect.X + 6, rect.Y + (rect.Height - 34) / 2, 34, 34), skill.Id, Color.FromArgb(179, 136, 255));

            using (Font nameFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (Font lvlFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (Font descFont = new Font("Segoe UI", 7.2f))
            using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(235, 240, 255)))
            using (SolidBrush lvlBrush = new SolidBrush(isMax ? Color.FromArgb(0, 230, 118) : Color.FromArgb(179, 136, 255)))
            using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(145, 160, 180)))
            {
                g.DrawString(skill.Name, nameFont, nameBrush, rect.X + 46, rect.Y + 6);
                string lvlText = isMax ? "[MAX]" : $"[Lv.{currentLvl}/{skill.MaxLevel}]";
                g.DrawString(lvlText, lvlFont, lvlBrush, rect.X + 46 + (int)g.MeasureString(skill.Name, nameFont).Width + 4, rect.Y + 6);

                string dynamicDesc = skill.GetLevelDescription(currentLvl);
                g.DrawString(dynamicDesc, descFont, descBrush, 
                    new Rectangle(rect.X + 46, rect.Y + 23, rect.Width - 120, 32));
            }

            // 우측 연구/보유 버튼
            int btnW = 68;
            int btnH = 38;
            int bx = rect.Right - btnW - 6;
            int by = rect.Y + (rect.Height - btnH) / 2;
            Rectangle buyBtnRect = new Rectangle(bx, by, btnW, btnH);
            _cardBuyButtonBounds[index] = buyBtnRect;

            if (isMax)
            {
                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(25, 70, 45)))
                using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(120, 255, 180)))
                {
                    g.FillRectangle(bBg, buyBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("마스터\n(MAX)", bFont, tBrush, buyBtnRect, sf);
                }
            }
            else
            {
                Color bBg = canUpgrade ? Color.FromArgb(95, 40, 150) : Color.FromArgb(36, 32, 46);
                Color bBorder = canUpgrade ? Color.FromArgb(179, 136, 255) : Color.FromArgb(50, 46, 62);
                Color textCol = canUpgrade ? Color.White : Color.FromArgb(130, 125, 140);

                using (SolidBrush bBrush = new SolidBrush(bBg))
                using (Pen bPen = new Pen(bBorder, canUpgrade ? 1.5f : 1f))
                using (Font bFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(textCol))
                {
                    g.FillRectangle(bBrush, buyBtnRect);
                    g.DrawRectangle(bPen, buyBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string btnLabel = currentLvl == 0 ? $"연구 시작\n◈ {nextCost}개" : $"Lv.UP\n◈ {nextCost}개";
                    g.DrawString(btnLabel, bFont, tBrush, buyBtnRect, sf);
                }
            }
        }

        #region 1. S/W Studio Tab & Logic
        private void DrawSoftwareStudioTab(Graphics g, int w, int topY)
        {
            var studio = _state.SoftwareStudio;
            var categories = SoftwareStudioState.GetCategories();

            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 220, 245)))
            {
                double royaltyRps = _state.GetSoftwareRoyaltyRps();
                string title = $"◈ 소프트웨어 개발실 (초당 로열티: +${royaltyRps:N1}/초 | 총 출시: {studio.TotalSoftwareShipped}건)";
                g.DrawString(title, titleFont, titleBrush, 14, topY + 6);
            }

            int leftW = (int)(w * 0.52);
            int rightX = leftW + 18;
            int rightW = w - rightX - 10;
            int panelY = topY + 28;

            if (studio.CurrentProject != null)
            {
                var prj = studio.CurrentProject;
                var def = categories.Find(c => c.Id == prj.CategoryId) ?? categories[0];

                Rectangle projBox = new Rectangle(10, panelY, leftW, 205);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(22, 28, 38)))
                using (Pen pen = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                {
                    g.FillRectangle(bg, projBox);
                    g.DrawRectangle(pen, projBox);
                }

                // 프로젝트 카테고리 벡터 뱃지
                Rectangle pIconRect = new Rectangle(projBox.Right - 46, panelY + 12, 34, 34);
                DrawSoftwareCategoryIcon(g, pIconRect, prj.CategoryId, ColorTranslator.FromHtml(def.AccentColorHex ?? "#00E5FF"), true);

                using (Font pFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                using (Font subFont = new Font("Segoe UI", 8f))
                using (SolidBrush pBrush = new SolidBrush(Color.White))
                using (SolidBrush sBrush = new SolidBrush(Color.FromArgb(160, 180, 205)))
                {
                    g.DrawString($"▶ 개발 진행 중: {prj.Title}", pFont, pBrush, 22, panelY + 12);
                    g.DrawString($"분야: {def.Name} | 권장 배분: 디자인 {def.OptimalUx}% / 핵심 기능 {def.OptimalAlgo}% / 보안 {def.OptimalSec}%", subFont, sBrush, 22, panelY + 34);
                }

                int barX = 22;
                int barY = panelY + 60;
                int barW = leftW - 44;
                int barH = 24;
                Rectangle barBgRect = new Rectangle(barX, barY, barW, barH);

                double pct = prj.ProgressPercent;
                int fillW = (int)((pct / 100.0) * barW);

                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(14, 18, 24)))
                using (Pen bPen = new Pen(Color.FromArgb(40, 50, 70), 1))
                {
                    g.FillRectangle(bBg, barBgRect);
                    g.DrawRectangle(bPen, barBgRect);
                }

                if (fillW > 0)
                {
                    Color fillCol = prj.IsCompleted ? Color.FromArgb(0, 230, 118) : Color.FromArgb(0, 229, 255);
                    using (SolidBrush fillBrush = new SolidBrush(fillCol))
                    {
                        g.FillRectangle(fillBrush, barX, barY, fillW, barH);
                    }
                }

                using (Font progFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush progText = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string progStr = prj.IsCompleted ? "★ 프로그램 개발 완료! (100.0%)" : $"{pct:F1}% ({prj.CurrentPoints:N0} / {prj.TargetPoints:N0} 개발력)";
                    g.DrawString(progStr, progFont, progText, barBgRect, sf);
                }

                double devRps = _state.CalculateTotalRps();
                double speed = Math.Max(25.0, devRps * 0.45);
                using (Font infoFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (SolidBrush spdBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
                using (SolidBrush balBrush = new SolidBrush(Color.FromArgb(140, 160, 185)))
                {
                    g.DrawString($"⚡ 서버 개발 지원: +{speed:N0} 개발력/초", infoFont, spdBrush, 22, panelY + 92);
                    g.DrawString($"투자 비율: 디자인 {prj.FocusUx}%  |  핵심 기능 {prj.FocusAlgo}%  |  보안 {prj.FocusSec}%", infoFont, balBrush, 22, panelY + 110);
                }

                if (!prj.IsCompleted)
                {
                    _swCrunchBtnRect = new Rectangle(22, panelY + 140, barW, 42);
                    using (SolidBrush cBg = new SolidBrush(Color.FromArgb(36, 48, 70)))
                    using (Pen cPen = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    using (Font cFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (SolidBrush cText = new SolidBrush(Color.White))
                    {
                        g.FillRectangle(cBg, _swCrunchBtnRect);
                        g.DrawRectangle(cPen, _swCrunchBtnRect);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("⚡ 열정 부스트 코딩! (+250 개발력 즉시 추가)", cFont, cText, _swCrunchBtnRect, sf);
                    }
                }
                else
                {
                    _swLaunchBtnRect = new Rectangle(22, panelY + 140, barW, 42);
                    using (SolidBrush lBg = new SolidBrush(Color.FromArgb(0, 180, 80)))
                    using (Pen lPen = new Pen(Color.FromArgb(0, 255, 128), 2f))
                    using (Font lFont = new Font("Segoe UI", 10f, FontStyle.Bold))
                    using (SolidBrush lText = new SolidBrush(Color.Black))
                    {
                        g.FillRectangle(lBg, _swLaunchBtnRect);
                        g.DrawRectangle(lPen, _swLaunchBtnRect);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString("★ 글로벌 마켓 공식 출시! (보너스 & 정기 수입 수령)", lFont, lText, _swLaunchBtnRect, sf);
                    }
                }
            }
            else
            {
                Rectangle planBox = new Rectangle(10, panelY, leftW, 205);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(20, 24, 32)))
                using (Pen pen = new Pen(Color.FromArgb(38, 46, 60), 1))
                {
                    g.FillRectangle(bg, planBox);
                    g.DrawRectangle(pen, planBox);
                }

                int catW = (leftW - 24) / 5;
                for (int i = 0; i < categories.Count; i++)
                {
                    int cx = 14 + i * (catW + 2);
                    _swCategoryBounds[i] = new Rectangle(cx, panelY + 8, catW, 58);
                    bool isSel = studio.SelectedCategoryIndex == i;

                    Color cBg = isSel ? Color.FromArgb(36, 48, 70) : Color.FromArgb(16, 20, 28);
                    Color cBorder = isSel ? Color.FromArgb(0, 229, 255) : Color.FromArgb(40, 48, 62);

                    using (SolidBrush cb = new SolidBrush(cBg))
                    using (Pen cp = new Pen(cBorder, isSel ? 1.5f : 1f))
                    using (Font catFont = new Font("Segoe UI", 7.2f, FontStyle.Bold))
                    using (SolidBrush ct = new SolidBrush(isSel ? Color.White : Color.FromArgb(140, 155, 175)))
                    {
                        g.FillRectangle(cb, _swCategoryBounds[i]);
                        g.DrawRectangle(cp, _swCategoryBounds[i]);

                        // 상단: 멈춰있는 고품질 GDI+ 절차적 벡터 아이콘
                        Rectangle iconRect = new Rectangle(cx + (catW - 26) / 2, panelY + 11, 26, 24);
                        DrawSoftwareCategoryIcon(g, iconRect, categories[i].Id, ColorTranslator.FromHtml(categories[i].AccentColorHex ?? "#00E5FF"), isSel);

                        // 하단: 간결한 카테고리 레이블
                        Rectangle nameRect = new Rectangle(cx + 1, panelY + 38, catW - 2, 17);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(GetCategoryShortName(categories[i].Id), catFont, ct, nameRect, sf);
                    }
                }

                var selDef = categories[Math.Min(categories.Count - 1, studio.SelectedCategoryIndex)];

                using (Font subTitleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (Font descFont = new Font("Segoe UI", 7.5f))
                using (SolidBrush wBrush = new SolidBrush(Color.FromArgb(230, 240, 255)))
                using (SolidBrush dBrush = new SolidBrush(Color.FromArgb(150, 170, 195)))
                {
                    g.DrawString($"[선택] {selDef.Name} ({selDef.SubTitle})", subTitleFont, wBrush, 16, panelY + 72);
                    g.DrawString($"개발비: ${selDef.UnlockCost:N0}  |  필요 개발력: {selDef.RequiredPoints:N0}  |  기본 수입: +${selDef.BaseRoyaltyPerSec:N0}/초", descFont, dBrush, 16, panelY + 90);
                }

                int sliderY = panelY + 114;
                DrawFactorStepper(g, 16, sliderY, "디자인", studio.SelectedCategoryIndex, 0, out _swUxMinusRect, out _swUxPlusRect);
                DrawFactorStepper(g, 16, sliderY + 24, "핵심 기능", studio.SelectedCategoryIndex, 1, out _swAlgoMinusRect, out _swAlgoPlusRect);
                DrawFactorStepper(g, 16, sliderY + 48, "보안 및 안정", studio.SelectedCategoryIndex, 2, out _swSecMinusRect, out _swSecPlusRect);

                _swStartDevBtnRect = new Rectangle(leftW - 130, panelY + 130, 120, 36);
                bool canAfford = _state.Money >= selDef.UnlockCost;
                Color sBg = canAfford ? Color.FromArgb(0, 150, 200) : Color.FromArgb(32, 36, 46);
                Color sBorder = canAfford ? Color.FromArgb(0, 229, 255) : Color.FromArgb(50, 56, 70);

                using (SolidBrush sb = new SolidBrush(sBg))
                using (Pen sp = new Pen(sBorder, canAfford ? 1.5f : 1f))
                using (Font sFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush st = new SolidBrush(canAfford ? Color.White : Color.FromArgb(120, 130, 145)))
                {
                    g.FillRectangle(sb, _swStartDevBtnRect);
                    g.DrawRectangle(sp, _swStartDevBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"▶ 개발 시작\n(-${FormatCompactNumber(selDef.UnlockCost)})", sFont, st, _swStartDevBtnRect, sf);
                }
            }

            // 우측 패널: 출시된 S/W 마켓 현황 & 포트폴리오
            Rectangle rightBox = new Rectangle(rightX, panelY, rightW, 205);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(18, 22, 30)))
            using (Pen pen = new Pen(Color.FromArgb(36, 44, 58), 1))
            {
                g.FillRectangle(bg, rightBox);
                g.DrawRectangle(pen, rightBox);
            }

            using (Font rTitleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush rTitleBrush = new SolidBrush(Color.FromArgb(200, 215, 235)))
            {
                g.DrawString("◈ 출시된 프로그램 목록 (정기 수입)", rTitleFont, rTitleBrush, rightX + 12, panelY + 10);
            }

            if (studio.ReleasedProducts.Count == 0)
            {
                using (Font emptyFont = new Font("Segoe UI", 8f))
                using (SolidBrush emptyBrush = new SolidBrush(Color.FromArgb(110, 125, 145)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("현재 서비스 중인 프로그램이 없습니다.\n좌측에서 새 프로그램을 기획하고 출시해 보세요!", emptyFont, emptyBrush, rightBox, sf);
                }
            }
            else
            {
                int itemH = 38;
                for (int i = 0; i < studio.ReleasedProducts.Count && i < 4; i++)
                {
                    var prod = studio.ReleasedProducts[i];
                    int iy = panelY + 34 + i * (itemH + 3);
                    Rectangle iRect = new Rectangle(rightX + 8, iy, rightW - 16, itemH);

                    using (SolidBrush ib = new SolidBrush(Color.FromArgb(24, 30, 42)))
                    using (Pen ip = new Pen(Color.FromArgb(45, 55, 75), 1))
                    using (Font nFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                    using (Font starFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                    using (Font subFont = new Font("Segoe UI", 7.2f))
                    using (SolidBrush nBrush = new SolidBrush(Color.White))
                    using (SolidBrush sBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
                    using (SolidBrush dBrush = new SolidBrush(Color.FromArgb(0, 230, 118)))
                    using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(130, 145, 165)))
                    {
                        g.FillRectangle(ib, iRect);
                        g.DrawRectangle(ip, iRect);

                        g.DrawString(prod.Title, nFont, nBrush, iRect.X + 8, iRect.Y + 4);
                        g.DrawString($"★ {prod.Rating:F1}", starFont, sBrush, iRect.X + 8 + (int)g.MeasureString(prod.Title, nFont).Width + 6, iRect.Y + 4);
                        g.DrawString($"\"{prod.ReviewSummary}\"", subFont, tBrush, iRect.X + 8, iRect.Y + 20);

                        StringFormat rsf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                        Rectangle earnRect = new Rectangle(iRect.Right - 120, iRect.Y, 112, iRect.Height);
                        g.DrawString($"+${prod.RoyaltyPerSec:N1}/초\n(잔여 {prod.RemainingSecs}s)", subFont, dBrush, earnRect, rsf);
                    }
                }
            }
        }

        private void DrawFactorStepper(Graphics g, int x, int y, string label, int catIdx, int factorIdx, out Rectangle minusRect, out Rectangle plusRect)
        {
            minusRect = new Rectangle(x + 85, y, 20, 18);
            plusRect = new Rectangle(x + 145, y, 20, 18);

            int val = 33;
            var def = SoftwareStudioState.GetCategories()[catIdx];
            if (factorIdx == 0) val = def.OptimalUx;
            else if (factorIdx == 1) val = def.OptimalAlgo;
            else val = def.OptimalSec;

            using (Font f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(170, 185, 205)))
            {
                g.DrawString(label, f, tBrush, x, y + 2);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString($"{val}% (최적)", f, tBrush, new Rectangle(x + 70, y, 100, 18), sf);
            }
        }

        private void HandleSoftwareStudioClick(Point pt)
        {
            var studio = _state.SoftwareStudio;
            var categories = SoftwareStudioState.GetCategories();

            if (studio.CurrentProject != null)
            {
                var prj = studio.CurrentProject;
                if (!prj.IsCompleted && _swCrunchBtnRect.Contains(pt))
                {
                    prj.CurrentPoints += 250.0;
                    AddFloatingText("+250 개발력!", pt.X, pt.Y, Color.FromArgb(0, 229, 255), true);
                    Invalidate();
                    return;
                }
                else if (prj.IsCompleted && _swLaunchBtnRect.Contains(pt))
                {
                    var def = categories.Find(c => c.Id == prj.CategoryId) ?? categories[0];
                    int uxDiff = Math.Abs(prj.FocusUx - def.OptimalUx);
                    int algoDiff = Math.Abs(prj.FocusAlgo - def.OptimalAlgo);
                    int secDiff = Math.Abs(prj.FocusSec - def.OptimalSec);
                    double penalty = (uxDiff + algoDiff + secDiff) * 0.05;
                    double rating = Math.Max(3.5, Math.Min(10.0, 9.4 - penalty + (_rand.NextDouble() * 1.2 - 0.5)));
                    rating = Math.Round(rating, 1);

                    double royalty = Math.Round(def.BaseRoyaltyPerSec * (rating / 6.0) * _state.GetPrestigeMultiplier(), 1);
                    double launchBonus = Math.Round(def.UnlockCost * (rating / 4.0), 0);

                    _state.Money += launchBonus;
                    _state.CurrentRunEarned += launchBonus;
                    _state.TotalEarned += launchBonus;

                    string review = rating >= 8.5 ? "★ 역대급 완성도! 전 세계 유저 대호평!" : (rating >= 6.5 ? "준수한 기능과 안정적인 시장 반응" : "최적화와 UX 부족으로 아쉬운 평점");
                    studio.ReleasedProducts.Add(new ReleasedSoftware
                    {
                        Title = prj.Title,
                        CategoryName = def.Name,
                        Rating = rating,
                        ReviewSummary = review,
                        RoyaltyPerSec = royalty,
                        RemainingSecs = 240,
                        TotalEarned = 0
                    });

                    studio.TotalSoftwareShipped++;
                    studio.CurrentProject = null;
                    AddFloatingText($"★ 론칭 완료! 평점 {rating:F1}점! +${launchBonus:N0}", pt.X, pt.Y, Color.FromArgb(255, 215, 64), true);
                    Invalidate();
                    return;
                }
            }
            else
            {
                for (int i = 0; i < _swCategoryBounds.Length; i++)
                {
                    if (_swCategoryBounds[i].Contains(pt))
                    {
                        studio.SelectedCategoryIndex = i;
                        Invalidate();
                        return;
                    }
                }

                if (_swStartDevBtnRect.Contains(pt))
                {
                    var def = categories[Math.Min(categories.Count - 1, studio.SelectedCategoryIndex)];
                    if (_state.Money >= def.UnlockCost)
                    {
                        _state.Money -= def.UnlockCost;
                        studio.CurrentProject = new ActiveSoftwareProject
                        {
                            CategoryId = def.Id,
                            Title = $"{def.Name} v{studio.TotalSoftwareShipped + 1}.0",
                            TargetPoints = def.RequiredPoints,
                            CurrentPoints = 0,
                            FocusUx = def.OptimalUx,
                            FocusAlgo = def.OptimalAlgo,
                            FocusSec = def.OptimalSec
                        };
                        AddFloatingText($"▶ {def.Name} 개발 착수!", pt.X, pt.Y, Color.FromArgb(0, 229, 255), true);
                        Invalidate();
                    }
                    else
                    {
                        AddFloatingText($"착수금 부족! (${def.UnlockCost:N0} 필요)", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                    }
                    return;
                }
            }
        }
        #endregion

        #region 2. B2B Contracts Tab & Logic
        private void DrawB2BContractsTab(Graphics g, int w, int topY)
        {
            var b2b = _state.B2BMarket;
            if (b2b.AvailableOffers.Count == 0)
            {
                b2b.RefreshOffers(_rand, _state.CalculateTotalRps());
            }

            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 220, 245)))
            using (Font tierFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (SolidBrush tierBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
            {
                string tierDesc = $"[{b2b.ReputationTier} RANK (신용도 {b2b.ReputationScore}점)]";
                g.DrawString($"★ 기업 외주 수주 계약실  |  내 서버실 신용 등급: ", titleFont, titleBrush, 14, topY + 6);
                int offset = (int)g.MeasureString("★ 기업 외주 수주 계약실  |  내 서버실 신용 등급: ", titleFont).Width + 14;
                g.DrawString(tierDesc, tierFont, tierBrush, offset, topY + 5);
            }

            _b2bRefreshBtnRect = new Rectangle(w - 95, topY + 4, 85, 22);
            using (SolidBrush refBg = new SolidBrush(Color.FromArgb(32, 42, 60)))
            using (Pen refPen = new Pen(Color.FromArgb(0, 229, 255), 1))
            using (Font refF = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush refT = new SolidBrush(Color.White))
            {
                g.FillRectangle(refBg, _b2bRefreshBtnRect);
                g.DrawRectangle(refPen, _b2bRefreshBtnRect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("● 새 의뢰 찾기", refF, refT, _b2bRefreshBtnRect, sf);
            }

            int panelY = topY + 28;

            if (b2b.ActiveContract != null)
            {
                var c = b2b.ActiveContract;
                Rectangle missionBox = new Rectangle(14, panelY, w - 28, 195);

                bool isWarning = c.ViolationsCount > 0;
                Color mBg = isWarning ? Color.FromArgb(32, 20, 26) : Color.FromArgb(20, 28, 38);
                Color mBorder = isWarning ? Color.FromArgb(255, 82, 82) : Color.FromArgb(0, 229, 255);

                using (SolidBrush bg = new SolidBrush(mBg))
                using (Pen pen = new Pen(mBorder, 2f))
                {
                    g.FillRectangle(bg, missionBox);
                    g.DrawRectangle(pen, missionBox);
                }

                using (Font clientFont = new Font("Segoe UI", 12f, FontStyle.Bold))
                using (Font subFont = new Font("Segoe UI", 8.5f))
                using (SolidBrush cBrush = new SolidBrush(Color.White))
                using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(170, 190, 215)))
                {
                    g.DrawString($"▶ 계약 진행 중: {c.ClientName} - {c.Title}", clientFont, cBrush, 28, panelY + 16);
                    g.DrawString($"요구 서버 성능: {c.RequiredRps:N0}  |  계약 준수 조건: {c.SlaRuleDescription}", subFont, subBrush, 28, panelY + 42);
                }

                int barX = 28;
                int barY = panelY + 70;
                int barW = w - 56;
                int barH = 28;
                Rectangle barRect = new Rectangle(barX, barY, barW, barH);

                double timeRatio = (double)c.RemainingSecs / Math.Max(1, c.DurationSecs);
                int timeFill = (int)(timeRatio * barW);

                using (SolidBrush bBg = new SolidBrush(Color.FromArgb(14, 18, 24)))
                using (Pen bPen = new Pen(Color.FromArgb(45, 55, 75), 1))
                {
                    g.FillRectangle(bBg, barRect);
                    g.DrawRectangle(bPen, barRect);
                }

                if (timeFill > 0)
                {
                    Color fCol = isWarning ? Color.FromArgb(255, 120, 80) : Color.FromArgb(0, 230, 118);
                    using (SolidBrush fb = new SolidBrush(fCol))
                    {
                        g.FillRectangle(fb, barX, barY, timeFill, barH);
                    }
                }

                using (Font tFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (SolidBrush tb = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString($"납기 완수까지: {c.RemainingSecs}초 남음 ({timeRatio * 100:F0}%)", tFont, tb, barRect, sf);
                }

                using (Font slaFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                using (SolidBrush sBrush = new SolidBrush(c.ViolationsCount == 0 ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82)))
                {
                    string slaText = c.ViolationsCount == 0 
                        ? "● 계약 준수 상태: 무결점 진행 중 (정상)" 
                        : $"▲ 계약 위반 경고: 누적 {c.ViolationsCount}회! (4회 이상 시 계약 파기 및 위약금 발생)";
                    g.DrawString(slaText, slaFont, sBrush, 28, panelY + 110);
                }

                using (Font rFont = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (SolidBrush rBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
                {
                    g.DrawString($"성공 완수 보수: +${c.RewardCash:N0}  |  위약금: -${c.PenaltyCash:N0}", rFont, rBrush, 28, panelY + 135);
                }

                _b2bCancelBtnRect = new Rectangle(w - 140, panelY + 145, 110, 32);
                using (SolidBrush cBg = new SolidBrush(Color.FromArgb(60, 25, 30)))
                using (Pen cPen = new Pen(Color.FromArgb(200, 60, 70), 1))
                using (Font bFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(255, 150, 160)))
                {
                    g.FillRectangle(cBg, _b2bCancelBtnRect);
                    g.DrawRectangle(cPen, _b2bCancelBtnRect);
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("계약 중도 파기", bFont, tBrush, _b2bCancelBtnRect, sf);
                }
            }
            else
            {
                int cardPad = 8;
                int cardW = (w - 20 - cardPad * 2) / 3;
                int cardH = 185;

                double currentRps = _state.CalculateTotalRps();

                for (int i = 0; i < b2b.AvailableOffers.Count && i < 3; i++)
                {
                    var offer = b2b.AvailableOffers[i];
                    int cx = 10 + i * (cardW + cardPad);
                    Rectangle oRect = new Rectangle(cx, panelY, cardW, cardH);
                    _b2bOfferBounds[i] = oRect;

                    bool canAccept = currentRps >= offer.RequiredRps;

                    using (SolidBrush ob = new SolidBrush(Color.FromArgb(20, 24, 34)))
                    using (Pen op = new Pen(ColorTranslator.FromHtml(offer.ClientColorHex ?? "#00E5FF"), 1.2f))
                    {
                        g.FillRectangle(ob, oRect);
                        g.DrawRectangle(op, oRect);
                    }

                    using (Font cNameFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (Font tierFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                    using (SolidBrush cNameBrush = new SolidBrush(Color.White))
                    using (SolidBrush badgeBg = new SolidBrush(ColorTranslator.FromHtml(offer.ClientColorHex ?? "#00E5FF")))
                    using (SolidBrush badgeText = new SolidBrush(Color.Black))
                    {
                        Rectangle badgeRect = new Rectangle(oRect.X + 8, oRect.Y + 8, 20, 16);
                        g.FillRectangle(badgeBg, badgeRect);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString(offer.TierBadge, tierFont, badgeText, badgeRect, sf);

                        g.DrawString(offer.ClientName, cNameFont, cNameBrush, oRect.X + 32, oRect.Y + 8);
                    }

                    using (Font tFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                    using (Font dFont = new Font("Segoe UI", 7.5f))
                    using (SolidBrush tBrush = new SolidBrush(Color.FromArgb(220, 235, 255)))
                    using (SolidBrush rpsBrush = new SolidBrush(canAccept ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82)))
                    using (SolidBrush dBrush = new SolidBrush(Color.FromArgb(145, 160, 185)))
                    using (SolidBrush wColorBrush = new SolidBrush(Color.FromArgb(255, 200, 100)))
                    {
                        g.DrawString(offer.Title, tFont, tBrush, new Rectangle(oRect.X + 8, oRect.Y + 30, oRect.Width - 16, 32));
                        g.DrawString($"⚡ 필요 서버 성능: {offer.RequiredRps:N0}", dFont, rpsBrush, oRect.X + 8, oRect.Y + 66);
                        g.DrawString($"● 기간: {offer.DurationSecs}초  |  보수: +${FormatCompactNumber(offer.RewardCash)}", dFont, dBrush, oRect.X + 8, oRect.Y + 84);
                        g.DrawString($"● 계약 조건: {offer.SlaRuleDescription}", dFont, wColorBrush, new Rectangle(oRect.X + 8, oRect.Y + 102, oRect.Width - 16, 28));
                    }

                    int btnH = 34;
                    Rectangle acceptRect = new Rectangle(oRect.X + 8, oRect.Bottom - btnH - 8, oRect.Width - 16, btnH);
                    _b2bAcceptBtnBounds[i] = acceptRect;

                    Color btnBg = canAccept ? Color.FromArgb(0, 140, 90) : Color.FromArgb(32, 36, 44);
                    Color btnBorder = canAccept ? Color.FromArgb(0, 230, 118) : Color.FromArgb(50, 56, 68);

                    using (SolidBrush bb = new SolidBrush(btnBg))
                    using (Pen bp = new Pen(btnBorder, canAccept ? 1.5f : 1f))
                    using (Font btnFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    using (SolidBrush bt = new SolidBrush(canAccept ? Color.White : Color.FromArgb(120, 130, 145)))
                    {
                        g.FillRectangle(bb, acceptRect);
                        g.DrawRectangle(bp, acceptRect);
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        string bText = canAccept ? $"외주 계약 수락 (+${FormatCompactNumber(offer.RewardCash)})" : "서버 성능 부족";
                        g.DrawString(bText, btnFont, bt, acceptRect, sf);
                    }
                }
            }
        }

        private void HandleB2BContractsClick(Point pt)
        {
            var b2b = _state.B2BMarket;
            if (_b2bRefreshBtnRect.Contains(pt))
            {
                b2b.RefreshOffers(_rand, _state.CalculateTotalRps());
                AddFloatingText("새로운 의뢰 도착!", pt.X, pt.Y, Color.FromArgb(0, 229, 255), false);
                Invalidate();
                return;
            }

            if (b2b.ActiveContract != null)
            {
                if (_b2bCancelBtnRect.Contains(pt))
                {
                    b2b.ActiveContract = null;
                    AddFloatingText("계약 중도 취소", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                    Invalidate();
                    return;
                }
            }
            else
            {
                for (int i = 0; i < _b2bAcceptBtnBounds.Length && i < b2b.AvailableOffers.Count; i++)
                {
                    if (_b2bAcceptBtnBounds[i].Contains(pt))
                    {
                        var offer = b2b.AvailableOffers[i];
                        if (_state.CalculateTotalRps() >= offer.RequiredRps)
                        {
                            b2b.ActiveContract = offer;
                            b2b.AvailableOffers.RemoveAt(i);
                            AddFloatingText($"★ {offer.ClientName} 계약 체결!", pt.X, pt.Y, Color.FromArgb(0, 229, 255), true);
                        }
                        else
                        {
                            AddFloatingText("서버 성능 부족으로 수락 불가!", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                        }
                        Invalidate();
                        return;
                    }
                }
            }
        }
        #endregion

        #region 3. Crypto Market Tab & Logic
        private void DrawCryptoMarketTab(Graphics g, int w, int topY)
        {
            var crypto = _state.CryptoMarket;
            crypto.InitHistory();

            using (Font titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (Font tickerFont = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 220, 245)))
            {
                bool isUp = crypto.PriceChangePercent >= 0;
                Color tickCol = isUp ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82);
                string sign = isUp ? "▲ +" : "▼ ";

                g.DrawString("▲ 바이트코인(BYT) 거래소  |  현재 시세: ", titleFont, titleBrush, 14, topY + 6);
                int off = (int)g.MeasureString("▲ 바이트코인(BYT) 거래소  |  현재 시세: ", titleFont).Width + 14;
                using (SolidBrush tb = new SolidBrush(tickCol))
                {
                    g.DrawString($"${crypto.CurrentPrice:N2} ({sign}{crypto.PriceChangePercent:F2}%)", tickerFont, tb, off, topY + 4);
                }
            }

            int panelY = topY + 28;
            int chartW = (int)(w * 0.55);
            int rightX = chartW + 18;
            int rightW = w - rightX - 10;

            Rectangle chartBox = new Rectangle(10, panelY, chartW, 170);
            using (SolidBrush cBg = new SolidBrush(Color.FromArgb(16, 20, 28)))
            using (Pen cPen = new Pen(Color.FromArgb(36, 46, 62), 1))
            {
                g.FillRectangle(cBg, chartBox);
                g.DrawRectangle(cPen, chartBox);
            }

            using (Pen gridPen = new Pen(Color.FromArgb(26, 34, 46), 1))
            {
                for (int y = chartBox.Y + 20; y < chartBox.Bottom; y += 35)
                {
                    g.DrawLine(gridPen, chartBox.X, y, chartBox.Right, y);
                }
            }

            if (crypto.PriceHistory.Count >= 2)
            {
                double minP = double.MaxValue, maxP = double.MinValue;
                foreach (var p in crypto.PriceHistory)
                {
                    if (p < minP) minP = p;
                    if (p > maxP) maxP = p;
                }
                if (maxP - minP < 5.0) { minP -= 2.5; maxP += 2.5; }

                PointF[] points = new PointF[crypto.PriceHistory.Count];
                float stepX = (chartBox.Width - 40) / (float)(crypto.PriceHistory.Count - 1);

                for (int i = 0; i < crypto.PriceHistory.Count; i++)
                {
                    float px = chartBox.X + 20 + i * stepX;
                    double norm = (crypto.PriceHistory[i] - minP) / (maxP - minP);
                    float py = (float)(chartBox.Bottom - 25 - norm * (chartBox.Height - 50));
                    points[i] = new PointF(px, py);
                }

                bool isBull = crypto.CurrentPrice >= crypto.PriceHistory[0];
                Color lineColor = isBull ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82);

                PointF[] fillPoly = new PointF[points.Length + 2];
                Array.Copy(points, fillPoly, points.Length);
                fillPoly[points.Length] = new PointF(points[points.Length - 1].X, chartBox.Bottom - 10);
                fillPoly[points.Length + 1] = new PointF(points[0].X, chartBox.Bottom - 10);

                using (SolidBrush areaBrush = new SolidBrush(Color.FromArgb(25, lineColor)))
                {
                    g.FillPolygon(areaBrush, fillPoly);
                }

                using (Pen lPen = new Pen(lineColor, 2f))
                {
                    g.DrawLines(lPen, points);
                }

                PointF lastPt = points[points.Length - 1];
                using (SolidBrush dotB = new SolidBrush(lineColor))
                using (Pen dotP = new Pen(Color.White, 1.5f))
                {
                    g.FillEllipse(dotB, lastPt.X - 4, lastPt.Y - 4, 8, 8);
                    g.DrawEllipse(dotP, lastPt.X - 4, lastPt.Y - 4, 8, 8);
                }
            }

            Rectangle newsBox = new Rectangle(10, chartBox.Bottom + 4, chartW, 26);
            using (SolidBrush nBg = new SolidBrush(Color.FromArgb(22, 28, 38)))
            using (Pen nPen = new Pen(Color.FromArgb(42, 54, 72), 1))
            using (Font nFont = new Font("Segoe UI", 7.5f))
            using (SolidBrush nBrush = new SolidBrush(Color.FromArgb(255, 200, 100)))
            {
                g.FillRectangle(nBg, newsBox);
                g.DrawRectangle(nPen, newsBox);
                StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString($"[NEWS] {crypto.MarketNews}", nFont, nBrush, new Rectangle(newsBox.X + 8, newsBox.Y, newsBox.Width - 16, newsBox.Height), sf);
            }

            // 우측 메인 패널: 내 투자 현황판 & 매매 콘솔
            Rectangle rightBox = new Rectangle(rightX, panelY, rightW, 200);
            using (SolidBrush rBg = new SolidBrush(Color.FromArgb(20, 24, 32)))
            using (Pen rPen = new Pen(Color.FromArgb(38, 48, 62), 1))
            {
                g.FillRectangle(rBg, rightBox);
                g.DrawRectangle(rPen, rightBox);
            }

            // 1. [★ 내 투자 현황판] 요약 카드
            Rectangle cardRect = new Rectangle(rightX + 8, panelY + 6, rightW - 16, 78);
            using (SolidBrush cardBg = new SolidBrush(Color.FromArgb(26, 32, 44)))
            using (Pen cardPen = new Pen(Color.FromArgb(48, 60, 80), 1))
            {
                g.FillRectangle(cardBg, cardRect);
                g.DrawRectangle(cardPen, cardRect);
            }

            // 뱃지 아이콘 (우측 상단 장식)
            DrawCryptoCoinBadge(g, new Rectangle(cardRect.Right - 36, cardRect.Y + 6, 28, 28));

            double valDollars = crypto.TotalValuation;
            double profitDollars = crypto.UnrealizedProfit;
            double profitRate = crypto.ProfitRatePercent;
            bool hasCoins = crypto.MinedCoins > 0.000001;

            using (Font valFont = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (Font profitFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (Font infoFont = new Font("Segoe UI", 7.6f))
            using (Font labelFont = new Font("Segoe UI", 7f))
            using (SolidBrush goldBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(140, 155, 175)))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(220, 230, 245)))
            {
                // 총 평가금액
                g.DrawString("내 코인 평가 금액", labelFont, labelBrush, cardRect.X + 8, cardRect.Y + 5);
                g.DrawString($"${FormatCompactNumber(valDollars)}", valFont, goldBrush, cardRect.X + 8, cardRect.Y + 16);

                // 평가 손익 및 수익률 (수익: 초록, 손실: 빨강, 무보유: 회색)
                if (hasCoins)
                {
                    bool isProfit = profitDollars >= 0;
                    Color pColor = isProfit ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82);
                    string pSign = isProfit ? "+$" : "-$";
                    string rSign = isProfit ? "+" : "";
                    using (SolidBrush pb = new SolidBrush(pColor))
                    {
                        string profitStr = $"{pSign}{Math.Abs(profitDollars):N1} ({rSign}{profitRate:F1}%)";
                        g.DrawString(profitStr, profitFont, pb, cardRect.X + 115, cardRect.Y + 18);
                    }
                }
                else
                {
                    using (SolidBrush pb = new SolidBrush(Color.FromArgb(120, 135, 150)))
                    {
                        g.DrawString("보유 코인 없음", profitFont, pb, cardRect.X + 115, cardRect.Y + 18);
                    }
                }

                // 구분선
                using (Pen sepPen = new Pen(Color.FromArgb(42, 52, 68), 1))
                {
                    g.DrawLine(sepPen, cardRect.X + 8, cardRect.Y + 39, cardRect.Right - 8, cardRect.Y + 39);
                }

                // 2열 상세 정보: [매수 평단가 / 투자 원금] & [보유 수량 / 채굴 속도]
                int col1X = cardRect.X + 8;
                int col2X = cardRect.X + (cardRect.Width / 2) + 2;

                string avgBuyStr = hasCoins 
                    ? (crypto.TotalInvestedUsd > 0.001 ? $"${crypto.AverageBuyPrice:N1}" : "$0.0 (무료 채굴)")
                    : "-";
                string investedStr = crypto.TotalInvestedUsd > 0.001 ? $"${FormatCompactNumber(crypto.TotalInvestedUsd)}" : "$0";

                g.DrawString($"매수 평단가: {avgBuyStr}", infoFont, textBrush, col1X, cardRect.Y + 43);
                g.DrawString($"투자 원금: {investedStr}", infoFont, textBrush, col2X, cardRect.Y + 43);

                double rps = _state.CalculateTotalRps();
                double mineRate = (crypto.MiningHashPercent > 0 && rps > 0)
                    ? (rps * (crypto.MiningHashPercent / 100.0)) / Math.Max(20.0, crypto.CurrentPrice * 2.2)
                    : 0.0;

                g.DrawString($"보유 수량: {crypto.MinedCoins:F4} BYT", infoFont, textBrush, col1X, cardRect.Y + 59);
                g.DrawString($"채굴 속도: +{mineRate:F4}/초", infoFont, textBrush, col2X, cardRect.Y + 59);
            }

            // 2. 서버 채굴 전환 비율 버튼 (Y: panelY + 89)
            using (Font secFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush secBrush = new SolidBrush(Color.FromArgb(180, 200, 225)))
            {
                g.DrawString("⚡ 서버 자원 채굴 전환 비율:", secFont, secBrush, rightX + 12, panelY + 89);
            }

            int btnW = (rightW - 36) / 4;
            int btnY = panelY + 106;
            _cryptoHash0Rect = new Rectangle(rightX + 12, btnY, btnW, 20);
            _cryptoHash25Rect = new Rectangle(rightX + 12 + btnW + 4, btnY, btnW, 20);
            _cryptoHash50Rect = new Rectangle(rightX + 12 + (btnW + 4) * 2, btnY, btnW, 20);
            _cryptoHash100Rect = new Rectangle(rightX + 12 + (btnW + 4) * 3, btnY, btnW, 20);

            DrawToggleButton(g, _cryptoHash0Rect, "0%", crypto.MiningHashPercent == 0);
            DrawToggleButton(g, _cryptoHash25Rect, "25%", crypto.MiningHashPercent == 25);
            DrawToggleButton(g, _cryptoHash50Rect, "50%", crypto.MiningHashPercent == 50);
            DrawToggleButton(g, _cryptoHash100Rect, "100%", crypto.MiningHashPercent == 100);

            // 3. 간편 매수 & 매도 버튼 (2행 2열)
            int tBtnW = (rightW - 28) / 2;
            int row1Y = panelY + 132;
            int row2Y = panelY + 164;

            _cryptoBuy1kBtnRect = new Rectangle(rightX + 10, row1Y, tBtnW, 26);
            _cryptoBuyMaxBtnRect = new Rectangle(rightX + 18 + tBtnW, row1Y, tBtnW, 26);

            _cryptoSell50BtnRect = new Rectangle(rightX + 10, row2Y, tBtnW, 26);
            _cryptoSellAllBtnRect = new Rectangle(rightX + 18 + tBtnW, row2Y, tBtnW, 26);

            DrawTradingButton(g, _cryptoBuy1kBtnRect, "▲ $1,000 매수", Color.FromArgb(24, 110, 220));
            DrawTradingButton(g, _cryptoBuyMaxBtnRect, "▲ 최대 풀매수 (MAX)", Color.FromArgb(30, 136, 229));

            DrawTradingButton(g, _cryptoSell50BtnRect, "▼ 50% 절반 매도", Color.FromArgb(0, 140, 90));
            DrawTradingButton(g, _cryptoSellAllBtnRect, "▼ 전량 매도 (100%)", Color.FromArgb(0, 180, 100));
        }

        private void DrawTradingButton(Graphics g, Rectangle rect, string text, Color accent)
        {
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(35, accent)))
            using (Pen pen = new Pen(accent, 1.2f))
            using (Font f = new Font("Segoe UI", 7.8f, FontStyle.Bold))
            using (SolidBrush tb = new SolidBrush(Color.White))
            {
                g.FillRectangle(bg, rect);
                g.DrawRectangle(pen, rect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, f, tb, rect, sf);
            }
        }

        private void DrawCryptoCoinBadge(Graphics g, Rectangle bounds)
        {
            int cx = bounds.X + bounds.Width / 2;
            int cy = bounds.Y + bounds.Height / 2;
            int radius = Math.Min(bounds.Width, bounds.Height) / 2;

            using (SolidBrush coinBg = new SolidBrush(Color.FromArgb(24, 32, 48)))
            using (Pen goldRing = new Pen(Color.FromArgb(255, 215, 64), 2f))
            using (Pen cyanInner = new Pen(Color.FromArgb(0, 229, 255), 1f))
            using (Font f = new Font("Segoe UI", 12f, FontStyle.Bold))
            using (SolidBrush bBrush = new SolidBrush(Color.FromArgb(255, 215, 64)))
            using (Pen hashPen = new Pen(Color.FromArgb(255, 215, 64), 1.5f))
            {
                Rectangle ringRect = new Rectangle(cx - radius + 2, cy - radius + 2, (radius - 2) * 2, (radius - 2) * 2);
                g.FillEllipse(coinBg, ringRect);
                g.DrawEllipse(goldRing, ringRect);
                g.DrawEllipse(cyanInner, ringRect.X + 3, ringRect.Y + 3, ringRect.Width - 6, ringRect.Height - 6);

                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("B", f, bBrush, cx, cy - 1, sf);
                g.DrawLine(hashPen, cx - 2, cy - radius + 5, cx - 2, cy - radius + 9);
                g.DrawLine(hashPen, cx + 2, cy - radius + 5, cx + 2, cy - radius + 9);
                g.DrawLine(hashPen, cx - 2, cy + radius - 9, cx - 2, cy + radius - 5);
                g.DrawLine(hashPen, cx + 2, cy + radius - 9, cx + 2, cy + radius - 5);
            }
        }

        private string GetCategoryShortName(int id)
        {
            switch (id)
            {
                case 0: return "인디 게임";
                case 1: return "업무 관리";
                case 2: return "인공지능";
                case 3: return "보안 백신";
                case 4: return "클라우드";
                default: return "프로그램";
            }
        }

        private void DrawSoftwareCategoryIcon(Graphics g, Rectangle bounds, int categoryId, Color accent, bool isSelected)
        {
            int cx = bounds.X + bounds.Width / 2;
            int cy = bounds.Y + bounds.Height / 2;

            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(isSelected ? 60 : 30, accent)))
            using (Pen borderPen = new Pen(accent, isSelected ? 1.5f : 1f))
            {
                g.FillEllipse(bgBrush, bounds);
                g.DrawEllipse(borderPen, bounds);
            }

            switch (categoryId)
            {
                case 0:
                    // 🎮 인디 모바일 게임: 레트로 콘솔 게임패드
                    using (SolidBrush padBody = new SolidBrush(Color.FromArgb(45, 55, 75)))
                    using (Pen padEdge = new Pen(Color.FromArgb(255, 64, 129), 1f))
                    using (SolidBrush btnPink = new SolidBrush(Color.FromArgb(255, 64, 129)))
                    using (SolidBrush btnCyan = new SolidBrush(Color.FromArgb(0, 229, 255)))
                    using (Pen dpadPen = new Pen(Color.FromArgb(220, 230, 245), 1.2f))
                    {
                        Rectangle padRect = new Rectangle(cx - 8, cy - 5, 16, 10);
                        g.FillRectangle(padBody, padRect);
                        g.DrawRectangle(padEdge, padRect);

                        // 좌측 십자키 (+)
                        g.DrawLine(dpadPen, cx - 5, cy - 3, cx - 5, cy + 3);
                        g.DrawLine(dpadPen, cx - 8, cy, cx - 2, cy);

                        // 우측 버튼 2개
                        g.FillEllipse(btnPink, cx + 3, cy - 3, 3, 3);
                        g.FillEllipse(btnCyan, cx + 1, cy + 1, 3, 3);
                    }
                    break;

                case 1:
                    // 📊 B2B 전사 ERP: 상승 성장 차트 막대 & 트렌드 화살표
                    using (Pen baseLine = new Pen(Color.FromArgb(140, 160, 190), 1f))
                    using (SolidBrush bar1 = new SolidBrush(Color.FromArgb(68, 138, 255)))
                    using (SolidBrush bar2 = new SolidBrush(Color.FromArgb(0, 229, 255)))
                    using (SolidBrush bar3 = new SolidBrush(Color.FromArgb(0, 230, 118)))
                    using (Pen trendPen = new Pen(Color.FromArgb(255, 215, 64), 1.2f))
                    {
                        g.DrawLine(baseLine, cx - 7, cy + 5, cx + 7, cy + 5);
                        g.FillRectangle(bar1, cx - 6, cy + 1, 3, 4);
                        g.FillRectangle(bar2, cx - 1, cy - 3, 3, 8);
                        g.FillRectangle(bar3, cx + 4, cy - 6, 3, 11);

                        g.DrawLine(trendPen, cx - 6, cy + 1, cx + 5, cy - 5);
                        g.DrawLine(trendPen, cx + 5, cy - 5, cx + 2, cy - 5);
                        g.DrawLine(trendPen, cx + 5, cy - 5, cx + 5, cy - 2);
                    }
                    break;

                case 2:
                    // 🧠 생성형 AI 모델: 인공신경망 코어 & 시냅스 노드
                    using (Pen synapsePen = new Pen(Color.FromArgb(179, 136, 255), 1f))
                    using (SolidBrush coreNode = new SolidBrush(Color.White))
                    using (SolidBrush subNode = new SolidBrush(Color.FromArgb(179, 136, 255)))
                    {
                        PointF c0 = new PointF(cx, cy);
                        PointF p1 = new PointF(cx - 5, cy - 5);
                        PointF p2 = new PointF(cx + 5, cy - 5);
                        PointF p3 = new PointF(cx - 5, cy + 5);
                        PointF p4 = new PointF(cx + 5, cy + 5);

                        g.DrawLine(synapsePen, c0, p1);
                        g.DrawLine(synapsePen, c0, p2);
                        g.DrawLine(synapsePen, c0, p3);
                        g.DrawLine(synapsePen, c0, p4);

                        g.FillEllipse(subNode, p1.X - 2, p1.Y - 2, 4, 4);
                        g.FillEllipse(subNode, p2.X - 2, p2.Y - 2, 4, 4);
                        g.FillEllipse(subNode, p3.X - 2, p3.Y - 2, 4, 4);
                        g.FillEllipse(subNode, p4.X - 2, p4.Y - 2, 4, 4);
                        g.FillEllipse(coreNode, c0.X - 3, c0.Y - 3, 6, 6);
                    }
                    break;

                case 3:
                    // 🛡️ 제로트러스트 보안 백신: 방패 & 확인 체크마크
                    using (SolidBrush shieldBg = new SolidBrush(Color.FromArgb(0, 230, 118)))
                    using (Pen shieldBorder = new Pen(Color.FromArgb(220, 255, 240), 1.2f))
                    using (Pen checkPen = new Pen(Color.FromArgb(20, 24, 32), 1.5f))
                    {
                        Point[] shieldPts = {
                            new Point(cx - 6, cy - 6),
                            new Point(cx + 6, cy - 6),
                            new Point(cx + 6, cy + 1),
                            new Point(cx, cy + 7),
                            new Point(cx - 6, cy + 1)
                        };
                        g.FillPolygon(shieldBg, shieldPts);
                        g.DrawPolygon(shieldBorder, shieldPts);

                        g.DrawLine(checkPen, cx - 4, cy, cx - 1, cy + 3);
                        g.DrawLine(checkPen, cx - 1, cy + 3, cx + 4, cy - 2);
                    }
                    break;

                case 4:
                    // 🌌 분산 클라우드 OS: 클라우드 & 궤도 링
                    using (Pen orbitPen = new Pen(Color.FromArgb(255, 215, 64), 1.2f))
                    using (SolidBrush cloudBrush = new SolidBrush(Color.FromArgb(255, 235, 120)))
                    using (SolidBrush satNode = new SolidBrush(Color.FromArgb(0, 229, 255)))
                    {
                        g.FillEllipse(cloudBrush, cx - 6, cy - 3, 7, 7);
                        g.FillEllipse(cloudBrush, cx - 2, cy - 5, 8, 8);
                        g.FillEllipse(cloudBrush, cx + 2, cy - 2, 6, 6);

                        g.DrawEllipse(orbitPen, cx - 8, cy - 4, 16, 8);
                        g.FillEllipse(satNode, cx - 7, cy - 2, 3, 3);
                        g.FillEllipse(satNode, cx + 5, cy + 1, 3, 3);
                    }
                    break;
            }
        }

        private void HandleCryptoMarketClick(Point pt)
        {
            var crypto = _state.CryptoMarket;

            if (_cryptoHash0Rect.Contains(pt)) { crypto.MiningHashPercent = 0; Invalidate(); return; }
            if (_cryptoHash25Rect.Contains(pt)) { crypto.MiningHashPercent = 25; Invalidate(); return; }
            if (_cryptoHash50Rect.Contains(pt)) { crypto.MiningHashPercent = 50; Invalidate(); return; }
            if (_cryptoHash100Rect.Contains(pt)) { crypto.MiningHashPercent = 100; Invalidate(); return; }

            // 1. $1,000 매수 (자금 부족 시 남은 잔액 전액 매수)
            if (_cryptoBuy1kBtnRect.Contains(pt))
            {
                double spend = Math.Min(1000.0, _state.Money);
                if (spend >= 1.0)
                {
                    double coins = spend / crypto.CurrentPrice;
                    _state.Money -= spend;
                    crypto.MinedCoins += coins;
                    crypto.TotalInvestedUsd += spend;
                    AddFloatingText($"BYT 매수: +{coins:F2}개 (평단: ${crypto.AverageBuyPrice:N1})", pt.X, pt.Y, Color.FromArgb(0, 229, 255), true);
                    Invalidate();
                }
                else
                {
                    AddFloatingText("매수 자금 부족! ($1 이상 필요)", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                }
                return;
            }

            // 2. 최대 풀매수
            if (_cryptoBuyMaxBtnRect.Contains(pt))
            {
                double spend = _state.Money;
                if (spend >= 1.0)
                {
                    double coins = spend / crypto.CurrentPrice;
                    _state.Money = 0;
                    crypto.MinedCoins += coins;
                    crypto.TotalInvestedUsd += spend;
                    AddFloatingText($"BYT 풀매수: +{coins:F2}개 (평단: ${crypto.AverageBuyPrice:N1})", pt.X, pt.Y, Color.FromArgb(0, 229, 255), true);
                    Invalidate();
                }
                else
                {
                    AddFloatingText("매수 자금 부족!", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                }
                return;
            }

            // 3. 50% 절반 매도
            if (_cryptoSell50BtnRect.Contains(pt))
            {
                double sellRatio = 0.50;
                double coinsToSell = crypto.MinedCoins * sellRatio;
                if (coinsToSell > 0.00001)
                {
                    double proceeds = coinsToSell * crypto.CurrentPrice;
                    double costBasis = crypto.TotalInvestedUsd * sellRatio;
                    double profit = proceeds - costBasis;

                    crypto.MinedCoins -= coinsToSell;
                    crypto.TotalInvestedUsd = Math.Max(0, crypto.TotalInvestedUsd - costBasis);
                    if (crypto.MinedCoins < 0.00001)
                    {
                        crypto.MinedCoins = 0;
                        crypto.TotalInvestedUsd = 0;
                    }
                    crypto.TotalRealizedProfit += profit;
                    _state.Money += proceeds;
                    _state.CurrentRunEarned += proceeds;
                    _state.TotalEarned += proceeds;

                    string profitStr = (costBasis > 0.001) 
                        ? (profit >= 0 ? $"+${profit:N0} 수익" : $"-${Math.Abs(profit):N0} 손실")
                        : "+100% 순수익 (채굴분)";
                    Color fCol = profit >= 0 ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82);
                    AddFloatingText($"50% 매도: +${proceeds:N0} ({profitStr})", pt.X, pt.Y, fCol, true);
                    Invalidate();
                }
                else
                {
                    AddFloatingText("매도할 코인이 없습니다!", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                }
                return;
            }

            // 4. 전량 매도
            if (_cryptoSellAllBtnRect.Contains(pt))
            {
                double coinsToSell = crypto.MinedCoins;
                if (coinsToSell > 0.00001)
                {
                    double proceeds = coinsToSell * crypto.CurrentPrice;
                    double costBasis = crypto.TotalInvestedUsd;
                    double profit = proceeds - costBasis;

                    crypto.MinedCoins = 0;
                    crypto.TotalInvestedUsd = 0;
                    crypto.TotalRealizedProfit += profit;
                    _state.Money += proceeds;
                    _state.CurrentRunEarned += proceeds;
                    _state.TotalEarned += proceeds;

                    string profitStr = (costBasis > 0.001) 
                        ? (profit >= 0 ? $"+${profit:N0} 수익" : $"-${Math.Abs(profit):N0} 손실")
                        : "+100% 순수익 (채굴분)";
                    Color fCol = profit >= 0 ? Color.FromArgb(0, 230, 118) : Color.FromArgb(255, 82, 82);
                    AddFloatingText($"전량 매도: +${proceeds:N0} ({profitStr})", pt.X, pt.Y, fCol, true);
                    Invalidate();
                }
                else
                {
                    AddFloatingText("매도할 코인이 없습니다!", pt.X, pt.Y, Color.FromArgb(255, 82, 82), false);
                }
                return;
            }
        }
        #endregion

        private void DrawUnitVectorIcon(Graphics g, Rectangle bounds, string id, Color accent)
        {
            // 배경 서클 뱃지
            using (SolidBrush badgeBg = new SolidBrush(Color.FromArgb(30, accent)))
            using (Pen badgePen = new Pen(Color.FromArgb(85, accent), 1.2f))
            {
                g.FillEllipse(badgeBg, bounds);
                g.DrawEllipse(badgePen, bounds);
            }

            int cx = bounds.X + bounds.Width / 2;
            int cy = bounds.Y + bounds.Height / 2;

            switch (id)
            {
                case "rpi":
                    // 녹색 PCB 기판 + 중앙 칩 + 상단 GPIO 핀
                    using (SolidBrush pcb = new SolidBrush(Color.FromArgb(46, 125, 50)))
                    using (SolidBrush chip = new SolidBrush(Color.FromArgb(33, 33, 33)))
                    using (Pen gold = new Pen(Color.FromArgb(255, 215, 64), 1.2f))
                    {
                        g.FillRectangle(pcb, cx - 9, cy - 7, 18, 14);
                        g.FillRectangle(chip, cx - 4, cy - 2, 8, 6);
                        for (int i = -6; i <= 6; i += 3)
                        {
                            g.DrawLine(gold, cx + i, cy - 7, cx + i, cy - 5);
                        }
                    }
                    break;

                case "tower":
                    // 슬릭 오피스 PC 타워 섀시
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(55, 71, 79)))
                    using (Pen edge = new Pen(Color.FromArgb(144, 164, 174), 1f))
                    using (SolidBrush pwr = new SolidBrush(Color.FromArgb(255, 167, 38)))
                    {
                        g.FillRectangle(body, cx - 6, cy - 9, 12, 18);
                        g.DrawRectangle(edge, cx - 6, cy - 9, 12, 18);
                        g.FillEllipse(pwr, cx - 2, cy - 6, 4, 4);
                        g.DrawLine(edge, cx - 4, cy + 2, cx + 3, cy + 2);
                        g.DrawLine(edge, cx - 4, cy + 5, cx + 3, cy + 5);
                    }
                    break;

                case "blade1u":
                    // 1U 랙 블레이드 서버 본체 + 상태 LED
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(38, 50, 56)))
                    using (Pen edge = new Pen(Color.FromArgb(0, 230, 118), 1f))
                    using (SolidBrush led1 = new SolidBrush(Color.FromArgb(0, 229, 255)))
                    using (SolidBrush led2 = new SolidBrush(Color.FromArgb(0, 230, 118)))
                    {
                        g.FillRectangle(body, cx - 11, cy - 4, 22, 9);
                        g.DrawRectangle(edge, cx - 11, cy - 4, 22, 9);
                        g.FillEllipse(led1, cx - 8, cy - 1, 3, 3);
                        g.FillEllipse(led2, cx - 3, cy - 1, 3, 3);
                        using (Pen vent = new Pen(Color.FromArgb(90, 110, 130), 1f))
                        {
                            g.DrawLine(vent, cx + 3, cy - 2, cx + 3, cy + 3);
                            g.DrawLine(vent, cx + 6, cy - 2, cx + 6, cy + 3);
                        }
                    }
                    break;

                case "xeon4u":
                    // 4U 제온 워크스테이션 + 듀얼 팬
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(33, 43, 54)))
                    using (Pen edge = new Pen(Color.FromArgb(0, 229, 255), 1.2f))
                    {
                        g.FillRectangle(body, cx - 10, cy - 8, 20, 16);
                        g.DrawRectangle(edge, cx - 10, cy - 8, 20, 16);
                        using (Pen fanP = new Pen(Color.FromArgb(0, 229, 255), 1f))
                        {
                            g.DrawEllipse(fanP, cx - 7, cy - 3, 6, 6);
                            g.DrawEllipse(fanP, cx + 1, cy - 3, 6, 6);
                        }
                    }
                    break;

                case "gpu_h100":
                    // 엔비디아 H100 GPU 가속기 카드 + PCIe 골드 핀
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(69, 39, 160)))
                    using (Pen edge = new Pen(Color.FromArgb(179, 136, 255), 1.2f))
                    using (SolidBrush gold = new SolidBrush(Color.FromArgb(255, 215, 64)))
                    {
                        g.FillRectangle(body, cx - 11, cy - 6, 22, 12);
                        g.DrawRectangle(edge, cx - 11, cy - 6, 22, 12);
                        g.FillRectangle(gold, cx - 7, cy + 5, 14, 2);
                        using (Pen fan = new Pen(Color.FromArgb(179, 136, 255), 1f))
                        {
                            g.DrawEllipse(fan, cx - 7, cy - 3, 5, 5);
                            g.DrawEllipse(fan, cx + 2, cy - 3, 5, 5);
                        }
                    }
                    break;

                case "quantum":
                    // 양자 초전도 큐비트 코어 + 원자 궤도 링
                    using (Pen orbit = new Pen(Color.FromArgb(105, 240, 174), 1.2f))
                    using (SolidBrush core = new SolidBrush(Color.White))
                    {
                        g.DrawEllipse(orbit, cx - 9, cy - 4, 18, 8);
                        g.DrawEllipse(orbit, cx - 4, cy - 9, 8, 18);
                        g.FillEllipse(core, cx - 2, cy - 2, 5, 5);
                    }
                    break;

                case "outlet":
                    // 전원 벽면 콘센트
                    using (SolidBrush face = new SolidBrush(Color.FromArgb(236, 239, 241)))
                    using (Pen edge = new Pen(Color.FromArgb(176, 190, 197), 1f))
                    using (SolidBrush pin = new SolidBrush(Color.FromArgb(38, 50, 56)))
                    {
                        g.FillEllipse(face, cx - 8, cy - 8, 16, 16);
                        g.DrawEllipse(edge, cx - 8, cy - 8, 16, 16);
                        g.FillRectangle(pin, cx - 4, cy - 3, 2, 6);
                        g.FillRectangle(pin, cx + 2, cy - 3, 2, 6);
                    }
                    break;

                case "multitab":
                    // 서지 보호 6구 멀티탭 + 적색 스위치
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(55, 71, 79)))
                    using (Pen edge = new Pen(Color.FromArgb(255, 167, 38), 1f))
                    using (SolidBrush sw = new SolidBrush(Color.FromArgb(255, 60, 0)))
                    using (SolidBrush hole = new SolidBrush(Color.FromArgb(30, 36, 42)))
                    {
                        g.FillRectangle(body, cx - 10, cy - 4, 20, 9);
                        g.DrawRectangle(edge, cx - 10, cy - 4, 20, 9);
                        g.FillRectangle(sw, cx - 8, cy - 2, 3, 5);
                        g.FillEllipse(hole, cx - 2, cy - 2, 4, 4);
                        g.FillEllipse(hole, cx + 4, cy - 2, 4, 4);
                    }
                    break;

                case "ups_mini":
                    // UPS 무정전 전원 배터리 + 노란 번개
                    using (SolidBrush body = new SolidBrush(Color.FromArgb(38, 50, 56)))
                    using (Pen edge = new Pen(Color.FromArgb(77, 208, 225), 1.2f))
                    using (SolidBrush bolt = new SolidBrush(Color.FromArgb(255, 215, 64)))
                    {
                        g.FillRectangle(body, cx - 7, cy - 9, 14, 18);
                        g.DrawRectangle(edge, cx - 7, cy - 9, 14, 18);
                        PointF[] pts = { new PointF(cx + 1, cy - 5), new PointF(cx - 3, cy), new PointF(cx, cy), new PointF(cx - 1, cy + 5), new PointF(cx + 4, cy - 1), new PointF(cx + 1, cy - 1) };
                        g.FillPolygon(bolt, pts);
                    }
                    break;

                case "high_voltage":
                    // 3상 고압 송전철탑 트러스
                    using (Pen pylon = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    {
                        g.DrawLine(pylon, cx, cy - 9, cx - 7, cy + 9);
                        g.DrawLine(pylon, cx, cy - 9, cx + 7, cy + 9);
                        g.DrawLine(pylon, cx - 8, cy - 2, cx + 8, cy - 2);
                        g.DrawLine(pylon, cx - 5, cy + 3, cx + 5, cy + 3);
                        g.DrawLine(pylon, cx - 7, cy + 9, cx + 7, cy + 9);
                    }
                    break;

                case "solar_array":
                    // 옥상 태양광 패널 그리드 + 태양
                    using (SolidBrush panel = new SolidBrush(Color.FromArgb(21, 101, 192)))
                    using (Pen grid = new Pen(Color.FromArgb(100, 181, 246), 1f))
                    using (SolidBrush sun = new SolidBrush(Color.FromArgb(255, 213, 79)))
                    {
                        Point[] poly = { new Point(cx - 8, cy + 5), new Point(cx + 6, cy + 5), new Point(cx + 9, cy - 5), new Point(cx - 5, cy - 5) };
                        g.FillPolygon(panel, poly);
                        g.DrawPolygon(grid, poly);
                        g.DrawLine(grid, cx - 6, cy, cx + 7, cy);
                        g.DrawLine(grid, cx + 1, cy - 5, cx - 1, cy + 5);
                        g.FillEllipse(sun, cx - 9, cy - 9, 5, 5);
                    }
                    break;

                case "smr_reactor":
                    // SMR 소형 원자로 방사능 심볼 (라디에이션 트레포일)
                    using (SolidBrush lime = new SolidBrush(Color.FromArgb(118, 255, 3)))
                    using (Pen limePen = new Pen(Color.FromArgb(118, 255, 3), 1.2f))
                    {
                        g.DrawEllipse(limePen, cx - 8, cy - 8, 16, 16);
                        g.FillPie(lime, cx - 7, cy - 7, 14, 14, 30, 60);
                        g.FillPie(lime, cx - 7, cy - 7, 14, 14, 150, 60);
                        g.FillPie(lime, cx - 7, cy - 7, 14, 14, 270, 60);
                        using (SolidBrush centerBg = new SolidBrush(Color.FromArgb(20, 24, 32)))
                        {
                            g.FillEllipse(centerBg, cx - 3, cy - 3, 6, 6);
                            g.FillEllipse(lime, cx - 1, cy - 1, 3, 3);
                        }
                    }
                    break;

                case "passive":
                    // 자연 대류 공기 흐름 웨이브
                    using (Pen wave = new Pen(Color.FromArgb(129, 199, 132), 1.5f))
                    {
                        g.DrawArc(wave, cx - 8, cy - 5, 8, 5, 180, 180);
                        g.DrawArc(wave, cx - 2, cy - 5, 8, 5, 0, 180);
                        g.DrawArc(wave, cx - 6, cy, 8, 5, 180, 180);
                        g.DrawArc(wave, cx, cy, 8, 5, 0, 180);
                        g.DrawArc(wave, cx - 8, cy + 5, 8, 5, 180, 180);
                        g.DrawArc(wave, cx - 2, cy + 5, 8, 5, 0, 180);
                    }
                    break;

                case "desk_fan":
                    // 3엽 탁상용 선풍기
                    using (Pen ring = new Pen(Color.FromArgb(79, 195, 247), 1.2f))
                    using (SolidBrush blade = new SolidBrush(Color.FromArgb(79, 195, 247)))
                    {
                        g.DrawEllipse(ring, cx - 8, cy - 8, 16, 16);
                        g.FillEllipse(blade, cx - 2, cy - 2, 4, 4);
                        for (int i = 0; i < 3; i++)
                        {
                            float a = _fanAngle + i * 120;
                            float rad = (float)(a * Math.PI / 180.0);
                            float bx = cx + (float)Math.Cos(rad) * 6;
                            float by = cy + (float)Math.Sin(rad) * 6;
                            g.DrawLine(ring, cx, cy, bx, by);
                        }
                    }
                    break;

                case "crac_unit":
                    // 항온항습기 눈꽃 결정 (Snowflake)
                    using (Pen flake = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            float a = i * 60;
                            float rad = (float)(a * Math.PI / 180.0);
                            float x1 = cx + (float)Math.Cos(rad) * 7;
                            float y1 = cy + (float)Math.Sin(rad) * 7;
                            float x2 = cx - (float)Math.Cos(rad) * 7;
                            float y2 = cy - (float)Math.Sin(rad) * 7;
                            g.DrawLine(flake, x1, y1, x2, y2);
                        }
                        g.DrawEllipse(flake, cx - 2, cy - 2, 5, 5);
                    }
                    break;

                case "water_loop":
                    // 커스텀 수냉 워터 루프 + 물방울
                    using (Pen pipe = new Pen(Color.FromArgb(41, 121, 255), 2f))
                    using (SolidBrush drop = new SolidBrush(Color.FromArgb(0, 229, 255)))
                    {
                        g.DrawArc(pipe, cx - 7, cy - 7, 14, 14, 45, 270);
                        g.FillEllipse(drop, cx - 2, cy - 2, 5, 5);
                    }
                    break;

                case "immersion":
                    // 액침 냉각 탱크 + 냉매 기포
                    using (Pen tank = new Pen(Color.FromArgb(179, 136, 255), 1.5f))
                    using (SolidBrush fluid = new SolidBrush(Color.FromArgb(90, 80, 40, 160)))
                    using (SolidBrush bubble = new SolidBrush(Color.FromArgb(200, 225, 255)))
                    {
                        g.FillRectangle(fluid, cx - 7, cy - 2, 14, 9);
                        g.DrawRectangle(tank, cx - 7, cy - 7, 14, 14);
                        g.FillEllipse(bubble, cx - 4, cy + 1, 2, 2);
                        g.FillEllipse(bubble, cx + 2, cy + 3, 2, 2);
                        g.FillEllipse(bubble, cx - 1, cy + 4, 3, 3);
                    }
                    break;

                case "quantum_cryo":
                    // 극저온 헬륨 희석 냉동기 (골드 샹들리에)
                    using (Pen gold = new Pen(Color.FromArgb(255, 215, 64), 1.5f))
                    using (SolidBrush cold = new SolidBrush(Color.FromArgb(105, 240, 174)))
                    {
                        g.DrawLine(gold, cx - 8, cy - 6, cx + 8, cy - 6);
                        g.DrawLine(gold, cx - 6, cy - 1, cx + 6, cy - 1);
                        g.DrawLine(gold, cx - 3, cy + 4, cx + 3, cy + 4);
                        g.DrawLine(gold, cx, cy - 6, cx, cy + 4);
                        g.FillEllipse(cold, cx - 2, cy + 6, 4, 4);
                    }
                    break;

                case "skill_power_eff":
                    using (SolidBrush bolt = new SolidBrush(Color.FromArgb(255, 215, 64)))
                    {
                        PointF[] pts = { new PointF(cx, cy - 7), new PointF(cx - 4, cy), new PointF(cx - 1, cy), new PointF(cx - 2, cy + 6), new PointF(cx + 4, cy), new PointF(cx + 1, cy) };
                        g.FillPolygon(bolt, pts);
                    }
                    break;

                case "skill_cooling_boost":
                    using (Pen ice = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    {
                        g.DrawLine(ice, cx, cy - 7, cx, cy + 7);
                        g.DrawLine(ice, cx - 6, cy - 3, cx + 6, cy + 3);
                        g.DrawLine(ice, cx - 6, cy + 3, cx + 6, cy - 3);
                    }
                    break;

                case "skill_backbone":
                    using (Pen net = new Pen(Color.FromArgb(0, 229, 255), 1.2f))
                    using (SolidBrush node = new SolidBrush(Color.White))
                    {
                        g.DrawEllipse(net, cx - 7, cy - 7, 14, 14);
                        g.DrawLine(net, cx - 7, cy, cx + 7, cy);
                        g.DrawLine(net, cx, cy - 7, cx, cy + 7);
                        g.FillEllipse(node, cx - 2, cy - 2, 4, 4);
                    }
                    break;

                case "skill_battery_max":
                    using (Pen bat = new Pen(Color.FromArgb(0, 230, 118), 1.2f))
                    using (SolidBrush bar = new SolidBrush(Color.FromArgb(0, 230, 118)))
                    {
                        g.DrawRectangle(bat, cx - 5, cy - 7, 10, 14);
                        g.FillRectangle(bar, cx - 2, cy - 9, 4, 2);
                        g.FillRectangle(bar, cx - 3, cy - 4, 6, 9);
                    }
                    break;

                case "skill_crit_packet":
                    using (SolidBrush star = new SolidBrush(Color.FromArgb(255, 215, 64)))
                    using (Pen starP = new Pen(Color.FromArgb(255, 80, 220), 1.5f))
                    {
                        g.FillRectangle(star, cx - 3, cy - 3, 6, 6);
                        g.DrawLine(starP, cx, cy - 7, cx, cy + 7);
                        g.DrawLine(starP, cx - 7, cy, cx + 7, cy);
                    }
                    break;

                case "skill_ai_devops":
                    using (SolidBrush bot = new SolidBrush(Color.FromArgb(38, 166, 154)))
                    using (Pen visor = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    {
                        g.FillRectangle(bot, cx - 6, cy - 5, 12, 10);
                        g.DrawLine(visor, cx - 4, cy - 1, cx + 4, cy - 1);
                        g.DrawLine(visor, cx, cy - 7, cx, cy - 5);
                    }
                    break;

                default:
                    using (SolidBrush dot = new SolidBrush(accent))
                    {
                        g.FillEllipse(dot, cx - 4, cy - 4, 8, 8);
                    }
                    break;
            }
        }

        private void DrawToggleButton(Graphics g, Rectangle rect, string text, bool isSelected)
        {
            Color bg = isSelected ? Color.FromArgb(0, 180, 215) : Color.FromArgb(30, 36, 48);
            Color textCol = isSelected ? Color.White : Color.FromArgb(160, 175, 195);
            using (SolidBrush brush = new SolidBrush(bg))
            using (Pen pen = new Pen(isSelected ? Color.FromArgb(0, 229, 255) : Color.FromArgb(50, 60, 80), 1))
            using (Font f = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            using (SolidBrush tBrush = new SolidBrush(textCol))
            {
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(text, f, tBrush, rect, sf);
            }
        }

        private void DrawOfflineBanner(Graphics g, int w)
        {
            _offlineBannerRect = new Rectangle(20, 14, w - 40, 48);
            using (SolidBrush bannerBg = new SolidBrush(Color.FromArgb(240, 24, 34, 48)))
            using (Pen bannerPen = new Pen(Color.FromArgb(0, 229, 255), 2))
            using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                g.FillRectangle(bannerBg, _offlineBannerRect);
                g.DrawRectangle(bannerPen, _offlineBannerRect);

                StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center };
                g.DrawString(_offlineBannerText, font, textBrush, 
                    new Rectangle(_offlineBannerRect.X + 16, _offlineBannerRect.Y, _offlineBannerRect.Width - 110, _offlineBannerRect.Height), sf);
            }

            _offlineClaimButtonRect = new Rectangle(_offlineBannerRect.Right - 90, _offlineBannerRect.Y + 10, 80, 28);
            using (SolidBrush claimBg = new SolidBrush(Color.FromArgb(0, 230, 118)))
            using (Font cFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (SolidBrush cText = new SolidBrush(Color.Black))
            {
                g.FillRectangle(claimBg, _offlineClaimButtonRect);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("수령하기", cFont, cText, _offlineClaimButtonRect, sf);
            }
        }

        private void DrawFloatingTexts(Graphics g)
        {
            using (Font ftFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                foreach (var ft in _floatingTexts)
                {
                    int alpha = (int)Math.Max(0, Math.Min(255, ft.Alpha * 255));
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, ft.TextColor)))
                    {
                        g.DrawString(ft.Text, ftFont, b, ft.X, ft.Y);
                    }
                }
            }
        }

        private string GetShortName(string id)
        {
            switch (id)
            {
                case "rpi": return "미니 PC";
                case "tower": return "사무용 PC";
                case "blade1u": return "랙 서버";
                case "xeon4u": return "워크스테이션";
                case "gpu_h100": return "AI 슈퍼컴";
                case "quantum": return "양자컴퓨터";
                default: return id;
            }
        }

        private string FormatCompactNumber(double value)
        {
            if (value >= 1_000_000_000_000) return $"{value / 1_000_000_000_000:F1}T";
            if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F1}B";
            if (value >= 1_000_000) return $"{value / 1_000_000:F1}M";
            if (value >= 10_000) return $"{value / 1_000:F1}K";
            return $"{value:N0}";
        }

        private void AddFloatingText(string text, float x, float y, Color col, bool isCrit = false)
        {
            _floatingTexts.Add(new FloatingText
            {
                Text = text,
                X = x - 20,
                Y = y - 10,
                VelocityY = -1.8f,
                Alpha = 1.0f,
                TextColor = col,
                IsCrit = isCrit
            });

            if (_floatingTexts.Count > 30)
            {
                _floatingTexts.RemoveAt(0);
            }
        }

        #endregion

        #region User Interaction (Mouse Events)

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point pt = e.Location;

            int prevRack = _hoveredRackIndex;
            _hoveredRackIndex = -1;
            for (int i = 0; i < _rackBounds.Length; i++)
            {
                if (_rackBounds[i].Contains(pt))
                {
                    _hoveredRackIndex = i;
                    break;
                }
            }

            int prevCard = _hoveredCardIndex;
            _hoveredCardIndex = -1;
            for (int i = 0; i < _cardBounds.Length; i++)
            {
                if (_cardBounds[i].Contains(pt))
                {
                    _hoveredCardIndex = i;
                    break;
                }
            }

            bool prevDeploy = _isDeployHovered;
            _isDeployHovered = _deployButtonRect.Contains(pt);

            if (prevRack != _hoveredRackIndex || prevCard != _hoveredCardIndex || prevDeploy != _isDeployHovered)
            {
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Point pt = e.Location;

            // 1. 오프라인 보상 수령
            if (_showOfflineBanner && _offlineClaimButtonRect.Contains(pt))
            {
                _showOfflineBanner = false;
                Invalidate();
                return;
            }

            // 2. 뒤로가기 버튼 [◀ SandBox]
            if (_backButtonRect.Contains(pt))
            {
                if (this.ParentForm is SandBox sb)
                {
                    sb.BackToList();
                }
                return;
            }

            // 3. 서브 탭 전환 [서버실, 전력실, 냉각실, 연구/환생]
            for (int i = 0; i < _subTabBounds.Length; i++)
            {
                if (_subTabBounds[i].Contains(pt))
                {
                    _activeSubTab = i;
                    Invalidate();
                    return;
                }
            }

            // 4. 구매 배수 토글 버튼 [x1], [x10], [MAX] (탭 0, 1, 2)
            if (_activeSubTab < 3)
            {
                if (_buy1Rect.Contains(pt)) { _buyMultiplier = 1; Invalidate(); return; }
                if (_buy10Rect.Contains(pt)) { _buyMultiplier = 10; Invalidate(); return; }
                if (_buyMaxRect.Contains(pt)) { _buyMultiplier = -1; Invalidate(); return; }
            }

            // 5. 돌발 이벤트 배너/랙 클릭
            if (_state.CurrentIncident != null)
            {
                int target = _state.CurrentIncident.TargetServerIndex;
                if (target >= 0 && target < _rackBounds.Length)
                {
                    var rackRect = _rackBounds[target];
                    Rectangle clickArea = new Rectangle(rackRect.X - 10, rackRect.Y - 40, rackRect.Width + 20, rackRect.Height + 50);
                    if (clickArea.Contains(pt))
                    {
                        double reward = _state.CurrentIncident.RewardCash;
                        int aiLvl = _state.GetSkillLevel("skill_ai_devops");
                        if (aiLvl > 0) reward = Math.Round(reward * (1.0 + (aiLvl * 0.30)), 0);
                        _state.Money += reward;
                        _state.TotalEarned += reward;
                        _state.CurrentRunEarned += reward;
                        AddFloatingText($"장애 조치 완료! +${reward:N0}", pt.X, pt.Y, Color.FromArgb(0, 255, 128), true);
                        _state.CurrentIncident = null;
                        Invalidate();
                        return;
                    }
                }
            }

            // 6. 메인 [ 🚀 PING / DEPLOY ] 버튼 클릭
            if (_deployButtonRect.Contains(pt))
            {
                PerformClickAction(pt.X, pt.Y);
                return;
            }

            // 7. 하단 탭별 아이템 구매 및 연구 액션
            switch (_activeSubTab)
            {
                case 0: // 서버 구매
                    for (int i = 0; i < _cardBuyButtonBounds.Length; i++)
                    {
                        if (_cardBuyButtonBounds[i].Contains(pt))
                        {
                            PerformBuyServer(i, pt);
                            return;
                        }
                    }
                    break;
                case 1: // 전력 설비 구매
                    for (int i = 0; i < _cardBuyButtonBounds.Length; i++)
                    {
                        if (_cardBuyButtonBounds[i].Contains(pt))
                        {
                            PerformBuyPower(i, pt);
                            return;
                        }
                    }
                    break;
                case 2: // 냉각 설비 구매
                    for (int i = 0; i < _cardBuyButtonBounds.Length; i++)
                    {
                        if (_cardBuyButtonBounds[i].Contains(pt))
                        {
                            PerformBuyCooling(i, pt);
                            return;
                        }
                    }
                    break;
                case 3: // S/W 스튜디오 인터랙션
                    HandleSoftwareStudioClick(pt);
                    break;
                case 4: // B2B 수주 인터랙션
                    HandleB2BContractsClick(pt);
                    break;
                case 5: // 코인 거래소 인터랙션
                    HandleCryptoMarketClick(pt);
                    break;
                case 6: // 기술 연구 및 환생
                    if (_prestigeActionBtnRect.Contains(pt))
                    {
                        PerformPrestige();
                        return;
                    }
                    var skills = TycoonGameState.GetSkillDefinitions();
                    for (int i = 0; i < skills.Count && i < _cardBuyButtonBounds.Length; i++)
                    {
                        if (_cardBuyButtonBounds[i].Contains(pt))
                        {
                            PerformUnlockSkill(skills[i], pt);
                            return;
                        }
                    }
                    break;
            }

            // 8. 서버실 랙 클릭 시에도 가벼운 핑 생성
            for (int i = 0; i < _rackBounds.Length; i++)
            {
                if (_rackBounds[i].Contains(pt))
                {
                    PerformClickAction(pt.X, pt.Y);
                    return;
                }
            }
        }

        private void PerformClickAction(int clickX, int clickY)
        {
            _state.TotalClicks++;
            double power = _state.GetClickPower();

            bool isJackpot = false;
            int critLvl = _state.GetSkillLevel("skill_crit_packet");
            if (critLvl > 0)
            {
                int jackpotChance = 5 + (critLvl * 4); // Lv1=9%, Lv2=13%, Lv3=17%, Lv4=21%, Lv5=25%
                if (_rand.Next(0, 100) < jackpotChance)
                {
                    double mult = 3.0 + (critLvl * 1.0); // Lv1=4x, Lv2=5x, Lv3=6x, Lv4=7x, Lv5=8x
                    power *= mult;
                    isJackpot = true;
                }
                else if (_rand.Next(0, 100) < 15)
                {
                    power *= 2.0;
                }
            }
            else if (_rand.Next(0, 100) < 15)
            {
                power *= 2.0;
            }

            _state.Money += power;
            _state.TotalEarned += power;
            _state.CurrentRunEarned += power;

            if (!_state.IsFeverActive)
            {
                _state.FeverMeter += 4.5f;
                if (_state.FeverMeter >= 100f)
                {
                    _state.FeverMeter = 100f;
                    _state.IsFeverActive = true;
                    _state.FeverRemainingSeconds = 10f;
                    AddFloatingText("★ FEVER OVERDRIVE!", Width / 2f, 150, Color.FromArgb(255, 60, 0), true);
                }
            }

            string text = isJackpot ? $"★ JACKPOT! +${power:N1}" : (power > _state.GetClickPower() ? $"CRIT! +${power:N1}" : $"+${power:N1}");
            Color col = isJackpot ? Color.FromArgb(255, 80, 220) : (power > _state.GetClickPower() ? Color.FromArgb(255, 230, 0) : Color.FromArgb(0, 229, 255));
            AddFloatingText(text, clickX, clickY, col, isJackpot || power > _state.GetClickPower());

            _shockwaves.Add(new Shockwave
            {
                X = clickX,
                Y = clickY,
                Radius = 5f,
                MaxRadius = isJackpot ? 60f : 45f,
                Alpha = 1.0f,
                WaveColor = col
            });

            Invalidate();
        }

        private void PerformBuyServer(int index, Point clickPt)
        {
            if (index < 0 || index >= _state.Servers.Count) return;
            var server = _state.Servers[index];

            int count = _buyMultiplier == -1 ? server.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = server.GetNextCost(count);

            if (_state.Money >= cost && count > 0)
            {
                _state.Money -= cost;
                server.Level += count;
                _state.Save();

                AddFloatingText($"+{count} {server.Name}!", clickPt.X, clickPt.Y, server.AccentColor, true);

                _shockwaves.Add(new Shockwave
                {
                    X = clickPt.X,
                    Y = clickPt.Y,
                    Radius = 6f,
                    MaxRadius = 45f,
                    Alpha = 1.0f,
                    WaveColor = server.AccentColor
                });

                Invalidate();
            }
            else
            {
                AddFloatingText("자금 부족!", clickPt.X, clickPt.Y, Color.FromArgb(255, 80, 80), false);
            }
        }

        private void PerformBuyPower(int index, Point clickPt)
        {
            if (index < 0 || index >= _state.PowerFacilities.Count) return;
            var power = _state.PowerFacilities[index];
            if (power.BaseCost <= 0) return;

            int count = _buyMultiplier == -1 ? power.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = power.GetNextCost(count);

            if (_state.Money >= cost && count > 0)
            {
                _state.Money -= cost;
                power.Level += count;
                _state.Save();

                AddFloatingText($"+{count} {power.Name} 증설!", clickPt.X, clickPt.Y, power.AccentColor, true);
                Invalidate();
            }
            else
            {
                AddFloatingText("자금 부족!", clickPt.X, clickPt.Y, Color.FromArgb(255, 80, 80), false);
            }
        }

        private void PerformBuyCooling(int index, Point clickPt)
        {
            if (index < 0 || index >= _state.CoolingFacilities.Count) return;
            var cooling = _state.CoolingFacilities[index];
            if (cooling.BaseCost <= 0) return;

            int count = _buyMultiplier == -1 ? cooling.GetMaxAffordable(_state.Money) : _buyMultiplier;
            double cost = cooling.GetNextCost(count);

            if (_state.Money >= cost && count > 0)
            {
                _state.Money -= cost;
                cooling.Level += count;
                _state.Save();

                AddFloatingText($"+{count} {cooling.Name} 가동!", clickPt.X, clickPt.Y, cooling.AccentColor, true);
                Invalidate();
            }
            else
            {
                AddFloatingText("자금 부족!", clickPt.X, clickPt.Y, Color.FromArgb(255, 80, 80), false);
            }
        }

        private void PerformUnlockSkill(SkillNode skill, Point clickPt)
        {
            int currentLvl = _state.GetSkillLevel(skill.Id);
            if (currentLvl >= skill.MaxLevel) return;

            int cost = skill.GetCostForLevel(currentLvl);
            if (_state.ResearchTokens >= cost)
            {
                _state.ResearchTokens -= cost;
                _state.SetSkillLevel(skill.Id, currentLvl + 1);
                _state.Save();

                AddFloatingText($"◈ {skill.Name} Lv.{currentLvl + 1} 업그레이드!", clickPt.X, clickPt.Y, Color.FromArgb(0, 230, 118), true);
                Invalidate();
            }
            else
            {
                AddFloatingText($"연구 토큰 부족! (◈ {cost}개 필요)", clickPt.X, clickPt.Y, Color.FromArgb(255, 80, 80), false);
            }
        }

        private void PerformPrestige()
        {
            int tokens = _state.CalculatePrestigeTokens();
            if (tokens < 1)
            {
                double remain = Math.Max(0, 1000000.0 - _state.CurrentRunEarned);
                MessageBox.Show(
                    $"이번 회차 수익이 부족하여 클라우드 마이그레이션이 불가합니다.\n\n" +
                    $"• 현재 회차 수익: ${FormatCompactNumber(_state.CurrentRunEarned)}\n" +
                    $"• 첫 연구 토큰 필요액: $1,000,000 (남은 금액: ${FormatCompactNumber(remain)})\n\n" +
                    $"IDC 서버 인프라를 더 육성한 후 다시 시도하세요!", 
                    "Server Farm Tycoon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double nextMult = 1.0 + ((_state.PrestigeCount + 1) * 0.15) + ((_state.TotalTokensEarned + tokens) * 0.015);
            var res = MessageBox.Show(
                $"[클라우드 마이그레이션 - 환생]\n\n" +
                $"현재 보유 자금과 서버/전력/냉각 인프라가 초기화되는 대신,\n" +
                $"다음 혜택을 영구히 획득합니다:\n\n" +
                $"1. 영구 글로벌 생산 배율: x{_state.GetPrestigeMultiplier():F2} -> x{nextMult:F2}\n" +
                $"2. 신규 IDC 연구 토큰: ◈ +{tokens}개 영구 획득\n\n" +
                $"※ 연구실의 스킬 레벨은 환생해도 영구 보존됩니다.\n" +
                $"마이그레이션을 진행하시겠습니까?",
                "Server Farm Tycoon", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                _state.PrestigeCount++;
                _state.ResearchTokens += tokens;
                _state.TotalTokensEarned += tokens;
                _state.Money = 50.0;
                _state.CurrentRunEarned = 50.0;
                _state.Servers = TycoonGameState.CreateDefaultServers();
                _state.PowerFacilities = TycoonGameState.CreateDefaultPower();
                _state.CoolingFacilities = TycoonGameState.CreateDefaultCooling();
                _state.Save();

                AddFloatingText($"[마이그레이션 완료] ◈ +{tokens} 토큰 획득!", Width / 2f, 150, Color.FromArgb(179, 136, 255), true);
                Invalidate();
            }
        }

        #endregion
    }
}
