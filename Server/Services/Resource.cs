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

namespace Server.Services
{
    public class Resource : Services.IResourceManage
    {
        SqlConnection connection;
       
        DBManager.DBAccess db = new DBManager.DBAccess();

        public Resource()
        {
            connection = db.getDBAccess();
            Console.WriteLine("Connection setup");
        }

        /// <summary>
        /// This method return the containerID by givenname and uid
        /// returns rid only when uid is listed corresponding to given container name 
        /// else it is considered that there exists no such container for userid
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="givenname"></param>
        /// <returns>rid/-1</returns>
        public int getContainerID(int userid, String givenname) 
        {
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            
            try
            {
                Model.AzureContainerModel resource = new Model.AzureContainerModel();
                resource = resmgr.getResourceByGivenName(connection, userid, givenname);

                if (resource == null)
                {
                    Model.CListModel cl = new Model.CListModel();
                    DBManager.CListM cmgr = new DBManager.CListM();

                    cl = cmgr.getSharedResourceByGivenName(connection, userid, givenname);

                    if (cl == null)
                    {
                        return -1;
                    }

                    return cl.getRid();
                }
                else 
                {
                    return resource.getRid();
                }
            }

            catch (DBLikeExceptions.CloudContainerNotFoundException)
            {
                return -1;
            }
        }

        /// <summary>
        /// This createSharedContainer() method creates the shared container which is owned by given userid
        /// CloudContainerAlreadyExistException if already exists
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="givenname"></param>
        /// <returns>id for new container on success, else -1</returns>
        public int createSharedContainer(int userid, String givenname)
        {
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            Model.AzureContainerModel resource = new Model.AzureContainerModel();

            try
            {
                resource = resmgr.getResourceByGivenName(connection, userid, givenname);
                Model.UserModel user = new Model.UserModel();
                DBManager.UserM umgr = new DBManager.UserM();
                user = umgr.getUserRecordByID(connection, userid);
                File f = new File();
                String[] list = f.list(user.getEmailId(), "");
                
                for (int i = 0; i < list.Length; i++) 
                {
                    String[] tokens = list[i].Split(':');
                    if (tokens.Length == 3)
                    {
                        if (tokens[2].Equals(givenname))  //there exist the container with same name
                        {
                            throw new DBLikeExceptions.CloudContainerAlreadyExistException();
                        }
                    }
                    else if (tokens.Length == 2)
                    {
                        if (tokens[1].Equals(givenname))  //there exist the container with same name
                        {
                            throw new DBLikeExceptions.CloudContainerAlreadyExistException();
                        }
                    }
                }

                if (resource != null)
                {
                   throw new DBLikeExceptions.CloudContainerAlreadyExistException();
                }

                DBManager.CListM cmgr = new DBManager.CListM();

                //cmgr.insertIntoCList();
                String[] sharedContainers = cmgr.getSharedContainersOwnedByUser(connection, userid);

                int counter = 001;
                //Console.WriteLine("shareddddddddd"+sharedContainers.Length);

                if (sharedContainers != null)
                {
                    int size = sharedContainers.Length;
                    String[] containerSuffix = new String[size];
                    String[] temp;
                    //SHOULD NOT ALLOW USER TO GIVE SAME NAME TO DIFFERENT SHARED CONTAINER
                    for (int i = 0; i < size; i++)
                    {
                        temp = sharedContainers[i].Split(':');
                        int pos = temp[0].IndexOf("d") + 1;
                        Console.WriteLine("Position" + pos);
                        String scnt = temp[0].Substring(pos);
                        containerSuffix[i] = new string(scnt.Where(char.IsDigit).ToArray());    //no need but leaving as it is
                    }
                    var result = (from m in containerSuffix select m).Max();
                    Console.WriteLine("result"+result);
                    
                    counter = Int32.Parse(result) + 1;
                    Console.WriteLine("New counter--" + counter);
                }

                String containerName = userid + "shared" + counter;
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(containerName);
                myContainer.CreateIfNotExists();

                Model.AzureContainerModel resource1 = new Model.AzureContainerModel();
                //Insert record into CONTAINERS table
                resource1.setOwner(userid);
                resource1.setContainerName(containerName);
                resource1.setGivenName(givenname);
                Boolean res = resmgr.insertIntoResources(connection, resource1);
                if (res)
                {
                    int rid1 = getContainerID(userid, givenname);
                    return rid1;
                }
                return -1;
            }
            catch (SqlException)
            {
                return -1;
            }
        }

        /// <summary>
        /// This deleteSharedContainer() method just deletes container as per given containerID 
        /// which means pre-check is required to call this method
        /// </summary>
        /// <param name="containerID"></param>
        /// <returns>true/false/exception</returns>
        public Boolean deleteSharedContainer(int containerID)
        { 
            //throw new NotImplementedException();
            //Console.WriteLine("DeleteSharedCOntainer NYI");
            try
            {
                Model.AzureContainerModel res = new Model.AzureContainerModel();
                DBManager.ResourceM rmgr = new DBManager.ResourceM();
                res = rmgr.getResourceById(connection, containerID);

                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(res.getContainerName()); //Container ref
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);

                if (isContainerexists == false)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                if (myContainer.ListBlobs().GetEnumerator().MoveNext())
                {
                    throw new ArgumentException("container not empty");
                }
                myContainer.Delete();

                //Delete from database
                DBManager.CListM cmgr = new DBManager.CListM();
                if (cmgr.deleteUserEntryFromCLIST(connection, 0, containerID)) //delete all records from CLIST
                {
                    if (rmgr.deleteContainerEntryFromCONTAINERS(connection, containerID.ToString())) //delete from CONTAINERS
                    {
                        return true;
                    }
                }
                
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException e)
            {
                Console.WriteLine("Resource:deleteSharedContainer Exception:"+e.Message);
            }
            return false;
        }

