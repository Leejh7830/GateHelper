using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper
{
    public class TransparentPictureBox : PictureBox
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // 배경을 그리지 않음으로써 부모 배경이 그대로 보이게 함
        }
    }
}
