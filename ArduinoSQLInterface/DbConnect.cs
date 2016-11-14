using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.ComponentModel;

namespace ERP
{
    public class DbConnect
    {
        private SqlConnection connection;
        public string batchid = "";
        public string conString = "";



        public DbConnect()
        {
            //Initialze connection with connectionString
            conString = "Data Source = 192.168.2.15\\SQLEXPRESS; Initial Catalog = IA5-5-16; User ID = sa; Password = " + "netlab_1";
            connection = new SqlConnection(conString);
        }
        public bool PingHost()
        {
            string nameOrAddress = "192.168.2.15";
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return pingable;
        }

        public bool OpenConnection()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);

                return false;
            }
        }

        public bool CloseConnection()
        {

            try
            {
                connection.Close();
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }


        
        public void ArduinoDataToDb(byte[] data, char[] weight)
        {
            string w1 = weight[0].ToString();
            string w2 = weight[1].ToString();
            string w3 = weight[2].ToString();
            string w4 = weight[3].ToString();
            string w5 = weight[4].ToString();

            string weightValue = Convert.ToString(string.Format("{0}{1}{2}{3}{4}", w1, w2, w3, w4, w5));    //concatenate strings

            try
            {
                string query = "UPDATE CupOrdre SET ActualWeight = @ActualWeight, CompletedApproved = @CompletedApproved, CompletedDiscard = @CompletedDiscard WHERE CupID = @CupID";
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        OpenConnection();
                        cmd.Parameters.AddWithValue("@CupID", Convert.ToInt32(data[0]));
                        cmd.Parameters.AddWithValue("@ActualWeight",  weightValue);
                        cmd.Parameters.AddWithValue("@CompletedApproved", data[1]);
                        cmd.Parameters.AddWithValue("@CompletedDiscard", data[2]);
                        cmd.ExecuteNonQuery();
                        CloseConnection();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    
    }
}

