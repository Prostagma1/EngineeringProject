using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;
using System;
using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Size = OpenCvSharp.Size;

namespace EngineeringProject
{
    public partial class Form1 : Form
    {
        bool runVideo;
        VideoCapture capture;
        HttpListener listener;
        Dictionary markers;
        Mat matInput;
        Thread cameraThread, httpThread;
        readonly Size sizeObject = new Size(640, 480);
        string pathToFile;
        int[] ids;
        Form2 form;
        bool showMarks, searchMarks, sendData;
        int idDict = 14;
        readonly string providerDB = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseStorage.mdb;";
        OleDbConnection DataConnection;
        public Form1()
        {
            InitializeComponent();
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:7777/");
        }
        private void DisposeVideo()
        {
            pictureBox1.Image = null;
            if (cameraThread != null && cameraThread.IsAlive) cameraThread.Abort();
            if (httpThread != null && httpThread.IsAlive) httpThread.Abort();
            listener.Stop();
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
                listener.Start();
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
                httpThread = new Thread(new ThreadStart(SendDataToUser));
                httpThread.Start();
                button1.Text = "Стоп";
            }
        }
        private void AddItemsToListBox(int[] items)
        {
            if (items == null) return;
            listBox1.Items.Clear();
            foreach (var item in items)
            {
                listBox1.Items.Add(item);
            }
        }
        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);
                if (searchMarks)
                {
                    SearchAndShowMarks(markers, ref matInput, out ids, showMarks);
                }
                Invoke(new Action(() =>
                {
                    AddItemsToListBox(ids);
                    pictureBox1.Image = BitmapConverter.ToBitmap(matInput);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }));
            }
        }
        private void SearchAndShowMarks(Dictionary marksDict, ref Mat inputMat, out int[] ids, bool drawDetectedMarks)
        {
            Point2f[][] point2Fs;
            var param = new DetectorParameters();
            CvAruco.DetectMarkers(inputMat, marksDict, out point2Fs, out ids, param, out _);
            if (drawDetectedMarks) CvAruco.DrawDetectedMarkers(inputMat, point2Fs, ids);
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
            DataConnection = new OleDbConnection(providerDB);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            idDict = form.idDict;
            markers = form.markersDicionary;
            label3.Text = $"Используемый словарь {(PredefinedDictionaryName)idDict}";
            if (form.stopTimer) timer1.Stop();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            sendData = checkBox3.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            searchMarks = checkBox1.Checked;
        }

        private void SendDataToUser()
        {
            while (true)
            {

                HttpListenerContext context = listener.GetContext();
                HttpListenerResponse response = context.Response;
                byte[] buffer = { };
                if (sendData && ids != null && ids.Length != 0)
                {
                    foreach (var id in ids)
                    {
                        buffer = Encoding.UTF8.GetBytes(GetDataFromDB(idDict, id));
                    }
                }
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

            }
        }
        private string GetDataFromDB(int numberOfDic, int idMar)
        {
            string[] temp;
            string result = "";
            DataConnection.Open();
            OleDbCommand oleDbCommand = DataConnection.CreateCommand();
            oleDbCommand.CommandText = $"SELECT [Команда] FROM [Sets] WHERE [Тип матрицы] = '{numberOfDic}' AND [ID метки] = {idMar}";
            OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
            oleDbDataReader.Read();
            try
            {
                temp = oleDbDataReader.GetString(0).Split(';');

                result = $"{{\"X\":{temp[0]},\"Y\":{temp[1]},\"Z\":{temp[2]},\"Gripper\":{temp[3]}}}";
            }
            catch (Exception ex)
            {
                result = "Data error";
            }
            DataConnection.Close();
            return result;
        }

    }
}
