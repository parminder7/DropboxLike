using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test.ui
{
    public partial class Notify : Form
    {
        
        public Notify()
        {
            InitializeComponent();
            this.components = new System.ComponentModel.Container();

            // Set up how the form should be displayed. 
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "DBLite Change Notification";

            // Create the NotifyIcon. 
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);

            // The Text property sets the text that will be displayed, 
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon1.Text = "DBLite Change Notification";
            notifyIcon1.Visible = true;
        }

       

        public void notifyChange(String path, String type)
        {
            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;

            notifyIcon1.BalloonTipText = path;

            notifyIcon1.BalloonTipTitle = "File " + type;



            notifyIcon1.ShowBalloonTip(1000);
            notifyIcon1.Visible = true;

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
         
                   
        }


    }
}
