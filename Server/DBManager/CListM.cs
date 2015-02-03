using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;

//STATUS : One method named getSharedContainersOwnedByUser not yet implemented --CLOSED

namespace Server.DBManager
{
    class CListM
    {
        SqlDataReader reader;
        SqlCommand command;
        
        /// <summary>
        /// This method insert record into CLIST table
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cm"></param>
        /// <returns></returns>
        public Boolean insertIntoCList(SqlConnection connection, Model.CListModel cm)
        {
            try
            {
                command = new SqlCommand("INSERT INTO CLIST (UID, RID, READWRITE, OWNER) VALUES (@UID, @RID, @READWRITE, @OWNER)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", cm.getUid());
                command.Parameters.AddWithValue("@RID", cm.getRid());
                command.Parameters.AddWithValue("@OWNER", cm.getOwner());
                command.Parameters.AddWithValue("@READWRITE", cm.getReadwrite());
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("inserIntoCList method says->" + e.Message);
            }
            return false;
        }

        /// <summary>
        /// This method checks the user's permission on particular container
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="containerName"></param>
        /// <returns>NIL/READ/WRITE</returns>
        public String checkResourcePermission(SqlConnection connection, int uid, String containerName)
        {
            ResourceM resource = new ResourceM();
            Model.AzureContainerModel r = new Model.AzureContainerModel();
            r = resource.getResourceByContainerName(connection, containerName);
            int resourceID = r.getRid();
            try
            {
                command = new SqlCommand("SELECT READWRITE FROM CLIST WHERE RID = @RID AND UID = @UID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", uid);
                command.Parameters.AddWithValue("@RID", resourceID);
                reader = command.ExecuteReader();

                //If no record is available; it means no permission is granted to given user on given container
                if (!reader.HasRows)
                {
                    return "NIL";
                }

                int permission = 0;
                while (reader.Read())
                {
                    permission = reader.GetInt32(0);
                }
                reader.Close();

                if (permission == 0)
                {
                    return "READ";
                }
                else
                {
                    return "WRITE";
                }
            }
            catch (SqlException ex) 
            {
                Console.WriteLine("CListM class: checkResourcePermission method exception->"+ex.Message);
            }
            return "NIL";
        }

        /// <summary>
        /// This method returns the list of containers which are shared with given user
        /// each element seems like "CONTAINERNAME:OWNERFULLNAME:GIVENCONTAINERNAME" //User friendly manner
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userid"></param>
        /// <returns>null/string array contains the name of containers</returns>
        public String[] getSharedContainersWithUser(SqlConnection connection, int userid) {
            String[] sharedContainers = null;
            try {
                command = new SqlCommand("SELECT RID, OWNER FROM CLIST WHERE UID = @UID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", userid);
                reader = command.ExecuteReader();

                //no containers shared with user
                if (!reader.HasRows)
                {
                    return null;
                }

                ResourceM r = new ResourceM();
                UserM u = new UserM();
                int currRid = 0;
                String ownerName = null;
                Model.AzureContainerModel cont = new Model.AzureContainerModel();
                List<String> containersString = new List<String>();
                while (reader.Read())
                {
                    currRid = reader.GetInt32(0);
                    cont = r.getResourceById(connection, currRid);
                    Console.WriteLine("Owner: " + reader.GetInt32(1));
                    ownerName = (u.getUserRecordByID(connection, (reader.GetInt32(1)))).getFullName();
                    containersString.Add((cont.getContainerName())+":"+ownerName+":"+(cont.getGivenName()));
                }
                reader.Close();

                sharedContainers = containersString.ToArray();
                return sharedContainers;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("CListM class: getSharedContainersWithUser method Exception->"+ex.Message);
            }
            return null;
        }

