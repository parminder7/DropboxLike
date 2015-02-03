using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    class CListModel
    {
        private int rid;
        private int uid;
        private int owner;   //new field added
        private int readwrite;

        public int getRid()
        {
            return rid;
        }
        public int getUid()
        {
            return uid;
        }
        public int getOwner()
        {
            return owner;
        }
        public int getReadwrite()
        {
            return readwrite;
        }
        public void setRid(int rid)
        {
            this.rid = rid;
        }
        public void setUid(int uid)
        {
            this.uid = uid;
        }
        public void setOwner(int owner)
        {
            this.owner = owner;
        }
        public void setReadwrite(int readwrite)
        {
            this.readwrite = readwrite;
        }
        
    }
}
