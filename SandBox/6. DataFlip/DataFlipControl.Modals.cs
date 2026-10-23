using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace GateHelper.DataFlip
{
    public partial class DataFlipControl
    {
        #region Roguelike Modal & Game Over Modal

        private void DrawCardSelectionModal(Graphics g)
        {
            using (var dimBrush = new SolidBrush(Color.FromArgb(220, 6, 9, 15)))
            {
                g.FillRectangle(dimBrush, this.ClientRectangle);
            }

            using (var titleFont = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 10f))
            {
                TextRenderer.DrawText(g, "⚡ 전술 프로토콜 보급 (3턴 주기)", titleFont,
                    new Rectangle(0, 40, this.Width, 32), _colorGold, TextFormatFlags.HorizontalCenter);

                TextRenderer.DrawText(g, "이번 턴에 전군에 하달할 프로토콜 카드 1종을 선택하십시오. (효과는 영구 누적됩니다)", subFont,
                    new Rectangle(0, 75, this.Width, 24), Color.FromArgb(200, 215, 230), TextFormatFlags.HorizontalCenter);
            }

            for (int i = 0; i < _game.CurrentOfferedCards.Count && i < 3; i++)
            {
                var card = _game.CurrentOfferedCards[i];
                var rect = _cardRects[i];
                bool isHover = _hoverCardIndex == i;

                // 등급별 스타일 (Common / Rare / Epic)
                string rText;
                Color rColor;
                Color rBg;
                switch (card.Rarity)
                {
                    case CardRarity.Rare:
                        rText = "✦ RARE";
                        rColor = Color.FromArgb(0, 229, 255);
                        rBg = Color.FromArgb(15, 60, 85);
                        break;
                    case CardRarity.Epic:
                        rText = "★ EPIC";
                        rColor = Color.FromArgb(255, 190, 0);
                        rBg = Color.FromArgb(70, 50, 15);
                        break;
                    default:
                        rText = "◈ COMMON";
                        rColor = Color.FromArgb(170, 190, 215);
                        rBg = Color.FromArgb(30, 40, 55);
                        break;
                }

                Color cardBg = isHover ? Color.FromArgb(30, 38, 55) :
                               (card.Rarity == CardRarity.Epic ? Color.FromArgb(28, 26, 32) : Color.FromArgb(20, 26, 38));
                Color border = isHover ? card.AccentColor :
                               (card.Rarity == CardRarity.Epic ? Color.FromArgb(220, 170, 30) :
                               (card.Rarity == CardRarity.Rare ? Color.FromArgb(0, 180, 210) : Color.FromArgb(70, 85, 110)));
                float bWidth = isHover ? 2.5f : (card.Rarity == CardRarity.Epic ? 2f : 1.5f);

                using (var brush = new SolidBrush(cardBg))
                using (var pen = new Pen(border, bWidth))
                {
                    g.FillRoundedRectangle(brush, rect, 10);
                    g.DrawRoundedRectangle(pen, rect, 10);
                }

                // 상단 헤더 배경 (등급 컬러 틴트)
                var headerRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, 56);
                using (var hBrush = new SolidBrush(Color.FromArgb(card.Rarity == CardRarity.Epic ? 45 : 30, rColor)))
                {
                    g.FillRoundedRectangle(hBrush, headerRect, 8);
                }

                using (var fontBadge = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                using (var fontTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                using (var fontSub = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (var fontDesc = new Font("Segoe UI", 8.5f))
                {
                    // 1. 등급 뱃지
                    var badgeRect = new Rectangle(rect.X + (rect.Width - 92) / 2, rect.Y + 6, 92, 16);
                    using (var bBrush = new SolidBrush(rBg))
                    using (var bPen = new Pen(rColor, 1f))
                    {
                        g.FillRoundedRectangle(bBrush, badgeRect, 3);
                        g.DrawRoundedRectangle(bPen, badgeRect, 3);
                    }
                    TextRenderer.DrawText(g, rText, fontBadge, badgeRect, rColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 2. 카드 타이틀
                    TextRenderer.DrawText(g, card.Title, fontTitle, new Rectangle(rect.X + 4, rect.Y + 25, rect.Width - 8, 22),
                        card.AccentColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 3. 서브타이틀 (WordBreak 추가로 긴 문장도 깔끔하게 줄바꿈 및 잘림 원천 방지)
                    TextRenderer.DrawText(g, card.Subtitle, fontSub, new Rectangle(rect.X + 6, rect.Y + 49, rect.Width - 12, 28),
                        rColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);

                    // 4. 상세 설명 (충분한 높이와 WordBreak 및 \n 완벽 지원)
                    var descRect = new Rectangle(rect.X + 12, rect.Y + 84, rect.Width - 24, rect.Height - 84 - 46);
                    TextRenderer.DrawText(g, card.Description, fontDesc, descRect,
                        Color.FromArgb(230, 240, 250), TextFormatFlags.Left | TextFormatFlags.WordBreak);

                    // 5. 선택 버튼
                    var pickRect = new Rectangle(rect.X + 14, rect.Bottom - 40, rect.Width - 28, 30);
                    Color pickBg = isHover ? card.AccentColor : Color.FromArgb(35, 45, 65);
                    Color pickText = isHover ? Color.Black : Color.White;

                    using (var pBrush = new SolidBrush(pickBg))
                    using (var pPen = new Pen(card.AccentColor, 1.5f))
                    {
                        g.FillRoundedRectangle(pBrush, pickRect, 5);
                        g.DrawRoundedRectangle(pPen, pickRect, 5);
                    }
                    TextRenderer.DrawText(g, "선택 (SELECT)", fontSub, pickRect, pickText,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private void DrawGameOverModal(Graphics g)
        {
            using (var dimBrush = new SolidBrush(Color.FromArgb(220, 6, 8, 14)))
            {
                g.FillRectangle(dimBrush, this.ClientRectangle);
            }

            int cardW = 460;
            int cardH = 350;
            int cardX = (this.Width - cardW) / 2;
            int cardY = (this.Height - cardH) / 2 - 10;
            var cardRect = new Rectangle(cardX, cardY, cardW, cardH);

            Color accent = (_game.Winner == Owner.Player) ? _colorCyan : _colorCrimson;
            string title = (_game.Winner == Owner.Player) ? "🎉 VICTORY! 적 메인프레임 점령" : "💀 DEFEAT! 아군 코어 함락";

            using (var brush = new SolidBrush(Color.FromArgb(20, 26, 38)))
            using (var pen = new Pen(accent, 2.5f))
            {
                g.FillRoundedRectangle(brush, cardRect, 10);
                g.DrawRoundedRectangle(pen, cardRect, 10);
            }

            using (var titleFont = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var statLabelFont = new Font("Segoe UI", 8.5f))
            using (var statValFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var btnFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                // 1. 승리 / 패배 타이틀
                TextRenderer.DrawText(g, title, titleFont, new Rectangle(cardX, cardY + 16, cardW, 30),
                    accent, TextFormatFlags.HorizontalCenter);

                TextRenderer.DrawText(g, "◈ 전역 작전 결산 및 종합 전투 통계 ◈", subFont, new Rectangle(cardX, cardY + 46, cardW, 18),
                    Color.FromArgb(170, 190, 215), TextFormatFlags.HorizontalCenter);

                // 2. 종합 전투 통계 대시보드 HUD 박스
                int dashX = cardX + 18;
                int dashY = cardY + 70;
                int dashW = cardW - 36;
                int dashH = 205;
                var dashRect = new Rectangle(dashX, dashY, dashW, dashH);

                using (var dBrush = new SolidBrush(Color.FromArgb(14, 18, 26)))
                using (var dPen = new Pen(Color.FromArgb(45, 55, 75), 1.2f))
                {
                    g.FillRoundedRectangle(dBrush, dashRect, 6);
                    g.DrawRoundedRectangle(dPen, dashRect, 6);
                }

                // 통계 8대 항목 (2열 4행 배치)
                var statItems = new (string Icon, string Label, string Value, Color ValColor)[]
                {
                    ("⏱️", "총 작전 소요", $"{_game.StatsTurnsElapsed} 턴", Color.White),
                    ("💰", "총 획득 크레딧", $"{_game.StatsTotalCreditEarned} G", _colorGold),
                    ("⚔️", "적 유닛 처치", $"{_game.StatsPlayerUnitsKilled} 기", _colorCyan),
                    ("🛡️", "아군 유닛 손실", $"{_game.StatsPlayerUnitsLost} 기", Color.FromArgb(255, 140, 140)),
                    ("💥", "적 인프라 철거", $"{_game.StatsPlayerBuildingsDestroyed} 개", Color.FromArgb(255, 179, 0)),
                    ("🖥️", "서버 랙 돌파", $"{_game.StatsObstaclesDestroyed} 개 (+{_game.StatsObstaclesDestroyed * 2}G)", Color.FromArgb(130, 220, 255)),
                    ("🛡️", "아군 코어 상태", $"{_game.PlayerCoreHp} / {_game.PlayerCoreMaxHp} HP", _colorCyan),
                    ("🎯", "적 코어 피해", $"{_game.StatsDamageDealtToAiCore} DMG", Color.FromArgb(255, 75, 75))
                };

                int colW = (dashW - 20) / 2;
                int rowH = 46;

                for (int i = 0; i < statItems.Length; i++)
                {
                    int col = i % 2;
                    int row = i / 2;
                    int itemX = dashX + 10 + col * (colW + 10);
                    int itemY = dashY + 10 + row * rowH;

                    var itemRect = new Rectangle(itemX, itemY, colW, rowH - 6);
                    using (var iBrush = new SolidBrush(Color.FromArgb(22, 28, 40)))
                    using (var iPen = new Pen(Color.FromArgb(35, 45, 60), 1f))
                    {
                        g.FillRoundedRectangle(iBrush, itemRect, 4);
                        g.DrawRoundedRectangle(iPen, itemRect, 4);
                    }

                    var item = statItems[i];
                    TextRenderer.DrawText(g, $"{item.Icon} {item.Label}", statLabelFont,
                        new Rectangle(itemX + 8, itemY + 4, colW - 16, 16),
                        Color.FromArgb(150, 165, 185), TextFormatFlags.Left);

                    TextRenderer.DrawText(g, item.Value, statValFont,
                        new Rectangle(itemX + 8, itemY + 18, colW - 16, 18),
                        item.ValColor, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }

                // 3. 하단 액션 버튼 3종 (리플레이 / 다시하기 / 로비)
                // [▶ 전투 리플레이] 버튼
                bool isReplayHover = _hoverButtonId == 97;
                using (var repBrush = new SolidBrush(isReplayHover ? Color.FromArgb(30, 100, 160) : Color.FromArgb(20, 60, 105)))
                using (var repPen = new Pen(_colorCyan, 1.8f))
                {
                    g.FillRoundedRectangle(repBrush, _btnReplayRect, 5);
                    g.DrawRoundedRectangle(repPen, _btnReplayRect, 5);
                }
                TextRenderer.DrawText(g, "▶ 전투 리플레이", btnFont, _btnReplayRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // [다시 하기] 버튼
                bool isRestartHover = _hoverButtonId == 99;
                using (var rBrush = new SolidBrush(isRestartHover ? Color.FromArgb(40, 160, 100) : Color.FromArgb(25, 110, 65)))
                using (var rPen = new Pen(_colorEmerald, 1.8f))
                {
                    g.FillRoundedRectangle(rBrush, _gameOverRestartBtnRect, 5);
                    g.DrawRoundedRectangle(rPen, _gameOverRestartBtnRect, 5);
                }
                TextRenderer.DrawText(g, "🔄 다시하기", btnFont, _gameOverRestartBtnRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // [로비로] 버튼
                bool isBackHover = _hoverButtonId == 98;
                using (var bBrush = new SolidBrush(isBackHover ? Color.FromArgb(50, 60, 80) : Color.FromArgb(30, 38, 52)))
                using (var bPen = new Pen(Color.FromArgb(120, 140, 170), 1.5f))
                {
                    g.FillRoundedRectangle(bBrush, _gameOverBackBtnRect, 5);
                    g.DrawRoundedRectangle(bPen, _gameOverBackBtnRect, 5);
                }
                TextRenderer.DrawText(g, "◀ 로비", btnFont, _gameOverBackBtnRect, Color.FromArgb(220, 230, 245),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        /// <summary>
        /// 초경량 전장 시네마틱 리플레이 뷰어 모드 (자원 부하 제로, 고속 자동 재생)
        /// </summary>
        public void DrawReplayMode(Graphics g)
        {
            g.Clear(_colorBg);

            if (_game.ReplaySnapshots.Count == 0) return;
            int snapIdx = Math.Min(_replayIndex, _game.ReplaySnapshots.Count - 1);
            var snap = _game.ReplaySnapshots[snapIdx];

            // 1. 상단 리플레이 시네마틱 헤더 HUD
            var repHeaderRect = new Rectangle(14, 14, this.Width - 28, 48);
            using (var hBrush = new SolidBrush(Color.FromArgb(22, 28, 42)))
            using (var hPen = new Pen(_colorCyan, 1.5f))
            {
                g.FillRoundedRectangle(hBrush, repHeaderRect, 6);
                g.DrawRoundedRectangle(hPen, repHeaderRect, 6);
            }

            using (var titleFont = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 8.5f))
            using (var btnFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                // 타이틀 & 턴 진행률
                TextRenderer.DrawText(g, $"▶ 전투 리플레이 재생 중 [ 턴 {snap.TurnNumber} / {_game.ReplaySnapshots.Count} ]", titleFont,
                    new Rectangle(26, 20, 340, 20), _colorCyan, TextFormatFlags.Left);

                string statusText = $"아군 본진 HP {snap.PlayerCoreHp} (⬡{snap.PlayerCredit}G)   VS   적 본진 HP {snap.AiCoreHp} (⬡{snap.AiCredit}G)";
                TextRenderer.DrawText(g, statusText, subFont,
                    new Rectangle(26, 40, 400, 16), _colorGold, TextFormatFlags.Left);

                // 우측 리플레이 중단 버튼
                var exitBtnRect = new Rectangle(this.Width - 160, 22, 132, 32);
                using (var eBrush = new SolidBrush(Color.FromArgb(160, 30, 45)))
                using (var ePen = new Pen(_colorCrimson, 1.5f))
                {
                    g.FillRoundedRectangle(eBrush, exitBtnRect, 4);
                    g.DrawRoundedRectangle(ePen, exitBtnRect, 4);
                }
                TextRenderer.DrawText(g, "■ 리플레이 종료 (ESC)", btnFont, exitBtnRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 2. 보드 배경 및 타일 렌더링
            using (var boardBrush = new SolidBrush(_colorBoardBg))
            using (var boardPen = new Pen(_colorGridLine, 1.5f))
            {
                g.FillRectangle(boardBrush, _boardRect);
                g.DrawRectangle(boardPen, _boardRect);
            }

            // 그리드 선 & 타일
            for (int r = 0; r < _game.Rows; r++)
            {
                for (int c = 0; c < _game.Cols; c++)
                {
                    var tileRect = new Rectangle(_boardRect.X + c * _tileW, _boardRect.Y + r * _tileH, _tileW, _tileH);

                    // 타일 배경
                    if (_game.MapTiles[r, c] == TileType.Resource)
                    {
                        using (var resBrush = new SolidBrush(Color.FromArgb(20, 60, 45)))
                        {
                            g.FillRectangle(resBrush, tileRect);
                        }
                    }

                    using (var gridPen = new Pen(_colorGridLine, 1f))
                    {
                        g.DrawRectangle(gridPen, tileRect);
                    }
                }
            }

            // 3. 스냅샷 장애물(서버 랙) 렌더링
            foreach (var obs in snap.Obstacles)
            {
                var rect = new Rectangle(_boardRect.X + obs.Col * _tileW, _boardRect.Y + obs.Row * _tileH, _tileW, _tileH);
                using (var oBrush = new SolidBrush(Color.FromArgb(38, 48, 62)))
                using (var oPen = new Pen(Color.FromArgb(90, 110, 135), 1.2f))
                {
                    g.FillRoundedRectangle(oBrush, rect, 4);
                    g.DrawRoundedRectangle(oPen, rect, 4);
                }
                using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "🖥️", font, new Rectangle(rect.X, rect.Y + 2, rect.Width, rect.Height / 2),
                        Color.White, TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(g, $"{obs.Hp}HP", font, new Rectangle(rect.X, rect.Y + rect.Height / 2 - 2, rect.Width, rect.Height / 2),
                        Color.FromArgb(200, 215, 230), TextFormatFlags.HorizontalCenter);
                }
            }

            // 4. 스냅샷 건물 렌더링
            foreach (var bldg in snap.Buildings)
            {
                var rect = new Rectangle(_boardRect.X + bldg.Col * _tileW + 2, _boardRect.Y + bldg.Row * _tileH + 2, _tileW - 4, _tileH - 4);
                Color bColor = (bldg.Owner == Owner.Player) ? _colorCyan : _colorCrimson;
                using (var bBrush = new SolidBrush(Color.FromArgb(bldg.Owner == Owner.Player ? 25 : 35, bColor)))
                using (var bPen = new Pen(bColor, 1.5f))
                {
                    g.FillRoundedRectangle(bBrush, rect, 4);
                    g.DrawRoundedRectangle(bPen, rect, 4);
                }

                string bIcon = (bldg.Type == BuildingType.PowerPlant) ? "⚡" :
                               (bldg.Type == BuildingType.Turret) ? "🎯" :
                               (bldg.Type == BuildingType.Factory) ? "🏭" : "🧱";

                using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (var subFont = new Font("Segoe UI", 7f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, bIcon, font, new Rectangle(rect.X, rect.Y + 2, rect.Width, rect.Height / 2),
                        bColor, TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(g, $"{bldg.Hp}HP", subFont, new Rectangle(rect.X, rect.Y + rect.Height / 2 - 2, rect.Width, rect.Height / 2),
                        Color.White, TextFormatFlags.HorizontalCenter);
                }
            }

            // 5. 스냅샷 유닛 렌더링
            foreach (var u in snap.Units)
            {
                var rect = new Rectangle(_boardRect.X + u.Col * _tileW + 3, _boardRect.Y + u.Row * _tileH + 3, _tileW - 6, _tileH - 6);
                Color uColor = (u.Owner == Owner.Player) ? _colorCyan : _colorCrimson;

                using (var uBrush = new SolidBrush(uColor))
                using (var uPen = new Pen(Color.White, 1.2f))
                {
                    g.FillEllipse(uBrush, rect);
                    g.DrawEllipse(uPen, rect);
                }

                if (u.HasShield)
                {
                    using (var sPen = new Pen(Color.FromArgb(64, 196, 255), 2f))
                    {
                        g.DrawEllipse(sPen, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4));
                    }
                }

                string uIcon = (u.Type == UnitType.Vanguard) ? "●" :
                               (u.Type == UnitType.Raider) ? "▲" :
                               (u.Type == UnitType.Titan) ? "■" :
                               (u.Type == UnitType.Colossus) ? "✦" :
                               (u.Type == UnitType.Cheongung) ? "🚀" : "★";

                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                using (var hpFont = new Font("Segoe UI", 6.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, uIcon, font, new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height / 2),
                        Color.Black, TextFormatFlags.HorizontalCenter);
                    TextRenderer.DrawText(g, $"{u.Hp}", hpFont, new Rectangle(rect.X, rect.Y + rect.Height / 2 - 2, rect.Width, rect.Height / 2),
                        Color.Black, TextFormatFlags.HorizontalCenter);
                }
            }

            // 6. 하단 조작 안내 바
            var btmRect = new Rectangle(14, _boardRect.Bottom + 12, this.Width - 28, 36);
            using (var btmBrush = new SolidBrush(Color.FromArgb(18, 24, 34)))
            using (var btmPen = new Pen(_colorGridLine, 1f))
            {
                g.FillRoundedRectangle(btmBrush, btmRect, 4);
                g.DrawRoundedRectangle(btmPen, btmRect, 4);
            }
            using (var guideFont = new Font("Segoe UI", 9f))
            {
                TextRenderer.DrawText(g, "💡 화면 아무 곳이나 클릭하거나 [ESC] 키를 누르면 리플레이가 즉시 종료되고 결과 창으로 복귀합니다.",
                    guideFont, btmRect, Color.FromArgb(180, 200, 225), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawGuideModal(Graphics g)
        {
            using (var dimBrush = new SolidBrush(Color.FromArgb(220, 6, 8, 14)))
            {
                g.FillRectangle(dimBrush, this.ClientRectangle);
            }

            int cardW = 580;
            int cardH = 468;
            int cardX = (this.Width - cardW) / 2;
            int cardY = (this.Height - cardH) / 2;
            var cardRect = new Rectangle(cardX, cardY, cardW, cardH);

            using (var brush = new SolidBrush(Color.FromArgb(20, 26, 38)))
            using (var pen = new Pen(_colorCyan, 2f))
            {
                g.FillRoundedRectangle(brush, cardRect, 10);
                g.DrawRoundedRectangle(pen, cardRect, 10);
            }

            using (var titleFont = new Font("Segoe UI", 12.5f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 8.2f))
            using (var hFont = new Font("Segoe UI", 9.3f, FontStyle.Bold))
            using (var bodyFont = new Font("Segoe UI", 8.4f))
            {
                TextRenderer.DrawText(g, "◈ CORE FRONT 전술 작전 지침 & 게임 규칙 ◈", titleFont,
                    new Rectangle(cardX, cardY + 12, cardW, 24), _colorCyan, TextFormatFlags.HorizontalCenter);

                TextRenderer.DrawText(g, "인프라 자원 구축 x 라인 거점 진격 x 3턴 주기 로그라이크 프로토콜", subFont,
                    new Rectangle(cardX, cardY + 34, cardW, 16), Color.FromArgb(160, 185, 215), TextFormatFlags.HorizontalCenter);

                int y = cardY + 52;
                int textW = cardW - 48;

                // 1. 전략 빌드 & 시스템
                TextRenderer.DrawText(g, "1. 3대 전략 빌드 (내정 · 러시 · 방어)", hFont, new Rectangle(cardX + 24, y, textW, 18), _colorGold, TextFormatFlags.Left);
                y += 18;
                TextRenderer.DrawText(g, "• 내정 빌드: 발전소(8G~, +4G스케일링)와 에너지 노드를 선점하고 [한도증설]로 국고 극대화 (건물 2턴당 자체수리 +1HP)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 고속 러시: 침투병(2칸 이동)으로 적이 비워둔 빈 라인을 기습 돌파하여 발전소/코어 타격", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 우주 방어: 방화벽 포탑(13칸 전방위 요격)과 격벽을 구축하여 버틴 후 결전 병기 및 스킬로 역공", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 21;

                // 2. 건물 & 유닛 & 전술 스킬
                TextRenderer.DrawText(g, "2. 인프라 건물 · 작전 유닛 · 전술 스킬 (단축키: B: 건물, U: 유닛, S: 스킬, 1~6: 슬롯)", hFont, new Rectangle(cardX + 24, y, textW, 18), _colorEmerald, TextFormatFlags.Left);
                y += 18;
                TextRenderer.DrawText(g, "• 건물: ⚡ 발전소, 🛡️ 포탑(요격), 🧱 방화벽, 🏭 공장, 🏥 수리격납고(턴당수리), ⏩ 가속관문(이동+1), ⚡ 한도증설", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 유닛: ● 보병(2G), ▲ 침투병(4G), ■ 타이탄(10G), ⬢ 콜로서스(14G), ✦ 드레드노트(18G), 🚀 천궁(12G, 건물/랙 6뎀)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 전술스킬 [S]: 🎯 궤도폭격(8G), ⚡ EMP교란(5G), 🔧 나노복구(4G), 📡 정찰스캔(3G), 💚 치료광선(4G), 🛡️ 구축광선(5G)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.FromArgb(255, 140, 220), TextFormatFlags.Left);
                y += 21;

                // 3. 전장 오브젝트 & 특수 전투 상성
                TextRenderer.DrawText(g, "3. 전장 오브젝트 & 특수 전투 상성 (핵심 공략!)", hFont, new Rectangle(cardX + 24, y, textW, 18), Color.FromArgb(255, 125, 145), TextFormatFlags.Left);
                y += 18;
                TextRenderer.DrawText(g, "• ⚡ 최초 기지 설립: 1턴 시작 시 아군 본진 발전소 위치(Col 1~2)를 원하는 타일에 무료 직접 결정!", bodyFont, new Rectangle(cardX + 24, y, textW, 15), _colorGold, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 🛑 서버 랙(HP 6): 공격/포격으로 파괴 시 +2G 현상금 즉시 획득 및 진격로 개방", bodyFont, new Rectangle(cardX + 24, y, textW, 15), _colorGold, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 🚀 천궁 포대: 건물/서버랙에 2배 피해(6뎀)! 시즈 모드 없이 멈추지 않고 매 턴 무조건 직진 행군", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.FromArgb(255, 160, 90), TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 🛡️ 포탑 사각지대: 1칸 인접(상하좌우) 적 요격 불가 · ⬢ 콜로서스: 1칸 근접 불가 (발칸 카드로 해제)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• ✦ 드레드노트(소형 취약): 보병/침투병 피격 시 +2 피해 · ▲ 침투병: 첫 공격 전까지 은폐 ([정찰스캔]으로 즉시 노출)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 21;

                // 4. 로그라이크 프로토콜
                TextRenderer.DrawText(g, "4. 3턴 주기 로그라이크 전술 프로토콜 & 티어별 해금", hFont, new Rectangle(cardX + 24, y, textW, 18), _colorPurple, TextFormatFlags.Left);
                y += 18;
                TextRenderer.DrawText(g, "• 3턴마다 일반(50%), 희귀(35%), 에픽(15%) 카드 1장 선택. 버프 효과는 영구 누적 중첩!", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 순차 해금: 턴3(보병/내정) ➔ 턴6(침투병) ➔ 턴9(타이탄) ➔ 턴12(콜로서스/드레드노트)", bodyFont, new Rectangle(cardX + 24, y, textW, 15), _colorGold, TextFormatFlags.Left);
                y += 15;
                TextRenderer.DrawText(g, "• 유닛 공수 카드: 선택 후 유효 구역 타일을 직접 클릭하여 무료 전술 즉시 배치!", bodyFont, new Rectangle(cardX + 24, y, textW, 15), Color.White, TextFormatFlags.Left);

                var closeRect = new Rectangle(cardX + (cardW - 130) / 2, cardY + cardH - 38, 130, 28);
                using (var cBrush = new SolidBrush(Color.FromArgb(35, 45, 65)))
                using (var cPen = new Pen(_colorCyan, 1.5f))
                {
                    g.FillRoundedRectangle(cBrush, closeRect, 5);
                    g.DrawRoundedRectangle(cPen, closeRect, 5);
                }
                TextRenderer.DrawText(g, "확인 (ESC / 닫기)", hFont, closeRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        #endregion

        #region Map Size Selection Modal

        private void DrawMapSizeSelectModal(Graphics g)
        {
            // 1. 전체 화면 어두운 반투명 딤 처리
            using (var dimBrush = new SolidBrush(Color.FromArgb(225, 6, 10, 18)))
            {
                g.FillRectangle(dimBrush, this.ClientRectangle);
            }

            // 2. 모달 컨테이너 박스
            var modalRect = _mapSelectModalRect;
            using (var bgBrush = new SolidBrush(Color.FromArgb(18, 23, 34)))
            using (var borderPen = new Pen(Color.FromArgb(0, 180, 216), 2f))
            {
                g.FillRoundedRectangle(bgBrush, modalRect, 12);
                g.DrawRoundedRectangle(borderPen, modalRect, 12);
            }

            // 헤더 바 장식 라인
            using (var hBrush = new SolidBrush(Color.FromArgb(40, 0, 180, 216)))
            {
                g.FillRoundedRectangle(hBrush, new Rectangle(modalRect.X + 2, modalRect.Y + 2, modalRect.Width - 4, 60), 10);
            }

            // 3. 타이틀 & 서브타이틀
            using (var titleFont = new Font("Segoe UI", 13.5f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 9.5f))
            {
                TextRenderer.DrawText(g, "🌐 작전 구역 전장 규모 선택 (OPERATION THEATER SELECT)", titleFont,
                    new Rectangle(modalRect.X, modalRect.Y + 10, modalRect.Width, 26), _colorGold,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, "투입할 전장의 규모를 선택하십시오. (규모에 맞춰 작전창 크기가 자동 최적화됩니다)", subFont,
                    new Rectangle(modalRect.X, modalRect.Y + 36, modalRect.Width, 20), Color.FromArgb(200, 220, 240),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 4. 3대 전장 선택 카드 (소형 / 표준 / 대형)
            string[] titles = { "⚡ 소형 국지전", "🛡️ 표준 정규전", "★ 대형 전면전" };
            string[] tags = { "12 × 6 TACTICAL", "14 × 8 RECOMMENDED", "18 × 8 EPIC WAR" };
            string[] subs = { "[속전속결 기동전]", "[밸런스 공방전 (권장)]", "[광역 총력전]" };
            string[] descs = {
                "• 전선 규모: 12열 × 6라인\n• 교전 거리: 초근접 격돌\n• 특징: 빠른 템포 & 스피디 승부\n• 추천: 기동 침투 기습전\n• 최적 화면: 720 × 680",
                "• 전선 규모: 14열 × 8라인\n• 교전 거리: 표준 전술 공방전\n• 특징: 모든 유닛/포탑 정석 밸런스\n• 추천: 표준 사령관 전략\n• 최적 화면: 820 × 720",
                "• 전선 규모: 18열 × 8라인\n• 교전 거리: 광역 와이드 전선\n• 특징: 다수 자원 노드 & 대군단\n• 추천: 천궁 공성 & 총력 결전\n• 최적 화면: 1000 × 720"
            };
            Color[] accentColors = { _colorEmerald, _colorCyan, _colorGold };
            Color[] cardBgs = { Color.FromArgb(22, 28, 42), Color.FromArgb(22, 30, 46), Color.FromArgb(26, 28, 38) };

            for (int i = 0; i < 3; i++)
            {
                var card = _mapSelectCardRects[i];
                var btn = _mapSelectBtnRects[i];
                bool isHover = _hoverMapSelectIndex == i;
                Color accent = accentColors[i];

                Color bg = isHover ? Color.FromArgb(32, 42, 64) : cardBgs[i];
                float bWidth = isHover ? 2.5f : 1.2f;

                using (var cBrush = new SolidBrush(bg))
                using (var cPen = new Pen(isHover ? accent : Color.FromArgb(60, 75, 100), bWidth))
                {
                    g.FillRoundedRectangle(cBrush, card, 8);
                    g.DrawRoundedRectangle(cPen, card, 8);
                }

                // 상단 뱃지
                var badgeRect = new Rectangle(card.X + (card.Width - 136) / 2, card.Y + 10, 136, 18);
                using (var badgeBrush = new SolidBrush(Color.FromArgb(40, accent)))
                using (var badgePen = new Pen(accent, 1f))
                {
                    g.FillRoundedRectangle(badgeBrush, badgeRect, 4);
                    g.DrawRoundedRectangle(badgePen, badgeRect, 4);
                }
                using (var badgeFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, tags[i], badgeFont, badgeRect, accent,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                // 타이틀 & 서브타이틀
                using (var cTitleFont = new Font("Segoe UI", 11.5f, FontStyle.Bold))
                using (var cSubFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (var cDescFont = new Font("Segoe UI", 8f))
                {
                    TextRenderer.DrawText(g, titles[i], cTitleFont, new Rectangle(card.X, card.Y + 32, card.Width, 22),
                        accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    TextRenderer.DrawText(g, subs[i], cSubFont, new Rectangle(card.X, card.Y + 54, card.Width, 16),
                        Color.FromArgb(170, 195, 220), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    // 본문 설명 박스
                    var descRect = new Rectangle(card.X + 8, card.Y + 74, card.Width - 16, 95);
                    TextRenderer.DrawText(g, descs[i], cDescFont, descRect, Color.FromArgb(200, 215, 230),
                        TextFormatFlags.Left | TextFormatFlags.WordBreak);
                }

                // 하단 진입 버튼
                using (var btnBrush = new SolidBrush(isHover ? accent : Color.FromArgb(35, 45, 65)))
                using (var btnPen = new Pen(accent, 1.2f))
                {
                    g.FillRoundedRectangle(btnBrush, btn, 5);
                    g.DrawRoundedRectangle(btnPen, btn, 5);
                }
                using (var btnFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    Color btnText = isHover ? Color.FromArgb(10, 15, 24) : accent;
                    string keyHint = $"[작전 투입] (단축키 {i + 1})";
                    TextRenderer.DrawText(g, keyHint, btnFont, btn, btnText,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 하단 안내
            using (var tipFont = new Font("Segoe UI", 8.5f))
            {
                TextRenderer.DrawText(g, "💡 팁: 원하는 카드를 클릭하거나 키보드 [1], [2], [3] 키를 누르면 즉시 작전이 시작됩니다.", tipFont,
                    new Rectangle(modalRect.X, modalRect.Bottom - 26, modalRect.Width, 20), Color.FromArgb(140, 165, 190),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        #endregion

    }

    #region Internal Effect Structures

    internal class ShockwaveRing
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }
        public float MaxRadius { get; set; }
        public float Alpha { get; set; }
        public Color WaveColor { get; set; }
    }

    internal class CombatFloatingText
    {
        public string Text { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Alpha { get; set; }
        public Color TextColor { get; set; }
    }

    #endregion

    #region Drawing Extensions

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle bounds, int cornerRadius)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = cornerRadius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return path;

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    #endregion
}
