using MaterialSkin.Controls;
using System.Windows.Forms;

namespace GateHelper
{
    // SandBox 폼 클래스
    public partial class SandBox : MaterialForm
    {
        // 필드 선언 (에러 방지를 위해 초기에는 null로 설정)
        private GameListControl _gameList;
        private BitFlipControl _bitFlipGame;

        public SandBox()
        {
            InitializeComponent();

            // 디자인 모드가 아닐 때만 실행 (디자이너 에러 방지)
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

            if (SB_tabControl1.TabPages.Contains(SB_tpList))
            {
                SB_tpList.Controls.Add(_gameList);
            }


            //////////////////////////////////////////////////////////// L I S T //////////////////////////////////////////////////////////

            // 비트 플립 컨트롤 생성 및 추가
            _bitFlipGame = new BitFlipControl();
            _bitFlipGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpBitFlip))
            {
                tpBitFlip.Controls.Add(_bitFlipGame);
            }

            //////////////////////////////////////////////////////////// L I S T //////////////////////////////////////////////////////////


            // 초기 탭 설정
            SB_tabControl1.SelectedTab = SB_tpList;
        }

        public void BackToList()
        {
            SB_tabControl1.SelectedTab = SB_tpList; // 목록으로 이동
        }

        public void SwitchToGame(string gameName)
        {
            if (gameName == "BitFlip")
            {
                SB_tabControl1.SelectedTab = tpBitFlip;
            }
        }
    }
}