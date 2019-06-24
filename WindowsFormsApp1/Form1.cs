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

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        WebKit.WebKitBrowser browser = new WebKitBrowser();
        WebKit.DOM.Element element;
        WebKit.DOM.Element element2;
        WebKit.DOM.Element element3;
        WebKit.DOM.Element element4;
        WebKit.DOM.Element element5;
        WebKit.DOM.Element element6;
        WebKit.DOM.Element element7;
        private static bool IsDrag = false;
        private int enterX;
        private int enterY;
        bool first = true;
        bool concern = false;//默认关闭状态
        System.Windows.Forms.NotifyIcon notifyIcon = null;
        double min = 0;
        double max = 0;
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);
            browser.Location = new Point(0,0);
            browser.Width = 0;
            browser.Height = 0;
            this.Controls.Add(browser);
            browser.Navigate("http://fund.eastmoney.com/002611.html?spm=aladin");
            browser.DocumentCompleted += Browser_DocumentCompleted;
            this.TopMost = true;
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.Series[0].MarkerStyle = MarkerStyle.Circle;
            chart1.Series[0].MarkerSize = 2;
            if (File.Exists("PriceList.txt"))
            {
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
            Getdata();
            browser.Reload();
        }
        private void Getdata()
        {
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
//test
            if (first)
            {
                first = false;
                string tmp=  File.ReadAllText("PriceList.txt");
                File.WriteAllText("PriceList.txt", tmp+(Convert.ToDouble(element.TextContent) * 285).ToString() + "\r\n");
            }
            if (nowprice != Convert.ToDouble(element.TextContent) * 285)
            {
                System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint = new System.Windows.Forms.DataVisualization.Charting.DataPoint();
               chart1.Series[0].Points.Add(Convert.ToDouble(Convert.ToDouble(element.TextContent) * 285));
                string tmp = File.ReadAllText("PriceList.txt");
                File.WriteAllText("PriceList.txt", tmp + (Convert.ToDouble(element.TextContent) * 285).ToString() + "\r\n");
                nowprice = Convert.ToDouble(element.TextContent) * 285;
            }
           

        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            if(concern)
            {
                DateTime dt = DateTime.Now;
                if (dt.ToString("HH:mm").IndexOf("14:") != -1|| dt.ToString("HH:mm").IndexOf("15:") != -1||dt.ToString("HH:mm").IndexOf("16:") != -1)
                {
                    this.Visible = true;
                }
                else
                {
                    this.Visible = false;
                }
            }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
            chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
            //Application.DoEvents(); 使用此方法当有线程操作时会引发异常
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
    }
}
