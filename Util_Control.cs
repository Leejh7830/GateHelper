using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateBot
{
    internal class Util_Control
    {
        public static void MoveControl(Control control, int x, int y)
        {
            control.Location = new Point(x, y);
        }
    }
}
