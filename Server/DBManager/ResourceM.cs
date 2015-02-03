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
        /// This method inserts record to the CONTAINERS table.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="containerName"></param>
        /// <returns>true/false</returns>
        public Boolean insertIntoResources(SqlConnection connection, Model.AzureContainerModel resource)
        {
            try
            {
                //---Should have used auto-increment 
                command = new SqlCommand("SELECT MAX(RID) FROM CONTAINERS", connection);
                
                reader = command.ExecuteReader();
                int rid = 10000;    //starting resource id from 10001

                if (!reader.HasRows)
                {
                    rid = rid + 1;
                }
                else
                {
                    while (reader.Read())
                    {
                        rid = reader.GetInt32(0);
                    }
                    rid = rid + 1;
                }
                Console.WriteLine("Resource ID"+rid);
                command = new SqlCommand("INSERT INTO CONTAINERS (RID, GIVENNAME, OWNER, CONTAINERNAME) VALUES (@RID, @GIVENNAME, @OWNER, @CONTAINERNAME)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@RID", rid);
                command.Parameters.AddWithValue("@GIVENNAME", resource.getGivenName());
                command.Parameters.AddWithValue("@OWNER", resource.getOwner());
                command.Parameters.AddWithValue("@CONTAINERNAME", resource.getContainerName());
                command.ExecuteNonQuery();
                
                /* Following table retains the entry of user-resource which is sharing
                Model.CListModel cm = new Model.CListModel();
                cm.setRid(rid);
                cm.setUid(uid);
                cm.setReadwrite(1);
                reader.Close();
                CListM c = new CListM();

                //on adding resource, the default will be given to resource {DEFAULT(READ, READWRITE) = True}
                return c.insertIntoCList(connection, cm);
                 */
            }
            catch (Exception e)
            {
                Console.WriteLine("insertIntoResources in class User says->" + e.Message);
                return false;
            }
            reader.Close();
            return true;
        }

        /// <summary>
        /// This method returns the array of containers owned by user. <CONTAINERNAME:GIVENNAME>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public String[] getContainersOwnedByUser(SqlConnection connection, int userid) {
            String[] containers = null;
            try
            {
                command = new SqlCommand("SELECT CONTAINERNAME, GIVENNAME FROM CONTAINERS WHERE OWNER = @OWNER");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@OWNER", userid);
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
                    containersString.Add(reader.GetString(0)+":"+reader.GetString(1));
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

        /// <summary>
        /// This getResourceByContainerName() method returns the resourcemodel of given container name(unique)
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="containerName"></param>
        /// <returns>resource id</returns>
        public Model.AzureContainerModel getResourceByContainerName(SqlConnection connection, String containerName) 
        {
            Model.AzureContainerModel resource = new Model.AzureContainerModel();
            try
            {
                command = new SqlCommand("SELECT RID, GIVENNAME, OWNER, CONTAINERNAME FROM CONTAINERS WHERE CONTAINERNAME = @CONTAINERNAME");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@CONTAINERNAME", containerName);
                reader = command.ExecuteReader();

                //If no container is owned by user or we can say user has deleted his container
                if (!reader.HasRows)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                while (reader.Read())
                {
                    resource.setRid(reader.GetInt32(0));
                    resource.setGivenName(reader.GetString(1));
                    resource.setOwner(reader.GetInt32(2));
                    resource.setContainerName(reader.GetString(3));
                }
                reader.Close();
                return resource;
            }
            catch (Exception e)
            {
                Console.WriteLine("getResourceByContainerName in ResourceM class says->" + e.Message);
            }
            return null;
        }

        /// <summary>
        /// This getResourceById() method returns the resourcemodel by given resource id
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="rid"></param>
        /// <returns>container name</returns>
        public Model.AzureContainerModel getResourceById(SqlConnection connection, int rid) 
        {
            Model.AzureContainerModel resource = new Model.AzureContainerModel();
            try
            {
                command = new SqlCommand("SELECT RID, GIVENNAME, OWNER, CONTAINERNAME FROM CONTAINERS WHERE RID = @RID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@RID", rid);
                reader = command.ExecuteReader();

                //If no container is owned by user or we can say user has deleted his container
                if (!reader.HasRows)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                while (reader.Read())
                {
                    resource.setRid(reader.GetInt32(0));
                    resource.setGivenName(reader.GetString(1));
                    resource.setOwner(reader.GetInt32(2));
                    resource.setContainerName(reader.GetString(3));
                }
                reader.Close();
                return resource;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("ResourceM class: getResourceById method Exception->"+ex.Message);
            }
            return null;
        }

        /// <summary>
        /// This getResourceByGivenName() method returns the resourcemodel by given container name and its owner
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="givenName"></param>
        /// <returns>Model.ResourceModel</returns>
        public Model.AzureContainerModel getResourceByGivenName(SqlConnection connection, int uid, String givenName)
        {
            Model.AzureContainerModel resource = new Model.AzureContainerModel();
            try
            {
                command = new SqlCommand("SELECT RID, GIVENNAME, OWNER, CONTAINERNAME FROM CONTAINERS WHERE OWNER = @OWNER AND GIVENNAME = @GIVENNAME");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@OWNER", uid);
                command.Parameters.AddWithValue("@GIVENNAME", givenName);
                reader = command.ExecuteReader();

                //If no container is owned by user or we can say user has deleted his container
                if (!reader.HasRows)
                {
                    return null;
                }

                while (reader.Read())
                {
                    resource.setRid(reader.GetInt32(0));
                    resource.setGivenName(reader.GetString(1));
                    resource.setOwner(reader.GetInt32(2));
                    resource.setContainerName(reader.GetString(3));
                }
                reader.Close();
                return resource;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("ResourceM class: getResourceByGivenname method Exception->" + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// This deleteContainerEntryFromCONTAINERS() deletes the record from CONTAINERS table as per given containerid
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="containerID"></param>
        /// <returns>true/false</returns>
        public Boolean deleteContainerEntryFromCONTAINERS(SqlConnection connection, String containerID)
        {
            try
            {
                command = new SqlCommand("DELETE FROM CONTAINERS WHERE RID = @RID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@RID", containerID);
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("ResourceM class deleteContainerEntryFromCONTAINERS method exception->" + ex.Message);
                return false;
            }
        }
        
    }
}
