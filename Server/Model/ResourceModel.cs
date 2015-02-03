using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    class AzureContainerModel
    {
        private int rid;
        private String containerName;
        private String givenName;
        private int owner;   //new field added

        public int getRid()
        {
            return rid;
        }
        public String getContainerName()
        {
            return containerName;
        }
        public String getGivenName()
        {
            return givenName;
        }
        public int getOwner()
        {
            return owner;
        }
        public void setRid(int rid)
        {
            this.rid = rid;
        }
        public void setContainerName(String containerName)
        {
            this.containerName = containerName;
        }
        public void setGivenName(String givenName)
        {
            this.givenName = givenName;
        }
        public void setOwner(int owner)
        {
            this.owner = owner;
        }
    }
}
