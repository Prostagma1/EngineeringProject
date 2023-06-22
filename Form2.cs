using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;
using System;
using System.Windows.Forms;

namespace EngineeringProject
{
    public partial class Form2 : Form
    {
        public Dictionary markersDicionary = CvAruco.GetPredefinedDictionary((PredefinedDictionaryName)14);
        Mat matMarker = new Mat();
        public bool stopTimer;
        int id = 0;
        public byte idDict;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            idDict = (byte)(2 + comboBox1.SelectedIndex * 4);
            markersDicionary = CvAruco.GetPredefinedDictionary((PredefinedDictionaryName)idDict);
            DoExampleMarker();
        }
        private void DoExampleMarker()
        {
            markersDicionary.GenerateImageMarker(id, 300, matMarker);
            pictureBox1.Image = BitmapConverter.ToBitmap(matMarker);
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            stopTimer = false;
            comboBox1.SelectedIndex = 3;
            idDict = 14;
            button1_Click(null, null);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            id = trackBar1.Value;
            label2.Text = $"id = {id}";

            DoExampleMarker();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopTimer = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(null, null);
        }
    }
}
