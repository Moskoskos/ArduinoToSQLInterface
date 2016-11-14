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
        char[] weightValues = new char[5];
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
                udpClient.Close();
            }
        }


        //s
        private void btnActivate_Click(object sender, EventArgs e)
        {
            rtxtMessages.AppendText("Initializing port listening...\r\n");       //Dunno how long this will take, so message the user that work is in progress first.
            listeningActive = true;
            thdUDPServer = new Thread(() => ListenToPort(port));
            thdUDPServer.Start();                                             //Start configuration of port-listening
            DeactivateButtons();
            rtxtMessages.AppendText("Listening commenced...\r\n");

            
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            listeningActive = false;
            rtxtMessages.AppendText("Aborting port listening...\r\n");      //Dunno how long this will take, so message the user that work is in progress first.
            ActivateButtons();
            thdUDPServer.Abort();
            rtxtMessages.AppendText("Port listening aborted...\r\n");
            udpClient.Close();

        }

        private void rtxtMessages_TextChanged(object sender, EventArgs e)
        {
            rtxtMessages.SelectionStart = rtxtMessages.Text.Length;
            rtxtMessages.ScrollToCaret();                                       //Makes sure that the box continues to scroll downward as text is displayed
        }

        private void UDPClient()
        {
            //UdpClient udpc = new UdpClient();
        }


        //http://stackoverflow.com/questions/19786668/c-sharp-udp-socket-client-and-server
        private void ListenToPort(int portNumber)                                             //Listen to port, any IP.
        {
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(port);


            while (listeningActive == true)
            {

                receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                Array.Copy(receiveBytes, 3, weightValues, 0, 5);                                   //copy weight from receiveBytes array into a new array
                db.ArduinoDataToDb(receiveBytes, weightValues);                                    //Passing data from arduino to DB
                foreach (byte val in receiveBytes)
                {
                    AppendTextBox(RemoteIpEndPoint.Address.ToString() + val.ToString());
                }
                if (listeningActive == false)
                {
                    udpClient.Close();
                }
                //Does this one actually stop when thdUDP.Abort() is called
                //No. need to declare the instance as a global instance to be able to call it. that or a metho
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
            rtxtMessages.Text += ":  " + value + "\r\n";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}


