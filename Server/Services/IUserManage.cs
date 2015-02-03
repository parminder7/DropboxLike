using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public interface IUserManage
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
        Boolean createUser(String fullname, String username, String password);

        int findUserId(String username);
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
       /* Boolean checkAccess(String username, String path, char mode);

        void grantAccess(String username, String container, char mode); */
    }
}
