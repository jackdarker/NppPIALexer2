using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NppPIALexer2.Forms
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void btnTrue_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void frmAbout_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            Hide();
        }
    }
}
