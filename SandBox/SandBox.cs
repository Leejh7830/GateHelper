using MaterialSkin;
using MaterialSkin.Controls;
using System;
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
            // tpList는 TabControl에 미리 생성해둔 TabPage의 이름이어야 합니다.
            if (SB_tabControl1.TabPages.Contains(tpList))
            {
                tpList.Controls.Add(_gameList);
            }

            // 2. 비트 플립 컨트롤 생성 및 추가
            _bitFlipGame = new BitFlipControl();
            _bitFlipGame.Dock = DockStyle.Fill;
            if (SB_tabControl1.TabPages.Contains(tpBitFlip))
            {
                tpBitFlip.Controls.Add(_bitFlipGame);
            }

            // 초기 탭 설정
            SB_tabControl1.SelectedTab = tpList;
        }

        public void BackToList()
        {
            // tpList: 게임 목록(GameListControl)이 들어있는 탭 페이지 이름
            SB_tabControl1.SelectedTab = tpList;
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