using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net.Http;
using System.Net.Http.Headers;
//using System.Net.Http.Formatting;

namespace SensorSimulator
{
   

    public partial class frmMain : Form
    {
        bool ReadStatus;    // for RS232 port to interface with NodeMCU
        bool AutoPostDataStatus;  // True = send automatically the data to http server
        int HeartBeatRate;      // duration between sebsequent data to sent
        long postAttempts;      // Number of times data (json) sent to server
        String postData;
        long WaterTotal;
//        int nEN = 10, nRE = 10;
//        int nHVAC = 1, nAHU = 3, nCOND = 1, nFAN = 4, nZone = 5, nChiller = 1, nPump = 3, nSoV = 1, nWtTank = 1;
        int MacidInc = 1;
        long dPeriod = 1000; //1 seconds
        Random rnd = new Random();
        String IPaddress;
        bool AHU_Relay_R = true, AHU_Relay_Y = true, AHU_Relay_B = true;
        bool COND_Relay_R = true, COND_Relay_Y = true, COND_Relay_B = true;
        bool FAN_Relay_R = true, FAN_Relay_Y = true, FAN_Relay_B = true;
        bool Chiller_Relay_R = true, Chiller_Relay_Y = true, Chiller_Relay_B = true;
        bool Pump_Relay_R = true, Pump_Relay_Y = true, Pump_Relay_B = true;
        bool SoV_Relay_R = true, SoV_Relay_Y = true, SoV_Relay_B = true;
        int Relay_R = 1, Relay_Y=1, Relay_B=1;
        //long WaterTotal;
        long EnergyTotal = 16256;
        //public int Counter;

        
        public frmMain()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            portscan();
        }

