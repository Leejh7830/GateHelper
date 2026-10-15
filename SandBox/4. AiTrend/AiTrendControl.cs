using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;

namespace GateHelper.AiTrend
{
    public partial class AiTrendControl : UserControl
    {
        private readonly AiTrendService _service;
        private List<AiRepoItem> _currentItems = new List<AiRepoItem>();
        private AiTrendFetchResult _lastResult = null;
        private bool _isLoading = false;
        private bool _isDarkMode = false;

        // 빈 상태 및 오류 안내 오버레이 컨트롤
        private Panel _pnlEmptyOverlay = null;
        private Label _lblEmptyIcon = null;
        private Label _lblEmptyTitle = null;
        private Label _lblEmptyReason = null;
        private Button _btnEmptyRetry = null;

        public AiTrendControl()
        {
            InitializeComponent();
            _service = new AiTrendService();
        }

        private void AiTrendControl_Load(object sender, EventArgs e)
        {
            // Visual Studio 폼 디자이너에서 열릴 때는 네트워크/초기화 로직 건너뜀 (안전 방어)
            if (DesignMode) return;

            // 테마 동기화 (MaterialSkinManager 기준)
            try
            {
                bool isDark = MaterialSkinManager.Instance.Theme == MaterialSkinManager.Themes.DARK;
                ApplyTheme(isDark);
            }
            catch { }

            InitCategories();

            // 그리드 툴팁 바인딩 (긴 요약문 마우스 호버 시 전체 표시)
            dgvAiTrends.CellToolTipTextNeeded += DgvAiTrends_CellToolTipTextNeeded;

            if (cmbCategory.Items.Count > 0)
            {
                cmbCategory.SelectedIndex = 0;
            }
        }

