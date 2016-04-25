using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RLocal
{
    public partial class RLocalKeyboardConfigForm : Form
    {
        RLocalKeyboardInput keyboardInput;

        public RLocalKeyboardConfigForm(RLocalKeyboardInput keyboardInput)
        {
            this.keyboardInput = keyboardInput;
            InitializeComponent();
        } 
    }
}
