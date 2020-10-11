using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BSYAIInspection
{
    public partial class BSYForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {

        public BSYForm()
        {
            InitializeComponent(); // 初始化控件
            initPicturesLayout(4);
        }

        private void BSYForm_Load(object sender, EventArgs e)
        {
 
        }

        TableLayoutPanel tableLayoutPanel;
        PictureBox[] pictureBoxes;
        private void initPicturesLayout(int cameraNumber)
        {
            tableLayoutPanel = new TableLayoutPanel();

            pictureBoxes = new PictureBox[cameraNumber];
            for (int i = 0; i < cameraNumber; i++)
            {
                pictureBoxes[i] = new PictureBox();
            }

            ((ISupportInitialize)(ribbonControl1)).BeginInit();
            tableLayoutPanel.SuspendLayout();

            SuspendLayout();

            // 
            // tableLayoutPanel1
            // 
            if (1 == cameraNumber)
            {
                // tableLayoutPanel1
                // 
                tableLayoutPanel.ColumnCount = 1;
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Location = new System.Drawing.Point(225, 194);
                tableLayoutPanel.Name = "tableLayoutPanel1";
                tableLayoutPanel.RowCount = 1;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Size = new System.Drawing.Size(295, 195);
                tableLayoutPanel.TabIndex = 1;
            }
            else if (2 == cameraNumber)
            {
                tableLayoutPanel.ColumnCount = 2;
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Controls.Add(pictureBoxes[0], 0, 0);
                tableLayoutPanel.Controls.Add(pictureBoxes[1], 1, 0);
                tableLayoutPanel.Controls.Add(pictureBoxes[2], 0, 1);
                tableLayoutPanel.Controls.Add(pictureBoxes[3], 1, 1);
                tableLayoutPanel.Dock = DockStyle.Fill;
                tableLayoutPanel.Location = new Point(0, 147);
                tableLayoutPanel.Name = "tableLayoutPanel1";
                tableLayoutPanel.RowCount = 1;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Size = new Size(295, 195);
                tableLayoutPanel.TabIndex = 1;
            }
            else
            {
                tableLayoutPanel.ColumnCount = 2;
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Controls.Add(this.pictureBoxes[0], 0, 0);
                tableLayoutPanel.Controls.Add(this.pictureBoxes[1], 1, 0);
                tableLayoutPanel.Controls.Add(this.pictureBoxes[2], 0, 1);
                tableLayoutPanel.Controls.Add(this.pictureBoxes[3], 1, 1);
                tableLayoutPanel.Dock = DockStyle.Fill;
                tableLayoutPanel.Location = new Point(0, 147);
                tableLayoutPanel.Name = "tableLayoutPanel1";
                tableLayoutPanel.RowCount = 2;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                tableLayoutPanel.Size = new Size(983, 483);
                tableLayoutPanel.TabIndex = 3;
            }
            // 
            // pictureBox1
            // 
            for (int i = 0; i < cameraNumber; i++)
            {
                pictureBoxes[i].BackColor = Color.Blue;
                pictureBoxes[i].Dock = DockStyle.Fill;
                //this.pictureBoxes[i].Location = new System.Drawing.Point(3, 3);
                pictureBoxes[i].Name = "pictureBox" + (i + 1).ToString();
                pictureBoxes[i].Size = new Size(485, 235);
                pictureBoxes[i].TabIndex = 0;
                pictureBoxes[i].TabStop = false;
            }
            //// 
            //// BSYForm
            //// 
            Controls.Add(tableLayoutPanel);
            

            ((ISupportInitialize)(ribbonControl1)).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            for (int i = 0; i < cameraNumber; i++)
            {
                //((System.ComponentModel.ISupportInitialize)(this.pictureBoxes[i])).EndInit();
            }


            ResumeLayout(false);
            PerformLayout();

        }
        TabControl tabControl;
        private void initPicturesTab(int cameraNumber)
        {
            tabControl = new TabControl();
        }

        private void BSYForm_MaximumSizeChanged(object sender, EventArgs e)
        {
            
        }


    }
}
