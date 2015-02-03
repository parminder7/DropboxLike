using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    interface IResourceManage
    {
        /* return the id of a container with the given name that can be seen by userid.
         * GIVENNAME is the name of container given by user  
         * This name is mapped to actual container name in database 
         * Return -1 if no such container */
        int getContainerID(int userid, String givenname);
        /* create a new shared container for userid and return its assigned id.
         * Return -1 if not possible to create for any reason
         * CloudContainerAlreadyExistException if container already exists
         * otherwise it will return 0 
         */
        int createSharedContainer(int userid, String givenname);

        /* delete an existing shared container, return true if success */
        Boolean deleteSharedContainer(int containerID);

        /* grant read or read/write rights depending on writeAccess value */
        Boolean grantRights(int otheruserID, int containerID, Boolean writeAccess);
        /* remove ALL rights */
        void removeRights(int otheruserID, int containerID);


        /* return true if userID is the owner of containerID */
        Boolean isOwner(int userID, int containerID);
        /* return true if userID can read files from containerID */
        Boolean canRead(int userID, int containerID);
        /* return true if userID can write to files in containerID */
        Boolean canWrite(int userID, int containerID);
    }
}
