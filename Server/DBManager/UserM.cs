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
        public void insertIntoUsers(SqlConnection connection, String fullname, String username, String password)
        {
            try
            {
                command = new SqlCommand("INSERT INTO USERS (EMAIL, PASSWORD, FULLNAME) VALUES (@EMAIL, @PASSWORD, @FULLNAME)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                //command.Parameters.AddWithValue("@UID", );
                command.Parameters.AddWithValue("@EMAIL", username);
                command.Parameters.AddWithValue("@PASSWORD", password);
                command.Parameters.AddWithValue("@FULLNAME", fullname);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("inserIntoUser method says->" + e.Message);
            }
        }

        /// <summary>
        /// This getUserRecord() method returns user record by given email id
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="email"></param>
        /// <returns>usermodel object</returns>
        public Model.UserModel getUserRecord(SqlConnection connection, String email)
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

                int count = 0;

                Model.UserModel user = new Model.UserModel();
                user.setEmailId(email);
                //read row by row
                while (reader.Read())
                {
                    count++;
                    user.setUid(reader.GetInt32(0));
                    user.setPassword(reader.GetString(2));
                }//loop
                reader.Close();

                if (count == 0) { return null; }
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine("getUserRecord in User class says->" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// This method returns the name of the given userid
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Model.UserModel getUserRecordByID(SqlConnection connection, int uid)
        {
            Model.UserModel user = new Model.UserModel();
            try
            {
                command = new SqlCommand("SELECT UID, EMAIL, PASSWORD, FULLNAME FROM USERS WHERE UID = @UID", connection);
                command.Parameters.AddWithValue("@UID", uid);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    user.setUid(reader.GetInt32(0));
                    user.setEmailId(reader.GetString(1));
                    user.setPassword(reader.GetString(2));
                    user.setFullName(reader.GetString(3));
                }
                reader.Close();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("UserM class getFullname method Exception->"+ex.Message);
            }
            return user;
        }
    }
}
