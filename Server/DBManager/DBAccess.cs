using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Server.DBManager
{
    class DBAccess
    {
        SqlConnection connection = null;
        ConnectionStringSettings conSettings;
        public DBAccess()
        {
            conSettings = ConfigurationManager.ConnectionStrings["Server.Properties.Settings.dblikeConnectionString"];
        }
        public SqlConnection getDBAccess()
        {
            //String connectionString = conSettings.ConnectionString;
            String connectionString = "Data Source=i2fgak0wp2.database.windows.net;Initial Catalog=dblike;User ID=myadmin@i2fgak0wp2;Password=ABC!1234;MultipleActiveResultSets=true";

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                //Console.WriteLine("Set");
            }
            catch (SqlException e)
            {
                Console.WriteLine("SQL EXCEPTION "+ e.Message);
            }
            return connection;
        }
    }
}