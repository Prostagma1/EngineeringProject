using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.Extensions;
using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace EngineeringProject
{
    public partial class Form2 : Form
    {
        public Dictionary markersDicionary = CvAruco.GetPredefinedDictionary((PredefinedDictionaryName)14);
        readonly string providerDB = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseStorage.mdb;";
        OleDbConnection DataConnection;
        Mat matMarker = new Mat();
        public bool stopTimer;
        int id = 0;
        public byte idDict;
        public Form2()
        {
            InitializeComponent();
            DataConnection = new OleDbConnection(providerDB);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            idDict = (byte)(2 + comboBox1.SelectedIndex * 4);
            markersDicionary = CvAruco.GetPredefinedDictionary((PredefinedDictionaryName)idDict);
            DoExampleMarker();
            trackBar1.Enabled = true;
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
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].HeaderText = "Код";
            dataGridView1.Columns[1].HeaderText = "Тип матрицы";
            dataGridView1.Columns[2].HeaderText = "ID метки";
            dataGridView1.Columns[3].HeaderText = "Команда";
            button5_Click(null, null);
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == String.Empty)
            {
                MessageBox.Show("Команда для метки не введена!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            DataConnection.Open();
            OleDbCommand myOleDbCommand = DataConnection.CreateCommand();
            myOleDbCommand.CommandText = $"INSERT INTO [Sets] ([Тип матрицы],[ID метки],[Команда]) VALUES ('{idDict}',{id},'{textBox1.Text}')";
            OleDbDataReader myOleDbDataReader = myOleDbCommand.ExecuteReader();
            myOleDbDataReader.Read();
            DataConnection.Close();
            textBox1.Text = String.Empty;
            button5_Click(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DataConnection.Open();

            

            OleDbCommand oleDbCommand = DataConnection.CreateCommand();
            oleDbCommand.CommandText = "SELECT * FROM [Sets]";
            OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
            dataGridView1.RowCount = 1;
            int i = 0;
            while (oleDbDataReader.Read())
            {
                dataGridView1.RowCount += 1;
                dataGridView1.Rows[i].Cells[0].Value = oleDbDataReader["Код"];
                dataGridView1.Rows[i].Cells[1].Value = oleDbDataReader["Тип матрицы"];
                dataGridView1.Rows[i].Cells[2].Value = oleDbDataReader["ID метки"];
                dataGridView1.Rows[i].Cells[3].Value = oleDbDataReader["Команда"];
                i++;
            }
            oleDbDataReader.Close();
            DataConnection.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы точно хотите удалить все записи в БД?", "Внимание!", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {
                DataConnection.Open();
                OleDbCommand oleDbCommand = DataConnection.CreateCommand();
                oleDbCommand.CommandText = "DELETE FROM [Sets]";
                OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
                oleDbDataReader.Read();
                DataConnection.Close();
                button5_Click(sender, e);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            OleDbCommand oleDbCommand = DataConnection.CreateCommand();
            oleDbCommand.CommandText = $"DELETE FROM [Sets] WHERE [Код] = {dataGridView1.SelectedRows[0].Cells[0].Value}";
            DataConnection.Open();
            OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();
            oleDbDataReader.Read();
            DataConnection.Close();
            button5_Click(sender, e);
        }
    }
}
