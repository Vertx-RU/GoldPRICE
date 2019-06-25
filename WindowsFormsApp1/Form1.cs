using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using WebKit;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        WebKit.WebKitBrowser browser = new WebKitBrowser();
        private static bool IsDrag = false;
        private int enterX;
        private int enterY;
        bool first = true;
        bool concern = false;//默认关闭状态
        bool Market = false;//交易模式默认关闭
        double min = 0;
        double max = 0;
        bool LeftHide = false;
        bool RightHide = false;
        bool isfinish = true;
        Rectangle ScreenArea;
        [DllImport("User32.dll")]
        public static extern bool PtInRect(ref Rectangle Rects, Point lpPoint);
        private void Form1_Load(object sender, EventArgs e)
        {
             ScreenArea = System.Windows.Forms.Screen.GetWorkingArea(this);
            this.Location = new Point(0, 0);
            browser.Location = new Point(0,0);
            browser.Width = 0;
            browser.Height = 0;
            this.Controls.Add(browser);
 //      browser.Navigate("http://fund.eastmoney.com/002611.html?spm=aladin");

            browser.Url = new Uri("http://fund.eastmoney.com/002611.html?spm=aladin");
            browser.DocumentCompleted += Browser_DocumentCompleted;
            this.TopMost = true;
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.Series[0].MarkerStyle = MarkerStyle.Circle;
            chart1.Series[0].MarkerSize = 2;
            if (File.Exists("PriceList.txt"))
            {
                FileInfo fi = new FileInfo("PriceList.txt");
                DateTime LastWriteTime = fi.LastWriteTime;
                if (LastWriteTime.Date.Month < DateTime.Now.Month || LastWriteTime.Date.Day < DateTime.Now.Day)
                {
                    File.Create("PriceList.txt").Close();
                }
                string[] tmp = File.ReadAllText("PriceList.txt").Split(new string[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < tmp.Length; i++)
                {
                    try
                    {
                        if (min == 0 && max == 0)
                        {
                            min = Convert.ToDouble(tmp[i].Replace("\r\n",""));
                            max = Convert.ToDouble(tmp[i]);
                        }
                        if (tmp[i] != "")
                        {
                            if (min > Convert.ToDouble(tmp[i].Replace("\r\n", "")))
                            {
                                min = Convert.ToDouble(tmp[i].Replace("\r\n", ""));
                            }
                            if (max < Convert.ToDouble(tmp[i].Replace("\r\n", "")))
                            {
                                max = Convert.ToDouble(tmp[i].Replace("\r\n", ""));
                            }
                            chart1.Series[0].Points.Add(Convert.ToDouble(tmp[i].Replace("\r\n", "")));
                        }
                    }
                    catch
                    {
                        //start Initial configuration.....
                    }

                }
                if (min != 0 && max != 0)
                {
                    try
                    {
                        chart1.ChartAreas[0].AxisY.Minimum = min;
                        chart1.ChartAreas[0].AxisY.Maximum = max;
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                File.Create("PriceList.txt").Close();
            }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            label2.Visible = false;
            panel1.Visible = true;
            Getdata();
            timer1.Start();
            timer2.Start();
            
        }

        double nowprice = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Getdata();
                
            }
            catch
            {

            }
            browser.Reload();
        }
        private void Getdata()
        {
            WebKit.DOM.Element element;
            WebKit.DOM.Element element2;
            WebKit.DOM.Element element3;
            element = browser.Document.GetElementById("gz_gsz");
            element2 = browser.Document.GetElementById("gz_gszze");
            element3 = browser.Document.GetElementById("gz_gszzl");
            price.Text = Convert.ToDouble(element.TextContent) * 285 + " per/g";
            percent.Text = element2.TextContent;
            ppercent.Text = element3.TextContent;
            if (min != 0)
            {
                if (Convert.ToDouble(element.TextContent) * 285 < min)
                {
                    min = Convert.ToDouble(element.TextContent) * 285;
                }
                if (Convert.ToDouble(element.TextContent) * 285 > max && min != 0 && Convert.ToDouble(element.TextContent) * 285 > min)
                {
                    max = Convert.ToDouble(element.TextContent) * 285;
                }
            }
            else
            {
                min = Convert.ToDouble(element.TextContent) * 285;
                nowprice = min;
            }
            if (min != 0 && max != 0)
            {
                chart1.ChartAreas[0].AxisY.Minimum = min;
                chart1.ChartAreas[0].AxisY.Maximum = max;
            }
            if (first)
            {
                first = false;
                string tmp=  File.ReadAllText("PriceList.txt");
                File.WriteAllText("PriceList.txt", tmp+(Convert.ToDouble(element.TextContent) * 285).ToString() + "\r\n");
            }
            if (nowprice != Convert.ToDouble(element.TextContent) * 285)
            {
               chart1.Series[0].Points.Add(Convert.ToDouble(Convert.ToDouble(element.TextContent) * 285));
                string tmp = File.ReadAllText("PriceList.txt");
                File.WriteAllText("PriceList.txt", tmp + (Convert.ToDouble(element.TextContent) * 285).ToString() + "\r\n");
                nowprice = Convert.ToDouble(element.TextContent) * 285;
            }
            browser.Dispose();
            browser.Url = new Uri("http://fund.eastmoney.com/002611.html?spm=aladin");
            browser.DocumentCompleted += Browser_DocumentCompleted;
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoldPrice.Dispose();
            System.Environment.Exit(0);
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            Getdata();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            IsDrag = true;
            enterX = e.Location.X;
            enterY = e.Location.Y;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDrag&&checkBox1.Checked)
            {
                Left += e.Location.X - enterX;
                Top += e.Location.Y - enterY;
                this.TopMost = true;
            }

        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            IsDrag = false;
            enterX = 0;
            enterY = 0;
            this.BringToFront();
            this.TopMost = true;
           
            StartHide();
        }
        private void StartHide()
        {
            if (checkBox1.Checked == false)
            {
                if ((this.Location.X == 0 || this.Location.X < 5) && isfinish)
                {
                    LeftHide = true;
                    isfinish = false;
                    HideTimer.Start();
                }

                if ((this.Location.X + this.Width >= ScreenArea.Width || this.Location.X + this.Width > ScreenArea.Width - 5) && isfinish)
                {
                    RightHide = true;
                    isfinish = false;
                    HideTimer.Start();
                }
            }
        }
        
        private void concernModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!concern)
            {
                concern = true;ModelTimer.Enabled = true;
                concernModelToolStripMenuItem.Text = "Concern Model (√)";
                GoldPrice.BalloonTipTitle = "关注模式已启动";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipText = "仅每天下午的2点到4点显示报表";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipIcon = ToolTipIcon.Info;
                GoldPrice.Visible = true;
                GoldPrice.ShowBalloonTip(3000);
            }
            else
            {
                concern = false; ModelTimer.Enabled = false;
                concernModelToolStripMenuItem.Text = "Start Concern Model";
                this.Visible = true;
                GoldPrice.BalloonTipTitle = "关注模式已关闭";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipIcon = ToolTipIcon.Info;
                GoldPrice.Visible = true;
                GoldPrice.ShowBalloonTip(3000);
            }
        }

        private void ModelTimer_Tick(object sender, EventArgs e)
        {

            //关注模式
            //每天下午的2点到4点显示窗体，其余时间不显示
            #region
            if (concern)
            {
                DateTime dt1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 0, 0);
                DateTime dt2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 0, 0);
                TimeSpan ts1 = dt1.Subtract(DateTime.Now);
                TimeSpan ts2 = dt2.Subtract(DateTime.Now);
                if (ts1.TotalSeconds < 0 && ts2.TotalSeconds > 1)
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = false;
                }
            }
            #endregion
            //交易模式
            //每天的08:59至18:00期间收集数据
            #region
            if (Market)
            {
                //交易模式开始于结束时间
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 08, 59, 0);
                DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
                TimeSpan ts1 = start.Subtract(DateTime.Now);
                TimeSpan ts2 = end.Subtract(DateTime.Now);
                if (ts1.TotalSeconds < 0 && ts2.TotalSeconds > 1)
                {
                    timer1.Enabled = true;
                }
                else
                {
                    timer1.Enabled = false; timer2.Enabled = false;
                    DateTime dt = DateTime.Now;
                    //18:01 的时候清空txt数据
                    if (dt.ToString("HH:mm:ss").IndexOf("18:01") != -1)
                    {
                        FileStream stream = File.Open("PriceList.txt", FileMode.OpenOrCreate, FileAccess.Write);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                        stream.Close();
                    }
                }
            }
            #endregion
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
            chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
            //Application.DoEvents(); 使用此方法当有线程操作时会引发异常
            if (checkBox1.Checked == false)
            {
                if ((LeftHide || RightHide) && isfinish)
                {
                    isfinish = false;
                    ShowTimer.Start();
                }
            }
        }

        private void chart1_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                label3.Text = string.Format("{1:F3}", dp.XValue, dp.YValues[0]);
            }
        }

        private void timerShowHide_Tick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {

                System.Drawing.Point cursorPoint = new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);//获取鼠标在屏幕的坐标点
                Rectangle Rects = new Rectangle(this.Left, this.Top, this.Left + this.Width, this.Top + this.Height);//存储当前窗体在屏幕的所在区域
                bool prInRect = PtInRect(ref Rects, cursorPoint);
                if (prInRect)
                {//当鼠标在当前窗体内
                    if (this.Top < 0)//窗体的Top属性小于0
                        this.Top = 0;
                    else if (this.Left < 0)//窗体的Left属性小于0
                        this.Left = 0;
                    else if (this.Right > Screen.PrimaryScreen.WorkingArea.Width)//窗体的Right属性大于屏幕宽度
                        this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                }
            }
            else
            {
                if (this.Top < 5)               //当窗体的上边框与屏幕的顶端的距离小于5时
                this.Top = 5 - this.Height; //将窗体隐藏到屏幕的顶端
                else if (this.Left < 5)         //当窗体的左边框与屏幕的左端的距离小于5时
                this.Left = 5 - this.Width; //将窗体隐藏到屏幕的左端
                else if (this.Right > Screen.PrimaryScreen.WorkingArea.Width - 5)//当窗体的右边框与屏幕的右端的距离小于5时
                this.Left = Screen.PrimaryScreen.WorkingArea.Width - 5;//将窗体隐藏到屏幕的右端
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //释放托盘
            GoldPrice.Dispose();
        }

        private void startGeneralModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //交易模式
            if (!Market)
            {
                timer1.Enabled = true;
                Market = true; ModelTimer.Enabled = true;
                startGeneralModelToolStripMenuItem.Text = "Market Model (√)";
                GoldPrice.BalloonTipTitle = "交易模式已启动";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipText = "只接收每天9点到18点数据";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipIcon = ToolTipIcon.Info;
                GoldPrice.Visible = true;
                GoldPrice.ShowBalloonTip(3000);
            }
            else
            {
                timer1.Enabled = true;
                Market = false; ModelTimer.Enabled = false;
                startGeneralModelToolStripMenuItem.Text = "Start Market Model";
                GoldPrice.BalloonTipTitle = "交易模式已关闭";//设置系统托盘启动时显示的文本
                GoldPrice.BalloonTipIcon = ToolTipIcon.Info;
                GoldPrice.Visible = true;
                GoldPrice.ShowBalloonTip(3000);
            }
        }

        private void HideTimer_Tick(object sender, EventArgs e)
        {
            if (LeftHide)
            {
                if (this.Location.X <= -this.Width + 8)
                {
                    isfinish = true;
                    HideTimer.Stop();
                    this.Location = new Point(-this.Width + 5, this.Location.Y);
                }
                else
                {
                    this.Location = new Point(this.Location.X - 20, this.Location.Y);
                }
            }
            if (RightHide)
            {
               
                if (this.Location.X+8>= ScreenArea.Width)
                {
                    isfinish = true;
                    HideTimer.Stop();
                    this.Location = new Point(ScreenArea.Width - 5, this.Location.Y);
                }
                else
                {
                    this.Location = new Point(this.Location.X + 20, this.Location.Y);
                }
            }
        }

        private void ShowTimer_Tick(object sender, EventArgs e)
        {
            if (LeftHide)
            {
                if (this.Location.X >= 0)
                {
                    LeftHide = false;
                    isfinish = true;
                    ShowTimer.Stop();
                    this.Location = new Point(0, this.Location.Y);
                }
                else
                {
                    this.Location = new Point(this.Location.X + 20, this.Location.Y);
                }
            }
            if (RightHide)
            {
                if (this.Location.X <= ScreenArea.Width-this.Width)
                {
                    RightHide = false;
                    isfinish = true;
                    ShowTimer.Stop();
                    this.Location = new Point(ScreenArea.Width - this.Width, this.Location.Y);
                }
                else
                {
                    this.Location = new Point(this.Location.X - 20, this.Location.Y);
                }
            }
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            StartHide();
        }

        private void chart1_MouseLeave(object sender, EventArgs e)
        {
           // StartHide();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                StartHide();
                timerShowHide.Stop();
            }
            else
            {
                timerShowHide.Start();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GoldPrice.Dispose();
        }
    }
}