        /// <summary>
        /// This grantRights() method set the rights to the otheruserID
        /// </summary>
        /// <param name="otheruserID"></param>
        /// <param name="containerID"></param>
        /// <param name="writeAccess"></param>
        /// <returns>true/false</returns>
        public Boolean grantRights(int otheruserID, int containerID, Boolean writeAccess)
        {
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            Model.AzureContainerModel rmodel = new Model.AzureContainerModel();
            Model.CListModel cmodel = new Model.CListModel();
             
            try
            {
                
                rmodel = resmgr.getResourceById(connection, containerID);
                DBManager.CListM cmgr = new DBManager.CListM();
                Model.CListModel cmodel1 = new Model.CListModel();
                cmodel1 = cmgr.getSharedResourceByGivenName(connection, otheruserID, rmodel.getGivenName());
                
                if(cmodel1 != null)
                {
                    //If permission already granted to otheruser, then check if requested permission is same
                    Boolean test = false;
                    if (cmodel1.getReadwrite() == 1)
                    {
                        test = true;
                    }
                    if (test == writeAccess)
                    {
                        return true;    //already exist with same permission
                    }
                    else 
                    {
                        cmgr.deleteUserEntryFromCLIST(connection, otheruserID, containerID); //delete the record and re-enter
                    }
                }

                cmodel.setRid(containerID);
                cmodel.setUid(otheruserID);
                cmodel.setOwner(rmodel.getOwner());
                if (writeAccess) 
                {
                    cmodel.setReadwrite(1);
                }
                else
                {
                    cmodel.setReadwrite(0);
                }
                
                //Insert into CLIST table
                bool success = cmgr.insertIntoCList(connection, cmodel);

                return success;
            }
            catch (DBLikeExceptions.CloudContainerNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// This removeRights() method revoke the user right by deleting the record from CLIST table
        /// </summary>
        /// <param name="otheruserID"></param>
        /// <param name="containerID"></param>
        public void removeRights(int otheruserID, int containerID)
        {
            DBManager.CListM cmgr = new DBManager.CListM();

            try
            {
                cmgr.deleteUserEntryFromCLIST(connection, otheruserID, containerID);
                
                //CHECK if no user exists in cLIST corresponding to given container
                //if so delete container and omit record form RESOURCE table as well
                Console.WriteLine("ress"+cmgr.isRecordExistsForContainerID(connection, containerID));
                if(!cmgr.isRecordExistsForContainerID(connection, containerID)) //if no other user is sharing this container
                {
                    DBManager.ResourceM rmgr = new DBManager.ResourceM();
                    
                    if (deleteSharedContainer(containerID)) 
                    {
                        //delete container and entry
                        rmgr.deleteContainerEntryFromCONTAINERS(connection, containerID.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Resource class removeRights method Exception->"+e.Message);
            }
        }

        /// <summary>
        /// This isOwner() method returns true if given user is owner otherwise false 
        /// FALSE: if container doesn't exit or user is not owner
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="containerID"></param>
        /// <returns></returns>
        public Boolean isOwner(int userID, int containerID)
        {
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            Model.AzureContainerModel rmodel = new Model.AzureContainerModel();
            try 
            {
                rmodel = resmgr.getResourceById(connection, containerID);
                if (userID == rmodel.getOwner())
                {
                    return true;
                }
            }
            catch (DBLikeExceptions.CloudContainerNotFoundException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// This canRead() method returns true if user can read else false
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="containerID"></param>
        /// <returns></returns>
        public Boolean canRead(int userID, int containerID)
        {
            DBManager.CListM cmgr = new DBManager.CListM();
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            Model.AzureContainerModel rmodel = new Model.AzureContainerModel();
            try
            {
                rmodel = resmgr.getResourceById(connection, containerID);
                String permission = cmgr.checkResourcePermission(connection, userID, rmodel.getContainerName());

                if ((permission.Equals("READ")) || (permission.Equals("WRITE")))
                {
                    return true;
                }
            }
            catch (DBLikeExceptions.CloudContainerNotFoundException)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// This canWrite() method return true if user has write permission
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="containerID"></param>
        /// <returns></returns>
        public Boolean canWrite(int userID, int containerID)
        {
            if (isOwner(userID, containerID))
            {
                return true;
            }
            DBManager.CListM cmgr = new DBManager.CListM();
            DBManager.ResourceM resmgr = new DBManager.ResourceM();
            Model.AzureContainerModel rmodel = new Model.AzureContainerModel();
            try
            {
                rmodel = resmgr.getResourceById(connection, containerID);
                String permission = cmgr.checkResourcePermission(connection, userID, rmodel.getContainerName());

                if (permission.Equals("WRITE"))
                {
                    return true;
                }
            }
            catch (DBLikeExceptions.CloudContainerNotFoundException)
            {
                return false;
            }
            return false;
        }
    }
}
