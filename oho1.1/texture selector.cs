using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace oho1._1
{
    public partial class texture_selector : Form
    {
        public Image ResizeImg(Image b, int nWidth, int nHeight)
        {
            Image result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                g.Dispose();
            }
            return result;
        }

        private List<imageContainer> images; 
        public int selectedIndex;
        public texture_selector(List<imageContainer>source )
        {
            selectedIndex = 0;
            InitializeComponent();
            if (source == null)
                return;
            if (source.Count == 0)
                return;
            images = source;
            int counter = 0;
            foreach (var imageContainer in source)
            {
                if (counter==1)
                    dataGridView1.Rows.Add(true, ResizeImg(imageContainer.image, 50, 50), imageContainer.filename);

                else

                dataGridView1.Rows.Add(false, ResizeImg(imageContainer.image,50,50), imageContainer.filename);
                dataGridView1.Rows[dataGridView1.Rows.Count-1].Height = 50;
            }
            dataGridView1.Invalidate();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedIndex = e.RowIndex;
            dataGridView1.Rows.Clear();
            int counter = 0;
            foreach (var imageContainer in images)
            {
                if (counter++ == selectedIndex) 
                    dataGridView1.Rows.Add(true, ResizeImg(imageContainer.image, 50, 50), imageContainer.filename);
                else
                dataGridView1.Rows.Add(false, ResizeImg(imageContainer.image, 50, 50), imageContainer.filename);
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Height = 50;
            }
            dataGridView1.Invalidate();
            
        }
    }
}
