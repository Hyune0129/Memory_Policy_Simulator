using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Memory_Policy_Simulator
{
    public partial class Form1 : Form
    {
        Graphics g;
        PictureBox pbPlaceHolder;
        Bitmap bResultImage;

        public Form1()
        {
            InitializeComponent();
            this.pbPlaceHolder = new PictureBox();
            this.bResultImage = new Bitmap(2048, 2048);
            this.pbPlaceHolder.Size = new Size(2048, 2048);
            g = Graphics.FromImage(this.bResultImage);
            pbPlaceHolder.Image = this.bResultImage;
            this.pImage.Controls.Add(this.pbPlaceHolder);
        }

        private void DrawBase(Core core, int windowSize, int dataLength)
        {
            /* parse window */
            string policy = comboBox1.GetItemText(comboBox1.SelectedItem);
            var psudoList = new List<char>();
            List<int> value = new List<int>(); //age, count를 위한 List
            int max = -1, min = int.MaxValue, temp,index=0;
            g.Clear(Color.Black);

            for ( int i = 0; i < dataLength; i++ ) // length
            {
                int psudoCursor = core.pageHistory[i].loc;
                char data = core.pageHistory[i].data;
                Page.STATUS status = core.pageHistory[i].status;
                if (status == Page.STATUS.PAGEFAULT)
                {
                    psudoList.Add(data);
                    value.Add(0);
                }
                else if (status == Page.STATUS.MIGRATION)
                {
                    switch (policy)
                    {
                        case "LRU":
                            for (int j = 0; j < value.Count; j++)
                            {
                                temp = value[j];
                                if (temp > max)
                                {
                                    index = j;
                                    max = temp;
                                }
                            }
                            value.RemoveAt(index);
                            psudoList.RemoveAt(index);
                            psudoList.Add(data);
                            break;
                        case "LFU":
                            for (int j = 0; j < value.Count; j++)
                            {
                                temp = value[j];
                                if (temp > max)
                                {
                                    index = j;
                                    max = temp;
                                }
                            }
                            value.RemoveAt(index);
                            psudoList.RemoveAt(index);
                            psudoList.Add(data);
                            break;
                        case "MFU":
                            for (int j = 0; j < value.Count; j++)
                            {
                                temp = value[j];
                                if (temp < min)
                                {
                                    index = j;
                                    min = temp;
                                }
                            }
                            value.RemoveAt(index);
                            psudoList.RemoveAt(index);
                            psudoList.Add(data);
                            break;
                        default:    //FIFO
                            psudoList.RemoveAt(0);
                            psudoList.Add(data);
                            break;
                    }
                }

                else if (status == Page.STATUS.HIT && (policy == "LFU" || policy == "MFU"))

                {

                    for (int j = 0; j < psudoList.Count; j++)

                    {

                        if (data == psudoList.ElementAt(j))

                        {

                            value[j]++;

                            break;

                        }

                    }

                }

                for ( int j = 0; j <= windowSize; j++) // height - STEP
                {
                    if (j == 0)
                    {
                        DrawGridText(i, j, data);
                    }
                    else
                    {
                        DrawGrid(i, j);
                    }
                }

                DrawGridHighlight(i, psudoCursor, status);
                int depth = 1;

                foreach ( char t in psudoList)
                {
                    DrawGridText(i, depth++, t);
                }
                switch(policy)
                {
                    case "LRU":
                        for (int j = 0; j < value.Count; j++)
                            value[j]++;
                        break;
                    case "LFU":

                        break;
                    case "MFU":
                        break;
                }
            }
        }


        private void DrawGrid(int x, int y)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            g.DrawRectangle(new Pen(Color.White), new Rectangle(
                gridBaseX + (x * gridSpace),
                gridBaseY,
                gridSize,
                gridSize
                ));
        }

        private void DrawGridHighlight(int x, int y, Page.STATUS status)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            SolidBrush highlighter = new SolidBrush(Color.LimeGreen);

            switch (status)
            {
                case Page.STATUS.HIT:
                    break;
                case Page.STATUS.MIGRATION:
                    highlighter.Color = Color.Purple;
                    break;
                case Page.STATUS.PAGEFAULT:
                    highlighter.Color = Color.Red;
                    break;
            }

            g.FillRectangle(highlighter, new Rectangle(
                gridBaseX + (x * gridSpace),
                gridBaseY,
                gridSize,
                gridSize
                ));
        }

        private void DrawGridText(int x, int y, char value)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            g.DrawString(
                value.ToString(), 
                new Font(FontFamily.GenericMonospace, 8), 
                new SolidBrush(Color.White), 
                new PointF(
                    gridBaseX + (x * gridSpace) + gridSize / 3,
                    gridBaseY + gridSize / 4));
        }

        private void btnOperate_Click(object sender, EventArgs e)
        {
            this.tbConsole.Clear();

            if (this.tbQueryString.Text != "" || this.tbWindowSize.Text != "")
            {
                string data = this.tbQueryString.Text;              //Reference String
                int windowSize = int.Parse(this.tbWindowSize.Text); //#Frame
                string select = comboBox1.GetItemText(comboBox1.SelectedItem);  //선택한 Policy
                /* initalize */
                var window = new Core(windowSize, select);

                foreach ( char element in data )
                {
                    var status = window.Operate(element);
                    this.tbConsole.Text += "DATA " + element + " is " + 
                        ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : "Hit")
                        + "\r\n";
                }

                DrawBase(window, windowSize, data.Length);
                this.pbPlaceHolder.Refresh();

                /* 차트 생성 */
                chart1.Series.Clear();
                Series resultChartContent = chart1.Series.Add("Statics");
                resultChartContent.ChartType = SeriesChartType.Pie;
                resultChartContent.IsVisibleInLegend = true;
                resultChartContent.Points.AddXY("Hit", window.hit);
                resultChartContent.Points.AddXY("Page Fault", window.fault-window.migration);
                resultChartContent.Points.AddXY("Migrated", window.migration);
                resultChartContent.Points[0].IsValueShownAsLabel = true;
                resultChartContent.Points[1].IsValueShownAsLabel = true;
                resultChartContent.Points[2].IsValueShownAsLabel = true;

                this.lbPageFaultRatio.Text = Math.Round(((float)window.fault / (window.fault + window.hit)), 2) * 100 + "%";
            }
            else
            {
            }

        }

        private void pbPlaceHolder_Paint(object sender, PaintEventArgs e)
        {
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void tbWindowSize_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tbWindowSize_KeyPress(object sender, KeyPressEventArgs e)
        {
                if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != 8)
                {
                    e.Handled = true;
                }
        }

        private void btnRand_Click(object sender, EventArgs e)
        {
            Random rd = new Random();

            int count = rd.Next(5, 50);
            StringBuilder sb = new StringBuilder();


            for ( int i = 0; i < count; i++ )
            {
                sb.Append((char)rd.Next(65, 90));
            }

            this.tbQueryString.Text = sb.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bResultImage.Save("./result.jpg");
        }
    }
}