        public void ApplyTheme(bool isDark)
        {
            _isDarkMode = isDark;

            if (isDark)
            {
                // [다크 모드]
                this.BackColor = Color.FromArgb(32, 33, 36);

                pnlTop.BackColor = Color.FromArgb(24, 25, 28);
                lblTitle.ForeColor = Color.FromArgb(240, 240, 240);
                lblSubtitle.ForeColor = Color.FromArgb(160, 160, 160);
                btnBack.BackColor = Color.FromArgb(50, 52, 58);
                btnBack.ForeColor = Color.White;

                pnlToolbar.BackColor = Color.FromArgb(32, 33, 36);
                lblCategory.ForeColor = Color.FromArgb(230, 230, 230);
                cmbCategory.BackColor = Color.FromArgb(45, 48, 55);
                cmbCategory.ForeColor = Color.White;
                btnRefresh.BackColor = Color.FromArgb(50, 52, 58);
                btnRefresh.ForeColor = Color.White;

                dgvAiTrends.BackgroundColor = Color.FromArgb(32, 33, 36);
                dgvAiTrends.GridColor = Color.FromArgb(55, 58, 65);
                dgvAiTrends.DefaultCellStyle.BackColor = Color.FromArgb(40, 44, 52);
                dgvAiTrends.DefaultCellStyle.ForeColor = Color.FromArgb(235, 240, 245);
                dgvAiTrends.DefaultCellStyle.SelectionBackColor = Color.FromArgb(65, 85, 125);
                dgvAiTrends.DefaultCellStyle.SelectionForeColor = Color.White;

                dgvAiTrends.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(24, 26, 30);
                dgvAiTrends.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(220, 220, 220);

                pnlDetail.BackColor = Color.FromArgb(28, 29, 32);
                lblDetailRepo.ForeColor = Color.FromArgb(245, 245, 245);
                lblDetailMeta.ForeColor = Color.FromArgb(180, 185, 190);

                txtDetailSummary.BackColor = Color.FromArgb(20, 35, 55);
                txtDetailSummary.ForeColor = Color.FromArgb(130, 195, 255);

                txtDetailDescription.BackColor = Color.FromArgb(38, 40, 45);
                txtDetailDescription.ForeColor = Color.FromArgb(210, 215, 220);

                btnToggleFavorite.BackColor = Color.FromArgb(50, 52, 58);
                btnToggleFavorite.ForeColor = Color.White;

                btnCopyUrl.BackColor = Color.FromArgb(50, 52, 58);
                btnCopyUrl.ForeColor = Color.White;

                btnConfigApiKey.BackColor = Color.FromArgb(50, 52, 58);
                btnConfigApiKey.ForeColor = Color.White;
            }
            else
            {
                // [라이트 모드]
                this.BackColor = Color.White;

                pnlTop.BackColor = Color.FromArgb(245, 247, 250);
                lblTitle.ForeColor = Color.FromArgb(33, 33, 33);
                lblSubtitle.ForeColor = Color.Gray;
                btnBack.BackColor = Color.FromArgb(224, 224, 224);
                btnBack.ForeColor = Color.Black;

                pnlToolbar.BackColor = Color.White;
                lblCategory.ForeColor = Color.Black;
                cmbCategory.BackColor = Color.White;
                cmbCategory.ForeColor = Color.Black;
                btnRefresh.BackColor = Color.FromArgb(240, 240, 240);
                btnRefresh.ForeColor = Color.Black;

                dgvAiTrends.BackgroundColor = Color.White;
                dgvAiTrends.GridColor = Color.FromArgb(230, 230, 230);
                dgvAiTrends.DefaultCellStyle.BackColor = Color.White;
                dgvAiTrends.DefaultCellStyle.ForeColor = Color.Black;
                dgvAiTrends.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                dgvAiTrends.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;

                dgvAiTrends.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 242, 245);
                dgvAiTrends.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

                pnlDetail.BackColor = Color.FromArgb(250, 250, 250);
                lblDetailRepo.ForeColor = Color.FromArgb(33, 33, 33);
                lblDetailMeta.ForeColor = Color.FromArgb(100, 100, 100);

                txtDetailSummary.BackColor = Color.FromArgb(243, 248, 255);
                txtDetailSummary.ForeColor = Color.FromArgb(13, 71, 161);

                txtDetailDescription.BackColor = Color.White;
                txtDetailDescription.ForeColor = Color.FromArgb(80, 80, 80);

                btnToggleFavorite.BackColor = Color.FromArgb(240, 242, 245);
                btnToggleFavorite.ForeColor = Color.FromArgb(33, 33, 33);

                btnCopyUrl.BackColor = Color.FromArgb(240, 242, 245);
                btnCopyUrl.ForeColor = Color.FromArgb(33, 33, 33);

                btnConfigApiKey.BackColor = Color.FromArgb(240, 242, 245);
                btnConfigApiKey.ForeColor = Color.FromArgb(33, 33, 33);
            }

            // 현재 선택된 아이템 세부 뷰 색상 리프레시
            if (dgvAiTrends.SelectedRows.Count > 0 && dgvAiTrends.SelectedRows[0].Tag is AiRepoItem item)
            {
                UpdateDetailView(item);
            }
        }

        private void InitCategories()
        {
            cmbCategory.Items.Clear();
            var categories = _service.GetCategories();
            foreach (var cat in categories)
            {
                cmbCategory.Items.Add(cat);
            }
        }

