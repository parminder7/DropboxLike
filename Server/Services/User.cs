using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
//using Microsoft.WindowsAzure.StorageClient;


namespace Server.Services
{
    public class User : Services.IUserManage
    {
        SqlConnection connection;

        DBManager.DBAccess db = new DBManager.DBAccess();

        public User()
        {
            connection = db.getDBAccess();
            Console.WriteLine("Connection setup");
        }//constructor

        /// <summary>
        /// This method returns TRUE if the following hold:
        ///     username exists in the user table
        ///     password matches with the value in the user table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Boolean validateUser(String email, String password)
        {
            if ((email.Equals(null)) || (password.Equals(null)))
            {
                return false;
            }

            Model.UserModel user = new Model.UserModel();
            DBManager.UserM uu = new DBManager.UserM();
            user = uu.getUserRecord(connection, email);

            //Email validatation imposed on client side

            //if user doesn't exist, false
            if (user == null)
            {
                return false;
            }


            //return true if crediential matches
            if ((user.getPassword()).Equals(password))
            {
                Console.WriteLine("Match Found!");
                return true;
            }

            Console.WriteLine("invalid creds");

            return false;
        }

        /// <summary>
        /// This method findUserId method returns id of given username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public int findUserId(string username)
        {
            DBManager.UserM u = new DBManager.UserM();
            Model.UserModel user = u.getUserRecord(connection, username);
            if (user == null)
                return -1;
            return user.getUid();
        }

        /// <summary>
        /// This method should return TRUE if user account created successfully
        /// FALSE if user record already exist in database
        /// THE CONTAINER IS NAMED AS USERID [unique identifier] 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Boolean createUser(String fullname, String username, String password)
        {
            Model.UserModel user = new Model.UserModel();
            DBManager.UserM u = new DBManager.UserM();
            user = u.getUserRecord(connection, username);

            //if user already exist, false
            if (!(user == null))
            {
                return false;
            }

            //Insert record into USERS table
            u.insertIntoUsers(connection, fullname, username, password);
            user = u.getUserRecord(connection, username);

            //default private container created
            //Container get created by the unique user id
            Console.WriteLine("Container created with " + user.getUid() + " name");

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer((user.getUid()).ToString());
            myContainer.CreateIfNotExists();

            String containerName = (user.getUid()).ToString();

            //Insert record into CONTAINERS table
            DBManager.ResourceM r = new DBManager.ResourceM();
            Console.WriteLine(containerName + "////" + user.getUid());
            Model.AzureContainerModel re = new Model.AzureContainerModel();
            re.setOwner(user.getUid());
            re.setContainerName(containerName);
            re.setGivenName(user.getEmailId());  //Changed here since server front end is considering private container name as user email id
            Boolean res = r.insertIntoResources(connection, re);
            return res;
        }
    }
}