        /// <summary>
        /// This method returns the list of containers which are shared by given ownerid
        /// each element seems like "CONTAINERNAME:GIVENNAME" //User friendly manner
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ownerid"></param>
        /// <returns></returns>
        public String[] getSharedContainersOwnedByUser(SqlConnection connection, int ownerid)
        { 
            String[] sharedContainers = null;
            try {
                command = new SqlCommand("SELECT RID FROM CLIST WHERE OWNER = @OWNER");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@OWNER", ownerid);
                reader = command.ExecuteReader();
                
                //no containers shared by ownerid
                if (!reader.HasRows)
                {
                    return null;
                }

                ResourceM r = new ResourceM();
                
                int currRid = 0;
                List<String> containersString = new List<String>();
                while (reader.Read())
                {
                    currRid = reader.GetInt32(0);
                    containersString.Add(((r.getResourceById(connection, currRid)).getContainerName()) + ":" + ((r.getResourceById(connection, currRid)).getGivenName()));
                }
                reader.Close();
                foreach (string author in containersString)
                {
                    Console.WriteLine(author);
                }
                sharedContainers = containersString.ToArray();
                return sharedContainers;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("CListM class: getSharedContainersOwnedByUser method Exception->" + ex.Message);
            }
            return null;
            
        }

        /// <summary>
        /// This getSharedResourceByGivenName() method returns the shared folder model of givenname and uid
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="givenName"></param>
        /// <returns></returns>
        public Model.CListModel getSharedResourceByGivenName(SqlConnection connection, int uid, String givenName)
        {
            try
            {
                command = new SqlCommand("SELECT C.RID, C.UID, C.OWNER, C.READWRITE FROM CLIST C, CONTAINERS CO WHERE C.UID = @UID AND CO.GIVENNAME = @GIVENNAME AND C.RID = CO.RID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", uid);
                command.Parameters.AddWithValue("@GIVENNAME", givenName);
                reader = command.ExecuteReader();

                //no containers shared by ownerid
                if (!reader.HasRows)
                {
                    return null;
                }
                Model.CListModel cl = new Model.CListModel();

                while (reader.Read())
                {
                    cl.setRid(reader.GetInt32(0));
                    cl.setUid(reader.GetInt32(1));
                    cl.setOwner(reader.GetInt32(2));
                    cl.setReadwrite(reader.GetInt32(3));
                }
                
                reader.Close();
                return cl;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("CLISTM class getSharedResouceByGivenName method exception->" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// This deleteEntryFromCLIST() method deletes the entry from the CLIST as per
        /// the given userid and containerID
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userid"></param>
        /// <param name="containerID"></param>
        /// <returns></returns>
        public Boolean deleteUserEntryFromCLIST(SqlConnection connection, int userid, int containerID)
        {
            try
            {
                if (userid == 0)    //delete all records witth respect to given containerID
                {
                    command = new SqlCommand("DELETE FROM CLIST WHERE RID = @RID");
                    command.CommandType = CommandType.Text;
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@RID", containerID);
                }
                else
                {
                    command = new SqlCommand("DELETE FROM CLIST WHERE UID = @UID AND RID = @RID");
                    command.CommandType = CommandType.Text;
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@UID", userid);
                    command.Parameters.AddWithValue("@RID", containerID);
                }
                
                command.ExecuteNonQuery();
                return true;
            }
            catch(SqlException ex)
            {
                Console.WriteLine("CLISTM class deleteUserEntryFromCLIST method exception->"+ex.Message);
                return false;
            }
        }

        /// <summary>
        /// This isRecordExistsForContainID() method checks whether any record exists for given containerID
        /// which means it check whether any permission is granted to other user on given container
        /// </summary>
        /// <returns></returns>
        public Boolean isRecordExistsForContainerID(SqlConnection connection, int containerID)
        {
            try
            {
                command = new SqlCommand("SELECT * FROM CLIST WHERE RID = @RID");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@RID", containerID);
                reader = command.ExecuteReader();
                             
                //no containers shared by ownerid
                int count = 0;
                while(reader.Read())
                {
                    count++;
                }
                if (count == 0) { return false; }
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("CLISTM class isRecordExistsForContainerID method exception->" + ex.Message);
                return false;
            }
        }
    }
}
