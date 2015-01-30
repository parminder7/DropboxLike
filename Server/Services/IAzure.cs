using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    interface IAzure
    {
        /// <summary>
        /// This method returns TRUE if the following hold:
        ///     *username exists in the user table
        ///     *password matches with the value in the user table
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Boolean validateUser(String username, String password);

        /// <summary>
        /// This method should return TRUE if user account created successfully
        /// *FALSE if user record already exist in database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Boolean createUser(String username, String password);

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
        void upload(String username, String path, String localpath);

        /// <summary>
        /// This method takes (username, path{userid:containername}, local path) and download file from blob storage
        /// to given local path
        /// THROW UNAUTHORIZEDACCESSEXCEPTION IF USER HAS NO RIGHT TO ACCESS THE MENTIONED PATH
        /// THROW CLOUDBLOBNOTFOUNDEXCEPTION IF GIVEN FILENAME DOESN'T EXISTS
        /// </summary>
        /// <param name="username"></param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        void download(String username, String path, Stream targetStream);

        /*
         * Preconditions: username, path formatted as above
         *  mode is either 'r' or 'w'
         * 
         * Returns: true if username has read or write access to the 
         * resource indicated by path, depending on mode (i.e. 'r'
         * checks for read access, 'w' checks for write access).
         * For now, write access includes create access (but not delete)
         * 
         * No side effects.
         */
        Boolean checkAccess(String username, String path, char mode);
    }
}