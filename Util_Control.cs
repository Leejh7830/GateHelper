using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateBot
{
    internal class Util_Control
    {
        public static void MoveFormToTop(Form form)
        {
            Thread.Sleep(500);
            form.TopMost = true;
            form.Activate();
            form.TopMost = false;
        }

        public static void MoveControl(Control control, int x, int y)
        {
            control.Location = new Point(x, y);
        }

        public static void UpdateCheckBoxText(int popUpCount, CheckBox disablePopupCheckBox1)
        {
            disablePopupCheckBox1.Text = $"DISABLE POP-UP ({popUpCount})";
        }
    }
}
