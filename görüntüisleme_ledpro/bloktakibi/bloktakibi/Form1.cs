using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Drawing.Imaging;
using AForge.Video.DirectShow;
using AForge;
using AForge.Math;
using AForge.Video;
using AForge.Math.Geometry;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace bloktakibi
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection CaptureDevice;    //aygıtları görüntüleme
        private VideoCaptureDevice FinalFrame;         //kullanacağımız kamerayı seçme
        public Form1()
        {
            InitializeComponent();
        }
        Bitmap video_1, video_2;
        int R, G, B;
        int x, y;
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackBar1.Maximum = 255;
            trackBar1.Minimum = 0;
            trackBar1.SmallChange = 1;
            trackBar1.LargeChange = 1;
            trackBar1.TickFrequency = 1;
            label1.Text = "Red: " + trackBar1.Value.ToString();
            R = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            trackBar2.Maximum = 255;
            trackBar2.Minimum = 0;
            trackBar2.SmallChange = 1;
            trackBar2.LargeChange = 1;
            trackBar2.TickFrequency = 1;
            label2.Text = "Green: " + trackBar2.Value.ToString();
            G = trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            trackBar3.Maximum = 255;
            trackBar3.Minimum = 0;
            trackBar3.SmallChange = 1;
            trackBar3.LargeChange = 1;
            trackBar3.TickFrequency = 1;
            label3.Text = "Blue: " + trackBar3.Value.ToString();
            B = trackBar3.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(FinalFrame.IsRunning)
            {
                FinalFrame.Stop();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = 0;
            serialPort1.BaudRate = 9600;
            serialPort1.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Kamera başlaması için gerekli kod satırı
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_newFrame);
            FinalFrame.NewFrame += FinalFrame_newFrame;
            FinalFrame.Start();

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // Arduino
            comboBox2.DataSource = SerialPort.GetPortNames();
            serialPort1.PortName = comboBox2.SelectedItem.ToString();
            //Camera
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {

                comboBox1.Items.Add(Device.Name);
            }
            comboBox1.SelectedIndex = 0;
        }

        //kameradan alınan görüntünün işleme kısmı

        private void FinalFrame_newFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video_1 = (Bitmap)eventArgs.Frame.Clone();
            video_2 = (Bitmap)eventArgs.Frame.Clone();
            



            Mirror filter2 = new Mirror(false, true);
            filter2.ApplyInPlace(video_1);

            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
            filter.Radius = 100;
            filter.ApplyInPlace(video_1);
            BitmapData objectsData = video_1.LockBits(new Rectangle(0, 0, video_1.Width, video_1.Height), ImageLockMode.ReadOnly, video_1.PixelFormat);
            Grayscale grifiltresi = new Grayscale(0.2125, 0.7154, 0.0721); //Gri filtresi
            UnmanagedImage griresim = grifiltresi.Apply(new UnmanagedImage(objectsData));
            video_1.UnlockBits(objectsData);
            BlobCounter blobcounter = new BlobCounter(); //birleşik pikselleri numaralandırma işlemi, nesne sayma
            blobcounter.MinWidth = 18; //sayılan nesnelerin minimum genişliği
            blobcounter.MinHeight = 18; //"" yüksekliği
            blobcounter.FilterBlobs = true; //şartlara uyanların işaretlenmesi
            blobcounter.ObjectsOrder = ObjectsOrder.Size; //boyutlarına göre sıralamaya sokma işlemi


            blobcounter.ProcessImage(griresim); //videodaki bloblar sayılıp işleniyor
            Rectangle[] rects = blobcounter.GetObjectsRectangles(); //blobcounter içerisindeki blobların verileri alınıyor
            Blob[] blobs = blobcounter.GetObjectsInformation();
            pictureBox2.Image = video_2;

            foreach (Rectangle recs in rects)
            {
                Graphics g = Graphics.FromImage(video_1);
                if (rects.Length > 0)
                {
                    Rectangle objectRect = rects[0];
                    x = objectRect.X + (objectRect.Width / 2);
                    y = objectRect.Y + (objectRect.Height / 2);

                    if (x < 213 && y < 160)
                    {
                        serialPort1.Write("1");
                    }
                    if ((213 < x && x < 426) && y < 160)
                    {
                        serialPort1.Write("2");
                    }
                    if (426 < x && x < 640 && y < 160)
                    {
                        serialPort1.Write("3");
                    }
                    if ((x < 213 && (160 < y && y < 320)))
                    {
                        serialPort1.Write("4");
                    }
                    if ((213 < x && x < 426) && (160 < y && y < 320))
                    {
                        serialPort1.Write("5");
                    }
                    if ((426 < x && x < 639) && (160 < y && y < 320))
                    {
                        serialPort1.Write("6");
                    }
                    if (x < 213 && (320 < y && y < 480))
                    {
                        serialPort1.Write("7");
                    }
                    if ((213 < x && x < 426) && (320 < y && y < 480))
                    {
                        serialPort1.Write("8");
                    }
                    if ((426 < x && x < 639) && (320 < y && y < 480))
                    {
                        serialPort1.Write("9");
                    }
                    

                    using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                    {
                        g.DrawRectangle(pen, objectRect);
                    }
                    
                    g.DrawString(x.ToString() + "X" + y.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(x, y));
                    g.Dispose();
                }
            }
            pictureBox1.Image = video_1;
        }
        
        public void cisimbul(Bitmap video_1)
        {
            
            }
        

        
        private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(FinalFrame.IsRunning)
            {
                FinalFrame.Stop();
                serialPort1.Write("0");
                this.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}