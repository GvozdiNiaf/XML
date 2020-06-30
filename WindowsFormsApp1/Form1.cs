using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string filePath = "";
        bool needSave = false;
        DataSet dataSet = new DataSet();
        List<Student> students;
        public Form1()
        {
            InitializeComponent();
            dataSet.ReadXml("students.xml");
            InitDataGridPages();
            DesirializeDataSet();
            students.Add(new Student
            {
                name = "Студент 10",
                group = new Group { number = 4, course = 3, track = "ПОКС" },
                marks = new List<byte> { 5, 5, 5, 5 }
            });
            dataSet.Clear();
            SerialiseDataSet();
        }

        private void InitDataGridPages()
        {
            foreach (var table in dataSet.Tables)
            {
                this.tabControl1.TabPages.Add(table.ToString());
                var page = tabControl1.TabPages[tabControl1.TabPages.Count - 1];
                var dataGrid = new DataGridView();
                page.Controls.Add(dataGrid);
                dataGrid.Dock = DockStyle.Fill;
                dataGrid.DataSource = table;
                var t = table as DataTable;
                foreach (var c in t.Columns)
                {
                    var col = c as DataColumn;
                    if (col.ColumnMapping == MappingType.Hidden)
                        col.ColumnMapping = MappingType.Attribute;
                }
                
                dataGrid.RowValidating += RowValidating;
                dataGrid.DataError += DataError;
                
            }
        }

        private void DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            
            var dataGrid = sender as DataGridView;
            dataGrid.Rows[e.RowIndex].ErrorText = e.Exception.Message;
            MessageBox.Show(e.Exception.Message, "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            Debug.WriteLine($"{ e.RowIndex } : { e.ColumnIndex }");
            var dataGrid = sender as DataGridView;
            dataGrid.Rows[e.RowIndex].ErrorText = "";
            string headerText = dataGrid.Columns[e.ColumnIndex].HeaderText;
            if (headerText != "name")
                return;
            string name = dataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            if (name.Contains("@"))
            {
                e.Cancel = true;
                dataGrid.Rows[e.RowIndex].ErrorText = "Wrong symbol";
            }
            
        }

        private void DesirializeDataSet()
        {
            XmlReader xml = XmlReader.Create(new StringReader(dataSet.GetXml()));
            var serializer = new XmlSerializer(typeof(List<Student>));
            students = serializer.Deserialize(xml) as List<Student>;
        }
        private void SerialiseDataSet()
        {
            var serializer = new XmlSerializer(typeof(List<Student>));
            StringBuilder data = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.Unicode;
            var xml = XmlWriter.Create(new StringWriter(data), settings);
            serializer.Serialize(xml, students);
            dataSet.ReadXml(XmlReader.Create(new StringReader(data.ToString())));
        }

        private void ExitToolStripMenuItem(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                tabControl1.TabPages.Clear();
                if (Path.GetExtension(filePath) == ".xml")
                {
                    dataSet.Clear();
                    dataSet.ReadXml(filePath);
                }
                InitDataGridPages();
                needSave = false;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                dataSet.WriteXml(saveFileDialog1.FileName);
                filePath = saveFileDialog1.FileName;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataSet.WriteXml(filePath);
        }
    }
}
