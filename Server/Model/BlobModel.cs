

/**
 * * Will not be using this class.... ALT > BLOB METADATA feature is used
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    class BlobModel
    {
        private int rid;
        private String blobname;
        private float size;
        private DateTime lastmodified;

        public int getRid()
        {
            return rid;
        }
        public String getBlobname()
        {
            return blobname;
        }
        public float getSize()
        {
            return size;
        }
        public DateTime getLastmodified()
        {
            return lastmodified;
        }
        public void setRid(int rid)
        {
            this.rid = rid;
        }
        public void setBlobname(String blobname)
        {
            this.blobname = blobname;
        }
        public void setSize(float size)
        {
            this.size = size;
        }
        public void setLastmodified(DateTime lastmodified)
        {
            this.lastmodified = lastmodified;
        }
    }
}
