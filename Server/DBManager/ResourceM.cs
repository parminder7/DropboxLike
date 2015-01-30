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
    class ResourceM
    {
        
        SqlDataReader reader;
        SqlCommand command;

        /// <summary>
        /// This method inserts record to the RESOURCES table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public Boolean insertIntoResources(SqlConnection connection, int uid, String containerName)
        {
            try
            {
                //---Should have used auto-increment 
                command = new SqlCommand("SELECT MAX(RID) FROM RESOURCES", connection);
                reader = command.ExecuteReader();
                int rid = 0;

                while (reader.Read())
                {
                    rid = reader.GetInt32(0);
                }
                rid = rid + 1;

                command = new SqlCommand("INSERT INTO RESOURCES (RID, CONTAINER) VALUES (@RID, @CONTAINER)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@RID", rid);
                command.Parameters.AddWithValue("@CONTAINER", containerName);
                command.ExecuteNonQuery();

                Model.CListModel cm = new Model.CListModel();
                cm.setRid(rid);
                cm.setUid(uid);
                cm.setReadonly(1);
                cm.setReadwrite(1);
                reader.Close();
                CListM c = new CListM();

                //on adding resource, the default will be given to resource {DEFAULT(READ, READWRITE) = True}
                return c.insertIntoCList(connection, cm);
            }
            catch (Exception e)
            {
                Console.WriteLine("insertIntoResources in class User says->" + e.Message);
            }
            reader.Close();
            return false;
        }

        /// <summary>
        /// This method returns the array of containers owned by user.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public String[] getContainersOwnedByUser(SqlConnection connection, int userid) {
            String[] containers = null;
            try
            {
                command = new SqlCommand("SELECT CONTAINER FROM RESOURCES WHERE RID IN (SELECT RID FROM CLIST WHERE UID = @UID)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", userid);
                reader = command.ExecuteReader();
                
                //If no container is owned by user or we can say user has deleted his container
                if (!reader.HasRows)
                {
                    return null;
                }
                
                List<String> containersString = new List<String>();
                
                //read row by row
                while (reader.Read())
                {
                    containersString.Add(reader.GetString(0));
                }//loop
                reader.Close();

                containers = containersString.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("getContainersOwnedByUser in ResourceM class says->" + e.Message);
            }
            return containers;
        }
    }
}
