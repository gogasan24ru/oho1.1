using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace oho1._1
{
    public partial class Form1 : Form
    {

        private List<obj> objects = new List<obj>();
        private List<Image> img = new List<Image>();
        private List<imageContainer> imgs = new List<imageContainer>();

        private Graphics gr;
        private int posX = 0, posY = 0;
        private double scale = 1;
        private texture_selector t;
        //private List<field> Field = new List<field>(); 
        
        private Random r;
        public Form1()
        {
            InitializeComponent();
            trackBar1.Value = 1;
            
            pictureBox1.Width = Width - pictureBox1.Left;
            pictureBox1.Height = Height - pictureBox1.Top;
            r = new Random();
            pictureBox1.Image = new Bitmap(Width - pictureBox1.Left, Height - pictureBox1.Top);
            gr = Graphics.FromImage(pictureBox1.Image);
            gr.Clear(Color.White);

            string[] files =
                Directory.GetFiles("./", "*.png");

            foreach (string file in files)
            {
                Image temp = Image.FromFile(file);
//                img.Add(Image.FromFile(file));
                imgs.Add (new imageContainer(file.Split('/')[file.Split('/').Length-1],temp));
            }

            t = new texture_selector(imgs);
            t.Show();
            pictureBox1.Focus();
        }

        private int G = 500;
        private double gravity(obj a, obj b)
        {
            G = 10000;
            //double G = 6.67*(0.00000000001); //6,67*10^(-11)
            double x = a.pos.X - b.pos.X;
            double y = a.pos.Y - b.pos.Y;
            double distance = Math.Sqrt(x*x + y*y);
            //double modifier = 1.5;
            
            return G * a.mass * b.mass / (distance * distance);

        }

        private void drawObjects()
        {


            if(!pictureBox1.Focused)pictureBox1.Focus();
            gr.Clear(Color.White);
            Pen pen = new Pen(Color.Black, 1);
            if (checkBox5.Checked)
            {
                int step = trackBar2.Value;
                for (int i = 0; i < pictureBox1.Width; i += step)
                {
                    gr.DrawLine(pen, i, 0, i, pictureBox1.Height);
                }
                for (int k = 0; k < pictureBox1.Height; k += step)
                {
                    gr.DrawLine(pen, 0, k, pictureBox1.Width, k);
                }

            }
            gr.DrawString("SCALE="+(int)(scale*100)+"%", new Font(Font,new FontStyle()), new HatchBrush(HatchStyle.Shingle, Color.Black),0,0);
            if (checkBox3.Checked)
            {
                int step = trackBar2.Value;
                for (int i = 0; i<pictureBox1.Height;i+=step)
                    for (int k = 0; k < pictureBox1.Width; k+=step)
                    {
                        field temp = null;

                        obj a = new obj(new vector(k, i), new vector(0, 0), 1, true); 
                        foreach (var o in objects)
                        {
                            double currForce = gravity(a, o);
                            vector gravVector = new vector(a.pos.X - o.pos.X, a.pos.Y - o.pos.Y);
                            double angleX = gravVector.X/gravVector.lenght();
                            double angleY = gravVector.Y/gravVector.lenght();
                            a.force.X += currForce*angleX;
                            a.force.Y += currForce*angleY;


                            temp= new field(a.pos, a.force);
                            //Field.Add(temp);

                        }
                        pen = new Pen(Color.Green, 1);
                        if (temp != null)
                            gr.DrawLine(pen, (int)temp.B.X, (int)temp.B.Y,
                                        (int)temp.E.X + (int)temp.B.X, (int)temp.E.Y + (int)temp.B.Y);
                    }

                //foreach (var field in Field)
                //{
                //}
            }
            foreach (var o in objects)
            {
                try
                {
                    //gr.DrawEllipse(new Pen(Color.Black, 3), (int)o.pos.X - o.mass / 2,
                    //    (int)o.pos.Y - o.mass / 2, o.mass, o.mass);

                    if (!checkBox4.Checked||(o.texture==null))
                        gr.DrawEllipse(new Pen(Color.Black, 3), (int) o.pos.X - o.mass/2,
                                       (int) o.pos.Y - o.mass/2, o.mass, o.mass);
                    else
                    {
                        gr.DrawImage(o.texture,(int)o.pos.X - o.mass/2,
                                   (int)o.pos.Y - o.mass/2);
                    }
                    if(checkBox2.Checked)
                    {
                        gr.DrawLine(new Pen(Color.Red, 2), (int) o.pos.X, (int) o.pos.Y,
                                    (int) (o.pos.X + o.speed.X), (int) (o.pos.Y + o.speed.Y));

                        gr.DrawLine(new Pen(Color.Blue, 2), (int) o.pos.X, (int) o.pos.Y,
                                    (int) (o.pos.X + o.force.X), (int) (o.pos.Y + o.force.Y));
                    }
                    if (checkBox1.Checked&&o.avalible)
                        gr.DrawString(o.ToString(), Font, new HatchBrush(HatchStyle.Shingle, Color.Black),
                                      (float)o.pos.X + o.mass / 2,
                                      (float)o.pos.Y - o.mass / 2);
                }
                catch (Exception)
                {


                }
            }
            pictureBox1.Invalidate();
            pictureBox1.Update();
            
        }

        private void processObjects()
        {
            objects.RemoveAll(a => !a.avalible);
            if (objects.Count   == 0)
            {
                button3_Click(new object(),new EventArgs());
            }
            Text = objects.Count.ToString();
            foreach (var o in objects)
            {
                vector force = new vector(0, 0);
                var another = objects.FindAll(a => a != o);
                foreach (var a in another)
                {
                    double currForce = gravity(a, o);
                    vector gravVector = new vector(a.pos.X - o.pos.X, a.pos.Y - o.pos.Y);
                    double angleX = gravVector.X/gravVector.lenght();
                    double angleY = gravVector.Y/gravVector.lenght();
                    force.X += currForce*angleX;
                    force.Y += currForce*angleY;

                    double x = a.pos.X - o.pos.X;
                    double y = a.pos.Y - o.pos.Y;
                    double distance = Math.Sqrt(x*x + y*y);
                    //double modifier = 1.5;
                    if ((distance <= (a.mass + o.mass)/2))
                    {
                        if (a.mass > o.mass)
                        {
                            o.avalible = false;
                            a.mass += o.mass/2;
                            if(a.textureIndex!=-1)
                            a.texture = ResizeImg(imgs[a.textureIndex].image, a.mass, a.mass);
                            if (!a.fix)
                            {

                                a.speed.X = (o.mass*o.speed.X + a.mass*a.speed.X)/(a.mass + o.mass);
                                a.speed.Y = (o.mass*o.speed.Y + a.mass*a.speed.Y)/(a.mass + o.mass);

                            }
                        }
                        else
                        {
                            a.avalible = false;
                            o.mass += a.mass / 2;
                            if (o.textureIndex != -1)
                            o.texture = ResizeImg(imgs[o.textureIndex].image, o.mass, o.mass);
                            if (!o.fix)
                            {
                                o.speed.X = (o.mass*o.speed.X + a.mass*a.speed.X)/(a.mass + o.mass);
                                o.speed.Y = (o.mass * o.speed.Y + a.mass * a.speed.Y) / (a.mass + o.mass);
                            }
                        }
                    }
                }
                double aX = force.X/o.mass;
                double aY = force.Y/o.mass;
                if (!o.fix)
                {
                    o.speed.X += aX * trackBar1.Value / 100;
                o.speed.Y += aY*trackBar1.Value/100;
                o.pos.X += o.speed.X*trackBar1.Value/100;
                o.pos.Y += o.speed.Y*trackBar1.Value/100;
 
                }

                o.force.X = force.X / 10;
                o.force.Y = force.Y / 10;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            processObjects();
            drawObjects();
        }

     

        private void checkBox1_SizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Width = Width - pictureBox1.Left;
            pictureBox1.Height = Height - pictureBox1.Top;
            pictureBox1.Width = Width - pictureBox1.Left;
            pictureBox1.Height = Height - pictureBox1.Top;

            pictureBox1.Image = new Bitmap(Width - pictureBox1.Left, Height - pictureBox1.Top);
            gr = Graphics.FromImage(pictureBox1.Image);
            gr.Clear(Color.White);
        }

        private int stage = 0;
        private int mX, mY;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void dravCross(int X, int Y, int Size)
        {
            gr.DrawLine(new Pen(Color.Black, 3), X, Y - Size, X, Y + Size);
            gr.DrawLine(new Pen(Color.Black, 3), X - Size, Y, X + Size, Y);

           // pictureBox1.Invalidate();// (new Rectangle(X - Size, Y - Size, X + Size, Y + Size));

        }



        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //while (scale != 1)
            //{
            //    if (scale > 1)
            //    {
            //        gr.ScaleTransform((float)0.8, (float)0.8);
            //        scale -= 0.2;
            //    } if (scale < 1)
            //    {
            //        gr.ScaleTransform((float)1.2, (float)1.2);
            //        scale += 0.2;
            //    }
            //}
            gr.PageScale=(float)0.8;
            drawObjects();
            //1,2
            if (stage == 0)
            {
                mX = e.X;
                mY = e.Y;
                stage++;
            }

        }

        private int sizeLast;
        private bool stoped;
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            pictureBox1.Focus();
            drawObjects();
            dravCross(e.X, e.Y, 10);
            if (stage != 0)
            {


                //dravCross(e.X, e.Y, 10);
                try
                {

                    switch (stage)
                    {
                        case 1: //рисование массы - диаметра объекта
                            {
                                int size =1+ (int) (Math.Sqrt((e.X - mX)*(e.X - mX) + (e.Y - mY)*(e.Y - mY))*2);
                                sizeLast = size;
                                gr.DrawEllipse(new Pen(Color.Black, 3), mX - (size)/2, mY - (size)/2,
                                               size, size);
                                if (checkBox1.Checked)
                                    gr.DrawString("MASS:" + size, Font, new HatchBrush(HatchStyle.Shingle, Color.Black),
                                                  (float) mX + size/2,
                                                  (float) mY - size/2);
                                //gr.DrawEllipse();
                                //pictureBox1.Invalidate();
                                //pictureBox1.Update();
                                break;
                            }


                        case 2: //ресование вектора скорости
                            {

                                gr.DrawEllipse(new Pen(Color.Black, 3), mX - (sizeLast)/2, mY - (sizeLast)/2,
                                               sizeLast, sizeLast);
                                gr.DrawLine(new Pen(Color.Red, 3), mX, mY, e.X, e.Y);
                                if (checkBox1.Checked)
                                    gr.DrawString(
                                        "XY:" + (e.X - mX) + "," + (e.Y - mY) + ",L:" +
                                        ((int)((new vector(e.X - mX, e.Y - mY)).lenght())).ToString(), Font,
                                        new HatchBrush(HatchStyle.Shingle, Color.Red),
                                        e.X, e.Y);
                                //pictureBox1.Invalidate();
                                //pictureBox1.Update();

                                break;
                            }
                    }
                    if (stage==3)
                       {
                        stage = 0;
                           obj temp;
                        if (!stoped)
                        {

                            temp=new obj(new vector(mX, mY), new vector(e.X - mX, e.Y - mY), sizeLast,stoped);
                            
                        }
                        else
                            temp=(new obj(new vector(mX, mY), new vector(0, 0), sizeLast, stoped));
                           if (checkBox4.Checked)
                           {
                               if(imgs.Count>0)
                               {
                                   int index = t.selectedIndex;
                                   temp.textureIndex = index;
                                   temp.texture = ResizeImg(imgs[index].image, sizeLast , sizeLast );
                               }
                               else
                               {
                                   MessageBox.Show("textures list is empty. copy PNGs to program folder and restart app.");
                               }
                           }
                        objects.Add(temp);
                        drawObjects();
                        
                    }
                }
                catch (Exception)
                {

                }
            }

            pictureBox1.Invalidate();
            pictureBox1.Update();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            
            switch (stage)
            {
                case 1: //рисование массы - диаметра объекта
                    {
                        stage++;
                        break;
                    }
                case 2: //ресование вектора скорости
                    {
                            stoped = e.Button == MouseButtons.Right;
                        stage++;
                        break;
                    }
                case 3: //все нарисовано 
                    {
                        stage = 0;
                        break;
                    }
            }
//            Text = stage.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            objects.Clear();
            scale = 1;
            pictureBox1.Image = new Bitmap(Width - pictureBox1.Left, Height - pictureBox1.Top);
            gr = Graphics.FromImage(pictureBox1.Image);
            gr.Clear(Color.White);
            drawObjects();
        }

        private void button2_SizeChanged(object sender, EventArgs e)
        {

        }
        bool do_ = false;
        private void button3_Click(object sender, EventArgs e)
        {
            if (!do_)
            {
                button3.BackColor = Color.Red;
                button3.Text = "►";
                do_ = true;
                
                while (do_)
                
                    {
                        Application.DoEvents();
                        button1_Click(sender, e);
                        if (!do_) break;
                    }
         

            }
            else
            {

                button3.BackColor = BackColor;
                button3.Text = "■";
                do_=false;
            }
        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            do_ = false;

        }

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

        public long getSizeOf(object element)
        {
            long size = 0;
            object obj = element;
            using (Stream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                size = stream.Length;
            }
            return size;
        }

        public byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public T ByteArrayToObject<T>(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = (T)binForm.Deserialize(memStream);
            return obj;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog()==DialogResult.OK)
                using (BinaryWriter writer = new BinaryWriter(File.Open(saveFileDialog1.FileName, FileMode.Create)))
            {
                writer.Write(objects.Count);
                foreach (var o in objects)
                {
                    byte[] bytes = ObjectToByteArray(o);
                    writer.Write(bytes.Length);

                    writer.Write(bytes);
                }
                
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                using (BinaryReader reader = new BinaryReader(File.Open(openFileDialog1.FileName, FileMode.Open)))
                {
                    int count  = reader.ReadInt32();
                    objects.Clear();
                    objects=new List<obj>();
                    for (int i = 0; i < count;i++ )
                    {
                        int size = reader.ReadInt32();
                        byte[] readen = reader.ReadBytes(size);
                        obj temp = ByteArrayToObject<obj>(readen);
                        objects.Add(temp);
                    }
                    drawObjects();
                    
                }
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            if (saveFileDialog1.FileName.Split('.')[saveFileDialog1.FileName.Split('.').Length-1]!="grav") saveFileDialog1.FileName+=".grav";
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int loposX = 0, loposY = 0;
            //double loscale = 1;
            if ((e.KeyData) == Keys.PageUp)
            {
                if (scale < 1000)
                {
                    scale *= 1.2;
                    gr.ScaleTransform((float)1.2, (float)1.2);
                }
            }
            else if (((e.KeyData) == Keys.PageDown))
            {
                if(scale>0.01)
                {

                    scale *= 0.8;
                    gr.ScaleTransform((float)0.8, (float)0.8);
                }
            }
            else if (((e.KeyData) == Keys.Left))
                loposX = -50;
            else if (((e.KeyData) == Keys.Right))
                loposX = 50;
            else if (((e.KeyData) == Keys.Up))
                loposY = -50;
            else if (((e.KeyData) == Keys.Down))
                loposY = 50;
            do_ = false;
            foreach (var o in objects)
            {
                o.pos.X += loposX;
                o.pos.Y += loposY;
            }

            //gr.ScaleTransform((float)scale, (float)scale);
            do_ = true;
            drawObjects();
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                if (t!=null) t.Close();
                t=new texture_selector(imgs);
                t.Show();
                return;
            }
            //if (t != null && (!t.Visible && checkBox4.Checked)) t.Visible = true;
        }
    }

    class field
    {
        public vector B, E;
        public field(vector b, vector e)
        {
            B = b;
            E = e;
        }

    }
    enum objectClass
    {
        star,
        comet,
        planet
    }

    [Serializable]
    internal class vector
    {
        public  double X, Y;

        //public static vector operator -(vector a)
        //{
        //    return new vector(X - a.getX(), Y - a.getY());
        //}

        private double getY()
        {
            return Y;
        }

        private double getX()
        {
            return X;
        }

        public vector(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double lenght()
        {
            return Math.Sqrt(X*X + Y*Y);
        }
    }

    public class imageContainer
    {
        public string filename;
        public Image image;
        public imageContainer(string f, Image i)
        {
            image = i;
            filename = f;
        }
    }

    [Serializable]
    class obj
    {
        public vector pos,speed,force;
        public int mass;
        public bool avalible;
        public bool fix;
        public Image texture;
        public int textureIndex;
        //public objectClass class;

        public obj(vector p, vector s, int m, bool f)
        {
            fix = f;
            avalible = true;
            force=new vector(0,0);
            pos = p;
            speed = s;
            mass = m;
            texture = null;
            textureIndex = -1;
        }

        public new string ToString()
        {
            if (!fix)
                return "POS(X,Y):" + (int) pos.X + "," + (int) pos.Y + ",V(X,Y,L):" + (int) speed.X + "," + (int) speed.Y +
                     ","+(int)(speed.lenght())+
                    ",MASS:" + mass;
            else
            {
                return "POS(X,Y):" + (int) pos.X + "," + (int) pos.Y + ",FIXED,MASS:" + mass;
            }
        }
    }
}
