using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GateHelper.LogValidator
{
    public partial class LogValidatorForm : MaterialForm
    {
        private readonly MaterialSkinManager _skinManager = MaterialSkinManager.Instance;

        public LogValidatorForm()
        {
            InitializeComponent();
            _skinManager.AddFormToManage(this); // 테마 동기화 인터락
        }
    }
}
