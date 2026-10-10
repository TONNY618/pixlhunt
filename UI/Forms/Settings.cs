using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace pxlhunt.FORMS
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            // Окно настроек всегда поверх остальных
            this.TopMost = true;
        }
    }
}
