using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientWorker;
using System.Threading;
using test.Background;

namespace test
{
    public partial class LoginForm : Form
    {
        Common common = new Common();
        public LoginForm()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text;
            string password = maskedTextBox1.Text;
            autheticateUser(email, password);
        }

        public void autheticateUser(string email, string password)
        {
            if (!common.isEmailPwdNull(email, password))
            {
                if (common.validateEmail(email))
                {
                    Login login = new Login();

                    if (login.userLogin(email, password))
                    {                        
                        this.Hide();
                        WatcherUtil watcherUtil = new WatcherUtil(email);
                        ClientForm clientForm = new ClientForm(email, watcherUtil);
                        clientForm.Show();
                        watcherUtil.startServerSync();
                        watcherUtil.startWatcher();
                    }
                    else
                    {
                        this.Hide();
                        LoginForm Lg = new LoginForm();
                        Lg.Show();

                        MessageBox.Show("LoginId or Password Is wrong"); //if user Name is not available in database

                    }
                }
                else
                {
                    MessageBox.Show("Please Enter valid emailID");
                }

            }
            else
            {
                MessageBox.Show("Please Enter Login Id and Password");
            }

        }



        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            SigninForm sf = new SigninForm();
            sf.Show();
        }


    }
}
