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
        private int rreadonly;
        private int readwrite;

        public int getRid()
        {
            return rid;
        }
        public int getUid()
        {
            return uid;
        }
        public int getReadonly()
        {
            return rreadonly;
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
        public void setReadonly(int rreadonly)
        {
            this.rreadonly = rreadonly;
        }
        public void setReadwrite(int readwrite)
        {
            this.readwrite = readwrite;
        }
        
    }
}
