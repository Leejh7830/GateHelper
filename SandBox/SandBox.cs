using MaterialSkin.Controls;
using System.Windows.Forms;
using GateHelper.MysteryTime;
using GateHelper.AiTrend;

namespace GateHelper
{
    public partial class SandBox : MaterialForm
    {
        private GameListControl _gameList;
        private BitFlipControl _bitFlipGame;
        private SignalLinkControl _signalLinkGame;
        private MysteryTimeControl _mysteryTimeGame;
        private AiTrendControl _aiTrendControl;

        public SandBox()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                InitializeCustomControls();
            }
        }

        private void InitializeCustomControls()
        {
            // 1. 게임 리스트 추가
            _gameList = new GameListControl();
            _gameList.Dock = DockStyle.Fill;

            if (SB_tabControl1.TabPages.Contains(tpList))
            {
                tpList.Controls.Add(_gameList);
            }

            // 2. Bit Flip 컨트롤 추가
            _bitFlipGame = new BitFlipControl();
            _bitFlipGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpBitFlip))
            {
                tpBitFlip.Controls.Add(_bitFlipGame);
            }

            // 3. Signal Link 컨트롤 추가
            _signalLinkGame = new SignalLinkControl();
            _signalLinkGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpSignalLink))
            {
                tpSignalLink.Controls.Add(_signalLinkGame);
            }

            // 4. Mystery Time 컨트롤 추가
            _mysteryTimeGame = new MysteryTimeControl();
            _mysteryTimeGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpMysteryTime))
            {
                tpMysteryTime.Controls.Add(_mysteryTimeGame);
            }

            // 5. AI Trend 컨트롤 추가
            _aiTrendControl = new AiTrendControl();
            _aiTrendControl.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpAiTrend))
            {
                tpAiTrend.Controls.Add(_aiTrendControl);
            }

            // 초기 탭 (리스트 화면)
            SB_tabControl1.SelectedTab = tpList;
        }

        public void BackToList()
        {
            SB_tabControl1.SelectedTab = tpList;
        }

        public void ShowGameList()
        {
            SB_tabControl1.SelectedTab = tpList;
        }

        public void SwitchToGame(string gameName)
        {
            if (gameName == "BitFlip")
            {
                SB_tabControl1.SelectedTab = tpBitFlip;
            }
            else if (gameName == "SignalLink")
            {
                SB_tabControl1.SelectedTab = tpSignalLink; 
            }
            else if (gameName == "MysteryTime")
            {
                if (SB_tabControl1.TabPages.Contains(tpMysteryTime))
                {
                    SB_tabControl1.SelectedTab = tpMysteryTime;
                }
            }
            else if (gameName == "AiTrend")
            {
                if (SB_tabControl1.TabPages.Contains(tpAiTrend))
                {
                    _aiTrendControl?.ApplyTheme(MaterialSkin.MaterialSkinManager.Instance.Theme == MaterialSkin.MaterialSkinManager.Themes.DARK);
                    SB_tabControl1.SelectedTab = tpAiTrend;
                }
            }
        }
    }
}