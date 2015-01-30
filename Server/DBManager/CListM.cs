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
    class CListM
    {
        
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
                command = new SqlCommand("INSERT INTO CLIST (UID, RID, READONLY, READWRITE) VALUES (@UID, @RID, @READONLY, @READWRITE)");
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                command.Parameters.AddWithValue("@UID", cm.getUid());
                command.Parameters.AddWithValue("@RID", cm.getRid());
                command.Parameters.AddWithValue("@READONLY", cm.getReadonly());
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
        /// <returns></returns>
        public Boolean checkResourcePermission(SqlConnection connection, int uid, String containerName){

            //NOT YET IMPLEMENTED

            return true;
        }
    }
}
