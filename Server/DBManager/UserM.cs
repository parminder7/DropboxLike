using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;

namespace Server.DBManager
{
    class UserM
    {
    
        SqlDataReader reader;
        SqlCommand command;
     

        
        /**
           * This insertIntoUsers method inserts record into USERS table
        */
        public void insertIntoUsers(SqlConnection connection, String username, String password)
        {
            try
            {
                command = new SqlCommand("INSERT INTO USERS (EMAIL, PASSWORD) VALUES (@EMAIL, @PASSWORD)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                //command.Parameters.AddWithValue("@UID", );
                command.Parameters.AddWithValue("@EMAIL", username);
                command.Parameters.AddWithValue("@PASSWORD", password);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("inserIntoUser method says->" + e.Message);
            }
        }

        public Model.UserModel getUserRecord(SqlConnection connection,String email)
        {
            try
            {
                command = new SqlCommand("SELECT * FROM USERS WHERE EMAIL = @EMAIL", connection);
                command.Parameters.AddWithValue("@EMAIL", email);
                reader = command.ExecuteReader();

                //if no record found, create new user
                if (!reader.HasRows)
                {
                    return null;
                }

                Model.UserModel user = new Model.UserModel();
                user.setEmailId(email);
                //read row by row
                while (reader.Read())
                {
                    user.setUid(reader.GetInt32(0));
                    user.setPassword(reader.GetString(2));
                }//loop
                reader.Close();
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine("getUserRecord in User class says->" + e.Message);
            }
            return null;
        }
    }
}
