using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SensorSimulator
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            int localcounter = Globals.counter;
            MessageBox.Show(localcounter.ToString(),
            "Operation Triggering for Form2",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button2);

            Globals.counter += 200;
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            String prefixM;   // prefix for Modules
            int a, i, n;
            n = 1;
            a = comboBox6.SelectedIndex;
            //sCmd = "";    // Send Command
            //rCmd = "";    //  Command for Receive from NodeMCU
            //MessageBox.Show(a.ToString() );
            prefixM = "Choose Main Module FIRST";
            if (a == 0)
            {
                prefixM = "HVAC";
                n = Globals.nHVAC;
            }
            if (a == 1)
            {
                prefixM = "AHU";
                n = Globals.nAHU;
            }
            if (a == 2)
            {
                prefixM = "COND";
                n = Globals.nCOND; 
            }
            if (a == 3)
            {
                prefixM = "FAN";
                n = Globals.nFAN; 
            }
            if (a == 4)
            {
                prefixM = "Zone";
                n = Globals.nZone; 
            }
            if (a == 5)
            {
                prefixM = "Chiller";
                n = Globals.nChiller; 
            }
            if (a == 6)
            {
                prefixM = "Pump";
                n = Globals.nPump; 
            }
            if (a == 7)
            {
                prefixM = "SoV";
                n = Globals.nSoV; 
            }
            if (a == 8)
            {
                prefixM = "WtTank";
                n = Globals.nWtTank;
             }
            if (a == 9)
            {
                prefixM = "dPeriod";
                comboBox1.Items.Clear();
                return;
            }
            comboBox1.Items.Clear();
            for (i = 1; i <=n; ++i)
            {
                comboBox1.Items.Add(prefixM + " " + i.ToString() );
                comboBox1.SelectedIndex = i - 1;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Globals.nHVAC  = Convert.ToInt16(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Globals.nAHU = Convert.ToInt16(textBox2.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Globals.nCOND  = Convert.ToInt16(textBox3.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Globals.nFAN  = Convert.ToInt16(textBox4.Text);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Globals.nZone  = Convert.ToInt16(textBox5.Text);
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Globals.nChiller  = Convert.ToInt16(textBox6.Text);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            Globals.nPump  = Convert.ToInt16(textBox7.Text);
            comboBox6_SelectedIndexChanged(sender, e);
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            Globals.nSoV  = Convert.ToInt16(textBox8.Text);
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            Globals.nWtTank  = Convert.ToInt16(textBox9.Text);
        }
    }
}