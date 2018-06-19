using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;//必须引用此项//警告：实验此代码可能给你的显示器的显示带来问题 ，花了两个小时查API的用法
using System.IO;

namespace OSD_TEST
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        bool leftFlag;
        Point mouseOff;
        string low, min, max;
        List<string> powerid = new List<string>();
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);
        RAMP ramp = new RAMP();[DllImport("gdi32.dll")]
        public static extern int SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);[DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Red;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Green;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public UInt16[] Blue;
    }
    void SetGamma(int gamma)
    {
            ramp.Red = new ushort[256];
            ramp.Green = new ushort[256];
            ramp.Blue = new ushort[256];
            for (int i = 1; i< 256; i++)
            {
                // gamma 必须在3和44之间
                ramp.Red[i] = (ushort)(Math.Min(65535, Math.Max(0, Math.Pow((i + 1) / 256.0, gamma * 0.1) * 65535 + 0.5)));
                ramp.Green[i] = (ushort)(Math.Min(65535, Math.Max(0, Math.Pow((i + 1) / 256.0, gamma * 0.1) * 65535 + 0.5)));
                ramp.Blue[i] = (ushort)(Math.Min(65535, Math.Max(0, Math.Pow((i + 1) / 256.0, gamma* 0.1) * 65535 + 0.5)));
            }
            SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            this.ShowInTaskbar = false;
            #region//获取初始信息
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "c:\\windows\\system32\\cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            p.StandardInput.WriteLine("powercfg -l" + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();


            p.WaitForExit();//等待程序执行完退出进程
            p.Close();

            string filePath = "D:\\info.text";
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(output);
            }

            #endregion
            using (StreamReader sr = new StreamReader("D:\\info.text"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IndexOf("电源方案") != -1)
                    {
                        powerid.Add(line);
                    }
                }
            }

            for (int i = 0; i < powerid.Count; i++)
            {
                if (powerid[i].IndexOf("节能") != -1)
                {
                    if (powerid[i].IndexOf("*") != -1)
                    {
                        radioButton1.Checked = true;
                    }
                    low = powerid[i].Trim();
                    low = low.Remove(0, low.IndexOf(":"));
                    low = low.Remove(low.IndexOf("("), low.Length - low.IndexOf("("));
                    low = low.Replace(":", "");
                    low = low.Trim();

                }
                if (powerid[i].IndexOf("平衡") != -1)
                {
                    if (powerid[i].IndexOf("*") != -1)
                    {
                        radioButton2.Checked = true;
                    }
                    min = powerid[i].Trim();
                    min = min.Remove(0, min.IndexOf(":"));
                    min = min.Remove(min.IndexOf("("), min.Length - min.IndexOf("("));
                    min = min.Replace(":", "");
                    min = min.Trim();
                }
                if (powerid[i].IndexOf("高性能") != -1)
                {
                    if (powerid[i].IndexOf("*") != -1)
                    {
                        radioButton3.Checked = true;
                    }
                    max = powerid[i].Trim();
                    max = max.Remove(0, max.IndexOf(":"));
                    max = max.Remove(max.IndexOf("("), max.Length - max.IndexOf("("));
                    max = max.Replace(":", "");
                    max = max.Trim();
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            timer1.Start();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
            timer1.Start();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
            timer1.Start();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
            this.Show();
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.05;
            if (this.Opacity <= 0.05)
            {
                timer1.Stop();
                this.Hide();
            }
        }
        private void radioButton1_Click(object sender, EventArgs e)
        {
            timer1.Start();
            SetModel(low);
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            timer1.Start();
            SetModel(min);
        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            timer1.Start();
            SetModel(max);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Close();
            Application.Exit();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            SetGamma(trackBar1.Value);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
        public void SetModel(string id)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "c:\\windows\\system32\\cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("powercfg /setactive" + " "+id + "&exit"); //设置模式

              p.WaitForExit();//等待程序执行完退出进程
              p.Close();
        }
    }
}
