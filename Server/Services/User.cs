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
    public class User : Services.IAzure
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
            if ((email.Equals(null)) || (password.Equals(null))) {
                return false;
            }

            Model.UserModel user = new Model.UserModel();
            DBManager.UserM uu = new DBManager.UserM();
            user = uu.getUserRecord(connection, email);
            
            //Email validatation imposed on client side
                        
            //if user doesn't exist, false
             if (user==null)
              {
                  //Console.WriteLine("NEw user");
                  //return createUser(email, password);
                  return false;
              }//if
                
                
                //return true if crediential matches
             if ((user.getPassword()).Equals(password))
              {
                    Console.WriteLine("Match Found!");
                    return true;
              }//if

               Console.WriteLine("invalid creds");

            return false;
        }//validateUser
        

        /// <summary>
        /// This method should return TRUE if user account created successfully
        /// FALSE if user record already exist in database
        /// THE CONTAINER IS NAMED AS USERID [unique identifier] 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Boolean createUser(String username, String password)
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
            u.insertIntoUsers(connection, username, password);
            user = u.getUserRecord(connection, username);

            //default private container created
            //Container get created by the unique user id
            Console.WriteLine("Container created with " + user.getUid() + " name");

            Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer((user.getUid()).ToString());
            myContainer.CreateIfNotExists();

            String containerName = (user.getUid()).ToString();

            //Insert record into RESOURCES table
            DBManager.ResourceM r = new DBManager.ResourceM();
            Boolean res = r.insertIntoResources(connection, user.getUid(), containerName);
            return res;

        }//createUser

        /// <summary>
        /// This method returns a list of strings as follows:
        ///   > if 'path' is empty, a list of all containers that username can access
        ///   > else path format userid:containername, it will list all files in given container
        ///   THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public String[] list(String username, String path) {
            DBManager.ResourceM resource = new DBManager.ResourceM();
            DBManager.UserM user = new DBManager.UserM();
            Model.UserModel u = new Model.UserModel();
            u = user.getUserRecord(connection, username);

            if (String.IsNullOrEmpty(path))
            {
                Console.WriteLine(u.getUid());
                //Get the list of containers user owns
                String[] containers = resource.getContainersOwnedByUser(connection, u.getUid());
                return containers;
            }

            //Break the path <uid:container>
            String[] usercont = path.Split(':');
            Model.UserModel u2 = user.getUserRecord(connection, usercont[0]);
            //int userid = Convert.ToInt32(usercont[0]);
            
            int userid = u2.getUid();
            String container;
            if (usercont.Length == 1)
            {
                container = null;
            }
            else 
            { 
                container = usercont[1];
            }
            //If the user wants to upload on other user's container, Check the container permission for user
            if (u.getUid() != userid)
            {
                DBManager.CListM clist = new DBManager.CListM();
                Boolean res = clist.checkResourcePermission(connection, u.getUid(), container); //ALWAYS RETURN TRUE
                if (res == false)
                {
                    throw new DBLikeExceptions.UnauthorizedAccessException();
                }
            }

            //Obtain reference of user's blob container
            CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(container);

            List<String> blobs = new List<String>();
            //List the files(or blobs) contained by folder(or container)
            foreach (var blobItem in myContainer.ListBlobs())
            {
                Console.WriteLine(blobItem.Uri+",");
                blobs.Add((((blobItem.Uri).LocalPath).Split('/')).Last());
            }

            String[] blobnames = blobs.ToArray();   
            return blobnames;
        }

        /// <summary>
        /// This method takes (username, path{userid:containername}, local path) and uploads the file to blob storage
        /// *if userid is not the same as username, the permission will be checked before uploading
        ///   THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void upload(String username, String path, String localpath){
            try{
                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);

                //Break the path <uid:container>
                String[] usercont = path.Split(':');
                //int userid = Convert.ToInt32(usercont[0]);
                String destinationCont = usercont[0];
                String destinationPath = usercont[1];

                Model.UserModel u2;
                u2 = user.getUserRecord(connection, destinationCont);
                String destContainerID = u2.getUid().ToString();

                //If the user wants to upload on other user's container, Check the container permission for user
                if (!(username.Equals(destinationCont)))
                {
                    DBManager.CListM clist = new DBManager.CListM();
                   ///// CHANGED HERE --- PRESUMABLY SHOULD USE THE LOGGED IN USER ID? ---TUDOR
                    Boolean res = clist.checkResourcePermission(connection, u.getUid(), u2.getUid().ToString()); //ALWAYS RETURN TRUE
                    if (res == false) {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                }

                //Get reference to container
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(destContainerID);

                String[] fn = destinationPath.Split('\\');
                String filename = fn.Last();
                String blobName = String.Format(filename);

                CloudBlockBlob file = myContainer.GetBlockBlobReference(blobName);

                using (var fstream = new FileStream(localpath, FileMode.Open)) {
                    file.UploadFromStream(fstream);
                }
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex){
                Console.WriteLine("upload method in User class says-> "+ex);
            }
        }

        /// <summary>
        /// This method takes (username, path{userid:containername}, local path) and download file from blob storage
        /// to given local path
        /// THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// THROW CLOUDBLOBNOTFOUNDEXCEPTION or CLOUDBLOBNOTFOUNDEXCEPTION IF GIVEN FILEPATH DOESN'T EXISTS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void download(String username, String path, Stream targetStream) {
            try
            {
                //Break the path <uid:container>
                String[] usercont = path.Split(':');

                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);

                //int userid = Convert.ToInt32(usercont[0]);
                String srcUser = usercont[0];
                //String srcPath = usercont[1];

                Model.UserModel u2;
                u2 = user.getUserRecord(connection, srcUser);

                String userid = u2.getUid().ToString();
                String fullpath = usercont[1];
                String blobName = fullpath.Substring(fullpath.IndexOf('/')+1);
                //String container = fullpath.Substring(0, fullpath.IndexOf('/'));
                String container = userid;

                Console.WriteLine("userid: "+userid);
                Console.WriteLine("fullpath: " + fullpath);
                Console.WriteLine("blobname: " + blobName);
                Console.WriteLine("container: " + container);

                //Check if container exists
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(container);
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);
                
                if (isContainerexists==false)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                //Check if blob exists
                CloudBlockBlob myblob = BlobStorageManager.BlobStorage.getCloudBlob(container+'/'+fullpath); //Get reference to blob
                Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(myblob);

                if (isBlobexists==false)
                {
                    Console.WriteLine("Container not found");
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }

                /*
                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u = new Model.UserModel();
                u = user.getUserRecord(connection, username);
                
                //If the user wants to upload on other user's container, Check the container permission for user
                if (u.getUid() != userid)
                {
                    DBManager.CListM clist = new DBManager.CListM();
                    Boolean res = clist.checkResourcePermission(connection, userid, container); //ALWAYS RETURN TRUE
                    
                    if (res == false)
                    {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                }

                */

                Console.WriteLine(myblob);
                myblob.DownloadToStream(targetStream);
               
                // Download the blob to a local file.
                //myblob.DownloadToFile(localpath+"/"+blobName);

                /*
                FileStream stream = new FileStream(localpath + "/" + blobName, FileMode.CreateNew);
                
                myblob.DownloadToStream(stream);

                Console.WriteLine("Downloaded!");
                return stream;
                 * */
            }
            catch(Exception ex)
            {
                //throw new Microsoft.WindowsAzure.Storage.StorageException();
                Console.WriteLine("download method in User class says->"+ex.Message);
                return;
            }
            
        }



        public object download(string username, string path)
        {
            throw new NotImplementedException();
        }

        public bool checkAccess(string username, string path, char mode)
        {
            int idx1 = path.IndexOf(':');
            return username.Equals(path.Substring(0, idx1));
        }
    }//class
}