using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;

namespace WindowsFormsApplication3
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string[] arr = listed.mail.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            textBox1.Text = arr[2];
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("jkhalid1@gmail.com");
            msg.To.Add(arr[2]);
            msg.Subject = "MBB blood request" + DateTime.Now.ToString();
            msg.Body = (richTextBox1.Text);
            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = true;
            client.Host = "smtp-mail.outlook.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("jkhalid1@gmail.com", "Aybz1829");
            client.Timeout = 20000;
            try
            {
                client.Send(msg);
                MessageBox.Show("Mail has been successfully sent!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Email Sending Failed. Please try again!" + ex.Message);
            }
            this.Hide();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
