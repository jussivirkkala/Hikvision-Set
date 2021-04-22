using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
 * 2021-04-21 Visual Studio 16.9.4
 * 2021-03-27 v1.2.2 Trim line. Compiled with Visual Studio 16.9.2.
 * 2021-03-14 v1.2.1 Option to set title. Compiled with Visual Studio 16.9.1.
 * 2021-03-01 v1.2.0 Option to set location. Start with Auto selected. Compiled with Visual Studio 16.8.6. No ClicOnce sign.
 * 2020-12-18 1.1.4 Testing NumLock indicator.
 * 2020-11-29 1.1.3 Writing separate log each computer. Using computer specific settings  
 *  if exist.
 * 2020-11-19 1.1.2 Log with fewer rows.
 * 2020-11-18 1.1.1 Log version, IP and port.
 * 2020-11-16 1.1.0 Removed ..\bin from CHCNetSDK, reading .ini, writing .log.
 * 2020-11-12 1.0.3 Application already running.
 * 2020-11-10 1.0.2 Opacity.
 * 2020-11-10 1.0.1 Git.
 * 2020-11-08 1.0.0 .NET 4.5, allow unsafe code, platform x64.
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
            appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Log("Version: " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
            InitializeComponent();
             m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                label1.Text = "SDK error";
                Log("SDK error");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        // 2020-12-18 Testing NumLock functions
        void Application_Idle(object sender, EventArgs e)
        {
            // label1.Text = Control.IsKeyLocked(Keys.CapsLock).ToString();
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.Idle -= Application_Idle;
            base.OnFormClosed(e);
        }

        string appName;
        private void Form1_Load(object sender, EventArgs e)
        {
            // Always on top
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            this.Text = appName;
            this.Opacity = .95;
            // On closing event
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);
            Application.Idle += Application_Idle;

            // Load file
            if (!File.Exists(appName + ".ini"))
            {
                MessageBox.Show("Missing "+appName+".ini", appName);
            }
            else
            {
                int row = 0;
                // 2020-12-01 Look for -computername.ini
                string s=appName+".ini";
                if (File.Exists(appName + "-" + Environment.MachineName + ".ini"))
                    s = appName + "-"+Environment.MachineName + ".ini";

                int x=0, y=0; 
                foreach (string line1 in File.ReadLines(s))
                    {
                    String line=line1.Trim(); // 2021-03-27
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
                                // 2020-02-28 Default location
                                x = Int16.Parse(line);
                                break;
                            case 10:
                                y = Int16.Parse(line);
                                if ((x>0) && (y>0))
                                { 
                                    Point p = new Point(x,y);
                                    this.Location = p;
                                }
                                break;
                            case 11:
                                this.Text = line;
                                break;
                        }
                    }

                }
                // 2020-02-28
                radioButton3.Checked = true;
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
                label1.Text += "Cam1: ";
                Preset(Camera1, Code);
            }
            if (Camera2.DVRPortNumber > 0)
            {
                label1.Text += "Cam2: ";
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
        // 2020-11-29 Machinename in log file.
        void Log(string s)
        {
            try {
                using (StreamWriter sw = File.AppendText(appName + "-"+Environment.MachineName + ".log"))
                    sw.WriteLine(DateTime.Now.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fff") + DateTime.Now.ToString("zzz") + "\t" + Environment.MachineName + "\t" + Environment.UserName + "\t"  + s);
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
                Log(Camera.DVRIPAddress+ "\tlogin err");
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_PTZPreset_Other(m_lUserID, 1, CHCNetSDK.GOTO_PRESET, (UInt32)(PreSetNo)))
                {
                    label1.Text += "set err "; 
                    Log(Camera.DVRIPAddress + "\tset err");
                }
                else
                {
                    label1.Text += "ok ";
                    Log(Camera.DVRIPAddress + "\t" + PreSetNo.ToString());
                }
                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    label1.Text += "logout err ";
                    Log(Camera.DVRIPAddress+ "\tlogout err");
                    m_lUserID = -1;
                }
            }
        }

    }
}

// End