using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/**
 * @jussivirkkala
 * https://www.hikvision.com/en/support/download/sdk/device-network-sdk--for-windows-64-bit-/
 * V6.1.6.3_build20200925
 * 
 * 2020-11-16 1.1.0 Removed ..\bin from CHCNetSDK, reading .ini, writing .log
 * 2020-11-12 1.0.3 Application already running
 * 2020-11-10 1.0.2 opacity
 * 2020-11-10 1.0.1 git
 * 2020-11-08 1.0.0 .NET 4.5, allow unsafe code, Platform x64
 */

namespace HIK_Set
{
    public partial class Form1 : Form
    {
        // On top
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        struct Camera
        {
            public string DVRIPAddress;
            public Int16 DVRPortNumber;
            public string DVRUserName;
            public string DVRPassword;
        }
        Camera Camera1;
        Camera Camera2;

        // Hikvision
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;

        public Form1()
        {
            InitializeComponent();
             m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                label1.Text = "SDK error";
            }
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        string appName;
        private void Form1_Load(object sender, EventArgs e)
        {
            // Always on top
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            this.Text = appName;
            this.Opacity = .95;
            Log("Opening");

            // On closing event
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);

            // Load file
            if (!File.Exists(appName + ".ini"))
            {
                MessageBox.Show("Missing "+appName+".ini", appName);
            }
            else
            {
                int row = 0;
                foreach (string line in File.ReadLines(this.Text + ".ini"))
                {
                    if (!line.StartsWith("#"))
                    {
                        row += 1;
                        switch (row)
                        {
                            case 1:
                                Camera1.DVRIPAddress = line;
                                break;
                            case 2:
                                Camera1.DVRPortNumber = Int16.Parse(line);
                                break;
                            case 3:
                                Camera1.DVRUserName= line;
                                break;
                            case 4:
                                Camera1.DVRPassword= line;
                                break;
                            case 5:
                                Camera2.DVRIPAddress = line;
                                break;
                            case 6:
                                Camera2.DVRPortNumber = Int16.Parse(line);
                                break;
                            case 7:
                                Camera2.DVRUserName = line;
                                break;
                            case 8:
                                Camera2.DVRPassword = line;
                                break;
                            case 9:
                                Point p = new Point(500, 500);
                                // this.Location = p;
                                break;
                        }
                    }
                }
            }
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            radioButton3.Checked = true;
            Log("Closing");
        }

        private void SendPreset(UInt32 Code)
        {
            label1.Text = "";
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            if (Camera1.DVRPortNumber > 0)
            {
                Log("Camera1");
                label1.Text += "Cam1: ";
                Log(Code.ToString());
                Preset(Camera1, Code);
            }
            if (Camera2.DVRPortNumber > 0)
            {
                Log("Camera2");
                label1.Text += "Cam2: ";
                Log(Code.ToString());
                Preset(Camera2, Code);
            }
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
        }

        // Day
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
                SendPreset(39);
        }
        // Night
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
                SendPreset(40);
        }
        // Auto
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
                SendPreset(46);
        }

        // Logging to file
        void Log(string s)
        {
            try {
                using (StreamWriter sw = File.AppendText(appName + ".log"))
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fff") + DateTime.Now.ToString("zzz") + "\t" + Environment.MachineName + "\t" + Environment.UserName + "\t" + "\t" + s);
            }
            catch
            { }

        }

        // Sending preset to camera
        private void Preset(Camera Camera, UInt32 PreSetNo)
        { 
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(Camera.DVRIPAddress, Camera.DVRPortNumber, Camera.DVRUserName, Camera.DVRPassword, ref DeviceInfo);
            if (m_lUserID < 0)
            {
                label1.Text += "login err ";
                Log("login err");
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_PTZPreset_Other(m_lUserID, 1, CHCNetSDK.GOTO_PRESET, (UInt32)(PreSetNo)))
                {
                    label1.Text += "set err "; 
                    Log("set err");
                }
                else
                    label1.Text += "ok";

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    label1.Text += "logout err ";
                    Log("logout err");
                    m_lUserID = -1;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}

// End