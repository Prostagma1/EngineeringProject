using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Size = OpenCvSharp.Size;

namespace EngineeringProject
{
    public partial class Form1 : Form
    {
        bool runVideo;
        VideoCapture capture;
        Mat matInput;
        Thread cameraThread;
        readonly Size sizeObject = new Size(640, 480);
        string pathToFile;
        public Form1()
        {
            InitializeComponent();
        }
        private void DisposeVideo()
        {
            pictureBox1.Image = null;
            if (cameraThread != null && cameraThread.IsAlive) cameraThread.Abort();
            matInput?.Dispose();
            capture?.Dispose();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeVideo();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Enabled = false;
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Enabled = true;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Multiselect = false
            };
            if (file.ShowDialog() == DialogResult.OK)
            {
                var tempPath = file.FileName;
                if (File.Exists(tempPath))
                {
                    var ext = Path.GetExtension(tempPath);
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        pathToFile = tempPath;
                        textBox1.Text = pathToFile;
                    }
                }
            }
            file.Dispose();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (runVideo)
            {
                runVideo = false;
                DisposeVideo();
            }
            else
            {
                runVideo = true;
                matInput = new Mat();

                if (radioButton1.Checked)
                {
                    capture = new VideoCapture(pathToFile)
                    {
                        FrameHeight = sizeObject.Height,
                        FrameWidth = sizeObject.Width,
                        AutoFocus = true
                    };
                }
                cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
                cameraThread.Start();
            }
        }
        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);


                Invoke(new Action(() =>
                {
                    pictureBox1.Image = BitmapConverter.ToBitmap(matInput);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }));
            }
        }
    }
}
