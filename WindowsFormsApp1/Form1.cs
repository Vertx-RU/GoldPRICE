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
            double min = 0;
            double max = 0;
            if (File.Exists("PriceList.txt"))
            {
                string[] tmp = File.ReadAllText("PriceList.txt").Split(new string[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (min == 0 && max == 0)
                    {
                        min = Convert.ToDouble(tmp[i]);
                        max = Convert.ToDouble(tmp[i]);
                    }
                    if (tmp[i] != "")
                    {
                        if (min > Convert.ToDouble(tmp[i]))
                        {
                            min = Convert.ToDouble(tmp[i]);
                        }
                        if (max < Convert.ToDouble(tmp[i]))
                        {
                            max = Convert.ToDouble(tmp[i]);
                        }
                        chart1.Series[0].Points.Add(Convert.ToDouble(tmp[i]));
                    }
                }
                if (min != 0 && max != 0)
                {
                    chart1.ChartAreas[0].AxisY.Minimum = min;
                    chart1.ChartAreas[0].AxisY.Maximum = max;
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
        double newmax = 0;
        double neemin = 0;
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
            if (neemin != 0)
            {
                if (Convert.ToDouble(element.TextContent) * 285 < neemin)
                {
                    neemin = Convert.ToDouble(element.TextContent) * 285;
                }
                if (Convert.ToDouble(element.TextContent) * 285 > newmax && neemin != 0 && Convert.ToDouble(element.TextContent) * 285 > neemin)
                {
                    newmax = Convert.ToDouble(element.TextContent) * 285;
                }
            }
            else
            {
                neemin = Convert.ToDouble(element.TextContent) * 285;
                nowprice = neemin;
            }
            if (neemin != 0 && newmax != 0)
            {
                chart1.ChartAreas[0].AxisY.Minimum = neemin;
                chart1.ChartAreas[0].AxisY.Maximum = newmax;
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
           

        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            this.Close();
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
    }
}
