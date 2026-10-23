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
        #region Paint Rendering

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // 0. 리플레이 전용 시네마틱 뷰어 모드
            if (_isReplaying)
            {
                DrawReplayMode(g);
                return;
            }

            g.Clear(_colorBg);

            // 1. 헤더 & HUD (본진 HP, 자원 수입, 패시브 뱃지)
            DrawHeader(g);

            // 2. 12x8 전장 맵 타일 & 일체화된 메인프레임 코어 기지
            DrawMap(g);

            // 3. 건물 & 유닛 렌더링
            DrawBuildings(g);
            DrawUnits(g);

            // 4. 포탑 레이저 빔 렌더링
            DrawLaserBeams(g);

            // 4.5 사거리 원형 링 & 타일 하이라이트 (신규!)
            DrawRangeIndicators(g);

            // 5. 마우스 호버 가이드 / 고스트 렌더링
            DrawPlacementGhost(g);

            // 6. 비주얼 이펙트 & 플로팅 텍스트
            DrawEffects(g);

            // 7. 하단 조작 바 (건물, 유닛, 턴 종료)
            DrawBottomBar(g);

            // 7.5 마우스 호버 기물 & 타일 인스펙터 정보 카드 (신규!)
            DrawInspectorCard(g);

            // 8. 로그라이크 카드 선택 팝업 (3턴 주기)
            if (_game.IsCardSelectionActive)
            {
                DrawCardSelectionModal(g);
            }
            // 9. 룰 가이드
            else if (_showGuide)
            {
                DrawGuideModal(g);
            }
            // 10. 게임 종료 화면
            else if (_game.IsGameOver)
            {
                DrawGameOverModal(g);
            }
            // 11. 맵 사이즈 선택 모달 (게임 시작 / 재시작 시)
            else if (_isMapSizeSelectActive)
            {
                DrawMapSizeSelectModal(g);
            }
        }

        private void DrawHeader(Graphics g)
        {
            // 로비 버튼
            bool isBackHover = _hoverButtonId == 0;
            using (var bBrush = new SolidBrush(isBackHover ? Color.FromArgb(40, 50, 70) : Color.FromArgb(25, 32, 45)))
            using (var bPen = new Pen(isBackHover ? _colorCyan : _colorGridLine, 1f))
            {
                g.FillRoundedRectangle(bBrush, _btnBackRect, 4);
                g.DrawRoundedRectangle(bPen, _btnBackRect, 4);
            }
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "◀ 로비", font, _btnBackRect, Color.FromArgb(200, 215, 230),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 타이틀 (컴팩트 배치)
            using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var brush = new SolidBrush(_colorCyan))
            {
                g.DrawString("◈ CORE FRONT", font, brush, 88, 14);
            }

            // [맵 사이즈 선택 토글 3종] (소형 / 표준 / 대형)
            DrawMapSizeToggle(g, _btnMapSmallRect, "소형", MapSizeType.Small, 91);
            DrawMapSizeToggle(g, _btnMapStdRect, "표준", MapSizeType.Standard, 92);
            DrawMapSizeToggle(g, _btnMapLargeRect, "대형", MapSizeType.Large, 93);

            // 타이틀 줄 우측: 로그라이크 패시브 보너스 뱃지 렌더링 (HUD 바 내부가 아닌 상단 타이틀 줄 우측에 배치하여 겹침 원천 차단!)
            int badgeRight = this.Width - 14;
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "⚡한도" + _game.PlayerPlantLimit + "기", _colorCyan, _game.PlayerPlantLimit > 3);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🎯사거리+" + _game.TurretRangeBonus, _colorPurple, _game.TurretRangeBonus > 0);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🔋+" + _game.PlayerIncomeBonus + "G", _colorGold, _game.PlayerIncomeBonus > 0);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "⚔️공+" + _game.PlayerUnitAtkBonus, Color.FromArgb(215, 60, 255), _game.PlayerUnitAtkBonus > 0);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🛡️HP+" + _game.PlayerUnitHpBonus, _colorEmerald, _game.PlayerUnitHpBonus > 0);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🔭시즈", Color.FromArgb(186, 104, 200), _game.PlayerHasSiegeAnchor);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "⚔️근접발칸", Color.FromArgb(255, 179, 0), _game.PlayerHasColossusMelee);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🔭정밀궤적", Color.FromArgb(255, 87, 34), _game.PlayerHasCheongungTrajectory);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "🛡️실드", Color.FromArgb(64, 196, 255), _game.PlayerHasPhalanxShield);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "⚡스텔스", Color.FromArgb(38, 198, 218), _game.PlayerHasStealthCloak);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "📡레이더", Color.FromArgb(0, 200, 255), _game.PlayerHasTurretRadar);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "💥넉백", Color.FromArgb(255, 138, 101), _game.PlayerHasKineticSlam);
            DrawPassiveBadgeRight(g, ref badgeRight, 13, "✦십자방전", Color.FromArgb(0, 229, 255), _game.PlayerHasChainPlasma);

            // 상단 HUD 바 (X: 14, Y: 42, W: Width - 28, H: 28)
            int hudX = 14;
            int hudY = 42;
            int hudW = this.Width - 28;
            var hudRect = new Rectangle(hudX, hudY, hudW, 28);

            using (var brush = new SolidBrush(Color.FromArgb(20, 25, 35)))
            using (var pen = new Pen(_colorGridLine, 1f))
            {
                g.FillRoundedRectangle(brush, hudRect, 4);
                g.DrawRoundedRectangle(pen, hudRect, 4);
            }

            // [1. 좌측 플레이어 구역: HP바 (W:60) + 크레딧]
            int barW = 60;
            int barH = 8;
            int pBarX = hudX + 76;
            int pBarY = hudY + 10;
            float pRatio = (float)_game.PlayerCoreHp / _game.PlayerCoreMaxHp;

            using (var bgBrush = new SolidBrush(Color.FromArgb(40, 20, 30)))
            using (var fillBrush = new SolidBrush(_colorCyan))
            {
                g.FillRoundedRectangle(bgBrush, new Rectangle(pBarX, pBarY, barW, barH), 2);
                if (pRatio > 0)
                    g.FillRoundedRectangle(fillBrush, new Rectangle(pBarX, pBarY, (int)(barW * pRatio), barH), 2);
            }

            // [2. 우측 AI 구역: HP바 (W:60) + 크레딧]
            int aBarX = hudX + hudW - 74 - barW;
            int aBarY = hudY + 10;
            float aRatio = (float)_game.AiCoreHp / _game.AiCoreMaxHp;

            using (var bgBrush = new SolidBrush(Color.FromArgb(40, 20, 30)))
            using (var fillBrush = new SolidBrush(_colorCrimson))
            {
                g.FillRoundedRectangle(bgBrush, new Rectangle(aBarX, aBarY, barW, barH), 2);
                if (aRatio > 0)
                    g.FillRoundedRectangle(fillBrush, new Rectangle(aBarX, aBarY, (int)(barW * aRatio), barH), 2);
            }

            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                // 플레이어 본진 HP
                TextRenderer.DrawText(g, $"본진 HP {_game.PlayerCoreHp}", font, new Rectangle(hudX + 6, hudY + 4, 68, 20),
                    _colorCyan, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                // 플레이어 크레딧 (HP 바 우측)
                int pIncome = _game.CalculateIncome(Owner.Player);
                TextRenderer.DrawText(g, $"⬡ {_game.PlayerCredit}G (+{pIncome}G)", font, new Rectangle(pBarX + barW + 8, hudY + 4, 90, 20),
                    _colorGold, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                // [중앙 턴 정보] - 독립된 중앙 영역에 고정 배치 (어떤 해상도에서도 좌우와 겹치지 않음!)
                string turnText = $"[ 턴 {_game.TurnNumber} ]";
                TextRenderer.DrawText(g, turnText, font, new Rectangle(hudX + (hudW - 90) / 2, hudY + 4, 90, 20),
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // AI 크레딧 (AI 바 좌측)
                int aIncome = _game.CalculateIncome(Owner.Ai);
                TextRenderer.DrawText(g, $"⬡ {_game.AiCredit}G (+{aIncome}G)", font, new Rectangle(aBarX - 98, hudY + 4, 90, 20),
                    Color.FromArgb(220, 160, 120), TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                // AI 본진 HP
                TextRenderer.DrawText(g, $"적 본진 {_game.AiCoreHp}", font, new Rectangle(hudX + hudW - 74, hudY + 4, 68, 20),
                    _colorCrimson, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawPassiveBadgeRight(Graphics g, ref int rightX, int y, string text, Color color, bool active)
        {
            if (!active) return;
            int w = 52;
            int h = 20;
            int x = rightX - w;
            var rect = new Rectangle(x, y, w, h);
            using (var brush = new SolidBrush(Color.FromArgb(45, color)))
            using (var pen = new Pen(color, 1.2f))
            {
                g.FillRoundedRectangle(brush, rect, 3);
                g.DrawRoundedRectangle(pen, rect, 3);
            }
            using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, text, font, rect, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            rightX = x - 4; // 다음 뱃지를 왼쪽으로 4px 띄워서 나란히 배치!
        }

        private void DrawMapSizeToggle(Graphics g, Rectangle rect, string text, MapSizeType size, int btnId)
        {
            bool isCurrent = (_game.CurrentMapSize == size);
            bool isHover = (_hoverButtonId == btnId);

            Color bg = isCurrent ? Color.FromArgb(20, 50, 70) :
                       isHover ? Color.FromArgb(35, 45, 60) : Color.FromArgb(22, 28, 38);
            Color border = isCurrent ? _colorCyan :
                           isHover ? Color.FromArgb(120, 150, 180) : _colorGridLine;
            Color textCol = isCurrent ? _colorCyan :
                            isHover ? Color.White : Color.FromArgb(160, 180, 200);

            using (var brush = new SolidBrush(bg))
            using (var pen = new Pen(border, isCurrent ? 1.5f : 1f))
            {
                g.FillRoundedRectangle(brush, rect, 3);
                g.DrawRoundedRectangle(pen, rect, 3);
            }
            using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, text, font, rect, textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawMap(Graphics g)
        {
            // 보드 전체 배경
            using (var brush = new SolidBrush(_colorBoardBg))
            using (var pen = new Pen(Color.FromArgb(50, 65, 85), 2f))
            {
                g.FillRoundedRectangle(brush, _boardRect, 6);
                g.DrawRoundedRectangle(pen, _boardRect, 6);
            }

            // 1. Col 0: 플레이어 아군 거대 기지 섀시 (Cyber Fortress)
            int pBaseW = _tileW;
            var pBaseRect = new Rectangle(_boardRect.X, _boardRect.Y, pBaseW, _boardRect.Height);
            using (var baseBrush = new SolidBrush(Color.FromArgb(10, 22, 35)))
            using (var basePen = new Pen(_colorCyan, 1.5f))
            {
                g.FillRectangle(baseBrush, pBaseRect);
                g.DrawRectangle(basePen, pBaseRect.X + 2, pBaseRect.Y + 2, pBaseW - 4, _boardRect.Height - 4);

                // 중앙 코어 리액터 심볼
                int coreCy = _boardRect.Y + _boardRect.Height / 2;
                using (var cBrush = new SolidBrush(Color.FromArgb(60, 0, 229, 255)))
                using (var cPen = new Pen(_colorCyan, 2f))
                {
                    g.FillEllipse(cBrush, pBaseRect.X + 6, coreCy - 16, 32, 32);
                    g.DrawEllipse(cPen, pBaseRect.X + 6, coreCy - 16, 32, 32);
                }
                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "CORE", font, new Rectangle(pBaseRect.X, coreCy - 8, pBaseW, 16),
                        _colorCyan, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 플레이어 에너지 방어선 (Laser Fence between Col 0 and 1)
            int fenceX1 = _boardRect.X + _tileW;
            using (var fencePen = new Pen(_colorCyan, 2.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(fencePen, fenceX1, _boardRect.Y, fenceX1, _boardRect.Bottom);
            }

            // 2. Col 11: 적 AI 거대 기지 섀시 (Demon Fortress)
            int aBaseX = _boardRect.Right - _tileW;
            var aBaseRect = new Rectangle(aBaseX, _boardRect.Y, pBaseW, _boardRect.Height);
            using (var baseBrush = new SolidBrush(Color.FromArgb(30, 10, 16)))
            using (var basePen = new Pen(_colorCrimson, 1.5f))
            {
                g.FillRectangle(baseBrush, aBaseRect);
                g.DrawRectangle(basePen, aBaseRect.X + 2, aBaseRect.Y + 2, pBaseW - 4, _boardRect.Height - 4);

                int coreCy = _boardRect.Y + _boardRect.Height / 2;
                using (var cBrush = new SolidBrush(Color.FromArgb(60, 255, 23, 68)))
                using (var cPen = new Pen(_colorCrimson, 2f))
                {
                    g.FillEllipse(cBrush, aBaseRect.X + 6, coreCy - 16, 32, 32);
                    g.DrawEllipse(cPen, aBaseRect.X + 6, coreCy - 16, 32, 32);
                }
                using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "CORE", font, new Rectangle(aBaseRect.X, coreCy - 8, pBaseW, 16),
                        _colorCrimson, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 적 에너지 방어선 (Laser Fence between Col 10 and 11)
            int fenceX2 = _boardRect.Right - _tileW;
            using (var fencePen = new Pen(_colorCrimson, 2.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(fencePen, fenceX2, _boardRect.Y, fenceX2, _boardRect.Bottom);
            }

            // 3. 중앙 10개 열(Col 1~10) 활성 전장 타일 격자 렌더링
            for (int r = 0; r < _game.Rows; r++)
            {
                for (int c = 1; c < _game.Cols - 1; c++)
                {
                    int x = _boardRect.X + c * _tileW;
                    int y = _boardRect.Y + r * _tileH;
                    var tileRect = new Rectangle(x, y, _tileW, _tileH);

                    using (var gridPen = new Pen(_colorGridLine, 1f))
                    {
                        g.DrawRectangle(gridPen, tileRect);
                    }

                    var tType = _game.MapTiles[r, c];
                    if (tType == TileType.Resource)
                    {
                        using (var resBrush = new SolidBrush(Color.FromArgb(35, 255, 215, 0)))
                        using (var resPen = new Pen(_colorGold, 1.5f) { DashStyle = DashStyle.Dash })
                        {
                            g.FillRectangle(resBrush, tileRect);
                            g.DrawRectangle(resPen, x + 2, y + 2, _tileW - 4, _tileH - 4);
                        }
                        using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "⚡2X", font, tileRect, _colorGold,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    else if (tType == TileType.Obstacle)
                    {
                        using (var obsBrush = new SolidBrush(Color.FromArgb(35, 42, 54)))
                        using (var obsPen = new Pen(Color.FromArgb(70, 80, 95), 1.5f))
                        {
                            g.FillRectangle(obsBrush, tileRect);
                            g.DrawRectangle(obsPen, x + 3, y + 3, _tileW - 6, _tileH - 6);
                            g.DrawLine(obsPen, x + 5, y + 5, x + _tileW - 5, y + _tileH - 5);
                            g.DrawLine(obsPen, x + _tileW - 5, y + 5, x + 5, y + _tileH - 5);
                        }

                        // 파괴 가능한 서버 랙 체력 바 (HP 6)
                        int obsHp = _game.ObstacleHp[r, c];
                        DrawMiniHpBar(g, x + 4, y + _tileH - 5, _tileW - 8, 3, obsHp, 6, Color.FromArgb(180, 180, 180));
                        using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "🛑", font, tileRect, Color.FromArgb(170, 180, 195),
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                }
            }
        }

        private void DrawBuildings(Graphics g)
        {
            for (int r = 0; r < _game.Rows; r++)
            {
                for (int c = 1; c < _game.Cols - 1; c++)
                {
                    var b = _game.Buildings[r, c];
                    if (b == null) continue;

                    int x = _boardRect.X + c * _tileW + (int)b.ShakeOffset;
                    int y = _boardRect.Y + r * _tileH;
                    var rect = new Rectangle(x + 3, y + 3, _tileW - 6, _tileH - 6);

                    Color teamColor = (b.Owner == Owner.Player) ? _colorCyan : _colorCrimson;

                    // 1. 발전소 (Power Plant)
                    if (b.Type == BuildingType.PowerPlant)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(45, 255, 215, 0)))
                        using (var rimPen = new Pen(_colorGold, 1.8f))
                        {
                            g.FillRoundedRectangle(bgBrush, rect, 4);
                            g.DrawRoundedRectangle(rimPen, rect, 4);
                        }
                        using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "⚡", font, rect, _colorGold,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    // 2. 포탑 (Turret)
                    else if (b.Type == BuildingType.Turret)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(45, 0, 230, 118)))
                        using (var rimPen = new Pen(_colorEmerald, 2f))
                        {
                            g.FillEllipse(bgBrush, rect);
                            g.DrawEllipse(rimPen, rect);

                            int dir = (b.Owner == Owner.Player) ? 1 : -1;
                            int cx = rect.X + rect.Width / 2;
                            int cy = rect.Y + rect.Height / 2;
                            g.DrawLine(rimPen, cx, cy, cx + dir * 14, cy);
                        }
                    }
                    // 3. 방화벽 격벽 (Wall)
                    else if (b.Type == BuildingType.Wall)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(60, 75, 90)))
                        using (var rimPen = new Pen(Color.FromArgb(140, 160, 180), 2f))
                        {
                            g.FillRectangle(bgBrush, rect);
                            g.DrawRectangle(rimPen, rect);
                        }
                        using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "🧱", font, rect, Color.White,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    // 4. 전진 공장 (Factory)
                    else if (b.Type == BuildingType.Factory)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(45, teamColor)))
                        using (var rimPen = new Pen(teamColor, 2f))
                        {
                            g.FillRoundedRectangle(bgBrush, rect, 5);
                            g.DrawRoundedRectangle(rimPen, rect, 5);
                        }
                        using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "🏭", font, rect, teamColor,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    // 5. 수리 격납고 (RepairDepot)
                    else if (b.Type == BuildingType.RepairDepot)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(45, 0, 229, 255)))
                        using (var rimPen = new Pen(Color.FromArgb(0, 229, 255), 2f))
                        {
                            g.FillRoundedRectangle(bgBrush, rect, 4);
                            g.DrawRoundedRectangle(rimPen, rect, 4);
                        }
                        using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "🏥", font, rect, Color.FromArgb(0, 229, 255),
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    // 6. 가속 관문 (PhaseGate)
                    else if (b.Type == BuildingType.PhaseGate)
                    {
                        using (var bgBrush = new SolidBrush(Color.FromArgb(50, 186, 104, 200)))
                        using (var rimPen = new Pen(Color.FromArgb(206, 147, 216), 2f))
                        {
                            g.FillEllipse(bgBrush, rect);
                            g.DrawEllipse(rimPen, rect);
                        }
                        using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "⏩", font, rect, Color.FromArgb(225, 190, 231),
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    // 피격 플래시 (Hit Flash)
                    if (b.HitFlashAlpha > 0f)
                    {
                        int flashA = (int)(b.HitFlashAlpha * 180);
                        using (var fBrush = new SolidBrush(Color.FromArgb(flashA, 255, 255, 255)))
                        {
                            g.FillRectangle(fBrush, rect);
                        }
                    }

                    // EMP 기능 정지 효과 렌더링
                    if (b.EmpTurns > 0)
                    {
                        using (var empBrush = new SolidBrush(Color.FromArgb(160, 10, 20, 35)))
                        using (var empPen = new Pen(_colorCyan, 1.8f) { DashStyle = DashStyle.Dash })
                        {
                            g.FillRoundedRectangle(empBrush, rect, 4);
                            g.DrawRoundedRectangle(empPen, rect, 4);
                        }
                        using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, $"⚡EMP {b.EmpTurns}T", font, rect, _colorCyan,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }

                    // 체력 바
                    DrawMiniHpBar(g, rect.X, rect.Bottom - 4, rect.Width, 3, b.Hp, b.MaxHp, teamColor);
                }
            }
        }

        private void DrawUnits(Graphics g)
        {
            foreach (var u in _game.Units)
            {
                float colToDraw = (u.VisualCol >= 0) ? u.VisualCol : u.Col;
                int x = _boardRect.X + (int)(colToDraw * _tileW) + (int)u.BumpOffset + (int)u.ShakeOffset;
                int y = _boardRect.Y + u.Row * _tileH;
                var rect = new Rectangle(x + 4, y + 4, _tileW - 8, _tileH - 8);

                Color teamColor = (u.Owner == Owner.Player) ? _colorCyan : _colorCrimson;

                // 1. 보병 (Vanguard) - 원형 네온 드론
                if (u.Type == UnitType.Vanguard)
                {
                    // 전술 방패 결진 실드 효과 렌더링
                    if (u.HasShield)
                    {
                        using (var sBrush = new SolidBrush(Color.FromArgb(40, 64, 196, 255)))
                        using (var sPen = new Pen(Color.FromArgb(220, 64, 196, 255), 1.8f))
                        {
                            sPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                            var sRect = new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                            g.FillEllipse(sBrush, sRect);
                            g.DrawEllipse(sPen, sRect);
                        }
                    }

                    using (var bBrush = new SolidBrush(Color.FromArgb(60, teamColor)))
                    using (var bPen = new Pen(teamColor, 2f))
                    {
                        g.FillEllipse(bBrush, rect);
                        g.DrawEllipse(bPen, rect);
                    }
                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "●", font, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                // 2. 침투병 (Raider) - 날렵한 세모 화살표 (은폐 시 반투명 아지랑이 & 유령 뱃지)
                else if (u.Type == UnitType.Raider)
                {
                    Point[] pts;
                    if (u.Owner == Owner.Player)
                    {
                        pts = new Point[] {
                            new Point(rect.Right, rect.Y + rect.Height / 2),
                            new Point(rect.Left, rect.Y),
                            new Point(rect.Left + 5, rect.Y + rect.Height / 2),
                            new Point(rect.Left, rect.Bottom)
                        };
                    }
                    else
                    {
                        pts = new Point[] {
                            new Point(rect.Left, rect.Y + rect.Height / 2),
                            new Point(rect.Right, rect.Y),
                            new Point(rect.Right - 5, rect.Y + rect.Height / 2),
                            new Point(rect.Right, rect.Bottom)
                        };
                    }

                    int raiderAlpha = u.IsCloaked ? 35 : 70;
                    float raiderPenW = u.IsCloaked ? 1.5f : 2f;
                    using (var bBrush = new SolidBrush(Color.FromArgb(raiderAlpha, teamColor)))
                    using (var bPen = new Pen(teamColor, raiderPenW))
                    {
                        if (u.IsCloaked) bPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        g.FillPolygon(bBrush, pts);
                        g.DrawPolygon(bPen, pts);
                    }

                    if (u.IsCloaked)
                    {
                        using (var ghostFont = new Font("Segoe UI", 7f, FontStyle.Bold))
                        {
                            TextRenderer.DrawText(g, "👻", ghostFont, new Rectangle(rect.X, rect.Y - 11, rect.Width, 12), Color.FromArgb(220, 255, 255, 255),
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                }
                // 3. 타이탄 (Titan) - 묵직한 이중 장갑차
                else if (u.Type == UnitType.Titan)
                {
                    using (var bBrush = new SolidBrush(Color.FromArgb(80, teamColor)))
                    using (var bPen = new Pen(teamColor, 2.5f))
                    using (var innerPen = new Pen(Color.FromArgb(255, 240, 200), 1.5f))
                    {
                        g.FillRoundedRectangle(bBrush, rect, 4);
                        g.DrawRoundedRectangle(bPen, rect, 4);
                        g.DrawRectangle(innerPen, rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
                    }
                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "■", font, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                // 4. 공성 콜로서스 (Colossus) - 팔각 중포 거신
                else if (u.Type == UnitType.Colossus)
                {
                    Color colossusAccent = Color.FromArgb(220, 100, 255);
                    Point[] octPts = new Point[] {
                        new Point(rect.Left + 8, rect.Top),
                        new Point(rect.Right - 8, rect.Top),
                        new Point(rect.Right, rect.Top + 8),
                        new Point(rect.Right, rect.Bottom - 8),
                        new Point(rect.Right - 8, rect.Bottom),
                        new Point(rect.Left + 8, rect.Bottom),
                        new Point(rect.Left, rect.Bottom - 8),
                        new Point(rect.Left, rect.Top + 8)
                    };

                    using (var bBrush = new SolidBrush(Color.FromArgb(90, teamColor)))
                    using (var bPen = new Pen(teamColor, 2f))
                    using (var accentPen = new Pen(colossusAccent, 1.5f))
                    {
                        g.FillPolygon(bBrush, octPts);
                        g.DrawPolygon(bPen, octPts);
                        // 포탑 조준 링
                        g.DrawEllipse(accentPen, rect.X + 7, rect.Y + 7, rect.Width - 14, rect.Height - 14);
                    }
                    using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "⬢", font, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                // 5. 플라즈마 드레드노트 (Dreadnought) - 다이아몬드형 중장갑 십자 방전 워커
                else if (u.Type == UnitType.Dreadnought)
                {
                    Color dreadAccent = Color.FromArgb(255, 190, 0);
                    int cx = rect.X + rect.Width / 2;
                    int cy = rect.Y + rect.Height / 2;
                    Point[] diamondPts = new Point[] {
                        new Point(cx, rect.Top),
                        new Point(rect.Right, cy),
                        new Point(cx, rect.Bottom),
                        new Point(rect.Left, cy)
                    };

                    using (var bBrush = new SolidBrush(Color.FromArgb(95, dreadAccent)))
                    using (var bPen = new Pen(teamColor, 2f))
                    using (var crossPen = new Pen(dreadAccent, 1.8f))
                    {
                        g.FillPolygon(bBrush, diamondPts);
                        g.DrawPolygon(bPen, diamondPts);
                        // 십자 발광 코어 라인
                        g.DrawLine(crossPen, cx - 7, cy, cx + 7, cy);
                        g.DrawLine(crossPen, cx, cy - 7, cx, cy + 7);
                    }
                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "✦", font, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
                // 6. 천궁 (Cheongung) - 초장거리 대공성 미사일 포대
                else if (u.Type == UnitType.Cheongung)
                {
                    Color missileColor = (u.Owner == Owner.Player) ? Color.FromArgb(120, 230, 255) : Color.FromArgb(255, 120, 60);
                    int dir = (u.Owner == Owner.Player) ? 1 : -1;
                    int cx = rect.X + rect.Width / 2;
                    int cy = rect.Y + rect.Height / 2;

                    using (var bBrush = new SolidBrush(Color.FromArgb(95, teamColor)))
                    using (var bPen = new Pen(teamColor, 2f))
                    using (var missilePen = new Pen(missileColor, 2.2f))
                    {
                        g.FillRoundedRectangle(bBrush, rect, 4);
                        g.DrawRoundedRectangle(bPen, rect, 4);
                        // 전방 트윈 미사일 발사 레일
                        g.DrawLine(missilePen, cx - dir * 4, cy - 5, cx + dir * 12, cy - 5);
                        g.DrawLine(missilePen, cx - dir * 4, cy + 5, cx + dir * 12, cy + 5);
                    }
                    using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "🚀", font, rect, Color.White,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                // 피격 플래시 (Hit Flash)
                if (u.HitFlashAlpha > 0f)
                {
                    int flashA = (int)(u.HitFlashAlpha * 200);
                    using (var fBrush = new SolidBrush(Color.FromArgb(flashA, 255, 255, 255)))
                    {
                        g.FillEllipse(fBrush, rect);
                    }
                }

                // 상단 체력 도트 게이지
                DrawHpDots(g, rect.X, rect.Y - 2, rect.Width, u.Hp, u.MaxHp, teamColor);

                // 자폭 폭탄 장착 시 유닛 좌측 하단 💣 뱃지 렌더링
                if (u.HasBomb)
                {
                    var bombBadgeRect = new Rectangle(rect.Left + 1, rect.Bottom - 13, 14, 12);
                    using (var bBg = new SolidBrush(Color.FromArgb(230, 30, 10, 15)))
                    using (var bPen = new Pen(Color.FromArgb(255, 23, 68), 1.2f))
                    using (var bFont = new Font("Segoe UI", 6f, FontStyle.Bold))
                    {
                        g.FillRoundedRectangle(bBg, bombBadgeRect, 2);
                        g.DrawRoundedRectangle(bPen, bombBadgeRect, 2);
                        TextRenderer.DrawText(g, "💣", bFont, bombBadgeRect, Color.FromArgb(255, 60, 60),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                // 건물 중첩 시 타일 우하단에 건물 미니 뱃지 렌더링
                var underBldg = _game.Buildings[u.Row, u.Col];
                if (underBldg != null && underBldg.Hp > 0)
                {
                    string bIcon = underBldg.Type == BuildingType.PowerPlant ? "⚡" :
                                   underBldg.Type == BuildingType.Turret ? "🛡️" :
                                   underBldg.Type == BuildingType.Wall ? "🧱" :
                                   underBldg.Type == BuildingType.Factory ? "🏭" :
                                   underBldg.Type == BuildingType.RepairDepot ? "🏥" : "⏩";
                    Color bCol = underBldg.Owner == Owner.Player ? _colorGold : _colorCrimson;
                    var bBadgeRect = new Rectangle(rect.Right - 15, rect.Bottom - 13, 14, 12);
                    using (var bBg = new SolidBrush(Color.FromArgb(220, 10, 14, 22)))
                    using (var bPen = new Pen(bCol, 1.2f))
                    using (var bFont = new Font("Segoe UI", 6f, FontStyle.Bold))
                    {
                        g.FillRoundedRectangle(bBg, bBadgeRect, 2);
                        g.DrawRoundedRectangle(bPen, bBadgeRect, 2);
                        TextRenderer.DrawText(g, bIcon, bFont, bBadgeRect, bCol,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            }
        }

        private void DrawLaserBeams(Graphics g)
        {
            foreach (var beam in _laserBeams)
            {
                int sx = _boardRect.X + beam.StartCol * _tileW + _tileW / 2;
                int sy = _boardRect.Y + beam.StartRow * _tileH + _tileH / 2;
                int ex = _boardRect.X + beam.EndCol * _tileW + _tileW / 2;
                int ey = _boardRect.Y + beam.EndRow * _tileH + _tileH / 2;

                int a = Math.Max(0, Math.Min(255, (int)(beam.Alpha * 255)));

                // 외곽 글로우
                using (var glowPen = new Pen(Color.FromArgb((int)(a * 0.4f), beam.BeamColor), 6f))
                {
                    g.DrawLine(glowPen, sx, sy, ex, ey);
                }
                // 중앙 고열 백색 코어
                using (var corePen = new Pen(Color.FromArgb(a, Color.White), 2f))
                {
                    g.DrawLine(corePen, sx, sy, ex, ey);
                }
            }
        }

        private void DrawMiniHpBar(Graphics g, int x, int y, int w, int h, int hp, int maxHp, Color color)
        {
            float ratio = Math.Max(0f, Math.Min(1f, (float)hp / maxHp));
            using (var bg = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
            using (var fill = new SolidBrush(color))
            {
                g.FillRectangle(bg, x, y, w, h);
                g.FillRectangle(fill, x, y, (int)(w * ratio), h);
            }
        }

        private void DrawHpDots(Graphics g, int x, int y, int w, int hp, int maxHp, Color color)
        {
            int dots = Math.Min(8, maxHp);
            int dotSize = 3;
            int totalW = dots * (dotSize + 2);
            int startX = x + (w - totalW) / 2;

            for (int i = 0; i < dots; i++)
            {
                bool active = i < hp;
                using (var brush = new SolidBrush(active ? color : Color.FromArgb(70, 70, 80)))
                {
                    g.FillEllipse(brush, startX + i * (dotSize + 2), y, dotSize, dotSize);
                }
            }
        }

        private void DrawPlacementGhost(Graphics g)
        {
            // 0. [최초 1턴 본진 발전소 무료 배치 모드]
            if (_game.IsInitialBasePlacementActive)
            {
                // (A) Col 1~2 배치 가능 구역 전체에 부드러운 골드 네온 점선 격자 하이라이트
                for (int r = 0; r < _game.Rows; r++)
                {
                    for (int c = 1; c <= 2; c++)
                    {
                        if (_game.MapTiles[r, c] != TileType.Obstacle &&
                            _game.Buildings[r, c] == null &&
                            !_game.Units.Exists(u => u.Col == c && u.Row == r))
                        {
                            int tx = _boardRect.X + c * _tileW;
                            int ty = _boardRect.Y + r * _tileH;
                            var tRect = new Rectangle(tx + 2, ty + 2, _tileW - 4, _tileH - 4);
                            using (var bBrush = new SolidBrush(Color.FromArgb(28, 255, 215, 0)))
                            using (var bPen = new Pen(Color.FromArgb(140, 255, 215, 0), 1.2f) { DashStyle = DashStyle.Dot })
                            {
                                g.FillRectangle(bBrush, tRect);
                                g.DrawRectangle(bPen, tRect);
                            }
                            using (var dotFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                            {
                                TextRenderer.DrawText(g, "⚡", dotFont, tRect, Color.FromArgb(180, 255, 215, 0),
                                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                            }
                        }
                    }
                }

                // (B) 마우스 호버 타일에 배치될 발전소의 네온 고스트 실루엣
                if (_hoverTile.X >= 1 && _hoverTile.X <= 2 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                {
                    int hc = _hoverTile.X;
                    int hr = _hoverTile.Y;
                    bool isValid = (_game.MapTiles[hr, hc] != TileType.Obstacle &&
                                    _game.Buildings[hr, hc] == null &&
                                    !_game.Units.Exists(u => u.Col == hc && u.Row == hr));

                    Color ghostCol = isValid ? _colorGold : Color.FromArgb(200, 255, 60, 60);
                    int hx = _boardRect.X + hc * _tileW;
                    int hy = _boardRect.Y + hr * _tileH;
                    var hRect = new Rectangle(hx + 3, hy + 3, _tileW - 6, _tileH - 6);

                    using (var gBrush = new SolidBrush(Color.FromArgb(50, ghostCol)))
                    using (var gPen = new Pen(ghostCol, 2f))
                    {
                        g.FillRoundedRectangle(gBrush, hRect, 4);
                        g.DrawRoundedRectangle(gPen, hRect, 4);
                    }

                    using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "⚡발전소", font, hRect, ghostCol,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                // (C) 전장 상단 안내 배너 (가변 맵 최적화 및 텍스트 잘림 방지)
                int bannerW = Math.Max(460, Math.Min(_boardRect.Width - 10, 560));
                int bannerH = 26;
                int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                int by = _boardRect.Y + 6;
                var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                using (var bbBrush = new SolidBrush(Color.FromArgb(220, 15, 25, 40)))
                using (var bbPen = new Pen(_colorGold, 1.5f))
                {
                    g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                    g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                }
                using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    string msg = "⚡ [초기 기지 설립] 1~2열을 클릭하여 첫 발전소를 무료 건설하세요";
                    TextRenderer.DrawText(g, msg, bFont, bannerRect, _colorGold,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            // 1. [프로토콜 카드 전술 투하/배치 지정 모드]
            if (_game.PendingDropRemainingCount > 0 && _game.PendingDropUnitType != UnitType.None)
            {
                // (A) 배치 가능 구역 전체에 부드러운 네온 점선 격자 하이라이트 표시
                for (int r = 0; r < _game.Rows; r++)
                {
                    for (int c = _game.PendingDropMinCol; c <= _game.PendingDropMaxCol; c++)
                    {
                        if (_game.MapTiles[r, c] != TileType.Obstacle &&
                            _game.Buildings[r, c]?.Type != BuildingType.Wall &&
                            !_game.Units.Exists(u => u.Col == c && u.Row == r))
                        {
                            int tx = _boardRect.X + c * _tileW;
                            int ty = _boardRect.Y + r * _tileH;
                            var tRect = new Rectangle(tx + 2, ty + 2, _tileW - 4, _tileH - 4);
                            using (var bBrush = new SolidBrush(Color.FromArgb(28, 0, 229, 255)))
                            using (var bPen = new Pen(Color.FromArgb(140, 0, 229, 255), 1.2f) { DashStyle = DashStyle.Dot })
                            {
                                g.FillRectangle(bBrush, tRect);
                                g.DrawRectangle(bPen, tRect);
                            }
                            using (var dotFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                            {
                                TextRenderer.DrawText(g, "✦", dotFont, tRect, Color.FromArgb(180, 0, 229, 255),
                                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                            }
                        }
                    }
                }

                // (B) 마우스 호버 타일에 배치될 유닛의 네온 고스트 실루엣 렌더링
                if (_hoverTile.X >= _game.PendingDropMinCol && _hoverTile.X <= _game.PendingDropMaxCol &&
                    _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                {
                    int hc = _hoverTile.X;
                    int hr = _hoverTile.Y;
                    bool isValid = (_game.MapTiles[hr, hc] != TileType.Obstacle &&
                                    _game.Buildings[hr, hc]?.Type != BuildingType.Wall &&
                                    !_game.Units.Exists(u => u.Col == hc && u.Row == hr));

                    Color ghostCol = isValid ? Color.FromArgb(200, 0, 229, 255) : Color.FromArgb(200, 255, 60, 60);
                    int hx = _boardRect.X + hc * _tileW;
                    int hy = _boardRect.Y + hr * _tileH;
                    var hRect = new Rectangle(hx + 3, hy + 3, _tileW - 6, _tileH - 6);

                    using (var gBrush = new SolidBrush(Color.FromArgb(50, ghostCol)))
                    using (var gPen = new Pen(ghostCol, 2f))
                    {
                        g.FillRoundedRectangle(gBrush, hRect, 4);
                        g.DrawRoundedRectangle(gPen, hRect, 4);
                    }

                    string icon = (_game.PendingDropUnitType == UnitType.Vanguard) ? "●" :
                                  (_game.PendingDropUnitType == UnitType.Raider) ? "▲" : "■";
                    using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, icon, font, hRect, ghostCol,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }

                // (C) 전장 상단 안내 배너 (클릭 안내)
                int bannerW = 340;
                int bannerH = 24;
                int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                int by = _boardRect.Y + 6;
                var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                using (var bbBrush = new SolidBrush(Color.FromArgb(220, 15, 25, 40)))
                using (var bbPen = new Pen(_colorCyan, 1.5f))
                {
                    g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                    g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                }
                using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    string msg = $"🚀 [전술 투하] 타일을 클릭하세요! (남은 유닛: {_game.PendingDropRemainingCount}기)";
                    TextRenderer.DrawText(g, msg, bFont, bannerRect, _colorGold,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                return;
            }

            // 2. [유저 액션 스킬 타깃팅 모드]
            if (_selectedSkill != UserSkillType.None)
            {
                if (_selectedSkill == UserSkillType.OrbitalStrike)
                {
                    int bannerW = 380;
                    int bannerH = 24;
                    int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                    int by = _boardRect.Y + 6;
                    var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                    using (var bbBrush = new SolidBrush(Color.FromArgb(220, 25, 15, 20)))
                    using (var bbPen = new Pen(_colorCrimson, 1.5f))
                    {
                        g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                        g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                    }
                    using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "🎯 [정밀 궤도 폭격 (8G)] 타깃 적 건물 또는 서버 랙을 클릭하세요 (6 피해)", bFont, bannerRect, _colorCrimson,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    if (_hoverTile.X >= 1 && _hoverTile.X < _game.Cols - 1 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                    {
                        int hx = _boardRect.X + _hoverTile.X * _tileW;
                        int hy = _boardRect.Y + _hoverTile.Y * _tileH;
                        var tRect = new Rectangle(hx + 2, hy + 2, _tileW - 4, _tileH - 4);
                        var targetBldg = _game.Buildings[_hoverTile.Y, _hoverTile.X];
                        bool isAiBldg = targetBldg?.Owner == Owner.Ai;
                        bool isObstacle = _game.MapTiles[_hoverTile.Y, _hoverTile.X] == TileType.Obstacle;
                        bool isTarget = isAiBldg || isObstacle;
                        Color cColor = isTarget ? _colorCrimson : Color.FromArgb(120, 100, 100, 100);

                        using (var brush = new SolidBrush(Color.FromArgb(50, cColor)))
                        using (var pen = new Pen(cColor, 2f))
                        {
                            g.FillRectangle(brush, tRect);
                            g.DrawRectangle(pen, tRect);
                            g.DrawLine(pen, tRect.X, tRect.Y + tRect.Height / 2, tRect.Right, tRect.Y + tRect.Height / 2);
                            g.DrawLine(pen, tRect.X + tRect.Width / 2, tRect.Y, tRect.X + tRect.Width / 2, tRect.Bottom);
                        }

                        // 타깃 기물 명칭 및 체력 실시간 플로팅 뱃지 (유닛 중첩 시 함께 표시)
                        if (isTarget)
                        {
                            string targetName = isObstacle ? $"서버랙 (HP {_game.ObstacleHp[_hoverTile.Y, _hoverTile.X]})" :
                                                targetBldg.Type == BuildingType.PowerPlant ? $"적 발전소 (HP {targetBldg.Hp})" :
                                                targetBldg.Type == BuildingType.Turret ? $"적 포탑 (HP {targetBldg.Hp})" :
                                                targetBldg.Type == BuildingType.Wall ? $"적 방화벽 (HP {targetBldg.Hp})" : $"적 공장 (HP {targetBldg.Hp})";

                            var onUnit = _game.Units.Find(u => u.Col == _hoverTile.X && u.Row == _hoverTile.Y && u.Hp > 0);
                            if (onUnit != null) targetName += $" + {onUnit.Type}";

                            int tipW = Math.Max(120, targetName.Length * 9 + 16);
                            int tipH = 20;
                            var tipRect = new Rectangle(tRect.X + (tRect.Width - tipW) / 2, tRect.Y - tipH - 3, tipW, tipH);
                            using (var tipBrush = new SolidBrush(Color.FromArgb(230, 20, 10, 15)))
                            using (var tipPen = new Pen(_colorCrimson, 1.2f))
                            using (var tipFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                            {
                                g.FillRoundedRectangle(tipBrush, tipRect, 3);
                                g.DrawRoundedRectangle(tipPen, tipRect, 3);
                                TextRenderer.DrawText(g, targetName, tipFont, tipRect, Color.White,
                                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                            }
                        }
                    }
                    return;
                }
                else if (_selectedSkill == UserSkillType.EmpJammer)
                {
                    int bannerW = 380;
                    int bannerH = 24;
                    int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                    int by = _boardRect.Y + 6;
                    var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                    using (var bbBrush = new SolidBrush(Color.FromArgb(220, 15, 25, 40)))
                    using (var bbPen = new Pen(_colorCyan, 1.5f))
                    {
                        g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                        g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                    }
                    using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "⚡ [전자기 EMP 교란 (5G)] 목표 타일을 클릭하세요 (3x3 범위 2턴간 기능 정지)", bFont, bannerRect, _colorCyan,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    if (_hoverTile.X >= 1 && _hoverTile.X < _game.Cols - 1 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                    {
                        for (int dr = -1; dr <= 1; dr++)
                        {
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                int tc = _hoverTile.X + dc;
                                int tr = _hoverTile.Y + dr;
                                if (tc >= 1 && tc < _game.Cols - 1 && tr >= 0 && tr < _game.Rows)
                                {
                                    int tx = _boardRect.X + tc * _tileW;
                                    int ty = _boardRect.Y + tr * _tileH;
                                    var tRect = new Rectangle(tx + 2, ty + 2, _tileW - 4, _tileH - 4);
                                    using (var brush = new SolidBrush(Color.FromArgb(35, 0, 229, 255)))
                                    using (var pen = new Pen(_colorCyan, 1.2f) { DashStyle = DashStyle.Dot })
                                    {
                                        g.FillRectangle(brush, tRect);
                                        g.DrawRectangle(pen, tRect);
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
                else if (_selectedSkill == UserSkillType.HealingBeam)
                {
                    int bannerW = 440;
                    int bannerH = 24;
                    int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                    int by = _boardRect.Y + 6;
                    var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                    using (var bbBrush = new SolidBrush(Color.FromArgb(220, 10, 30, 20)))
                    using (var bbPen = new Pen(Color.FromArgb(0, 230, 118), 1.5f))
                    {
                        g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                        g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                    }
                    using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "💚 [전술 치료 광선 (4G)] 치유할 행(Row)을 클릭하세요 (해당 행 모든 아군 유닛 HP +4)", bFont, bannerRect, Color.FromArgb(0, 230, 118),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    if (_hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                    {
                        int hy = _boardRect.Y + _hoverTile.Y * _tileH;
                        var rowRect = new Rectangle(_boardRect.X + _tileW, hy + 2, _boardRect.Width - _tileW * 2, _tileH - 4);
                        using (var brush = new SolidBrush(Color.FromArgb(35, 0, 230, 118)))
                        using (var pen = new Pen(Color.FromArgb(0, 230, 118), 1.8f) { DashStyle = DashStyle.Dash })
                        {
                            g.FillRectangle(brush, rowRect);
                            g.DrawRectangle(pen, rowRect);
                        }

                        int friendlyCount = _game.Units.FindAll(u => u.Owner == Owner.Player && u.Row == _hoverTile.Y && u.Hp > 0).Count;
                        int woundedCount = _game.Units.FindAll(u => u.Owner == Owner.Player && u.Row == _hoverTile.Y && u.Hp > 0 && u.Hp < u.MaxHp).Count;
                        string tip = $"💚 치료 대상: Row {_hoverTile.Y + 1} (아군 {friendlyCount}기 / 부상 {woundedCount}기)";
                        int tipW = Math.Max(180, tip.Length * 9);
                        var tipRect = new Rectangle(_boardRect.X + (_boardRect.Width - tipW) / 2, hy + (_tileH - 22) / 2, tipW, 22);
                        using (var tipBg = new SolidBrush(Color.FromArgb(230, 10, 30, 20)))
                        using (var tipPen = new Pen(Color.FromArgb(0, 230, 118), 1.2f))
                        using (var tipFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                        {
                            g.FillRoundedRectangle(tipBg, tipRect, 3);
                            g.DrawRoundedRectangle(tipPen, tipRect, 3);
                            TextRenderer.DrawText(g, tip, tipFont, tipRect, Color.White,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    return;
                }
                else if (_selectedSkill == UserSkillType.FortifyBeam)
                {
                    int bannerW = 460;
                    int bannerH = 24;
                    int bx = _boardRect.X + (_boardRect.Width - bannerW) / 2;
                    int by = _boardRect.Y + 6;
                    var bannerRect = new Rectangle(bx, by, bannerW, bannerH);
                    using (var bbBrush = new SolidBrush(Color.FromArgb(220, 10, 25, 35)))
                    using (var bbPen = new Pen(Color.FromArgb(0, 229, 255), 1.5f))
                    {
                        g.FillRoundedRectangle(bbBrush, bannerRect, 4);
                        g.DrawRoundedRectangle(bbPen, bannerRect, 4);
                    }
                    using (var bFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "🛡️ [전술 구축 광선 (5G)] 강화할 행(Row)을 클릭하세요 (해당 행 아군 최대HP+2 & 현재HP+2)", bFont, bannerRect, Color.FromArgb(0, 229, 255),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }

                    if (_hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
                    {
                        int hy = _boardRect.Y + _hoverTile.Y * _tileH;
                        var rowRect = new Rectangle(_boardRect.X + _tileW, hy + 2, _boardRect.Width - _tileW * 2, _tileH - 4);
                        using (var brush = new SolidBrush(Color.FromArgb(35, 0, 229, 255)))
                        using (var pen = new Pen(Color.FromArgb(0, 229, 255), 1.8f) { DashStyle = DashStyle.Dash })
                        {
                            g.FillRectangle(brush, rowRect);
                            g.DrawRectangle(pen, rowRect);
                        }

                        int friendlyCount = _game.Units.FindAll(u => u.Owner == Owner.Player && u.Row == _hoverTile.Y && u.Hp > 0).Count;
                        string tip = $"🛡️ 구축 대상: Row {_hoverTile.Y + 1} (아군 유닛 {friendlyCount}기 최대HP/현재HP +2)";
                        int tipW = Math.Max(200, tip.Length * 9);
                        var tipRect = new Rectangle(_boardRect.X + (_boardRect.Width - tipW) / 2, hy + (_tileH - 22) / 2, tipW, 22);
                        using (var tipBg = new SolidBrush(Color.FromArgb(230, 10, 25, 35)))
                        using (var tipPen = new Pen(Color.FromArgb(0, 229, 255), 1.2f))
                        using (var tipFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                        {
                            g.FillRoundedRectangle(tipBg, tipRect, 3);
                            g.DrawRoundedRectangle(tipPen, tipRect, 3);
                            TextRenderer.DrawText(g, tip, tipFont, tipRect, Color.White,
                                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                    }
                    return;
                }
            }

            // 3. 일반 건물/유닛 배치 모드
            if (_hoverTile.X < 1 || _hoverTile.X > _game.Cols - 2) return;
            if (_selectedBuilding == BuildingType.None && _selectedUnit == UnitType.None) return;

            int c2 = _hoverTile.X;
            int r2 = _hoverTile.Y;
            int x2 = _boardRect.X + c2 * _tileW;
            int y2 = _boardRect.Y + r2 * _tileH;
            var rect2 = new Rectangle(x2, y2, _tileW, _tileH);

            bool isValidNormal = false;
            if (_selectedBuilding != BuildingType.None)
                isValidNormal = _game.IsValidBuildingPlacement(c2, r2, Owner.Player);
            else if (_selectedUnit != UnitType.None)
                isValidNormal = _game.IsValidUnitSpawnPlacement(c2, r2, Owner.Player);

            Color ghostColor2 = isValidNormal ? Color.FromArgb(120, 0, 230, 118) : Color.FromArgb(120, 255, 23, 68);

            using (var brush = new SolidBrush(Color.FromArgb(40, ghostColor2)))
            using (var pen = new Pen(ghostColor2, 2f) { DashStyle = DashStyle.Dot })
            {
                g.FillRectangle(brush, rect2);
                g.DrawRectangle(pen, rect2.X + 2, rect2.Y + 2, rect2.Width - 4, rect2.Height - 4);
            }
        }

        #region Range Indicators & Inspector Tooltip

        private void DrawRangeIndicators(Graphics g)
        {
            if (_game.IsCardSelectionActive || _showGuide || _game.IsGameOver || _isTurnRunning) return;

            // 1. [배치 모드] 포탑 또는 타이탄 선택 상태에서 마우스 호버 시 실시간 사거리 미리보기
            if (_hoverTile.X >= 1 && _hoverTile.X < _game.Cols - 1 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
            {
                if (_selectedBuilding == BuildingType.Turret)
                {
                    int bonus = _game.TurretRangeBonus;
                    bool isValid = _game.IsValidBuildingPlacement(_hoverTile.X, _hoverTile.Y, Owner.Player);
                    Color rangeColor = isValid ? _colorEmerald : Color.FromArgb(180, 255, 23, 68);
                    DrawTurretOmniRange(g, _hoverTile.X, _hoverTile.Y, bonus, rangeColor, "🛡️ 포탑 배치 사거리 (13칸 전방위 요격)");
                    return;
                }
                else if (_selectedUnit == UnitType.Titan)
                {
                    bool isValid = _game.IsValidUnitSpawnPlacement(_hoverTile.X, _hoverTile.Y, Owner.Player);
                    Color rangeColor = isValid ? _colorCyan : Color.FromArgb(180, 255, 23, 68);
                    DrawRangeCircleAndTiles(g, _hoverTile.X, _hoverTile.Y, 2, 1, rangeColor, "■ 타이탄 스폰 사거리 (2칸 공성 포격)", 1);
                    return;
                }
                else if (_selectedUnit == UnitType.Colossus)
                {
                    bool isValid = _game.IsValidUnitSpawnPlacement(_hoverTile.X, _hoverTile.Y, Owner.Player);
                    Color rangeColor = isValid ? Color.FromArgb(220, 100, 255) : Color.FromArgb(180, 255, 23, 68);
                    DrawRangeCircleAndTiles(g, _hoverTile.X, _hoverTile.Y, 3, 1, rangeColor, "⬢ 콜로서스 스폰 사거리 (2~3칸 포격, 1칸 근접불가)", 2);
                    return;
                }
                else if (_selectedUnit == UnitType.Dreadnought)
                {
                    bool isValid = _game.IsValidUnitSpawnPlacement(_hoverTile.X, _hoverTile.Y, Owner.Player);
                    Color rangeColor = isValid ? Color.FromArgb(255, 190, 0) : Color.FromArgb(180, 255, 23, 68);
                    DrawRangeCircleAndTiles(g, _hoverTile.X, _hoverTile.Y, 2, 1, rangeColor, "✦ 드레드노트 스폰 사거리 (2칸 십자 스플래시 방전)", 1);
                    return;
                }
                else if (_selectedUnit == UnitType.Cheongung)
                {
                    bool isValid = _game.IsValidUnitSpawnPlacement(_hoverTile.X, _hoverTile.Y, Owner.Player);
                    Color rangeColor = isValid ? Color.FromArgb(255, 120, 50) : Color.FromArgb(180, 255, 23, 68);
                    int rng = _game.PlayerHasCheongungTrajectory ? 5 : 4;
                    int minRng = _game.PlayerHasCheongungTrajectory ? 3 : 2;
                    DrawRangeCircleAndTiles(g, _hoverTile.X, _hoverTile.Y, rng, 1, rangeColor, $"🚀 천궁 스폰 사거리 ({minRng}~{rng}칸 대공/대지 미사일 포대)", minRng);
                    return;
                }
            }

            // 2. [탐색 모드] 보드 위의 유닛 또는 건물에 마우스 호버 시 사거리 링 표시
            if (_hoverTile.X >= 1 && _hoverTile.X < _game.Cols - 1 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
            {
                int hx = _hoverTile.X;
                int hy = _hoverTile.Y;

                // (A) 유닛 사거리 확인
                var hoverUnit = _game.Units.Find(u => u.Col == hx && u.Row == hy && u.Hp > 0);
                if (hoverUnit != null)
                {
                    Color uColor = (hoverUnit.Owner == Owner.Player) ? _colorCyan : _colorCrimson;
                    if (hoverUnit.Type == UnitType.Colossus) uColor = Color.FromArgb(220, 100, 255);
                    else if (hoverUnit.Type == UnitType.Dreadnought) uColor = Color.FromArgb(255, 190, 0);
                    else if (hoverUnit.Type == UnitType.Cheongung) uColor = Color.FromArgb(255, 120, 50);
                    int dir = (hoverUnit.Owner == Owner.Player) ? 1 : -1;
                    string teamTag = (hoverUnit.Owner == Owner.Player) ? "아군" : "적군";
                    string label = (hoverUnit.Type == UnitType.Dreadnought)
                        ? $"✦ {teamTag} 드레드노트 사거리 (2칸 십자방전, 소형유닛 취약)"
                        : (hoverUnit.Type == UnitType.Colossus)
                        ? $"⬢ {teamTag} 콜로서스 사거리 (2~3칸 포격, 1칸 근접불가)"
                        : (hoverUnit.Type == UnitType.Cheongung)
                        ? $"🚀 {teamTag} 천궁 미사일 포대 ({hoverUnit.MinRange}~{hoverUnit.Range}칸 사거리, 건물/랙 2배 피해)"
                        : (hoverUnit.Type == UnitType.Titan)
                        ? $"■ {teamTag} 타이탄 사거리 ({hoverUnit.Range}칸 무반격)"
                        : $"{((hoverUnit.Type == UnitType.Raider) ? "▲ 침투병" : "● 보병")} 교전 사거리 (1칸)";

                    DrawRangeCircleAndTiles(g, hoverUnit.Col, hoverUnit.Row, hoverUnit.Range, dir, uColor, label, hoverUnit.MinRange);
                    return;
                }

                // (B) 건물 사거리 확인 (포탑 - 13칸 전방위 수호 요새)
                var hoverBldg = _game.Buildings[hy, hx];
                if (hoverBldg != null && hoverBldg.Type == BuildingType.Turret && hoverBldg.Hp > 0)
                {
                    int bonus = (hoverBldg.Owner == Owner.Player) ? _game.TurretRangeBonus : 0;
                    Color bColor = (hoverBldg.Owner == Owner.Player) ? _colorEmerald : _colorCrimson;
                    string teamTag = (hoverBldg.Owner == Owner.Player) ? "아군" : "적군";
                    DrawTurretOmniRange(g, hoverBldg.Col, hoverBldg.Row, bonus, bColor, $"🛡️ {teamTag} 포탑 수호 구역 (13칸 전방위)");
                    return;
                }
            }
        }

        private void DrawTurretOmniRange(Graphics g, int col, int row, int rangeBonus, Color color, string label)
        {
            float cx = _boardRect.X + col * _tileW + _tileW / 2f;
            float cy = _boardRect.Y + row * _tileH + _tileH / 2f;
            float pulse = (float)Math.Sin(_pulseAngle) * 2.5f;
            float radius = (2.2f + rangeBonus * 0.8f) * _tileW + pulse;

            // 1. 네온 펄스 사거리 원 (Omni Range Circle)
            using (var bgBrush = new SolidBrush(Color.FromArgb(16, color)))
            using (var rimPen = new Pen(Color.FromArgb(160, color), 1.8f) { DashStyle = DashStyle.Dash })
            {
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2);
                g.DrawEllipse(rimPen, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            // 2. 13칸 전방위 유효 타일 강조
            for (int r = 0; r < _game.Rows; r++)
            {
                for (int c = 1; c < _game.Cols - 1; c++)
                {
                    if (r == row && c == col) continue;
                    if (!CoreFrontGameState.IsTileInTurretRange(row, col, r, c, rangeBonus)) continue;

                    int tx = _boardRect.X + c * _tileW;
                    int ty = _boardRect.Y + r * _tileH;
                    var tRect = new Rectangle(tx + 2, ty + 2, _tileW - 4, _tileH - 4);

                    using (var tileBrush = new SolidBrush(Color.FromArgb(35, color)))
                    using (var tilePen = new Pen(color, 1.2f))
                    {
                        g.FillRoundedRectangle(tileBrush, tRect, 4);
                        g.DrawRoundedRectangle(tilePen, tRect, 4);
                    }

                    using (var font = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(g, "🎯", font, tRect, color,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            }

            // 3. 사거리 상단 네온 뱃지 라벨
            using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
            {
                Size sz = TextRenderer.MeasureText(label, font);
                int bw = sz.Width + 14;
                int bh = 20;
                int bx = (int)cx - bw / 2;
                int by = (int)(cy - radius - 16);
                if (by < _boardRect.Y + 4) by = (int)(cy + radius + 4);

                var badgeRect = new Rectangle(bx, by, bw, bh);
                using (var bBrush = new SolidBrush(Color.FromArgb(220, 14, 20, 30)))
                using (var bPen = new Pen(color, 1.2f))
                {
                    g.FillRoundedRectangle(bBrush, badgeRect, 4);
                    g.DrawRoundedRectangle(bPen, badgeRect, 4);
                }
                TextRenderer.DrawText(g, label, font, badgeRect, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawRangeCircleAndTiles(Graphics g, int col, int row, int range, int dir, Color color, string label, int minRange = 1)
        {
            float cx = _boardRect.X + col * _tileW + _tileW / 2f;
            float cy = _boardRect.Y + row * _tileH + _tileH / 2f;
            float pulse = (float)Math.Sin(_pulseAngle) * 2.5f;
            float radius = range * _tileW + 10f + pulse;

            // 1. 네온 펄스 사거리 원 (Range Circle)
            using (var bgBrush = new SolidBrush(Color.FromArgb(18, color)))
            using (var rimPen = new Pen(Color.FromArgb(160, color), 1.8f) { DashStyle = DashStyle.Dash })
            {
                g.FillEllipse(bgBrush, cx - radius, cy - radius, radius * 2, radius * 2);
                g.DrawEllipse(rimPen, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            // 2. 전방 사격 직선 타일 강조 (Target Lane Highlights)
            for (int dist = 1; dist <= range; dist++)
            {
                int tc = col + dist * dir;
                if (tc >= 1 && tc < _game.Cols - 1)
                {
                    int tx = _boardRect.X + tc * _tileW;
                    int ty = _boardRect.Y + row * _tileH;
                    var tRect = new Rectangle(tx + 2, ty + 2, _tileW - 4, _tileH - 4);

                    bool isBlindSpot = dist < minRange;
                    Color tileColor = isBlindSpot ? Color.FromArgb(255, 60, 60) : color;

                    using (var tileBrush = new SolidBrush(Color.FromArgb(isBlindSpot ? 30 : 45, tileColor)))
                    using (var tilePen = new Pen(tileColor, 1.4f) { DashStyle = isBlindSpot ? DashStyle.Dot : DashStyle.Solid })
                    {
                        g.FillRoundedRectangle(tileBrush, tRect, 4);
                        g.DrawRoundedRectangle(tilePen, tRect, 4);
                    }

                    using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        string mark = isBlindSpot ? "🛑사각" : $"🎯{dist}";
                        TextRenderer.DrawText(g, mark, font, tRect, tileColor,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    }
                }
            }

            // 3. 사거리 상단 네온 뱃지 라벨
            using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
            {
                Size sz = TextRenderer.MeasureText(label, font);
                int bw = sz.Width + 14;
                int bh = 20;
                int bx = (int)cx - bw / 2;
                int by = (int)(cy - radius - 16);
                if (by < _boardRect.Y + 4) by = (int)(cy + radius + 4);

                var badgeRect = new Rectangle(bx, by, bw, bh);
                using (var bBrush = new SolidBrush(Color.FromArgb(220, 14, 20, 30)))
                using (var bPen = new Pen(color, 1.2f))
                {
                    g.FillRoundedRectangle(bBrush, badgeRect, 4);
                    g.DrawRoundedRectangle(bPen, badgeRect, 4);
                }
                TextRenderer.DrawText(g, label, font, badgeRect, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawInspectorCard(Graphics g)
        {
            if (_game.IsCardSelectionActive || _showGuide || _game.IsGameOver || _isTurnRunning) return;

            string title = "";
            string teamTag = "";
            Color accentColor = _colorCyan;
            string stat1 = "";
            string stat2 = "";
            string desc1 = "";
            string desc2 = "";

            int cardW = 245;
            int cardH = 98;
            int cardX = 0;
            int cardY = 0;

            // 1. 하단 슬롯 액션 버튼에 마우스 호버 시 실시간 스펙 가이드 팝업
            if ((_hoverButtonId >= 1 && _hoverButtonId <= 6) || _hoverButtonId == 95)
            {
                Rectangle slotRect = (_hoverButtonId == 95) ? _btnExpandGridRect : _slotRects[_hoverButtonId - 1];
                cardX = slotRect.X + (slotRect.Width - cardW) / 2;
                cardX = Math.Max(12, Math.Min(this.Width - cardW - 12, cardX));
                cardY = (_hoverButtonId == 95) ? _slotRects[0].Y - cardH - 8 : slotRect.Y - cardH - 8;

                if (_hoverButtonId == 95)
                {
                    title = "⚡ 전력망 한도 증설";
                    int eCost = _game.GetGridExpandCost(Owner.Player);
                    accentColor = _colorGold;
                    teamTag = $"{eCost}G 연구";
                    stat1 = $"비용 {eCost}G  |  현재 한도 {_game.PlayerPlantLimit}기";
                    stat2 = "발전소 보유 최대 한도를 즉시 +1기 확장";
                    desc1 = "발전소 3기 초과 건설을 위한 전력망 인프라 투자";
                    desc2 = "증설할 때마다 다음 비용이 +4G 점진 증가";
                }
                else if (_currentCategory == BottomCategory.Units)
                {
                    int slotIndex = _hoverButtonId - 1;
                    switch (slotIndex)
                    {
                        case 0: // 보병
                            title = "● 보병 (Vanguard)";
                            accentColor = _colorCyan;
                            teamTag = "2G 유닛";
                            stat1 = $"HP {3 + _game.PlayerUnitHpBonus}  |  공격력 {1 + _game.PlayerUnitAtkBonus}  |  이속 1칸";
                            stat2 = _game.PlayerHasPhalanxShield ? "🛡️ [전술 방패 결진 활성화] 1회 무효 실드" : "사거리 1칸 근접  |  아군 건물 통과";
                            desc1 = "가성비 뛰어난 전선의 핵심 기본 보병";
                            desc2 = _game.PlayerHasPhalanxShield ? "특수: 소환 시 1회 피해를 완전 무효화하는 실드 장착!" : "적 유닛 조우 시 전선 저지선 형성 맞대치";
                            break;
                        case 1: // 침투병
                            title = "▲ 고속 침투병 (Raider)";
                            accentColor = Color.FromArgb(255, 170, 0);
                            teamTag = "4G 유닛";
                            stat1 = $"HP {3 + _game.PlayerUnitHpBonus}  |  공격력 {2 + _game.PlayerUnitAtkBonus}  |  이속 2칸!";
                            stat2 = _game.PlayerHasStealthCloak ? "⚡ [광학 스텔스 활성화] 첫 공격 시까지 은폐" : "사거리 1칸 근접  |  고속 기동 암살";
                            desc1 = "고속 이동으로 적 빈틈 침투 및 적 발전소 타격";
                            desc2 = _game.PlayerHasStealthCloak ? "특수: 적 바로 앞까지 도달해 공격할 때까지 원거리 공격 불가!" : "보병 상대 시 선공 피해를 입히고 반격을 받음";
                            break;
                        case 2: // 타이탄
                            title = "■ 타이탄 (Titan)";
                            accentColor = _colorCrimson;
                            teamTag = "10G 유닛";
                            stat1 = $"HP {10 + _game.PlayerUnitHpBonus}  |  공격력 {3 + _game.PlayerUnitAtkBonus}  |  이속 1칸";
                            stat2 = _game.PlayerHasKineticSlam ? "💥 [충격파 슬램 활성화] 1칸 넉백 & 충돌뎀" : "사거리 2칸 포격  |  반격 없는 공성";
                            desc1 = "중장갑 결전 병기 · 사거리 2칸 일방적 포격";
                            desc2 = _game.PlayerHasKineticSlam ? "특수: 적중 시 뒤로 1칸 밀쳐냄! 뒤가 막히면 양쪽 충돌뎀 1!" : "사거리 2에서 포격 시 적 근접 유닛 반격 없음";
                            break;
                        case 3: // 천궁
                            title = "🚀 천궁 미사일 포대 (Cheongung)";
                            accentColor = Color.FromArgb(255, 120, 50);
                            teamTag = "12G 유닛";
                            stat1 = $"HP {5 + _game.PlayerUnitHpBonus}  |  공격력 {3 + _game.PlayerUnitAtkBonus}  |  이속 1칸";
                            string cgRngStr = _game.PlayerHasCheongungTrajectory ? "사거리 3~5칸 [탄도 궤적 정밀 연산]" : "사거리 2~4칸 초장거리 포격 (1칸 근접불가)";
                            stat2 = cgRngStr;
                            desc1 = "공성 특화: 건물 및 서버 랙 타격 시 공격력 2배 (6 피해!)";
                            desc2 = "직진 행군: 시즈 모드 없이 멈추지 않고 앞으로 전진";
                            break;
                        case 4: // 콜로서스
                            title = "⬢ 공성 콜로서스 (Colossus)";
                            accentColor = Color.FromArgb(220, 100, 255);
                            teamTag = "14G 유닛";
                            stat1 = $"HP {8 + _game.PlayerUnitHpBonus}  |  공격력 {4 + _game.PlayerUnitAtkBonus}  |  이속 1칸";
                            string colRngStr = _game.PlayerHasColossusMelee ? "사거리 1~3칸 전거리 포격 [근접발칸]" : "사거리 2~3칸 초장거리 포격 (1칸 근접불가)";
                            stat2 = _game.PlayerHasSiegeAnchor ? "🔭 [시즈 닻 활성화] 사거리 내 제자리 포격!" : colRngStr;
                            desc1 = _game.PlayerHasSiegeAnchor ? "특수: 유효 사거리 내 적 포착 시 제자리 고정 시즈 포격!" : "특수: 일반 유닛처럼 전진하며 행군 (시즈 앵커 카드로 해금)";
                            desc2 = _game.PlayerHasColossusMelee ? "특수: 근접 방어 발칸포 탑재! 1칸 인접 근접 공격 및 반격 가능!" : "주의: 인접 1칸 근접 공격 불가 및 피격 시 반격 불가";
                            break;
                        case 5: // 드레드노트
                            title = "✦ 드레드노트 (Dreadnought)";
                            accentColor = Color.FromArgb(255, 190, 0);
                            teamTag = "18G 유닛";
                            stat1 = $"HP {12 + _game.PlayerUnitHpBonus}  |  공격력 {3 + _game.PlayerUnitAtkBonus}  |  사거리 2칸";
                            stat2 = _game.PlayerHasChainPlasma ? "✦ [연쇄 플라즈마 활성화] 십자 4방향 2뎀" : "사거리 2칸 원거리 워커  |  단일 타격";
                            desc1 = _game.PlayerHasChainPlasma ? "특수: 주 타격 시 상/하/좌/우 십자 4방향 2뎀 스플래시 방전!" : "최종 결전 워커 · (연쇄 플라즈마 카드로 스플래시 해금)";
                            desc2 = "핸디캡: 보병/침투병(소형)에게 맞을 시 +2 추가 피해!";
                            break;
                    }
                }
                else if (_currentCategory == BottomCategory.Buildings)
                {
                    int slotIndex = _hoverButtonId - 1;
                    switch (slotIndex)
                    {
                        case 0: // 발전소
                            title = "⚡ 발전소 (PowerPlant)";
                            accentColor = _colorGold;
                            int plantCost = _game.GetBuildingCost(BuildingType.PowerPlant, Owner.Player);
                            teamTag = $"{plantCost}G 건물";
                            int curP = _game.GetBuildingCount(BuildingType.PowerPlant, Owner.Player);
                            int limP = _game.PlayerPlantLimit;
                            stat1 = $"HP 4  |  비용 {plantCost}G (보유당 +4G 점진 증가)";
                            stat2 = $"현재 가동 {curP}/{limP}기" + (curP >= limP ? " [한도 도달]" : "");
                            desc1 = "턴당 +1G 생산 (자원 노드 위: +3G 3배 생산!)";
                            desc2 = "한도 초과 시 [5] 한도증설로 그리드 확장 필요 (2턴당 +1HP 자체수리)";
                            break;
                        case 1: // 포탑
                            title = "🛡️ 방화벽 포탑 (Turret)";
                            int tCost = _game.GetBuildingCost(BuildingType.Turret, Owner.Player);
                            accentColor = _colorEmerald;
                            teamTag = $"{tCost}G 건물";
                            stat1 = $"HP 6  |  요격력 2  |  비용 {tCost}G (지을수록 +2G)";
                            stat2 = _game.PlayerHasTurretRadar ? "📡 [양자 레이더 가동] 은폐 침투병 요격 가능!" : "13칸 전방위 요격 (1칸 인접 사각지대 존재)";
                            desc1 = "13칸 반경 내 가장 위험한 적 1기 매 턴 자동 요격";
                            desc2 = "주의: 인접 1칸(상하좌우) 사각지대 존재! 천궁이나 원거리 유닛으로 견제";
                            break;
                        case 2: // 격벽
                            title = "🧱 방화벽 격벽 (Wall)";
                            accentColor = Color.FromArgb(160, 180, 200);
                            teamTag = "3G 건물";
                            stat1 = "HP 8  |  공격력 0  |  바리케이드";
                            stat2 = "길목 차단  |  적과 아군 모두 이동 불가";
                            desc1 = "적 유닛의 진격을 물리적으로 막는 방어벽";
                            desc2 = "주의: 아군 유닛의 진격도 차단합니다!";
                            break;
                        case 3: // 공장
                            title = "🏭 전진 공장 (Factory)";
                            accentColor = _colorPurple;
                            teamTag = "9G 건물";
                            stat1 = "HP 10  |  전진 스폰 포인트 제공";
                            stat2 = "공장 바로 앞 1칸에서 유닛 즉시 스폰 가능";
                            desc1 = "본진이 아닌 전선 바로 앞에서 병력 전진 배치";
                            desc2 = "아군 유닛 자유 통과 · 이동 턴 절약";
                            break;
                        case 4: // 수리 격납고
                            title = "🏥 수리 격납고 (Repair Depot)";
                            int rdCost = _game.GetBuildingCost(BuildingType.RepairDepot, Owner.Player);
                            accentColor = Color.FromArgb(0, 229, 255);
                            teamTag = $"{rdCost}G 건물";
                            stat1 = $"HP 6  |  비용 {rdCost}G  |  턴당 아군 1기 +1 수리";
                            stat2 = "매 턴 부상당한 아군 유닛 1기 체력 회복";
                            desc1 = "나노 수리 드론을 파견하여 전선 유닛을 지속 정비합니다.";
                            desc2 = "아군 유닛 자유 통과 가능 (2턴당 +1HP 자체수리)";
                            break;
                        case 5: // 가속 관문
                            title = "⏩ 가속 관문 (Phase Gate)";
                            int pgCost = _game.GetBuildingCost(BuildingType.PhaseGate, Owner.Player);
                            accentColor = Color.FromArgb(206, 147, 216);
                            teamTag = $"{pgCost}G 건물";
                            stat1 = $"HP 6  |  비용 {pgCost}G  |  통과 아군 이동력 +1";
                            stat2 = "통과하거나 관문에서 출발하는 아군 유닛 이동력 +1";
                            desc1 = "공간 도약 필드를 전개하여 아군 유닛의 기동을 가속합니다.";
                            desc2 = "중장갑 병기나 보병의 전선 합류 속도를 대폭 향상!";
                            break;
                    }
                }
                else // BottomCategory.Skills
                {
                    int slotIndex = _hoverButtonId - 1;
                    switch (slotIndex)
                    {
                        case 0: // 궤도 폭격
                            title = "🎯 정밀 궤도 폭격 (Orbital Strike)";
                            accentColor = _colorCrimson;
                            teamTag = "8G 스킬";
                            stat1 = "비용 8G  |  타깃 지정 단일 포격";
                            stat2 = "적 건물 / 서버 랙 즉시 6 피해 (포탑 원샷!)";
                            desc1 = "전장의 적 건물이나 서버 랙 1개를 정밀 타격하여 파괴합니다.";
                            desc2 = "사각지대 없는 궤도 레이저로 방화벽 포탑을 즉시 철거!";
                            break;
                        case 1: // 전자기 EMP 교란
                            title = "⚡ 전자기 EMP 교란 (EMP Jammer)";
                            accentColor = _colorCyan;
                            teamTag = "5G 스킬";
                            stat1 = "비용 5G  |  3x3 광역 전자기 교란";
                            stat2 = "2턴간 발전 / 포탑 사격 완전 마비 (EmpTurns=2)";
                            desc1 = "목표 지점 3x3 범위 내 모든 적 기물의 기능을 2턴간 정지시킵니다.";
                            desc2 = "적의 위험한 포탑 밀집 구역이나 발전소 단지를 일시 무력화!";
                            break;
                        case 2: // 나노 복구
                            title = "🔧 긴급 나노 복구 (Nano Repair)";
                            accentColor = _colorEmerald;
                            teamTag = "4G 스킬";
                            stat1 = "비용 4G  |  즉발 전체 인프라 지원";
                            stat2 = "아군 모든 건물 체력 +3 즉시 수리";
                            desc1 = "전장에 배치된 모든 아군 건물(발전소, 포탑 등)의 체력을 즉시 +3 회복합니다.";
                            desc2 = "클릭 즉시 발동되는 광역 인프라 재생 스킬";
                            break;
                        case 3: // 정찰 스캔
                            title = "📡 전장 정찰 스캔 (Recon Scan)";
                            accentColor = _colorGold;
                            teamTag = "3G 스킬";
                            stat1 = "비용 3G  |  즉발 전장 시야 스캔";
                            stat2 = "전장의 모든 은폐 침투병 강제 해제";
                            desc1 = "클로킹 중인 적 침투병의 광학 스텔스를 강제로 영구 해제합니다.";
                            desc2 = "기본 포탑과 원거리 유닛들이 즉시 은폐 유닛을 요격 가능해집니다.";
                            break;
                        case 4: // 치료 광선
                            title = "💚 전술 치료 광선 (Healing Beam)";
                            accentColor = Color.FromArgb(0, 230, 118);
                            teamTag = "4G 스킬";
                            stat1 = "비용 4G  |  선택 행(Row) 광역 치유 레이저";
                            stat2 = "해당 행 모든 아군 유닛 체력 +4 회복";
                            desc1 = "고출력 나노 회복 빔을 발사하여 1개 행의 아군을 대폭 수리합니다.";
                            desc2 = "최대 체력 초과 불가 · 위기 라인을 일거에 구원!";
                            break;
                        case 5: // 구축 광선
                            title = "🛡️ 전술 구축 광선 (Fortify Beam)";
                            accentColor = Color.FromArgb(0, 229, 255);
                            teamTag = "5G 스킬";
                            stat1 = "비용 5G  |  선택 행(Row) 광역 강화 레이저";
                            stat2 = "해당 행 아군 유닛 최대 체력 +2 & 현재 체력 +2";
                            desc1 = "영구 방호 장갑 필드를 전개하여 1개 행 아군 내구도를 높입니다.";
                            desc2 = "최대 체력과 현재 체력이 동시에 +2 증가!";
                            break;
                    }
                }
            }
            // 2. 보드 위의 기물 또는 타일 호버 시 인스펙터 팝업
            else if (_hoverTile.X >= 1 && _hoverTile.X < _game.Cols - 1 && _hoverTile.Y >= 0 && _hoverTile.Y < _game.Rows)
            {
                int hx = _hoverTile.X;
                int hy = _hoverTile.Y;

                var unit = _game.Units.Find(u => u.Col == hx && u.Row == hy && u.Hp > 0);
                var bldg = _game.Buildings[hy, hx];
                var tile = _game.MapTiles[hy, hx];

                // 아무것도 없는 일반 빈 타일인 경우 표시하지 않음 (시야 보존)
                if (unit == null && bldg == null && tile == TileType.Plain) return;

                // [핵심 개선] 유닛과 건물이 중첩된 경우: 둘 다 HUD에 명확히 동시 표기!
                if (unit != null && bldg != null)
                {
                    cardW = 275;
                    cardH = 138;
                    accentColor = (_selectedSkill == UserSkillType.OrbitalStrike) ? _colorCrimson :
                                  (unit.Owner == Owner.Player ? _colorCyan : _colorCrimson);

                    string uName = unit.Type == UnitType.Vanguard ? "●보병" :
                                   unit.Type == UnitType.Raider ? "▲침투병" :
                                   unit.Type == UnitType.Titan ? "■타이탄" :
                                   unit.Type == UnitType.Cheongung ? "🚀천궁" :
                                   unit.Type == UnitType.Colossus ? "⬢콜로서스" : "✦드레드노트";

                    string bName = bldg.Type == BuildingType.PowerPlant ? "⚡발전소" :
                                   bldg.Type == BuildingType.Turret ? "🛡️포탑" :
                                   bldg.Type == BuildingType.Factory ? "🏭전진공장" :
                                   bldg.Type == BuildingType.RepairDepot ? "🏥수리격납고" :
                                   bldg.Type == BuildingType.PhaseGate ? "⏩가속관문" : "🧱격벽";

                    string uTag = unit.Owner == Owner.Player ? "아군" : "적";
                    string bTag = bldg.Owner == Owner.Player ? "아군" : "적";

                    title = $"{uName} + {bName}";
                    teamTag = $"{uTag}/{bTag}";

                    stat1 = $"⚔️ [유닛] HP {unit.Hp}/{unit.MaxHp}  |  공 {unit.Atk}  |  이속 {unit.Speed}칸" + (unit.HasActed ? " [⏳대기]" : "");
                    string bldgEffect = (bldg.Type == BuildingType.PowerPlant) ? (tile == TileType.Resource ? "+3G/턴 (3배!)" : "+1G/턴") :
                                        (bldg.Type == BuildingType.Turret) ? "요격 2 (13칸)" :
                                        (bldg.Type == BuildingType.Factory) ? "전진 스폰" :
                                        (bldg.Type == BuildingType.RepairDepot) ? "턴당 1기수리" :
                                        (bldg.Type == BuildingType.PhaseGate) ? "이동력+1" : "길목 차단";
                    stat2 = $"🏗️ [건물] {bName} (HP {bldg.Hp}/{bldg.MaxHp})  |  {bldgEffect}";
                    desc1 = (bldg.Owner == Owner.Player) ? "아군 유닛이 아군 인프라 위를 통과/점유 중입니다." : "적 유닛이 건물 타일 상에 위치해 있습니다.";
                    desc2 = (_selectedSkill == UserSkillType.OrbitalStrike) ?
                        "🎯 [궤도 폭격 타깃!] 클릭 시 하단 건물에 6 피해 적중 (원샷 철거)" :
                        "💡 건물과 유닛이 중첩된 타일 (폭격 시 건물 우선 타격)";
                }
                else if (unit != null)
                {
                    accentColor = (unit.Owner == Owner.Player) ? _colorCyan : _colorCrimson;
                    teamTag = (unit.Owner == Owner.Player) ? "아군" : "적 데몬";

                    switch (unit.Type)
                    {
                        case UnitType.Vanguard:
                            title = "● 보병 (Vanguard)";
                            desc1 = "든든한 기본 보병 · 아군 인프라 자유 통과";
                            desc2 = "적 유닛 조우 시 전선 저지선 형성 맞대치";
                            break;
                        case UnitType.Raider:
                            title = "▲ 고속 침투병 (Raider)";
                            desc1 = "이속 2칸 고속 기동 · 빈 라인 기습 침투";
                            desc2 = "적 발전소 기습 파괴 및 코어 자폭 특화";
                            break;
                        case UnitType.Titan:
                            title = "■ 타이탄 (Titan)";
                            desc1 = "중장갑 공성 거신 · 사거리 2칸 원거리 포격";
                            desc2 = "사거리 2에서 포격 시 적 근접 유닛 반격 없음";
                            break;
                        case UnitType.Colossus:
                            title = "⬢ 공성 콜로서스 (Colossus)";
                            accentColor = Color.FromArgb(220, 100, 255);
                            desc1 = "최강 공성 거포 · 사거리 2~3칸 일방적 초원거리 포격";
                            desc2 = "특성: 인접 1칸 근접 공격 불가 & 피격 시 반격 불가";
                            break;
                        case UnitType.Dreadnought:
                            title = "✦ 드레드노트 (Dreadnought)";
                            accentColor = Color.FromArgb(255, 190, 0);
                            desc1 = "최종 결전 워커 · 사거리 2칸 [가로세로 십자 스플래시 2뎀]";
                            desc2 = "핸디캡: 보병/침투병(소형)에게 피격 시 +2 추가 피해!";
                            break;
                        case UnitType.Cheongung:
                            title = "🚀 천궁 미사일 포대 (Cheongung)";
                            accentColor = Color.FromArgb(255, 120, 50);
                            desc1 = "초장거리 미사일 포대 · 건물/서버랙 2배 피해 (6 피해)";
                            desc2 = "무조건 전진 행군 (시즈 모드 없음, 최소 사거리 존재)";
                            break;
                    }

                    stat1 = $"HP {unit.Hp}/{unit.MaxHp}  |  공격력 {unit.Atk}";
                    stat2 = $"이속 {unit.Speed}칸  |  사거리 {unit.Range}칸" + (unit.HasActed ? "  [⏳ 준비중]" : "");
                }
                else if (bldg != null)
                {
                    accentColor = (bldg.Owner == Owner.Player) ? _colorEmerald : _colorCrimson;
                    teamTag = (bldg.Owner == Owner.Player) ? "아군" : "적 데몬";

                    switch (bldg.Type)
                    {
                        case BuildingType.PowerPlant:
                            title = "⚡ 발전소 (PowerPlant)";
                            accentColor = _colorGold;
                            int plantCount = _game.GetBuildingCount(BuildingType.PowerPlant, bldg.Owner);
                            int plantLimit = _game.GetPlantLimit(bldg.Owner);
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {_game.GetBuildingCost(BuildingType.PowerPlant)}G [가동 {plantCount}/{plantLimit}기]";
                            bool onResource = (tile == TileType.Resource);
                            stat2 = onResource ? "⚡ 자원 노드 가동 중: 수입 +3G/턴 (3배!)" : "기본 전력 생산 중: 수입 +1G/턴";
                            desc1 = $"매 턴 시작 시 크레딧(G) 생산 (현재 한도 {plantLimit}기)";
                            desc2 = "특성: 아군 유닛이 자유롭게 통과 가능 (한도 초과 시 골드로 증설)";
                            break;
                        case BuildingType.Turret:
                            title = "🛡️ 방화벽 포탑 (Turret)";
                            int tCost = _game.GetBuildingCost(BuildingType.Turret, bldg.Owner);
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {tCost}G (지을수록 +2G)";
                            int bonusR = (bldg.Owner == Owner.Player) ? _game.TurretRangeBonus : 0;
                            stat2 = "요격 공격력 2  |  13칸 전방위 수호 요새";
                            desc1 = "13칸 반경 내 가장 위협적인 적 1기를 매 턴 자동 요격";
                            desc2 = "특성: 아군 유닛 자유 통과 (3x3 박스 및 상하좌우 십자 사거리)";
                            break;
                        case BuildingType.Wall:
                            title = "🧱 방화벽 격벽 (Wall)";
                            accentColor = Color.FromArgb(160, 180, 200);
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {_game.GetBuildingCost(BuildingType.Wall)}G";
                            stat2 = "방어 바리케이드  |  공격력 0";
                            desc1 = "적 유닛의 진격을 막아 아군 기지를 보호";
                            desc2 = "주의: 아군 유닛의 진격도 차단합니다!";
                            break;
                        case BuildingType.Factory:
                            title = "🏭 전진 공장 (Factory)";
                            accentColor = _colorPurple;
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {_game.GetBuildingCost(BuildingType.Factory)}G";
                            stat2 = "전진 생산 기지  |  스폰 거점";
                            desc1 = "공장 바로 앞 1칸에서 유닛을 즉시 전진 소환";
                            desc2 = "특성: 아군 유닛이 자유롭게 통과 가능";
                            break;
                        case BuildingType.RepairDepot:
                            title = "🏥 수리 격납고 (Repair Depot)";
                            accentColor = Color.FromArgb(0, 229, 255);
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {_game.GetBuildingCost(BuildingType.RepairDepot)}G";
                            stat2 = "턴당 아군 1기 +1 수리  |  지원 기지";
                            desc1 = "매 턴 시작 시 부상당한 아군 유닛 1기를 나노 수리";
                            desc2 = "특성: 아군 유닛이 자유롭게 통과 가능 (2턴당 +1HP 자체수리)";
                            break;
                        case BuildingType.PhaseGate:
                            title = "⏩ 가속 관문 (Phase Gate)";
                            accentColor = Color.FromArgb(206, 147, 216);
                            stat1 = $"HP {bldg.Hp}/{bldg.MaxHp}  |  비용 {_game.GetBuildingCost(BuildingType.PhaseGate)}G";
                            stat2 = "통과 아군 이동력 +1  |  기동 가속";
                            desc1 = "관문을 통과하거나 관문에서 출발하는 아군 유닛 이동력 +1";
                            desc2 = "특성: 아군 유닛이 자유롭게 통과 가능 (2턴당 +1HP 자체수리)";
                            break;
                    }
                }
                else if (tile == TileType.Resource)
                {
                    title = "⚡ 중립 에너지 노드";
                    teamTag = "중립 자원";
                    accentColor = _colorGold;
                    stat1 = "자원 타일  |  선점 쟁탈 거점";
                    stat2 = "발전소 건설 시 턴당 +3G 수입 (일반의 3배!)";
                    desc1 = "전장의 핵심 에너지 집적 구역입니다.";
                    desc2 = "이곳에 발전소를 먼저 건설하면 자원 우위를 점합니다.";
                }
                else if (tile == TileType.Obstacle)
                {
                    int hp = _game.ObstacleHp[hy, hx];
                    title = "🛑 보안 서버 랙";
                    teamTag = "파괴 가능";
                    accentColor = Color.FromArgb(160, 175, 195);
                    stat1 = $"HP {hp} / 6  |  파괴 보상: +2G";
                    stat2 = "중립 방어벽  |  모든 공격/이동 차단";
                    desc1 = "유닛 또는 타이탄 포격으로 직접 파괴할 수 있습니다.";
                    desc2 = "파괴 시 통로가 개방되며 +2G 데이터 스크랩을 획득합니다!";
                }

                // 카드 위치 계산 (스마트 앵커링)
                int tileScreenX = _boardRect.X + hx * _tileW;
                int tileScreenY = _boardRect.Y + hy * _tileH;

                // X 위치: 좌/우 여백에 따라 타일 옆으로 자연스럽게 배치 (중앙 기준 6열)
                cardX = (hx <= 6) ? tileScreenX + _tileW + 10 : tileScreenX - cardW - 10;
                // Y 위치: 타일 수평 중앙 맞춤 후 화면 밖 벗어남 방지 클램핑
                cardY = tileScreenY + (_tileH - cardH) / 2;
                cardY = Math.Max(_boardRect.Y + 2, Math.Min(_boardRect.Bottom - cardH - 2, cardY));
            }
            else
            {
                return;
            }

            var cardRect = new Rectangle(cardX, cardY, cardW, cardH);

            // 1. 카드 배경 (다크 네온 글래스)
            using (var bgBrush = new SolidBrush(Color.FromArgb(242, 12, 18, 28)))
            using (var borderPen = new Pen(accentColor, 1.5f))
            {
                g.FillRoundedRectangle(bgBrush, cardRect, 6);
                g.DrawRoundedRectangle(borderPen, cardRect, 6);
            }

            // 2. 상단 헤더 (타이틀 + 소속 태그)
            using (var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var tagFont = new Font("Segoe UI", 7.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, title, titleFont, new Rectangle(cardRect.X + 8, cardRect.Y + 6, cardRect.Width - 68, 18),
                    Color.White, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                // 소속 태그 뱃지
                var tagRect = new Rectangle(cardRect.Right - 58, cardRect.Y + 6, 50, 16);
                using (var tBg = new SolidBrush(Color.FromArgb(50, accentColor)))
                using (var tPen = new Pen(accentColor, 1f))
                {
                    g.FillRoundedRectangle(tBg, tagRect, 3);
                    g.DrawRoundedRectangle(tPen, tagRect, 3);
                }
                TextRenderer.DrawText(g, teamTag, tagFont, tagRect, accentColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // 구분선
            using (var divPen = new Pen(Color.FromArgb(55, 75, 100), 1f))
            {
                g.DrawLine(divPen, cardRect.X + 8, cardRect.Y + 26, cardRect.Right - 8, cardRect.Y + 26);
            }

            // 3. 스탯 정보 (Row 1 & Row 2)
            using (var statFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var descFont = new Font("Segoe UI", 7.5f))
            {
                TextRenderer.DrawText(g, stat1, statFont, new Rectangle(cardRect.X + 8, cardRect.Y + 29, cardRect.Width - 16, 16),
                    _colorGold, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, stat2, statFont, new Rectangle(cardRect.X + 8, cardRect.Y + 47, cardRect.Width - 16, 16),
                    Color.FromArgb(200, 225, 250), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                // 4. 설명 텍스트
                int d1Y = (cardH > 100) ? cardRect.Y + 68 : cardRect.Y + 63;
                int d2Y = (cardH > 100) ? cardRect.Y + 88 : cardRect.Y + 77;
                Color d2Color = (_selectedSkill == UserSkillType.OrbitalStrike) ?
                    Color.FromArgb(255, 110, 110) : Color.FromArgb(115, 145, 170);

                TextRenderer.DrawText(g, desc1, descFont, new Rectangle(cardRect.X + 8, d1Y, cardRect.Width - 16, 16),
                    Color.FromArgb(145, 170, 195), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, desc2, descFont, new Rectangle(cardRect.X + 8, d2Y, cardRect.Width - 16, (cardH > 100) ? 38 : 16),
                    d2Color, TextFormatFlags.Left | TextFormatFlags.WordBreak);
            }
        }
        #endregion

        private void DrawEffects(Graphics g)
        {
            foreach (var sw in _shockwaves)
            {
                int alpha = Math.Max(0, Math.Min(255, (int)(sw.Alpha * 255)));
                using (var pen = new Pen(Color.FromArgb(alpha, sw.WaveColor), 2f))
                {
                    g.DrawEllipse(pen, sw.X - sw.Radius, sw.Y - sw.Radius, sw.Radius * 2, sw.Radius * 2);
                }
            }

            using (var font = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            {
                foreach (var ft in _floatingTexts)
                {
                    int alpha = Math.Max(0, Math.Min(255, (int)(ft.Alpha * 255)));
                    using (var brush = new SolidBrush(Color.FromArgb(alpha, ft.TextColor)))
                    {
                        g.DrawString(ft.Text, font, brush, ft.X, ft.Y);
                    }
                }
            }
        }

        private void DrawBottomBar(Graphics g)
        {
            // 1. 카테고리 탭 3종 (인프라 / 작전유닛 / 전술스킬)
            DrawCategoryTab(g, _tabBuildingsRect, "🏗️ 인프라 (B)", _currentCategory == BottomCategory.Buildings, 101, _colorGold);
            DrawCategoryTab(g, _tabUnitsRect, "⚔️ 작전유닛 (U)", _currentCategory == BottomCategory.Units, 102, _colorCyan);
            DrawCategoryTab(g, _tabSkillsRect, "⚡ 전술스킬 (S)", _currentCategory == BottomCategory.Skills, 103, Color.FromArgb(255, 100, 200));

            // 2. 카테고리별 5대 슬롯 렌더링
            if (_currentCategory == BottomCategory.Buildings)
            {
                int plantCost = _game.GetBuildingCost(BuildingType.PowerPlant);
                int plantCount = _game.GetBuildingCount(BuildingType.PowerPlant, Owner.Player);
                int plantLimit = _game.GetPlantLimit(Owner.Player);
                bool isPlantLimitReached = plantCount >= plantLimit;
                bool canAffordPlant = _game.PlayerCredit >= plantCost && !isPlantLimitReached;

                string plantTitle = isPlantLimitReached ? "⚡ 발전소(초과)" : "⚡ 발전소";
                string plantSub = isPlantLimitReached ? $"한도({plantCount}/{plantLimit})" : $"{plantCost}G (+1G/턴)";
                Color plantColor = isPlantLimitReached ? Color.FromArgb(255, 75, 75) : _colorGold;

                DrawActionCard(g, _slotRects[0], plantTitle, plantSub, "[1]",
                    _selectedBuilding == BuildingType.PowerPlant, canAffordPlant, 1, plantColor);

                int turretCost = _game.GetBuildingCost(BuildingType.Turret, Owner.Player);
                DrawActionCard(g, _slotRects[1], "🛡️ 포탑", $"{turretCost}G (요격)", "[2]",
                    _selectedBuilding == BuildingType.Turret, _game.PlayerCredit >= turretCost, 2, _colorEmerald);

                int wallCost = _game.GetBuildingCost(BuildingType.Wall);
                DrawActionCard(g, _slotRects[2], "🧱 방화벽", $"{wallCost}G (HP 8)", "[3]",
                    _selectedBuilding == BuildingType.Wall, _game.PlayerCredit >= wallCost, 3, Color.FromArgb(160, 180, 200));

                int factoryCost = _game.GetBuildingCost(BuildingType.Factory);
                DrawActionCard(g, _slotRects[3], "🏭 전진공장", $"{factoryCost}G (전진스폰)", "[4]",
                    _selectedBuilding == BuildingType.Factory, _game.PlayerCredit >= factoryCost, 4, _colorPurple);

                int depotCost = _game.GetBuildingCost(BuildingType.RepairDepot);
                DrawActionCard(g, _slotRects[4], "🏥 수리격납고", $"{depotCost}G (턴당수리)", "[5]",
                    _selectedBuilding == BuildingType.RepairDepot, _game.PlayerCredit >= depotCost, 5, Color.FromArgb(0, 229, 255));

                int gateCost = _game.GetBuildingCost(BuildingType.PhaseGate);
                DrawActionCard(g, _slotRects[5], "⏩ 가속관문", $"{gateCost}G (이동+1)", "[6]",
                    _selectedBuilding == BuildingType.PhaseGate, _game.PlayerCredit >= gateCost, 6, Color.FromArgb(206, 147, 216));

                // 발전소 한도 증설 버튼: 발전소(슬롯 0) 바로 아래 세로 배치!
                int expandCost = _game.GetGridExpandCost(Owner.Player);
                bool canAffordExpand = _game.PlayerCredit >= expandCost;
                DrawActionCard(g, _btnExpandGridRect, "⚡ 한도증설", $"{expandCost}G (한도+1)", "[+]",
                    false, canAffordExpand, 95, _colorGold);
            }
            else if (_currentCategory == BottomCategory.Units)
            {
                int vgCost = _game.GetUnitCost(UnitType.Vanguard);
                DrawActionCard(g, _slotRects[0], "● 보병", $"{vgCost}G (기본)", "[1]",
                    _selectedUnit == UnitType.Vanguard, _game.PlayerCredit >= vgCost, 1, _colorCyan);

                int rdCost = _game.GetUnitCost(UnitType.Raider);
                DrawActionCard(g, _slotRects[1], "▲ 침투병", $"{rdCost}G (이속2칸)", "[2]",
                    _selectedUnit == UnitType.Raider, _game.PlayerCredit >= rdCost, 2, Color.FromArgb(255, 170, 0));

                int ttCost = _game.GetUnitCost(UnitType.Titan);
                DrawActionCard(g, _slotRects[2], "■ 타이탄", $"{ttCost}G (사거리2)", "[3]",
                    _selectedUnit == UnitType.Titan, _game.PlayerCredit >= ttCost, 3, _colorCrimson);

                int cgCost = _game.GetUnitCost(UnitType.Cheongung);
                DrawActionCard(g, _slotRects[3], "🚀 천궁", $"{cgCost}G (공성6뎀)", "[4]",
                    _selectedUnit == UnitType.Cheongung, _game.PlayerCredit >= cgCost, 4, Color.FromArgb(255, 120, 50));

                int clCost = _game.GetUnitCost(UnitType.Colossus);
                DrawActionCard(g, _slotRects[4], "⬢ 콜로서스", $"{clCost}G (사거리3)", "[5]",
                    _selectedUnit == UnitType.Colossus, _game.PlayerCredit >= clCost, 5, Color.FromArgb(220, 100, 255));

                int dnCost = _game.GetUnitCost(UnitType.Dreadnought);
                DrawActionCard(g, _slotRects[5], "✦ 드레드노트", $"{dnCost}G (십자방전)", "[6]",
                    _selectedUnit == UnitType.Dreadnought, _game.PlayerCredit >= dnCost, 6, Color.FromArgb(255, 190, 0));
            }
            else // BottomCategory.Skills
            {
                int obCost = _game.GetSkillCost(UserSkillType.OrbitalStrike);
                DrawActionCard(g, _slotRects[0], "🎯 궤도폭격", $"{obCost}G (건물6뎀)", "[1]",
                    _selectedSkill == UserSkillType.OrbitalStrike, _game.PlayerCredit >= obCost, 1, _colorCrimson);

                int empCost = _game.GetSkillCost(UserSkillType.EmpJammer);
                DrawActionCard(g, _slotRects[1], "⚡ EMP교란", $"{empCost}G (3x3마비)", "[2]",
                    _selectedSkill == UserSkillType.EmpJammer, _game.PlayerCredit >= empCost, 2, _colorCyan);

                int repCost = _game.GetSkillCost(UserSkillType.NanoRepair);
                DrawActionCard(g, _slotRects[2], "🔧 나노복구", $"{repCost}G (전체+3)", "[3]",
                    false, _game.PlayerCredit >= repCost, 3, _colorEmerald);

                int scCost = _game.GetSkillCost(UserSkillType.ReconScan);
                DrawActionCard(g, _slotRects[3], "📡 정찰스캔", $"{scCost}G (은폐해제)", "[4]",
                    false, _game.PlayerCredit >= scCost, 4, _colorGold);

                int hlCost = _game.GetSkillCost(UserSkillType.HealingBeam);
                DrawActionCard(g, _slotRects[4], "💚 치료광선", $"{hlCost}G (행회복+4)", "[5]",
                    _selectedSkill == UserSkillType.HealingBeam, _game.PlayerCredit >= hlCost, 5, Color.FromArgb(0, 230, 118));

                int ftCost = _game.GetSkillCost(UserSkillType.FortifyBeam);
                DrawActionCard(g, _slotRects[5], "🛡️ 구축광선", $"{ftCost}G (행내구+2)", "[6]",
                    _selectedSkill == UserSkillType.FortifyBeam, _game.PlayerCredit >= ftCost, 6, Color.FromArgb(0, 229, 255));
            }

            // 턴 종료 버튼 (애니메이션 중에는 비활성화)
            bool isEndTurnHover = _hoverButtonId == 8 && !_isTurnRunning;
            Color endTurnBg = _isTurnRunning ? Color.FromArgb(20, 30, 25) :
                              isEndTurnHover ? Color.FromArgb(40, 160, 100) : Color.FromArgb(25, 80, 50);

            using (var brush = new SolidBrush(endTurnBg))
            using (var pen = new Pen(_isTurnRunning ? Color.FromArgb(50, 70, 60) : _colorEmerald, 2f))
            {
                g.FillRoundedRectangle(brush, _btnEndTurnRect, 6);
                g.DrawRoundedRectangle(pen, _btnEndTurnRect, 6);
            }
            using (var font = new Font("Segoe UI", 11.5f, FontStyle.Bold))
            {
                string btnText = _isTurnRunning ? "진격 중..." : "▶ 턴 종료";
                TextRenderer.DrawText(g, btnText, font, _btnEndTurnRect, _isTurnRunning ? Color.FromArgb(120, 150, 130) : Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            DrawSmallButton(g, _btnRestartRect, "🔄 리셋", 9);
            DrawSmallButton(g, _btnGuideRect, "❓ 룰", 10);
        }

        private void DrawCategoryTab(Graphics g, Rectangle rect, string text, bool isSelected, int btnId, Color accent)
        {
            bool isHover = _hoverButtonId == btnId && !_isTurnRunning;

            Color bgColor = isSelected ? Color.FromArgb(32, 42, 58) :
                            isHover ? Color.FromArgb(24, 30, 42) : Color.FromArgb(16, 20, 28);
            Color borderColor = isSelected ? accent :
                                isHover ? Color.FromArgb(90, 115, 145) : _colorGridLine;

            using (var brush = new SolidBrush(bgColor))
            using (var pen = new Pen(borderColor, isSelected ? 1.8f : 1f))
            {
                g.FillRoundedRectangle(brush, rect, 4);
                g.DrawRoundedRectangle(pen, rect, 4);
            }

            if (isSelected)
            {
                // 하단 활성 인디케이터 라인
                using (var linePen = new Pen(accent, 2f))
                {
                    g.DrawLine(linePen, rect.X + 8, rect.Bottom - 2, rect.Right - 8, rect.Bottom - 2);
                }
            }

            Color textColor = isSelected ? accent : isHover ? Color.FromArgb(220, 230, 245) : Color.FromArgb(150, 165, 185);
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, text, font, rect, textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawActionCard(Graphics g, Rectangle rect, string title, string sub, string hotkey, bool isSelected, bool canAfford, int btnId, Color accent)
        {
            bool isHover = _hoverButtonId == btnId && !_isTurnRunning;

            Color bgColor = isSelected ? Color.FromArgb(35, 45, 65) :
                            isHover ? Color.FromArgb(28, 35, 48) : Color.FromArgb(18, 22, 30);

            Color borderColor = isSelected ? accent :
                                isHover ? Color.FromArgb(100, 130, 170) : _colorGridLine;

            float bWidth = isSelected ? 2f : 1f;

            using (var brush = new SolidBrush(bgColor))
            using (var pen = new Pen(borderColor, bWidth))
            {
                g.FillRoundedRectangle(brush, rect, 5);
                g.DrawRoundedRectangle(pen, rect, 5);
            }

            // 우측 상단 단축키 미니 뱃지 (글자와 겹치지 않도록 깔끔한 독립 뱃지 처리)
            string keyNum = hotkey.Replace("[", "").Replace("]", "");
            var kRect = new Rectangle(rect.Right - 15, rect.Y + 3, 12, 12);
            using (var kBrush = new SolidBrush(Color.FromArgb(50, isSelected ? accent : Color.White)))
            using (var kPen = new Pen(isSelected ? accent : Color.FromArgb(80, 100, 125), 1f))
            {
                g.FillRoundedRectangle(kBrush, kRect, 3);
                g.DrawRoundedRectangle(kPen, kRect, 3);
            }
            using (var kFont = new Font("Segoe UI", 6.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, keyNum, kFont, kRect, isSelected ? accent : Color.FromArgb(170, 190, 215),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            Color titleColor = !canAfford ? Color.FromArgb(130, 95, 95) :
                               isSelected ? accent : Color.FromArgb(220, 230, 242);

            Color subColor = !canAfford ? Color.FromArgb(200, 75, 75) :
                             isSelected ? _colorGold : Color.FromArgb(130, 145, 165);

            // 타이틀 영역: 우측 뱃지 공간(16px)을 제외한 영역에 텍스트를 배치하여 겹침 100% 방지!
            using (var titleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 7.5f))
            {
                var titleRect = new Rectangle(rect.X + 2, rect.Y + 4, rect.Width - 18, 18);
                TextRenderer.DrawText(g, title, titleFont, titleRect,
                    titleColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                var subRect = new Rectangle(rect.X + 2, rect.Y + 24, rect.Width - 4, 18);
                TextRenderer.DrawText(g, sub, subFont, subRect,
                    subColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawSmallButton(Graphics g, Rectangle rect, string text, int btnId)
        {
            bool isHover = _hoverButtonId == btnId && !_isTurnRunning;
            Color bg = isHover ? Color.FromArgb(35, 45, 60) : Color.FromArgb(22, 28, 38);
            Color border = isHover ? _colorCyan : _colorGridLine;

            using (var brush = new SolidBrush(bg))
            using (var pen = new Pen(border, 1f))
            {
                g.FillRoundedRectangle(brush, rect, 4);
                g.DrawRoundedRectangle(pen, rect, 4);
            }
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, text, font, rect, Color.FromArgb(200, 215, 230),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        #endregion

    }
}
