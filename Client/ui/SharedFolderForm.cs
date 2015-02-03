using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using test.ClientManager;
using test.Model;

namespace test
{
    public partial class SharedFolderForm : Form
    {
        static int i = 1;
        public static bool isCreate = true;
       // OrderedDictionary inputList = new OrderedDictionary();
        //Dictionary<TextBox, CheckBox> inputList = new OrderedDictionary<TextBox, CheckBox>();
        List<KeyValuePair<TextBox, CheckBox>> inputList = new List<KeyValuePair<TextBox, CheckBox>>();
        ClientForm clientForm;

        public SharedFolderForm(ClientForm clientForm)
        {
            InitializeComponent();
            this.clientForm = clientForm;
            CenterToScreen();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            performAction();
           
        }

        private void performAction()
        {
            TextBox tb = new TextBox();

            CheckBox cb = new CheckBox();
            cb.Text = "R/W";

            Point p = new Point(132, 99 + 26 * i);
            tb.Location = p;
            this.Controls.Add(tb);

            Point cp = new Point(273, 101 + 26 * i);
            cb.Location = cp;
            this.Controls.Add(cb);

            inputList.Add(new KeyValuePair<TextBox, CheckBox>(tb, cb));

            i++;

            if (i == 5)
            {
                button1.Enabled = false;
            }
        }

        private void create_button_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please enter the folder name");
                return;
            }

            ShareFolder sf = new ShareFolder();
            StringBuilder sb = new StringBuilder();
            inputList.Add(new KeyValuePair<TextBox, CheckBox>(textBox2, checkBox1));
            //inputList.Add(textBox2, checkBox1);
            

            foreach (KeyValuePair<TextBox, CheckBox> kvp in inputList)
            {
                if (kvp.Key.Text != "")
                {
                    sb.Append(kvp.Key.Text + (kvp.Value.Checked == true ? " RW," : " R,")); 
                }
         
            }
            
            if (!sf.createSharedFolder(clientForm.getEmailId, textBox1.Text, sb.ToString().TrimEnd(',')))
            {
                MessageBox.Show("ERROR creating Shared Folder");
                return;
            }
            
            //String cloudPublicPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite\\Public";
            //bool isExists = System.IO.Directory.Exists((cloudPublicPath));

            //if (!isExists)
            //{
            //    System.IO.Directory.CreateDirectory((cloudPublicPath));
            //}
            //System.IO.Directory.CreateDirectory((cloudPublicPath + "\\" + textBox1.Text));
            
            clientForm.displayCloud();            
            this.Dispose();
        }

        public void display(Model.cloudinfoContainer container)
        {
            this.textBox1.Text = container.name;
            cloudinfoContainerUser[] users = container.user;
            int i = 0;
            foreach(cloudinfoContainerUser user in users)
            {
                if (i == 0)
                {
                    this.textBox2.Text = user.email;
                    this.checkBox1.Checked = user.access.Equals("readwrite") ? true : false;
                    
                }
                else
                {
                    performAction();
                    KeyValuePair<TextBox, CheckBox> valPair = inputList.Last();
                    valPair.Key.Text = user.email;
                    valPair.Value.Checked = user.access.Equals("readwrite") ? true : false;
                }
                i = i + 1;
            }
            this.ShowDialog();
        }
    }
}
