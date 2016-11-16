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
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace ArduinoSQLInterface
{
    public partial class frmMain : Form
    {

        private int port;                                                       //Spesific Port
        private bool listeningActive = false;
        Thread thdUDPServer;
        byte[] receiveBytes;
        UdpClient udpClient;
        IPEndPoint RemoteIpEndPoint;

        ERP.DbConnect db = new ERP.DbConnect();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            port = 3306;
            if (db.PingHost() != true)
            {
                MessageBox.Show("Host not connectable");
            }
            
        }

        private void frmMain_FormClosing(object sender, FormClosedEventArgs e)
        {
            if (string.Equals((sender as Button).Name, @"CloseButton"))
            {
                DeactivateListening();
                Application.Exit();
            }
        }


        //s
        private void btnActivate_Click(object sender, EventArgs e)
        {
            ActivateListening();
            DeactivateButtons();
        }

        private void ActivateListening()
        {
            listeningActive = true;
            thdUDPServer = new Thread(() => ListenToPort(port));
            thdUDPServer.Start();                                                //Start configuration of port-listening
            DeactivateButtons();
            rtxtMessages.AppendText("Listening commenced...\r\n");
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {

            DeactivateListening();
            ActivateButtons();
        }

        private void DeactivateListening()
        {
            listeningActive = false;
            thdUDPServer.Abort();
            rtxtMessages.AppendText("Port listening aborted...\r\n");
            udpClient.Close();
        }

        private void rtxtMessages_TextChanged(object sender, EventArgs e)
        {
            rtxtMessages.SelectionStart = rtxtMessages.Text.Length;
            rtxtMessages.ScrollToCaret();                                       //Makes sure that the box continues to scroll downward as text is displayed
        }


        //http://stackoverflow.com/questions/19786668/c-sharp-udp-socket-client-and-server
        private void ListenToPort(int portNumber)                               //Listen to port, any IP.
        {
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(port);


            while (listeningActive == true)
            {
                receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                db.ArduinoDataToDb(receiveBytes);                               //Passing data from arduino to DB
                foreach (byte val in receiveBytes)
                {
                    AppendTextBox(RemoteIpEndPoint.Address.ToString() + ":  " + val.ToString());
                }
                if (listeningActive == false)
                {
                    udpClient.Close();
                }
             
            }

        }

            
        private void ActivateButtons()
        {
            btnActivate.Enabled = true;
        }
        private void DeactivateButtons()
        {
            btnActivate.Enabled = false;
        }

        private void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            rtxtMessages.Text +=  value + "\r\n";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeactivateListening();
            Application.Exit();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.Equals((sender as Button).Name, @"CloseButton"))
            {
                DeactivateListening();
                Application.Exit();
            }
        }
    }
}

