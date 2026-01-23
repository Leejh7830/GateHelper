using MaterialSkin.Controls;
using System.Windows.Forms;

namespace GateHelper
{
    public partial class SandBox : MaterialForm
    {
        private GameListControl _gameList;
        private BitFlipControl _bitFlipGame;
        private SignalLinkControl _signalLinkGame;

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
            // 1. 게임 목록 컨트롤 생성 및 추가
            _gameList = new GameListControl();
            _gameList.Dock = DockStyle.Fill;

            if (SB_tabControl1.TabPages.Contains(tpList))
            {
                tpList.Controls.Add(_gameList);
            }

            // 2. Bit Flip 컨트롤 생성 및 추가
            _bitFlipGame = new BitFlipControl();
            _bitFlipGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpBitFlip))
            {
                tpBitFlip.Controls.Add(_bitFlipGame);
            }

            // 3. Signal Link 컨트롤 생성 및 추가
            _signalLinkGame = new SignalLinkControl();
            _signalLinkGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpSignalLink))
            {
                tpSignalLink.Controls.Add(_signalLinkGame);
            }

            // 초기 탭 설정 (목록 화면)
            SB_tabControl1.SelectedTab = tpList;
        }

        public void BackToList()
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
        }
    }
}