        private async void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DesignMode || _isLoading) return;
            await LoadDataAsync(forceRefresh: false);
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            if (DesignMode || _isLoading) return;
            await LoadDataAsync(forceRefresh: true);
        }

        private async Task LoadDataAsync(bool forceRefresh)
        {
            if (!(cmbCategory.SelectedItem is AiTrendCategory selectedCat)) return;

            try
            {
                SetLoading(true);
                lblStatus.Text = forceRefresh ? "GitHub 및 AI 요약 데이터 갱신 중..." : "데이터 로딩 중...";
                lblStatus.ForeColor = _isDarkMode ? Color.LightGray : Color.DimGray;
                lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.25F, FontStyle.Regular);

                var result = await _service.FetchRepositoriesAsync(selectedCat, forceRefresh);
                _lastResult = result;
                _currentItems = result.Items ?? new List<AiRepoItem>();

                BindGrid(_currentItems);

                lblStatus.Text = result.DisplayStatus;

                // 최신 데이터, 과거 캐시, 오프라인 스타터 팩의 시각적 구분 강화
                if (result.IsAiQuotaExceeded)
                {
                    // ⚠️ Gemini 429 무료 쿼터 초과 시 알림 (DarkOrange)
                    lblStatus.ForeColor = Color.DarkOrange;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.25F, FontStyle.Regular);
                    HideEmptyOverlay();
                }
                else if (result.IsStarterPack)
                {
                    // 📡 캐시조차 없는 오프라인: 주황색 볼드로 스타터 팩 표시
                    lblStatus.ForeColor = Color.DarkOrange;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.5F, FontStyle.Bold);
                    HideEmptyOverlay();
                }
                else if (result.IsFallbackCache)
                {
                    // ⚠️ 통신 실패로 이전 캐시 표시 중: 적갈색 볼드로 경고 표시
                    lblStatus.ForeColor = Color.Crimson;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.5F, FontStyle.Bold);
                    HideEmptyOverlay();
                }
                else if (!result.IsLiveSuccess && _currentItems.Count == 0)
                {
                    // 완전 실패 (캐시도 없고 스타터팩도 없는 극단적 상황)
                    lblStatus.ForeColor = Color.Red;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.5F, FontStyle.Bold);
                    ShowEmptyOverlay(result.FailureReason);
                }
                else if (result.IsLiveSuccess && !result.IsFallbackCache)
                {
                    // 실시간 최신 성공
                    lblStatus.ForeColor = Color.ForestGreen;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.25F, FontStyle.Regular);
                    HideEmptyOverlay();
                }
                else
                {
                    // 일반 캐시 (평상시 3시간 이내)
                    lblStatus.ForeColor = _isDarkMode ? Color.LightGray : Color.DimGray;
                    lblStatus.Font = new Font(lblStatus.Font.FontFamily, 8.25F, FontStyle.Regular);
                    HideEmptyOverlay();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "오류: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                LogManager.LogException(ex, LogManager.Level.Warning, "AiTrendControl LoadDataAsync Failed");
                if (_currentItems.Count == 0)
                {
                    ShowEmptyOverlay(ex.Message);
                }
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool loading)
        {
            _isLoading = loading;
            progressBar.Visible = loading;
            btnRefresh.Enabled = !loading;
            cmbCategory.Enabled = !loading;
        }

        private void BindGrid(List<AiRepoItem> items)
        {
            dgvAiTrends.Rows.Clear();

            foreach (var item in items)
            {
                string favIcon = item.IsFavorite ? "★ " : "";
                string displaySummary = item.DisplayFullSummary;

                int rowIndex = dgvAiTrends.Rows.Add(
                    item.Rank,
                    favIcon + item.FullName,
                    item.StarsFormatted,
                    item.Language,
                    displaySummary
                );

                dgvAiTrends.Rows[rowIndex].Tag = item;
            }

            if (dgvAiTrends.Rows.Count > 0)
            {
                dgvAiTrends.Rows[0].Selected = true;
                UpdateDetailView(items[0]);
            }
            else
            {
                ClearDetailView();
            }
        }

        private void dgvAiTrends_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAiTrends.SelectedRows.Count > 0)
            {
                var selectedRow = dgvAiTrends.SelectedRows[0];
                if (selectedRow.Tag is AiRepoItem item)
                {
                    UpdateDetailView(item);
                }
            }
        }

        private void dgvAiTrends_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvAiTrends.Rows.Count)
            {
                btnOpenGithub_Click(sender, EventArgs.Empty);
            }
        }

        private void dgvAiTrends_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // 요약 열(colSummary) 데이터 셀의 세로 간격 쾌적화 렌더링 & 테두리선(Border) 보존
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dgvAiTrends.Columns[e.ColumnIndex] == colSummary)
            {
                if (dgvAiTrends.Rows[e.RowIndex].Tag is AiRepoItem item)
                {
                    bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                    e.PaintBackground(e.ClipBounds, isSelected);

                    // 셀 내부 여백 (좌우 6px, 상하 5px)
                    Rectangle contentBounds = e.CellBounds;
                    contentBounds.X += 6;
                    contentBounds.Width -= 12;
                    contentBounds.Y += 5;
                    contentBounds.Height -= 10;

                    Color textColor = isSelected
                        ? e.CellStyle.SelectionForeColor
                        : (_isDarkMode ? Color.FromArgb(235, 240, 245) : Color.FromArgb(30, 30, 30));

                    Color tipColor = isSelected
                        ? e.CellStyle.SelectionForeColor
                        : (_isDarkMode ? Color.FromArgb(145, 200, 255) : Color.FromArgb(21, 101, 192));

                    if (!string.IsNullOrWhiteSpace(item.PracticalTip))
                    {
                        using (var font1 = new Font(e.CellStyle.Font.FontFamily, 8.75F, FontStyle.Regular))
                        using (var font2 = new Font(e.CellStyle.Font.FontFamily, 8.25F, FontStyle.Regular))
                        {
                            string line1 = $"📌 {item.AiSummary}";
                            string line2 = $"🚀 [활용] {item.PracticalTip}";

                            // 1번째 줄 (핵심 요약)
                            int line1Height = 17;
                            var rect1 = new Rectangle(contentBounds.X, contentBounds.Y, contentBounds.Width, line1Height);
                            TextRenderer.DrawText(e.Graphics, line1, font1, rect1, textColor,
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

                            // 2번째 줄 (실무 활용) - 1번째 줄과의 세로 간격을 5px 띄워서 시원하게 렌더링!
                            int lineGap = 5;
                            int line2Y = contentBounds.Y + line1Height + lineGap;
                            int line2Height = 17;
                            var rect2 = new Rectangle(contentBounds.X, line2Y, contentBounds.Width, line2Height);
                            TextRenderer.DrawText(e.Graphics, line2, font2, rect2, tipColor,
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                        }
                    }
                    else
                    {
                        // 팁이 없는 단일 줄 요약 또는 원문 설명: 셀 중앙(VerticalCenter)에 편안하게 정렬
                        using (var fontSingle = new Font(e.CellStyle.Font.FontFamily, 8.75F, FontStyle.Regular))
                        {
                            string textToDraw = !string.IsNullOrWhiteSpace(item.AiSummary)
                                ? $"📌 {item.AiSummary}"
                                : item.Description;

                            TextRenderer.DrawText(e.Graphics, textToDraw, fontSingle, contentBounds, textColor,
                                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                        }
                    }

                    // [중요] 기본 그리드 테두리선(GridLine/Border) 보존 렌더링
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Border);
                    e.Handled = true;
                    return;
                }
            }
        }

        private void UpdateDetailView(AiRepoItem item)
        {
            if (item == null)
            {
                ClearDetailView();
                return;
            }

            string favMark = item.IsFavorite ? " ⭐" : "";
            lblDetailRepo.Text = $"#{item.Rank} {item.FullName}{favMark}";

            // 오프라인 스타터 팩 / 과거 캐시 / 실시간 뱃지 구분
            string statusBadge = "";
            if (_lastResult != null && _lastResult.IsStarterPack)
            {
                statusBadge = "  |  📡 [오프라인 대표 AI 가이드]";
            }
            else if (_lastResult != null && _lastResult.IsFallbackCache)
            {
                statusBadge = $"  |  ⚠️ [오프라인 저장본: {_lastResult.DataTimestamp:yyyy-MM-dd HH:mm}]";
            }

            lblDetailMeta.Text = $"⭐ Stars: {item.StarsFormatted} ({item.Stars:N0})  |  🍴 Forks: {item.Forks:N0}  |  💻 언어: {item.Language}  |  ⏰ 최근 커밋: {item.PushedAtFormatted}{statusBadge}";

            // 찜하기 버튼 상태 동기화 및 고대비 색상 적용
            btnToggleFavorite.Text = item.IsFavorite ? "★ 찜 해제" : "⭐ 찜하기";
            if (item.IsFavorite)
            {
                btnToggleFavorite.BackColor = _isDarkMode ? Color.FromArgb(70, 60, 20) : Color.FromArgb(255, 243, 205);
                btnToggleFavorite.ForeColor = _isDarkMode ? Color.FromArgb(255, 220, 50) : Color.FromArgb(133, 100, 4);
            }
            else
            {
                btnToggleFavorite.BackColor = _isDarkMode ? Color.FromArgb(50, 52, 58) : Color.FromArgb(240, 242, 245);
                btnToggleFavorite.ForeColor = _isDarkMode ? Color.White : Color.FromArgb(33, 33, 33);
            }

            // 요약 표시 (2줄 구조: 1줄 핵심 정의, 2줄 실무/업무 활용 포인트 - 세로 여백 추가)
            if (_lastResult != null && _lastResult.IsStarterPack)
            {
                btnConfigApiKey.Visible = false; // 스타터 팩은 이미 최고 품질 한국어 해설이 완비됨
                txtDetailSummary.Text = $"📌 [핵심 요약] {item.AiSummary}\r\n\r\n🚀 [실무 활용] {item.PracticalTip}";
            }
            else
            {
                btnConfigApiKey.Visible = true;
                bool hasKey = _service.HasApiKey;
                if (!hasKey)
                {
                    btnConfigApiKey.Text = "🔑 API 등록";
                    btnConfigApiKey.BackColor = Color.FromArgb(255, 236, 179); // 골드 옐로우 강조
                    txtDetailSummary.Text = "💡 [Gemini API Key 미등록]\r\n\r\n우측 상단의 '🔑 API 등록' 버튼을 눌러 무료 키를 등록하시면 각 도구의 핵심 역할과 실무 활용 팁을 2줄로 요약해 드립니다.";
                }
                else
                {
                    btnConfigApiKey.Text = "🔑 API 설정";
                    btnConfigApiKey.BackColor = _isDarkMode ? Color.FromArgb(50, 52, 58) : Color.FromArgb(240, 242, 245);

                    if (!string.IsNullOrWhiteSpace(item.PracticalTip))
                    {
                        txtDetailSummary.Text = $"📌 [핵심 요약] {item.AiSummary}\r\n\r\n🚀 [실무 활용] {item.PracticalTip}";
                    }
                    else
                    {
                        txtDetailSummary.Text = string.IsNullOrWhiteSpace(item.AiSummary)
                            ? ("📄 [원문 요약] " + item.Description)
                            : ("📌 [핵심 요약] " + item.AiSummary);
                    }
                }
            }

            txtDetailDescription.Text = "📄 [원문 설명] " + item.Description;
            btnOpenGithub.Tag = item.HtmlUrl;
            btnOpenGithub.Enabled = !string.IsNullOrWhiteSpace(item.HtmlUrl);
            btnCopyUrl.Tag = item.HtmlUrl;
            btnCopyUrl.Enabled = !string.IsNullOrWhiteSpace(item.HtmlUrl);
        }

        private void ClearDetailView()
        {
            lblDetailRepo.Text = "표시할 프로젝트가 없습니다.";
            lblDetailMeta.Text = "-";
            txtDetailSummary.Clear();
            txtDetailDescription.Clear();
            btnOpenGithub.Tag = null;
            btnOpenGithub.Enabled = false;
            btnCopyUrl.Tag = null;
            btnCopyUrl.Enabled = false;
            btnToggleFavorite.Text = "⭐ 찜하기";
        }

        private void btnToggleFavorite_Click(object sender, EventArgs e)
        {
            if (dgvAiTrends.SelectedRows.Count == 0) return;
            if (!(dgvAiTrends.SelectedRows[0].Tag is AiRepoItem item)) return;

            bool isFavNow = _service.ToggleFavorite(item);
            UpdateDetailView(item);

            // 현재 '내가 찜한 프로젝트' 카테고리를 보고 있는 상태에서 찜 해제 시 목록에서 즉시 제거
            if (cmbCategory.SelectedItem is AiTrendCategory curCat && curCat.Id == "favorites")
            {
                if (!isFavNow)
                {
                    _currentItems.Remove(item);
                    BindGrid(_currentItems);
                    lblStatus.Text = $"⭐ 내가 찜한 AI 프로젝트 ({_currentItems.Count}개 보관 중)";
                    return;
                }
            }

            // 일반 카테고리에서는 그리드 행 텍스트 갱신 (★ 붙이기/떼기)
            var selRow = dgvAiTrends.SelectedRows[0];
            string favIcon = isFavNow ? "★ " : "";
            selRow.Cells[1].Value = favIcon + item.FullName;
        }

        private async void btnCopyUrl_Click(object sender, EventArgs e)
        {
            string url = btnCopyUrl.Tag?.ToString();
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                Clipboard.SetText(url);
                string origText = btnCopyUrl.Text;
                btnCopyUrl.Text = "✔ 복사됨!";
                btnCopyUrl.BackColor = Color.FromArgb(200, 230, 201);
                await Task.Delay(1200);
                btnCopyUrl.Text = origText;
                btnCopyUrl.BackColor = _isDarkMode ? Color.FromArgb(50, 52, 58) : Color.FromArgb(240, 242, 245);
            }
            catch (Exception ex)
            {
                MessageBox.Show("클립보드 복사 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DgvAiTrends_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvAiTrends.Rows.Count)
            {
                if (dgvAiTrends.Rows[e.RowIndex].Tag is AiRepoItem item)
                {
                    string summary = string.IsNullOrWhiteSpace(item.PracticalTip)
                        ? item.AiSummary
                        : $"📌 [핵심 요약] {item.AiSummary}\n🚀 [실무 활용] {item.PracticalTip}";

                    if (string.IsNullOrWhiteSpace(summary)) summary = item.Description;

                    string banner = "";
                    if (_lastResult != null && _lastResult.IsStarterPack)
                    {
                        banner = "[📡 인터넷 연결 대기 중 - 오프라인 AI 대표 명작 가이드]\n\n";
                    }
                    else if (_lastResult != null && _lastResult.IsFallbackCache)
                    {
                        banner = $"[⚠️ 실시간 갱신 실패로 {_lastResult.DataTimestamp:yyyy-MM-dd HH:mm} 저장된 캐시 데이터 표시 중]\n(실패 사유: {_lastResult.FailureReason})\n\n";
                    }

                    e.ToolTipText = $"{banner}[{item.FullName}]\n⭐ Stars: {item.StarsFormatted} ({item.Stars:N0}) | 언어: {item.Language} | 최근 커밋: {item.PushedAtFormatted}\n\n{summary}\n\n[📄 원문 설명]\n{item.Description}";
                }
            }
        }

        #region [Empty State Overlay]

        private void EnsureEmptyOverlayCreated()
        {
            if (_pnlEmptyOverlay != null) return;

            _pnlEmptyOverlay = new Panel
            {
                BackColor = _isDarkMode ? Color.FromArgb(32, 33, 36) : Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = dgvAiTrends.Location,
                Size = dgvAiTrends.Size
            };

            _lblEmptyIcon = new Label
            {
                Text = "📡 ❌",
                Font = new Font("맑은 고딕", 26F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            _lblEmptyTitle = new Label
            {
                Text = "데이터를 불러올 수 없습니다",
                Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                ForeColor = _isDarkMode ? Color.White : Color.FromArgb(50, 50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 35
            };

            _lblEmptyReason = new Label
            {
                Text = "인터넷 연결 상태를 확인하시거나 아래 버튼을 눌러 다시 시도하세요.",
                Font = new Font("맑은 고딕", 9F, FontStyle.Regular),
                ForeColor = _isDarkMode ? Color.LightGray : Color.FromArgb(120, 120, 120),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            _btnEmptyRetry = new Button
            {
                Text = "🔄 다시 시도 (Retry)",
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                Size = new Size(150, 36),
                BackColor = Color.FromArgb(36, 41, 46),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnEmptyRetry.FlatAppearance.BorderSize = 0;
            _btnEmptyRetry.Click += (s, ev) => btnRefresh_Click(s, ev);

            var btnContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            btnContainer.Controls.Add(_btnEmptyRetry);
            btnContainer.Padding = new Padding((_pnlEmptyOverlay.Width - 150) / 2, 5, 0, 0);

            _pnlEmptyOverlay.Controls.Add(btnContainer);
            _pnlEmptyOverlay.Controls.Add(_lblEmptyReason);
            _pnlEmptyOverlay.Controls.Add(_lblEmptyTitle);
            _pnlEmptyOverlay.Controls.Add(_lblEmptyIcon);
            _pnlEmptyOverlay.Padding = new Padding(20, 40, 20, 20);

            this.Controls.Add(_pnlEmptyOverlay);
            _pnlEmptyOverlay.BringToFront();
        }

        private void ShowEmptyOverlay(string reason)
        {
            EnsureEmptyOverlayCreated();
            _lblEmptyReason.Text = $"원인: {reason}\n네트워크 연결 상태를 확인하시거나 아래 버튼을 눌러 다시 시도하세요.";
            _pnlEmptyOverlay.Location = dgvAiTrends.Location;
            _pnlEmptyOverlay.Size = dgvAiTrends.Size;
            _pnlEmptyOverlay.Visible = true;
            _pnlEmptyOverlay.BringToFront();
        }

        private void HideEmptyOverlay()
        {
            if (_pnlEmptyOverlay != null)
            {
                _pnlEmptyOverlay.Visible = false;
            }
        }

        #endregion

        private async void btnConfigApiKey_Click(object sender, EventArgs e)
        {
            string currentKey = _service.GetGeminiApiKey();

            using (var form = new GateHelper.MysteryTime.MysteryApiKeyForm(currentKey))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    bool hasKeyNow = _service.HasApiKey;
                    if (hasKeyNow && _currentItems.Count > 0 && !(_lastResult?.IsStarterPack ?? false))
                    {
                        try
                        {
                            btnConfigApiKey.Enabled = false;
                            btnConfigApiKey.Text = "요약 생성 중...";
                            lblStatus.Text = "새로 등록된 키로 AI 핵심 요약 및 실무 활용 팁을 실시간 생성하고 있습니다...";
                            progressBar.Visible = true;

                            string catId = (cmbCategory.SelectedItem as AiTrendCategory)?.Id ?? "trending";
                            bool success = await _service.ReSummarizeItemsAsync(_currentItems, catId);

                            BindGrid(_currentItems);
                            lblStatus.Text = success 
                                ? "한국어 AI 요약 및 실무 활용 포인트가 성공적으로 적용되었습니다!" 
                                : "AI 요약 갱신 완료";
                        }
                        finally
                        {
                            btnConfigApiKey.Enabled = true;
                            btnConfigApiKey.Text = "🔑 API 설정";
                            btnConfigApiKey.BackColor = _isDarkMode ? Color.FromArgb(50, 52, 58) : Color.FromArgb(240, 242, 245);
                            progressBar.Visible = false;
                        }
                    }
                    else
                    {
                        if (dgvAiTrends.SelectedRows.Count > 0 && dgvAiTrends.SelectedRows[0].Tag is AiRepoItem item)
                        {
                            UpdateDetailView(item);
                        }
                    }
                }
            }
        }

        private void btnOpenGithub_Click(object sender, EventArgs e)
        {
            try
            {
                string url = btnOpenGithub.Tag?.ToString();
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("브라우저를 열지 못했습니다: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.BackToList();
            }
        }
    }
}
