using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace TasollerLED
{
    public partial class Form1 : Form
    {
        Padlight padlight = new Padlight();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ecex = Padlight.DeviceSetUp();
            label2.Text = ecex.Item1 != ErrorCode.None ? ecex + ":" : String.Empty + ecex.Item2;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            padlight.TimerStart();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            padlight.TimerStop();
        }

        private void trackBarR_Scroll(object sender, EventArgs e)
        {
            Padlight.padColor[(int)numericUpDown1.Value].R = (byte)trackBarR.Value;
        }

        private void trackBarG_Scroll(object sender, EventArgs e)
        {
            Padlight.padColor[(int)numericUpDown1.Value].G = (byte)trackBarG.Value;
        }

        private void trackBarB_Scroll(object sender, EventArgs e)
        {
            Padlight.padColor[(int)numericUpDown1.Value].B = (byte)trackBarB.Value;
        }
    }
}