        private void portscan()
        {
            comboBox1.Items.Clear();
            comboBox1.Text = "";
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                comboBox1.Items.Add(s);
            int a;
            a = comboBox1.Items.Count;
            if (a >= 1)
            {
                comboBox1.SelectedIndex = a - 1;
            }
            //MessageBox.Show("Number of COMM Port Detected : " + a.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 9;        // Baud Rate 5 = 9600; 9 = 115200
            comboBox3.SelectedIndex = 1;        // Stop bit = 1
            comboBox4.SelectedIndex = 0;        // Parity = None
            comboBox5.SelectedIndex = 1;        // Data bit = 8
            portscan();
            comboBox6.SelectedIndex = 1;        // AHU
            HeartBeatRate = Convert.ToInt16(textBox3.Text);
            postAttempts = 0;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.BaudRate = Convert.ToInt16(comboBox2.Text);
                serialPort1.Parity = comboBox4.SelectedIndex + System.IO.Ports.Parity.None;
                serialPort1.StopBits = comboBox3.SelectedIndex + System.IO.Ports.StopBits.None;
                serialPort1.DataBits = Convert.ToInt16(comboBox5.Text);   //8


                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
            }
            catch
            {
                MessageBox.Show("No Port Selected/Detected!");
                //stopSerial.Enabled = false;
            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //listBox1.Items.Add(label6.Text + textBox1.Text.Trim());

            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);  //115200; 
            serialPort1.DataBits = Convert.ToInt16(comboBox5.Text);
            serialPort1.Parity = comboBox4.SelectedIndex + System.IO.Ports.Parity.None;
            serialPort1.StopBits = comboBox3.SelectedIndex + System.IO.Ports.StopBits.None;

            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
                serialPort1.WriteLine(label6.Text + textBox1.Text.Trim());
                serialPort1.Close();
                DateTime dt = DateTime.Now;
                listBox1.Items.Add (String.Format("{0:T}", dt) + " Data Sent : " + label6.Text + textBox1.Text.Trim()); // "4:05:07 PM")
            }
            catch
            { listBox1.Items.Add("Error in Connecting Port "); }
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            String sCmd, rCmd;
            int a;
            a = comboBox6.SelectedIndex;
            sCmd = "";    // Send Command
            rCmd = "";    //  Command for Receive from NodeMCU
            //MessageBox.Show(a.ToString() );
            if (a == 0) { sCmd = "nHVAC="; rCmd = "nHVAC?"; }
            if (a == 1) { sCmd = "nAHU="; rCmd = "nAHU?"; }
            if (a == 2) { sCmd = "nCOND="; rCmd = "nCOND?"; }
            if (a == 3) { sCmd = "nFAN="; rCmd = "nFAN?"; }
            if (a == 4) { sCmd = "nZone="; rCmd = "nZone?"; }
            if (a == 5) { sCmd = "nChiller="; rCmd = "nChiller?"; }
            if (a == 6) { sCmd = "nPump="; rCmd = "nPump?"; }
            if (a == 7) { sCmd = "nSoV="; rCmd = "nSoV?"; }
            if (a == 8) { sCmd = "nWtTank="; rCmd = "nWtTank?"; }
            if (a == 9) { sCmd = "dPeriod="; rCmd = "dPeriod?"; }
            label6.Text = sCmd;
            label7.Text = rCmd;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Reading HVAC Simulator Parameters
            String txt, rCmd;
            txt = "";
            rCmd = label7.Text;    //  Command for Receive from NodeMCU
            if (radioButton1.Checked  ) { rCmd = "All?"; }
            //listBox1.Items.Add(label7.Text);

            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);  //115200; 
            serialPort1.DataBits = Convert.ToInt16(comboBox5.Text);
            serialPort1.Parity = comboBox4.SelectedIndex + System.IO.Ports.Parity.None;
            serialPort1.StopBits = comboBox3.SelectedIndex + System.IO.Ports.StopBits.None;

            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
                serialPort1.WriteLine(rCmd);
                //serialPort2.Close();
                DateTime dt = DateTime.Now;
                listBox1.Items.Add(String.Format("{0:T}", dt) + " Data Sent : " + rCmd ); 
                ReadStatus = true;
                while (ReadStatus)
                {
                    //txt += serialPort1.ReadExisting().ToString();
                    txt = serialPort1.ReadExisting().ToString();
                    richTextBox1.AppendText(txt);
                    // scroll it automatically
                    richTextBox1.ScrollToCaret();
                    //Don't use this!
                    //richTextBox1.AppendText(text);
                    //richTextBox1.ScrollToEnd();

                    Application.DoEvents();
                    if (ReadStatus == false)
                        break;
                }
                serialPort1.Close();

            }
            catch
            { listBox1.Items.Add("Error in Connecting Port "); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ReadStatus = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String address;
            //String postData;
            //byte[] dataJ;
                            
            address = textBox2.Text.Trim();
            //dataJ = Encoding.ASCII.GetBytes(textBox3.Text.Trim()); 
            using (var client = new System.Net.WebClient())
            {
                client.BaseAddress = address;       
                client.Headers.Add("Content-Type", "text/plain");   //application/json

                var result = client.UploadString(address, "PUT", textBox3.Text.Trim());

                //http://192.168.0.122:8080
                //client.UploadData(address, "PUT", dataJ);
                richTextBox1.AppendText(result);
                richTextBox1.ScrollToCaret();
            }
        }

        private void Converter4051(String UIDstr, String MACnum)
        {
            Application.DoEvents();
            var result = " ";
            richTextBox1.AppendText("\nSent as " + UIDstr);
            richTextBox1.AppendText(MACnum + "\n");

            postData = "   { ";
            postData += "\"TIM\": \"San Francisco USA Simulator\", ";
            //postData += "\"TIM\": \"" + String(t / 1000) + " s, C " + String(Cnt) + "\", ";
            postData += MACnum;     //postData += "\"MAC\": \"18-78-D4-00-00-02\", ";
            postData += UIDstr;     //postData += "\"UID\": \"AHU_E_192.168.0.2\", ";
            postData += "\"PE\": 128, ";
            postData += "\"Record\": [";
            postData += "[0,0,1,1],";
            postData += "[0,1,1,1],";
            postData += "[0,2,1,1],";
            postData += "[0,3,1,0],";
            postData += "[0,4,1,0],";
            postData += "[0,5,1,0],";
            postData += "[0,6,3,0],";
            int rpm = rnd.Next(250, 315);    //250*6 = 1500 rpm to 315*6 = 1890 rpm
            postData += "[0,7,3," + rpm.ToString() + "],";



            //Voltage R-Phase
            int Vr0;
            Vr0 = rnd.Next(36045, 36055);
            postData += "[1,0,32," + Vr0.ToString() + "],";
            postData += "[1,0,33,0],";
            int Vr1;
            //Vr1 = random(17255, 17265);     // 17255 decimal = 4367 Hex -->  43670000 = 199 Volt
            //42D40000 = 106 Volt     // 42DA0000 = 109 Volt //  42F40000 = 122 volt //  4300 0000 = 128 Volt
            //42DA = 17114 decimal  // 42F4 = 17140
            Vr1 = rnd.Next(17114, 17140);
            postData += "[1,1,32," + Vr1.ToString() + "],";
            postData += "[1,1,33,0],";

            //Voltage Y-Phase
            int Vr2;
            Vr2 = rnd.Next(36045, 58055);
            postData += "[1,2,32," + Vr2.ToString() + "],";
            postData += "[1,2,33,0],";
            int Vr3;
            Vr3 = rnd.Next(17114, 17140);
            postData += "[1,3,32," + Vr3.ToString() + "],";
            postData += "[1,3,33,0],";

            //Voltage B-Phase
            int Vr4;
            Vr4 = rnd.Next(36045, 36055);
            postData += "[1,4,32," + Vr4.ToString() + "],";
            postData += "[1,4,33,0],";
            int Vr5;
            Vr5 = rnd.Next(17114, 17140);
            postData += "[1,5,32," + Vr5.ToString() + "],";
            postData += "[1,5,33,0],";

            // Current R-Phase
            int Vr6;
            Vr6 = rnd.Next(45718, 45728);
            postData += "[1,6,32," + Vr6.ToString() + "],";
            postData += "[1,6,33,0],";
            int Vr7;
            //Vr7 = random(15756, 16540);
            //437A0000 = 250 Amp,  437A = 17274 decimal ;   438A0000 = 276 Amp,  438A = 17290 decimal
            Vr7 = rnd.Next(17274, 17290);
            postData += "[1,7,32," + Vr7.ToString() + "],";
            postData += "[1,7,33,0],";

            //Current Y-Phase
            int Vr8;
            Vr8 = rnd.Next(45718, 45728);
            postData += "[1,8,32," + Vr8.ToString() + "],";
            postData += "[1,8,33,0],";
            int Vr9;
            Vr9 = rnd.Next(17274, 17290);
            postData += "[1,9,32," + Vr9.ToString() + "],";
            postData += "[1,9,33,0],";

            //Current B-phase
            int Vr10;
            Vr10 = rnd.Next(45718, 45728);
            postData += "[1,10,32," + Vr10.ToString() + "],";
            postData += "[1,10,33,0],";
            int Vr11;
            Vr11 = rnd.Next(17274, 17290);
            postData += "[1,11,32," + Vr11.ToString() + "],";
            postData += "[1,11,33,0],";

            //Power - R kW1
            int Vr12;
            Vr12 = rnd.Next(57728, 57738);
            postData += "[1,12,32," + Vr12.ToString() + "],";
            postData += "[1,12,33,0],";
            int Vr13;
            //44700000 = 960   // 447A0000 = 1000
            //4470 hex = 17520 decimal   // 447A = 17530
            //438B0000 = 278 Amp    // 43960000 = 300
            //438B hex = 17291      // 4396 hex = 17302
            Vr13 = rnd.Next(15366, 15377);
            postData += "[1,13,32," + Vr13.ToString() + "],";
            postData += "[1,13,33,0],";

            //Power - Y  kW2
            int Vr14;
            Vr14 = rnd.Next(57728, 57738);
            postData += "[1,14,32," + Vr14.ToString() + "],";
            postData += "[1,14,33,0],";
            int Vr15;
            Vr15 = rnd.Next(15366, 15377);
            postData += "[1,15,32," + Vr15.ToString() + "],";
            postData += "[1,15,33,0],";

            //Power - B   kW3
            int Vr16;
            Vr16 = rnd.Next(57728, 57738);
            postData += "[1,16,32," + Vr16.ToString() + "],";
            postData += "[1,16,33,0],";
            int Vr17;
            Vr17 = rnd.Next(15366, 15377);
            postData += "[1,17,32," + Vr17.ToString() + "],";
            postData += "[1,17,33,0],";

            //Power Factor - R PF1
            int Vr18;
            Vr18 = rnd.Next(48130, 48140);
            postData += "[1,18,32," + Vr18.ToString() + "],";
            postData += "[1,18,33,0],";
            int Vr19;
            Vr19 = rnd.Next(48901, 49101);
            postData += "[1,19,32," + Vr19.ToString() + "],";
            postData += "[1,19,33,0],";

            //Power Factor - Y PF2
            int Vr20;
            Vr20 = rnd.Next(48130, 48140);
            postData += "[1,20,32," + Vr20.ToString() + "],";
            postData += "[1,20,33,0],";
            int Vr21;
            Vr21 = rnd.Next(48901, 49101);
            postData += "[1,21,32," + Vr21.ToString() + "],";
            postData += "[1,21,33,0],";

            //Power Factor - B PF3
            int Vr22;
            Vr22 = rnd.Next(48130, 48140);
            postData += "[1,22,32," + Vr22.ToString() + "],";
            postData += "[1,22,33,0],";
            int Vr23;
            Vr23 = rnd.Next(48901, 49101);
            postData += "[1,23,32," + Vr23.ToString()  + "],";
            postData += "[1,23,33,0],";


            //frequency
            int Vr24;
            Vr24 = rnd.Next(8913, 9013);
            postData += "[1,24,32," + Vr24.ToString() + "],";
            postData += "[1,24,33,0],";
            int Vr25;
            //Vr25 = random(16968, 17008);       // 16968 decimal = 4248 hex --> 42480000 = 50 Hz
            //17008 decimal = 4270 hex --> 42700000 = 60 Hz
            //426A0000 = 58.5 Hz   426E0000 = 59.5 Hz    42720000 = 60.5 Hz
            //426E Hex = 17006 Decimal,  4272 Hex = 17010 Decimal
            Vr25 = rnd.Next(16968, 17008);
            postData += "[1,25,32," + Vr25.ToString() + "],";
            postData += "[1,25,33,0],";

            //Energy
            //int Vr26;
            //Vr26 = random(8913, 9013);
            postData += "[1,26,32,0],";
            postData += "[1,26,33,0],";
            int Vr27;
            //Vr27 = rnd.Next(16255, 17265);
            //  postData += "[1,27,32," + Vr27.ToString()  + "],";
            postData += "[1,27,32," + EnergyTotal.ToString()  + "],";
            postData += "[1,27,33,0],";
            EnergyTotal += 1;
            if (EnergyTotal < 16256) EnergyTotal = 16256;
            postData += "[1,28,32,0],";
            postData += "[1,28,33,0],";

            int Vr29;
            Vr29 = rnd.Next(100, 150);  // Chiller Inlet Temperature  100 /10 = 10 degree C to 150/10 = 15 degree C
            //    Vr29 = random(-150, 100);   // Refregirator  -150 / 10 = -15,
            //    Serial.print("\nRef. Temperature : ");
            //    Serial.println(Vr29);
            postData += "[1,29,32," + Vr29.ToString()  + "],";
            postData += "[1,29,33,0],";
            postData += "[1,30,32,0],";
            postData += "[1,30,33,0],";

            int Vr31;
            Vr31 = rnd.Next(60, 120);  // Chiller Outlet Temperature 60 /10 = 6 degree C to 120/10 = 12 degree C
            postData += "[1,31,32," + Vr31.ToString()  + "],";
            postData += "[1,31,33,0],";
            postData += "[1,32,32,0],";
            postData += "[1,32,33,0],";

            //Water Flow rate
            int WaterFlow;
            WaterFlow = rnd.Next(1, 10);
            postData += "[1,33,32," + WaterFlow.ToString() + "],";
            postData += "[1,33,33,0],";
            postData += "[1,34,32,0],";
            postData += "[1,34,33,0],";

            //Water Totalizer
            WaterTotal = WaterTotal + rnd.Next(1, 10);
            postData += "[1,35,32," + WaterTotal.ToString()  + "],";
            postData += "[1,35,33,0],";
            postData += "[1,36,32,0],";
            postData += "[1,36,33,0],";

            //Refregirator - Humidity
            int h;
            h = rnd.Next(40, 65);
            postData += "[1,37,32," + h.ToString() + "],";
            postData += "[1,37,33,0],";
            postData += "[1,38,32,0],";
            postData += "[1,38,33,0],";

            //Refregirator - Light Intensity
            int LI;
            LI = rnd.Next(0, 100);
            postData += "[1,39,32," + LI.ToString() + "],";
            postData += "[1,39,33,0]]}";

            //postData += "  }";

            IPaddress = textBox2.Text.Trim();
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.BaseAddress = IPaddress;
                    //client.Headers.Add("Content-Type", "text/plain");   //application/json
                    client.Headers.Add("Content-Type", "application/json");   //application/json
                    result = client.UploadString(IPaddress, "PUT", postData);
                    richTextBox1.AppendText(result);
                    //richTextBox1.ScrollToCaret();
                }
            }
            catch (System.Net.WebException e2)
            {
                richTextBox1.AppendText("\nERROR : " + e2.Message);
                if (e2.Message == "Unable to connect to the remote server")
                { richTextBox1.AppendText("\nKindly check the availablity of IP : " + IPaddress); }
                //richTextBox1.AppendText(e2.ToString());
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("\nresult1: " + result + "\n");
                richTextBox1.AppendText("\n MESSAGE: " + ex.Message);
                richTextBox1.AppendText("\n STACK TRACE: " + ex.StackTrace);
                richTextBox1.AppendText("\n Source: " + ex.Source);
                richTextBox1.AppendText("\n TargetSite: " + ex.TargetSite + "\n");
                richTextBox1.AppendText(ex.ToString());

            }
            finally
            {
                //richTextBox1.AppendText("[Done]");
                richTextBox1.ScrollToCaret();
            }
        }


        private void Converter4220(String UIDstr, String MACnum)
        {
            Application.DoEvents();
            var result = " ";
            richTextBox1.AppendText("\n" + UIDstr );
            richTextBox1.AppendText(MACnum + "\n");
            postData = "{ ";
            postData += "\"TIM\": \"C#Simulator-01\", ";
            //postData += "\"TIM\": \"" + String(t / 1000) + " s, C " + String(Cnt) + "\", ";
            postData += MACnum;      //"\"MAC\": \"18-78-D4-00-00-01\", ";
            postData += UIDstr;      //"\"UID\": \"ZONE_192.168.0.1\", ";
            postData += "\"PE\": 128, ";
            double  Te, Hu;
            //Te = Math.Round(rnd.Next(180,300) / 9.9, 2);
            Te = rnd.Next(180, 300) / 10;
            //Hu = Math.Round(rnd.Next(400, 700) / 9.9 , 2);
            Hu = rnd.Next(400, 700) / 10;

            postData += "\"Record\": ";
            postData += "[ [ 0, 0, 40, " + Te.ToString()  + " ], ";
            postData += "[ 0, 0, 43, 0 ], ";
            postData += "[ 0, 1, 40, " + Hu.ToString()  + " ], ";
            postData += "[ 0, 1, 43, 0 ] ] ";
            postData += "  }";

            string jsonData = "{ \"FirstName\":\"Ramesh\",\"LastName\":\"Chinnaraju\" }";

            IPaddress = textBox2.Text.Trim();
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.BaseAddress = IPaddress;
                    //client.Headers.Add("Content-Type", "text/plain");   //application/json
                    client.Headers.Add("Content-Type", "application/json");   //application/json
                    result = client.UploadString(IPaddress, "PUT", postData);
                    richTextBox1.AppendText(result);
                    //richTextBox1.ScrollToCaret();
                    //listBox1.Items.Add(result);
                }
            }
            catch (System.Net.WebException e2)
            {
                //richTextBox1.AppendText("\nresult2: " + result + "\n");
                richTextBox1.AppendText("\nERROR : " + e2.Message);
                if (e2.Message == "Unable to connect to the remote server" )
                { richTextBox1.AppendText("\nKindly check the availablity of IP : " + IPaddress); }
                //richTextBox1.ScrollToCaret();
                //richTextBox1.AppendText("\n STACK TRACE2: " + e2.StackTrace);
                //richTextBox1.AppendText("\n Source2: " + e2.Source);
                //richTextBox1.AppendText("\n TargetSite2: " + e2.TargetSite);
                //richTextBox1.AppendText(e2.ToString());
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText("\nresult1: " + result + "\n");
                richTextBox1.AppendText("\nMESSAGE: " + ex.Message);
                richTextBox1.AppendText("\nSTACK TRACE: " + ex.StackTrace );
                richTextBox1.AppendText("\nSource: " + ex.Source );
                richTextBox1.AppendText("\nTarget Site: " + ex.TargetSite + "\n");
                richTextBox1.AppendText(ex.ToString());


            }
            finally
            {
                //richTextBox1.AppendText("[Done]");
                richTextBox1.ScrollToCaret();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            IPaddress = textBox2.Text.Trim();
            richTextBox1.AppendText("\nSending data to " + IPaddress + " initiated on ");
            richTextBox1.AppendText(DateTime.Now.ToString("dddd, dd MMMM yyyy") + "  " + DateTime.Now.ToString("h:mm:ss tt") + "\n");
            Converter4051("\"UID\": \"AHU_192.168.0.2\", ", "\"MAC\": \"18-78-D4-00-00-02\", ");
            Application.DoEvents();
            Converter4220("\"UID\": \"ZONE_192.168.0.1\", ", "\"MAC\": \"18-78-D4-00-00-01\", ");
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void localStaticIPDellEdgeAtIITMRPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox2.Text = "http://192.168.0.122:8080";
        }

        private void globalStaticIPDellEdgeAtIITMRPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox2.Text = "http://14.142.211.110:8080";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label15.Text = DateTime.Now.ToString("h:mm:ss tt");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            timer2.Enabled = true;
            HeartBeatRate = Convert.ToInt16(textBox3.Text);
            timer2.Interval = HeartBeatRate;
            AutoPostDataStatus = true;
             }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            HeartBeatRate = Convert.ToInt16(textBox3.Text);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            //timer2.Interval = 0;
            AutoPostDataStatus = false;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //
            postAttempts += 1;
            richTextBox1.AppendText("\nAttempt : " + postAttempts.ToString() );
            Application.DoEvents();
            Converter4051("\"UID\": \"AHU_192.168.0.2\", ", "\"MAC\": \"18-78-D4-00-00-02\", ");
            richTextBox1.ScrollToCaret();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Globals.counter += 101;
            Form2 frm = new Form2();
            frm.Show();
            return;
            //Form2.ActiveForm.Show();
            //MessageBox.Show("Form2 Activated",
            //"Operation Triggering for Form2",
            //MessageBoxButtons.OK,
            //MessageBoxIcon.Information,
            //MessageBoxDefaultButton.Button2);

            //this.Close();
            //this.Hide();
        }

        private void listBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int a, i;
            String Matter;
            a = listBox1.Items.Count - 1;

            if (a < 1)
            {
                MessageBox.Show("No Data Available for Copy to Clipboard",
            "Operation Aborted",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button2);
            return;
            }

            Matter = "";
            for (i = 0; i < a; i++)
            {
                Matter += listBox1.Items[i].ToString() + Environment.NewLine;
            }

            Matter += "Date of copy to Clipboard : " + DateTime.Now.ToString("dddd, dd MMMM yyyy") + "  " + DateTime.Now.ToString("h:mm:ss tt");
            Matter += "\n";
            Matter += "Your Valuable suggestions are welcome to tellramesh2008@gmail.com";

            Clipboard.Clear();
            Clipboard.SetText(Matter);
            DialogResult result3 = MessageBox.Show("Data Copied Successfully to Clipboard",
            "Successful",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button2);

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            richTextBox1.Clear();
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Fan Relay R
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Fan Relay Y
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Fan Relay B
        }

        private void checkBox12_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Chiller Relay R
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Chiller Relay Y
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Chiller Relay B
        }

        private void checkBox15_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Pump Relay R
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void hardwareInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();
            frm.Show();
            return;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Globals.counter += 101;
            Form2 frm = new Form2();
            frm.Show();
            return;
            //Form2.ActiveForm.Show();
            //MessageBox.Show("Form2 Activated",
            //"Operation Triggering for Form2",
            //MessageBoxButtons.OK,
            //MessageBoxIcon.Information,
            //MessageBoxDefaultButton.Button2);

            //this.Close();
            //this.Hide();
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Pump Relay Y
        }

        private void checkBox13_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //Pump Relay B
        }

        private void checkBox18_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //SoV Relay R
        }

        private void checkBox17_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //SoV Relay Y
        }

        private void checkBox16_CheckedChanged(object sender, EventArgs e)
        {
            ckBoxForeColor();  //SoV Relay B
        }

        private void richTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void richTextToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = Clipboard.GetText();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            AHU_Relay_R = checkBox1.Checked;
            ckBoxForeColor();   //AHU Relay R
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            AHU_Relay_Y = checkBox2.Checked;
            ckBoxForeColor();   //AHU Relay Y
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            AHU_Relay_B = checkBox3.Checked;
            ckBoxForeColor();   //AHU Relay B
        }

        private void ckBoxForeColor()
        {
            //AHU
            if (checkBox1.Checked) { checkBox1.ForeColor = Color.Red; }
            else { checkBox1.ForeColor = Color.Green; }

            if (checkBox2.Checked) { checkBox2.ForeColor = Color.Red; }
            else { checkBox2.ForeColor = Color.Green; }

            if (checkBox3.Checked) { checkBox3.ForeColor = Color.Red; }
            else { checkBox3.ForeColor = Color.Green; }

           if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked)
           { label8.ForeColor = Color.Red; }
           else { label8.ForeColor = Color.Green; }

            //CONDENSER
            checkBox4.ForeColor = checkBox4.Checked ? Color.Red : Color.Green;
            checkBox5.ForeColor = checkBox5.Checked ? Color.Red : Color.Green;
            checkBox6.ForeColor = checkBox6.Checked ? Color.Red : Color.Green;

            if (checkBox4.Checked || checkBox5.Checked || checkBox6.Checked)
            { label9.ForeColor = Color.Red; }
            else { label9.ForeColor = Color.Green; }

            //FAN
            checkBox7.ForeColor = checkBox7.Checked ? Color.Red : Color.Green;
            checkBox8.ForeColor = checkBox8.Checked ? Color.Red : Color.Green;
            checkBox9.ForeColor = checkBox9.Checked ? Color.Red : Color.Green;

            if (checkBox7.Checked || checkBox8.Checked || checkBox9.Checked)
            { label10.ForeColor = Color.Red; }
            else { label10.ForeColor = Color.Green; }

            //Chiller
            checkBox10.ForeColor = checkBox10.Checked ? Color.Red : Color.Green;
            checkBox11.ForeColor = checkBox11.Checked ? Color.Red : Color.Green;
            checkBox12.ForeColor = checkBox12.Checked ? Color.Red : Color.Green;

            if (checkBox10.Checked || checkBox11.Checked || checkBox12.Checked)
            { label11.ForeColor = Color.Red; }
            else { label11.ForeColor = Color.Green; }

            //Pump
            checkBox13.ForeColor = checkBox13.Checked ? Color.Red : Color.Green;
            checkBox14.ForeColor = checkBox14.Checked ? Color.Red : Color.Green;
            checkBox15.ForeColor = checkBox15.Checked ? Color.Red : Color.Green;

            if (checkBox13.Checked || checkBox14.Checked || checkBox15.Checked)
            { label12.ForeColor = Color.Red; }
            else { label12.ForeColor = Color.Green; }

            //SoV
            checkBox16.ForeColor = checkBox16.Checked ? Color.Red : Color.Green;
            checkBox17.ForeColor = checkBox17.Checked ? Color.Red : Color.Green;
            checkBox18.ForeColor = checkBox18.Checked ? Color.Red : Color.Green;

            if (checkBox16.Checked || checkBox17.Checked || checkBox18.Checked)
            { label13.ForeColor = Color.Red; }
            else { label13.ForeColor = Color.Green; }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            COND_Relay_R = checkBox6.Checked;
            ckBoxForeColor();  //COND Relay R
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            COND_Relay_Y = checkBox5.Checked;
            ckBoxForeColor(); //COND Relay Y
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            COND_Relay_B = checkBox4.Checked;
            ckBoxForeColor(); //COND Relay B
        }

        


    }

    // static class to hold global variables, etc.
    static class Globals
    {
        // global int
        public static int counter;
        public static int nEN = 10, nRE = 10;
        //public static int nHVAC;
        public static int nHVAC=1, nAHU = 3, nCOND = 1, nFAN = 4, nZone = 5, nChiller = 1, nPump = 3, nSoV = 1, nWtTank = 1;

        // global function
        public static string HelloWorld()
        {
            return "Hello World";
        }
    }
}
