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
using ClientWorker;
using System.Collections;

namespace Client
{
    public partial class ClientForm : Form
    {
        String path;
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        String emailId;
        ArrayList result;
        String fullNameOfFile;
        public ClientForm(String emailId)
        {
            this.emailId = emailId;
            InitializeComponent();
            CenterToScreen();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ClientForm_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                listView1.Items.Clear();
                textBox1.Text = fbd.SelectedPath;
                System.Diagnostics.Process.Start(fbd.SelectedPath);
                path = fbd.SelectedPath;
                display();

            }
        }

        private void display()
        {
            String[] files = Directory.GetFiles(fbd.SelectedPath);
            String[] dirs = Directory.GetDirectories(fbd.SelectedPath);
            foreach (string file in files)
            {
                listView1.Items.Add(Path.GetFileName(file));
            }
            foreach (string dir in dirs)
            {
                listView1.Items.Add(Path.GetFileName(dir));
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem.Index == -1)
                MessageBox.Show("Please select an Item first!");
            else
            {
                fullNameOfFile = listView1.SelectedItems[0].Text;
                StringBuilder sb = new StringBuilder();
                sb.Append(path).Append("\\").Append(fullNameOfFile);
                var fileInfo = new FileInfo(sb.ToString());
                Upload upload = new Upload();
                
                Boolean result = upload.DocUplaod(emailId, sb.ToString(), fileInfo.Length.ToString(), fullNameOfFile);
                if (result)
                {
                    MessageBox.Show("Uploaded Successfully");
                }

                listView1.Items.Clear();
                display();
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            new LoginForm().Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            listView1.Items.Clear();
            display();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            Download download = new Download();
            //Have not yet integrated the listing all files on cloud, so hardcoding the doc ID  for demonstration
            System.Net.Sockets.NetworkStream fileContent = download.DocDownload(emailId, "DesignDocument.docx");
            FileStream f = new FileStream(@path + "\\DesignDocument.docx", FileMode.OpenOrCreate);
            fileContent.CopyTo(f);
            download.Dispose();
            if (fileContent != null)
            {
                //MessageBox.Show("Download complete");
                listView1.Items.Clear();
                display();
            }
            f.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            SearchFile searchfile = new SearchFile();
            result = new ArrayList();
            result = searchfile.Listing(emailId, "");
            displayCloud();

        }

        private void displayCloud()
        {

            foreach (string res in result)
            {
                listView2.Items.Add(res);
            }

        }
    }
}
