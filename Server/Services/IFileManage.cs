using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public struct UPLOAD_INFO
    {
        public String username;
        public String path;
        public Int64 sz;
        public String curHash;
        public DateTimeOffset utcTime;
        public String absPath;
        public String prevHash;
    }
    public interface IFileManage
    {
        /// <summary>
        /// This method returns a list of strings as follows:
        ///   *if 'path' is empty, a list of all containers that username can access
        ///   else path format userid:containername, it will list all files in given container
        ///   *if userid is not the same as username, return a list of all files and folders that
        ///         userid is sharing with username. ((This functionality is NOT needed for the prototype))
        ///    THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        String[] list(String username, String path);

        /// <summary>
        /// This method takes (username, path{userid:containername}, local path) and uploads the file to blob storage
        /// *if userid is not the same as username, the permission will be checked before uploading
        /// THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        void upload(UPLOAD_INFO info, String localpath);

        /// <summary>
        /// This method takes (username, path{userid:containername}, local path) and download file from blob storage
        /// to given local path
        /// THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// THROW CLOUDBLOBNOTFOUNDEXCEPTION IF GIVEN FILENAME DOESN'T EXISTS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        void download(String username, String path, System.IO.Stream targetStream);

        /// <summary>
        /// This method returns SAS uri string which can be used at Client side
        /// to directly download the contents from the Blob storage at specified path
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        string downloadWithSAS(String username, String path);

        /// <summary>
        /// NEW METHOD FOR LISTING CONTENTS OF USER'S CONTAINERS
        /// </summary>
        /// <param name="emailID"></param>
        /// <returns></returns>
        Dictionary<String, Dictionary<String, Model.BlobFileModel>> getUserCloudFileSystem(String emailID);

        /// <summary>
        /// This deleteFileFromContainer() methd deletes the given file from the given container
        /// It may throw exceptions like UnauthorizedAccessException, CloudContainerNotFoundException, CloudBlobNotFoundException
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        void deleteFileFromContainer(String username, String path);

        /// <summary>
        /// This renameFile() method renames the given file with new name
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="newname"></param>
        void renameFile(String username, String path, String newname);
    }
}
