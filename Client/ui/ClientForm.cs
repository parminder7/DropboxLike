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
using test.Background;
using test.Model;
using client;
using System.Text.RegularExpressions;

namespace test
{
    public partial class ClientForm : Form
    {
        String path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        String cloudPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";
        String cloudPublicPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite\\Public";
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        FolderBrowserDialog cloudFbd = new FolderBrowserDialog();
        String emailId;
        String fullNameOfFile;
        WatcherUtil watcherUtil;

        public String getEmailId
        {
            get { return emailId; }
        }

        public ClientForm(String emailId, WatcherUtil watcherUtil)
        {
            this.emailId = emailId;
            InitializeComponent();
            CenterToScreen();
            this.watcherUtil = watcherUtil;
            textBox2.Text = cloudPath;
            textBox1.Text = path;
            loginName_textBox.Text = emailId;
            displayCloud();
            display();
        }

        //Local Browse button
        private void button1_Click(object sender, EventArgs e)
        {

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                listView1.Items.Clear();
                textBox1.Text = fbd.SelectedPath;
                path = fbd.SelectedPath;
                display();

            }
        }

        //upload button
        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem.Index == -1)
                MessageBox.Show("Please select an Item first!");
            else
            {
                fullNameOfFile = listView1.SelectedItems[0].Text;
                StringBuilder sb = new StringBuilder();
                String fileName = System.Text.RegularExpressions.Regex.Split(fullNameOfFile, @"\s+")[0];
                sb.Append(path).Append("\\").Append(fileName);
                var fileInfo = new FileInfo(sb.ToString());
                Upload upload = new Upload();

                Boolean result = upload.DocUplaod(emailId, textBox1.Text, fileInfo.Length.ToString(), fileName,
                                                new Common().getFileMD5(sb.ToString()), fileInfo.LastWriteTimeUtc.Ticks.ToString(), new Common().getOldHash(emailId,fileInfo));
                if (result)
                {
                    MessageBox.Show("Uploaded Successfully");
                }

                listView1.Items.Clear();
                display();
            }

        }

        //Logout button
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            new LoginForm().Show();
        }

        //local refresh button
        private void button6_Click(object sender, EventArgs e)
        {

            listView1.Items.Clear();
            display();
        }

        //Download button
        private void button3_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItem = listView2.SelectedItems;

            foreach (ListViewItem item in selectedItem)
            {
                downloadAsync download = new downloadAsync();
                
                String fileName = item.Text.ToString().Split(' ')[0];
                FileInfo fileInfo = new FileInfo(textBox2.Text + "\\" + fileName);
                Regex regex = new Regex(@"\bDBLite\\\b");
                String[] nameSplit = regex.Split(textBox2.Text + "\\" + fileName);
                if (nameSplit[1].Contains("\\"))
                {
                    Regex regexForBacklash = new Regex(@"\\\b");
                    String[] filenameSplit = regexForBacklash.Split(nameSplit[1], 2);
                    if (publicFlrOptions.Enabled)
                    {
                        download.download(filenameSplit[0], filenameSplit[1].Replace('\\', '/'), textBox1.Text, emailId, fileInfo.Length.ToString());
                    }
                    else
                    {
                        download.download(emailId, nameSplit[1].Replace('\\', '/'), textBox1.Text, emailId, fileInfo.Length.ToString());
                    }

                }
                else
                {
                    download.download(emailId, nameSplit[1].Replace('\\', '/'), textBox1.Text, emailId, fileInfo.Length.ToString());   
                }
                
                //MessageBox.Show("Download complete");
                listView1.Items.Clear();
                display();
                
                
                }
            

        }

        //Home button
        private void button5_Click(object sender, EventArgs e)
        {
            //SearchFile searchfile = new SearchFile();
            //result = new ArrayList();
            //result = searchfile.Listing(emailId, "");
            cloudPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";
            displayCloud();
        }

        //sync checkbox
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //WatcherUtil watcherUtil = new WatcherUtil();
            if (checkBox1.Checked == true)
            {
                watcherUtil.startServerSync();
                watcherUtil.startWatcher();
            }
            else
            {
                watcherUtil.stopServerSync();
                watcherUtil.stopWatcher();
            }
        }

        private void display()
        {
            listView1.Items.Clear();
            textBox1.Text = path;
            String[] files = Directory.GetFiles(path);
            String[] dirs = Directory.GetDirectories(path);
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                listView1.Items.Add(Path.GetFileName(file) + "   " + fileInfo.LastWriteTimeUtc + "   "
                                                                    + Math.Round((Double)fileInfo.Length / 1024, 2) + "kb");
            }
            foreach (string dir in dirs)
            {
                listView1.Items.Add(Path.GetFileName(dir));
            }

        }

        public void displayCloud()
        {
            listView2.Items.Clear();
            textBox2.Text = cloudPath;
            bool isExists = System.IO.Directory.Exists((cloudPath));

            if (!isExists)
            {
                System.IO.Directory.CreateDirectory((cloudPath));
            }

            String[] files = Directory.GetFiles(cloudPath);
            String[] dirs = Directory.GetDirectories(cloudPath);
            foreach (string file in files)
            {
                var fileInfo = new FileInfo(file);
                listView2.Items.Add(Path.GetFileName(file) + "   " + fileInfo.LastWriteTimeUtc + "   "
                                                            + Math.Round((Double)fileInfo.Length / 1024, 2) + "kb");
            }
            foreach (string dir in dirs)
            {
                listView2.Items.Add(Path.GetFileName(dir));
            }

                    }

        private void listView2_ItemActivate(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItem = listView2.SelectedItems;

            foreach (ListViewItem item in selectedItem)
            {
                if (!item.Text.Contains("kb"))
                {
                    cloudPath = cloudPath + ("\\" + item.Text);
                    displayCloud();
                }
                else
                {
                    String[] value = System.Text.RegularExpressions.Regex.Split(item.Text.ToString(), @"\s+");
                    String file = cloudPath + "\\" + value[0];
                    System.Diagnostics.Process.Start(file);
                }
            }



        }

        private void button7_Click(object sender, EventArgs e)
        {
            displayCloud();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SharedFolderForm.isCreate = true;
            SharedFolderForm sharedFolder = new SharedFolderForm(this);
            sharedFolder.ShowDialog();

        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItem = listView1.SelectedItems;

            foreach (ListViewItem item in selectedItem)
            {
                if (!item.Text.Contains("kb"))
                {
                    path = path + ("\\" + item.Text);
                    display();
                }
                else
                {
                    String[] value = System.Text.RegularExpressions.Regex.Split(item.Text.ToString(), @"\s+");
                    String file = path + "\\" + value[0];
                    System.Diagnostics.Process.Start(file);
                }
            }

        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedItem = listView2.SelectedItems;
            foreach (ListViewItem item in selectedItem)
            {
                if (!item.Text.Contains("kb"))
                {
                    button3.Enabled = false;
                    cloudinfo cloud = SearchFile.cloud;

                    cloudinfoContainer[] itemsField = cloud.Items;
                    if (!(itemsField == null || itemsField.Length == 0))
                    {
                        foreach (cloudinfoContainer container in itemsField)
                        {
                            if ((container.name.Equals(item.Text)))
                            {
                                if ((container.user != null && container.user.Length != 0))
                                {
                                    publicFlrOptions.Enabled = true;

                                }

                            }
                        }
                    }
                    else
                    {
                        publicFlrOptions.Enabled = false;
                    }
                }
                else
                {
                    button3.Enabled = true;
                }
                
            }
        }

        private void publicFlrOptions_Click(object sender, EventArgs e)
        {
            SharedFolderForm sharedFolder = new SharedFolderForm(this);
            ListView.SelectedListViewItemCollection selectedItem = listView2.SelectedItems;
            foreach (ListViewItem item in selectedItem)
            {
                cloudinfo cloud = SearchFile.cloud;

                    cloudinfoContainer[] itemsField = cloud.Items;
                    if (!(itemsField == null || itemsField.Length == 0))
                    {
                        foreach (cloudinfoContainer container in itemsField)
                        {
                            if ((container.name.Equals(item.Text)) && (container.user != null && container.user.Length != 0))
                            {
                                SharedFolderForm.isCreate = false;
                                sharedFolder.display(container);
                            }
                        }
                    }

            }
            
        }

        private void sync_button_Click(object sender, EventArgs e)
        {
            watcherUtil.stopWatcher();
            watcherUtil.startListing();
            watcherUtil.startWatcher();
            displayCloud();
        }
    }
}
