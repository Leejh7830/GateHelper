using System;
using System.Windows.Forms;

namespace GateHelper
{
    public partial class GameListControl : UserControl
    {
        public GameListControl()
        {
            InitializeComponent();
        }


        private void btnSelectBitFlip_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.SwitchToGame("BitFlip");
            }
        }

        private void GL_btnSelectSignalLink_Click(object sender, EventArgs e)
        {
            if (this.ParentForm is SandBox sb)
            {
                sb.SwitchToGame("SignalLink");
            }
        }
    }
}