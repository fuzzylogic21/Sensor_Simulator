using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.PerformanceData ;
using System.IO;
using System.Runtime.InteropServices;
using ComType = System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace SensorSimulator
{


    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

           

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            String Trial1;
            int Lstart, Lend;
            foreach (DriveInfo d in allDrives)
            {
                listBox1.Items.Add("Drive " + d.Name);
                //richTextBox1.ForeColor = Color.Red;
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectionColor = Color.Red;
                //richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Italic);
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);

                richTextBox1.AppendText("Drive " + d.Name +"\n");
                

                listBox1.Items.Add("  Drive type: " + d.DriveType);
                richTextBox1.AppendText("  Drive type: " + d.DriveType + "\n");

                if (d.IsReady == true)
                {
                    listBox1.Items.Add("  Volume label: " + d.VolumeLabel);
                    richTextBox1.AppendText("  Volume label: " + d.VolumeLabel + "\n");

                    listBox1.Items.Add("  File system: " + d.DriveFormat);
                    richTextBox1.AppendText("  File system: " + d.DriveFormat + "\n");

                    listBox1.Items.Add(
                        "  Available space to current user:  " +
                        d.AvailableFreeSpace + " bytes");
                    richTextBox1.AppendText(
                        "  Available space to current user:  " +
                        d.AvailableFreeSpace + " bytes\n");

                    long tFreeSpace; 
                    long tSize;

                    tFreeSpace = d.TotalFreeSpace;
                    tSize = d.TotalSize;

                    listBox1.Items.Add(
                        "  Total available space:  " +
                        d.TotalFreeSpace + " bytes");
                    richTextBox1.AppendText("  Total available space:  " + tFreeSpace + " bytes\n");

                    listBox1.Items.Add(
                        "  Total size of drive: " +
                        d.TotalSize + " bytes");
                    richTextBox1.AppendText("  Total size of drive: " + tSize + " bytes\n");

                    Single pFreeSpace;  // percentage Free Space
                    pFreeSpace = (Convert.ToSingle(tFreeSpace) / Convert.ToSingle(tSize)) * 100;
                    //pFreeSpace = Math.Round(pFreeSpace, 3);
                    listBox1.Items.Add("  Drive Usage: " + pFreeSpace  + " %");
                    richTextBox1.AppendText("  Drive Usage: "  + Convert.ToString(Math.Round(pFreeSpace, 3)) + " %\n");

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int a;
            //a=GetCpuUsage();
            //richTextBox1.AppendText(a.ToString());

            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            listBox1.Items.Add(cpuCounter);
            listBox1.Items.Add(ramCounter);
        }

        public int GetCpuUsage()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", "MyComputer");
            listBox1.Items.Add(cpuCounter);
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            return (int)cpuCounter.NextValue();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            richTextBox1.Clear();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = Clipboard.GetText();
        }
    }


}
