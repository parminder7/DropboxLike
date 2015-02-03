using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class File : Services.IFileManage
    {
        SqlConnection connection;
       
        DBManager.DBAccess db = new DBManager.DBAccess();

        public File()
        {
            connection = db.getDBAccess();
            Console.WriteLine("Connection setup");
        }

        /// <summary>
        /// This method returns a list of strings as follows:
        ///   > if 'path' is empty, a list of all containers that username can access
        ///   > else path format userid:containername, it will list all files in given container
        ///   THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public String[] list(String emailID, String givencontainerName)  //path containerName
        {
            DBManager.ResourceM resource = new DBManager.ResourceM();
            DBManager.UserM user = new DBManager.UserM();
            Model.UserModel u = new Model.UserModel();
            u = user.getUserRecord(connection, emailID);

            if (String.IsNullOrEmpty(givencontainerName))
            {
                Console.WriteLine("LIST ALL CONTAINERS FOR {0}", u.getUid());
                DBManager.CListM cl = new DBManager.CListM();
                String[] sharedContainers = null;
                sharedContainers = cl.getSharedContainersWithUser(connection, u.getUid());//CONTAINERNAME:OWNERFULLNAME:GIVENCONTAINERNAME
                //Get the list of containers user owns
                String[] containers = null;
                containers = resource.getContainersOwnedByUser(connection, u.getUid()); //<CONTAINERNAME:GIVENNAME>
                Console.WriteLine("Total containers OWNED BY user : {0}", (containers.Length));
                //Console.WriteLine("Total containers SHARED WITH user: {0}", sharedContainers.Length); //sometimes cause problem no share container exists
                String[] allContainers;

                if (sharedContainers == null)
                {
                    allContainers = new String[containers.Length];
                    containers.CopyTo(allContainers, 0);
                }
                else 
                {
                    allContainers = new String[containers.Length + sharedContainers.Length];
                    containers.CopyTo(allContainers, 0);
                    sharedContainers.CopyTo(allContainers, containers.Length);
                }
                
                return allContainers;
            }

            Console.WriteLine("GIVEN CONT: " + givencontainerName + " uid: " + u.getUid());

            string containerName;
            if (givencontainerName.Equals(emailID)) //Considering this new change, I'm going to map containername with user email id
            {
                containerName = u.getUid().ToString();
            }
            else
            {
                Resource r = new Resource();
                int containerID = r.getContainerID(u.getUid(), givencontainerName);
                if (containerID == -1)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }
                DBManager.ResourceM rmgr = new DBManager.ResourceM();

                Model.AzureContainerModel cont = rmgr.getResourceById(connection, containerID);
                if (cont == null) 
                {
                    return new String[0];
                }
                containerName = cont.getContainerName();
            }



            Console.WriteLine(containerName);
            //Obtain reference of user's blob container
            CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(containerName);

            List<String> blobs = new List<String>();
  
            //List the files(or blobs) contained by folder(or container)

            foreach (var blobItem in myContainer.ListBlobs())
            {
                //Console.WriteLine(blobItem.Uri+",");
                if (blobItem is CloudBlobDirectory)
                {
                    addRecursive((CloudBlobDirectory)blobItem, blobs);
                }
                else
                {
                    addFile(blobItem.Uri.LocalPath, blobs);                    
                }
            }

            String[] blobnames = blobs.ToArray();   
            return blobnames;
        }

        private void addRecursive(CloudBlobDirectory d, List<String> blobList)
        {
            foreach (var blobItem in d.ListBlobs())
            {
                if (blobItem is CloudBlobDirectory)
                {
                    addRecursive((CloudBlobDirectory)blobItem, blobList);
                }
                else
                {
                    addFile(blobItem.Uri.LocalPath, blobList);
                }
                 //.Split('/')).Last());
            }
        }
        private void addFile(string path, List<String> blobList)
        {
            string[] tokens = path.Split(new char[] { '/' }, 3);
            if (tokens.Length != 3)
            {
                throw new ArgumentException("bad resource URI {0}", path);
            }
            blobList.Add(tokens[2]);
        }
        /// <summary>
        /// This getUserCloudFileSystem() method returns the dictionary having 
        /// key as container and values as set of blob files.
        /// For getting containers: arguments are (emailid, "")
        /// For containers owned by used: arguments are (emailid, givencontname)
        /// For conts shared with user: arguments are (emailid, contaname)
        /// </summary>
        /// <param name="emailID"></param>
        /// <returns></returns>
        public Dictionary<String, Dictionary<String, Model.BlobFileModel>> getUserCloudFileSystem(String emailID)
        {
            Dictionary<String, Dictionary<String, Model.BlobFileModel>> listing = new Dictionary<String, Dictionary<String, Model.BlobFileModel>>();
            String[] containers = list(emailID, "");

            
            BlobStorageManager.BlobFileHandler fhandler = new BlobStorageManager.BlobFileHandler();
            for (int ii = 0; ii < containers.Length; ii++)
            {
                Console.WriteLine(containers[ii]);
            }

            for (int i = 0; i < containers.Length; i++)
            {
                Dictionary<String, Model.BlobFileModel> fileDetail = new Dictionary<String, Model.BlobFileModel>();
                String[] token = containers[i].Split(':');
                if (token.Length == 2) 
                {
                    String[] files = list(emailID, token[1]);
                    for (int x = 0; x < files.Length; x++)
                    {
                        Model.BlobFileModel fm = new Model.BlobFileModel();
                        fm.setLastmodifiedTime(fhandler.getBlobLastModifiedTime(token[0], files[x]));
                        fm.setSize(fhandler.getBlobSize(token[0], files[x]));
                        fm.seteTagForVersionControl(fhandler.getCurrentVersion(token[0], files[x]));
                        fm.setMD5HashValue(fhandler.getBlobMD5HashValue(token[0], files[x]));
                        fm.setDeleted(fhandler.getIsBlobDeleted(token[0], files[x]));
                        fileDetail.Add(files[x], fm);
                        Console.WriteLine("File: {0} Values: {1} {2} {3} hh{4}", files[x], fm.getSize(), fm.getLastmodifiedTime(), fm.geteTagForVersionControl(), fm.getMD5HashValue());
                    }
                    Console.WriteLine("Added: key: {0}", token[1]);
                    listing.Add(token[1], fileDetail);                
                }
                else if (token.Length == 3)
                {
                    String[] filess = list(emailID, token[2]);
                    for (int f = 0; f < filess.Length; f++) 
                    {
                        Model.BlobFileModel fm = new Model.BlobFileModel();
                        fm.seteTagForVersionControl(fhandler.getCurrentVersion(token[0], filess[f]));
                        fm.setLastmodifiedTime(fhandler.getBlobLastModifiedTime(token[0], filess[f]));
                        fm.setSize(fhandler.getBlobSize(token[0], filess[f]));
                        fm.setMD5HashValue(fhandler.getBlobMD5HashValue(token[0], filess[f]));
                        fm.setDeleted(fhandler.getIsBlobDeleted(token[0], filess[f]));
                        fileDetail.Add(filess[f], fm);
                        Console.WriteLine("File: {0} Values: {1} {2} {3} hh{4}", filess[f], fm.getSize(), fm.getLastmodifiedTime(), fm.geteTagForVersionControl(), fm.getMD5HashValue());
                    }
                    Console.WriteLine("Added: key: {0}", token[1]);
                    listing.Add(token[2], fileDetail);
                }
                //fileDetail.Clear();
            }

            return listing;
        }
        /// <summary>
        /// This method takes (path{containerID:path}, local path) and uploads the file to blob storage
        ///   THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// NOTE---------------->>>>>>>>>>  
        /// For uploading, first need to pass the containerID in path which can be obtained by using Resource:getContainerID()
        ///   then check for write permission using Resource:canWrite() then call this operation
        ///   For example :
        ///   Path <9043: bar/foo.txt> means we want to create a folder named bar and save foo.txt file in container 9043
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void upload(UPLOAD_INFO info, String localpath)
        {
            try
            {
                String username = info.username;
                String path = info.path;
                DBManager.UserM umgr = new DBManager.UserM();
                Model.UserModel user = new Model.UserModel();
                user = umgr.getUserRecord(connection, username);
                //Break the path <containerid:path>
                String[] pathTokens = path.Split(':');

                String destinationCont = pathTokens[0];
                String destinationPath = pathTokens[1];
                String mycontainer = destinationCont;

                CloudBlobContainer destContainer;
                if (username.Equals(destinationCont))
                {
                    Console.WriteLine("File.cs#upload(): Writing to user private area.");
                    destContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(user.getUid().ToString());
                }
                else
                {
                    DBManager.ResourceM contDetail = new DBManager.ResourceM();
                    Resource res = new Resource();
                    int containerId = res.getContainerID(user.getUid(), destinationCont);
                    
                    if (containerId == -1)
                    {
                        throw new DBLikeExceptions.CloudContainerNotFoundException("File.cs#upload: no container");
                    }
                    Model.AzureContainerModel container = contDetail.getResourceById(connection, containerId);
                    Console.WriteLine("File.cs#upload(): Writing to shared container {0}", container.getContainerName());
                    destContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(container.getContainerName());
                }
                /*
                Console.WriteLine("ResourceID:" + destinationCont);
                
                Resource res = new Resource();

                if (container.getOwner() != user.getUid())  //user is not owner
                {
                    if (!res.canWrite(user.getUid(), Int32.Parse(destinationCont)))   //not having write permission
                    {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                }
                mycontainer = container.getContainerName();
                 * */

                //CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(mycontainer);
                String blobName = String.Format(destinationPath);

                CloudBlockBlob file = destContainer.GetBlockBlobReference(blobName);

                using (FileStream fstream = new FileStream(localpath, FileMode.Open))
                {
                    file.UploadFromStream(fstream);
                }

                BlobStorageManager.BlobFileHandler bhandler = new BlobStorageManager.BlobFileHandler();
                //fstream = new FileStream(localpath, FileMode.Open);
                //file.Properties.
                file.Metadata["HashValue"] = info.curHash;
                file.Metadata["ClientModifyTime"] = info.utcTime.Ticks.ToString();
                file.SetMetadata();
                file.Properties.ContentType = "file/active";
                file.SetProperties();

            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            {
                Console.WriteLine("upload method in User class says-> " + ex);
            }
        }

        /// <summary>
        /// This method takes (username, path{containerid:path}, local path) and download file from blob storage
        /// to given local path
        /// THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// THROW CLOUDBLOBNOTFOUNDEXCEPTION or CLOUDBLOBNOTFOUNDEXCEPTION IF GIVEN FILEPATH DOESN'T EXISTS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void download(String username, String path, Stream targetStream) //containerid:filepath
        {   
            try
            {
                //Break the path <containerid>
                String[] usercont = path.Split(':');

                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);

                String containerP = usercont[0];
                String blobname = usercont[1];
                /*
                Model.UserModel u2;
                u2 = user.getUserRecord(connection, srcUser);

                String userid = u2.getUid().ToString();
                String fullpath = usercont[1];
                String blobName = fullpath.Substring(fullpath.IndexOf('/')+1);
                //String container = fullpath.Substring(0, fullpath.IndexOf('/'));
                 */
                
                String container = containerP;

                DBManager.ResourceM contDetail = new DBManager.ResourceM();
                Console.WriteLine("ResourceID:" + containerP);
                Model.AzureContainerModel cont = contDetail.getResourceById(connection, Int32.Parse(containerP));
                 
                Resource res = new Resource();
                if(cont.getOwner() != u.getUid())   //user isn't owner
                {
                    if (!res.canRead(u.getUid(), Int32.Parse(containerP)))   //not having read permission
                    {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                }
                container = cont.getContainerName();

                //Check if container exists
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(container);
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);
                
                if (isContainerexists==false)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                //Check if blob exists
                CloudBlockBlob myblob = BlobStorageManager.BlobStorage.getCloudBlob(container+'/'+blobname); //Get reference to blob
                Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(myblob);

                if (isBlobexists==false)
                {
                    Console.WriteLine("Container not found");
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }
                Console.WriteLine(myblob);
                myblob.DownloadToStream(targetStream);
               
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            {
                //throw new Microsoft.WindowsAzure.Storage.StorageException();
                Console.WriteLine("download method in User class says->"+ex.Message);
                return;
            }
            
        }

        /// <summary>
        /// This downloadWithSAS() method returns URI for downloading file
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <returns></returns>

        public string downloadWithSAS(String username, String path) //containerID:filepath
        {
            string sasUri = null;
            string sasBUri = null;
            try
            {
                //Break the path <containerID:filepath>
                String[] usercont = path.Split(':');

                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);
                
                String blobname = usercont[1];
                //String blobName = fullpath.Substring(fullpath.IndexOf('/') + 1);
                //String container = fullpath.Substring(0, fullpath.IndexOf('/'));
                String containerGivenName = usercont[0];
                string containerAzureName;
                if (containerGivenName.Equals(username))
                {
                    Console.WriteLine("File.cs#downloadSas: Download from private container");
                    containerAzureName = u.getUid().ToString();
                }
                else
                {
                    Console.WriteLine("File.cs#downloadSas: Download from shared container");
                    Resource r = new Resource();
                    int rid = r.getContainerID(u.getUid(), containerGivenName);
                    DBManager.ResourceM contDetail = new DBManager.ResourceM();
                    var container = contDetail.getResourceById(connection, rid);
                    if (container == null)
                    {
                        throw new DBLikeExceptions.CloudContainerNotFoundException();
                    }
                    containerAzureName = container.getContainerName();

                }


                Console.WriteLine("ResourceID:" + containerAzureName);
                /*
                Model.AzureContainerModel cont = contDetail.getResourceById(connection, con);

                Resource res = new Resource();
                if(cont.getOwner() != u.getUid())
                {
                    if (!res.canRead(u.getUid(), Int32.Parse(container)))   //not having read permission
                    {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                }
                container = cont.getContainerName();

                Console.WriteLine("blobname: " + blobname);
                Console.WriteLine("container: " + container);
                */
                //Check if container exists
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(containerAzureName); //Container ref
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);

                if (isContainerexists == false)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }
                CloudBlockBlob myblob;
                //Check if blob exists
                try
                {
                    myblob = BlobStorageManager.BlobStorage.getCloudBlob(containerAzureName + '/' + blobname); //Get reference to blob
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException)
                {
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }
                /*
                Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(myblob);

                if (isBlobexists == false)
                {
                    Console.WriteLine("Blob not found");
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }*/

                BlobStorageManager.SASGenerator sas = new BlobStorageManager.SASGenerator();
                sasUri = sas.getContainerSASURI(myContainer);
                sasBUri = sas.getBlobSASURI(myblob);

                Console.WriteLine("Download SAS String: \n" + sasBUri);
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            {
                //throw new Microsoft.WindowsAzure.Storage.StorageException();
                Console.WriteLine("sasdownload method in User class says->" + ex.Message);
                
            }
                return sasBUri;
        }

        /// <summary>
        /// This deleteFileFromContainer() methd deletes the given file from the given container
        /// It may throw exceptions like UnauthorizedAccessException, CloudContainerNotFoundException, CloudBlobNotFoundException
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        public void deleteFileFromContainer(String username, String path)   //path>> containerID:filepath 
        {
            try
            {
                //Break the path <containerID:filepath>
                String[] pathTokens = path.Split(':');

                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);

                if (u == null) 
                {
                    Console.WriteLine("No user found");
                    return;
                }

                String container = pathTokens[0];
                String blobfile = pathTokens[1];

                
                String containerAzureName;

                if (username.Equals(container))
                {
                    containerAzureName = u.getUid().ToString();
                }
                else
                {
                    Resource res = new Resource();
                    int containerID = res.getContainerID(u.getUid(), container);
                    if (containerID == -1)
                    {
                        throw new DBLikeExceptions.CloudContainerNotFoundException();
                    }
                    if (!res.canWrite(u.getUid(), containerID))
                    {
                        throw new DBLikeExceptions.UnauthorizedAccessException();
                    }
                    DBManager.ResourceM contDetail = new DBManager.ResourceM();
                    //Console.WriteLine("ResourceID:" + container);
                    Model.AzureContainerModel cont = contDetail.getResourceById(connection, containerID);
                    if (cont == null)
                    {
                        throw new DBLikeExceptions.CloudContainerNotFoundException();
                    }
                    containerAzureName = cont.getContainerName();
                }

                

                //if (cont.getOwner() != u.getUid())  //user is not owner
                //{
                //    Resource res = new Resource();
                //    if (!res.canWrite(u.getUid(), Int32.Parse(container)))   //not having write permission
                //    {
                //         throw new DBLikeExceptions.UnauthorizedAccessException();   
                //    }
                //}
                
                

                //Check if container exists
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(containerAzureName); //Container ref
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);

                if (isContainerexists == false)
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                //Check if blob exists
                try 
                {
                    CloudBlockBlob myblob = BlobStorageManager.BlobStorage.getCloudBlob(containerAzureName + '/' + blobfile); //Get reference to blob
                    Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(myblob);

                    if (isBlobexists == false || myblob.Properties.ContentType.Equals("file/deleted"))
                    {
                        Console.WriteLine("Blob not found or deleted");
                        throw new DBLikeExceptions.CloudBlobNotFoundException();
                    }

                    myblob.DeleteIfExists();
                    myblob.UploadFromByteArray(new byte[0], 0, 0);
                    myblob.Properties.ContentType = "file/deleted";
                    myblob.SetProperties();
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException e)
                {
                    Console.WriteLine("File:deleteFileFromContainer says->>" + e.Message);
                    throw new DBLikeExceptions.CloudBlobNotFoundException();                    
                }

            }
            catch (Exception)
            {
                throw;
            }
            
            
        }

        /// <summary>
        /// This renameFile() method renames the file
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        public void renameFile(String username, String path, String newname)    //path> containerID:filepath 
        {
            try
            {
                //Break the path <containerID:filepath>
                String[] pathTokens = path.Split(':');

                DBManager.UserM user = new DBManager.UserM();
                Model.UserModel u;
                u = user.getUserRecord(connection, username);

                String container = pathTokens[0];
                String blobfile = pathTokens[1];

                DBManager.ResourceM contDetail = new DBManager.ResourceM();
                Console.WriteLine("ResourceID:" + container);
                Model.AzureContainerModel cont = contDetail.getResourceById(connection, Int32.Parse(container));

                    if (cont.getOwner() != u.getUid())  //user is not owner
                    {
                        Resource res = new Resource();
                        if (!res.canWrite(u.getUid(), Int32.Parse(container)))   //not having write permission
                        {
                            throw new DBLikeExceptions.UnauthorizedAccessException();
                        }
                    }

                    container = cont.getContainerName();
                

                //Check if container exists
                CloudBlobContainer myContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(container); //Container ref
                Boolean isContainerexists = BlobStorageManager.BlobStorage.isContainerExists(myContainer);

                if (isContainerexists == false)
                {
                    Console.WriteLine("Container not found");
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                //Check if blob exists
                CloudBlockBlob oldblob = BlobStorageManager.BlobStorage.getCloudBlob(container + '/' + blobfile); //Get reference to blob
                Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(oldblob);

                if (isBlobexists == false)
                {
                    Console.WriteLine("Blob not found");
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }

                ICloudBlob newblob = null;
                if (oldblob is CloudBlockBlob)
                {
                    newblob = myContainer.GetBlockBlobReference(newname);
                }
                else
                {
                    newblob = myContainer.GetPageBlobReference(newname);
                }

                //CloudBlockBlob newblob = BlobStorageManager.BlobStorage.getCloudBlob(container + '/' + newname); //Get reference to blob

                //copy the blob
                newblob.StartCopyFromBlob(oldblob.Uri);

                while (true)
                {
                    newblob.FetchAttributes();
                    if (newblob.CopyState.Status != CopyStatus.Pending) //check the copying status
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(1000); //sleep for a second
                }

                //delete old blobfile
                oldblob.Delete();
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException e)
            {
                Console.WriteLine("File:renameFile says->>" + e.Message);
            }
        }
        /*
        /// <summary>
        /// This copyFile() method copies the File from <containerID:filepath> to <containerID:filepath>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void copyFile(String username, String from, String to)
        {
            try
            {
                //Break the path <containerID:filepath>
                String[] fromTokens = from.Split(':');
                String[] toTokens = from.Split(':');

                DBManager.UserM umgr = new DBManager.UserM();
                Model.UserModel user;
                user = umgr.getUserRecord(connection, username);

                String sourcecontainer = fromTokens[0];
                String sourceblobfile = fromTokens[1];

                String destcontainer = toTokens[0];
                String destblobfile = toTokens[1];

                CloudBlobContainer srcContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(sourcecontainer); //Container ref
                Boolean isSrcContainerexists = BlobStorageManager.BlobStorage.isContainerExists(srcContainer);

                //Check if blob exists
                CloudBlockBlob oldblobfile = BlobStorageManager.BlobStorage.getCloudBlob(sourcecontainer + '/' + sourceblobfile); //Get reference to blob
                Boolean isBlobexists = BlobStorageManager.BlobStorage.isBlobExists(oldblobfile);

                Boolean isDestContainerexists = true;
                if (sourcecontainer != destcontainer)
                {
                    CloudBlobContainer destContainer = BlobStorageManager.BlobStorage.getCloudBlobContainer(destcontainer); //Container ref
                    isDestContainerexists = BlobStorageManager.BlobStorage.isContainerExists(destContainer);
                }
                if ((isSrcContainerexists == false) || (isDestContainerexists == false))
                {
                    Console.WriteLine("Container not found");
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }

                if (isBlobexists == false)
                {
                    Console.WriteLine("Blob not found");
                    throw new DBLikeExceptions.CloudBlobNotFoundException();
                }

                ICloudBlob newblobfile = null;
                if (oldblobfile is CloudBlockBlob)
                {
                    newblobfile = destContainer.GetBlockBlobReference(newname);
                }
                else
                {
                    newblob = myContainer.GetPageBlobReference(newname);
                }

            }
            catch (Microsoft.WindowsAzure.Storage.StorageException e)
            {
                Console.WriteLine("Class File method copyFile Exception: " + e.Message);
            }
        }*/
    }
}
