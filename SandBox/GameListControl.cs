using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;

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
    }
}