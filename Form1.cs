using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Size = OpenCvSharp.Size;

namespace EngineeringProject
{
    public partial class Form1 : Form
    {
        bool runVideo;
        VideoCapture capture;
        Dictionary markers;
        Mat matInput;
        Thread cameraThread;
        readonly Size sizeObject = new Size(640, 480);
        string pathToFile;
        int[] ids;
        Form2 form;
        bool showMarks, searchMarks;
        int idDict = 14;
        int zCoord = 0;
        string stringForLabel = "Координаты метки: ";
        Point[] center;
        readonly int markerLength = 300;
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
                panel3.Enabled = false;
                DisposeVideo();
                button1.Text = "Старт";
            }
            else
            {
                runVideo = true;
                panel3.Enabled = true;
                matInput = new Mat();

                if (radioButton1.Checked)
                {
                    capture = new VideoCapture(0)
                    {
                        FrameHeight = sizeObject.Height,
                        FrameWidth = sizeObject.Width,
                        AutoFocus = true
                    };
                }
                cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
                cameraThread.Start();
                button1.Text = "Стоп";
            }
        }

        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);
                if (searchMarks)
                {
                    SearchAndShowMarks(markers, ref matInput, out ids, out center, showMarks);
                }
                Invoke(new Action(() =>
                {
                    label4.Text = stringForLabel;
                    label5.Text = center?.Length > 0 ? $"Расстояние до точки: X:{center[0].X - robotCoords[0]} Y:{center[0].Y - robotCoords[1]} Z:{zCoord - robotCoords[2]}" : label5.Text;
                    pictureBox1.Image = BitmapConverter.ToBitmap(matInput);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }));
            }
        }

        private void SearchAndShowMarks(Dictionary marksDict, ref Mat inputMat, out int[] ids, out Point[] centers, bool drawDetectedMarks)
        {
            Point2f[][] corners;
            var param = new DetectorParameters();
            CvAruco.DetectMarkers(inputMat, marksDict, out corners, out ids, param, out _);
            centers = new Point[corners.Length];
            for (short i = 0; i < centers.Length; i++)
            {
                for (byte j = 0; j < 4; j++)
                {
                    centers[i].X += (int)corners[i][j].X;
                    centers[i].Y += (int)corners[i][j].Y;
                }
                centers[i].X /= 4;
                centers[i].Y /= 4;
            }
            if (drawDetectedMarks && corners.Length > 0)
            {

                zCoord = markerLength - (int)Point2f.Distance(corners[0][0], corners[0][1]);

                CvAruco.DrawDetectedDiamonds(inputMat, corners);
                for (short i = 0; i < centers.Length; i++)
                {
                    inputMat.Circle(centers[i], 2, Scalar.Red, 2);
                    stringForLabel = $"Координаты метки: X:{centers[i].X - 5} Y:{centers[i].Y - 5} Z:{zCoord}";
                    inputMat.PutText($"X:{centers[i].X - 5} Y:{centers[i].Y - 5} Z:{zCoord}", centers[i], HersheyFonts.HersheySimplex, 0.5, Scalar.Red);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            form?.Dispose();
            form = new Form2();
            form.Show();
            timer1.Start();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            showMarks = checkBox2.Checked;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            markers = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict7X7_250);
            button4_Click(null, null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            idDict = form.idDict;
            markers = form.markersDicionary;
            label3.Text = $"Используемый словарь {(PredefinedDictionaryName)idDict}";
            if (form.stopTimer) timer1.Stop();
        }
        int[] robotCoords;
        private void button4_Click(object sender, EventArgs e)
        {
            var items = textBox2.Text.Split(';');
            robotCoords = new int[]{
                int.Parse(items[0]),
                int.Parse(items[1]),
                int.Parse(items[2])};
            label2.Text = $"Координаты робота : X:{items[0]} Y:{items[1]} Z:{items[2]}";
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            searchMarks = checkBox1.Checked;
        }

    }
}
