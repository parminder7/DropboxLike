using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    public class BlobFileModel
    {
        private DateTimeOffset lastmodifiedTime;
        private long size;
        private String eTagForVersionControl;
        private String MD5HashValue;
        private Boolean deleted;

        public Boolean isDeleted()
        {
            return deleted;
        }
        public void setDeleted(Boolean isDeleted)
        {
            deleted = isDeleted;
        }
        
        public String geteTagForVersionControl()
        {
            return eTagForVersionControl;
        }

        public String getMD5HashValue()
        {
            return MD5HashValue;
        }

        public long getSize()
        {
            return size;
        }

        public DateTimeOffset getLastmodifiedTime()
        {
            return lastmodifiedTime;
        }
        
        public void seteTagForVersionControl(String eTagForVersionControl)
        {
            this.eTagForVersionControl = eTagForVersionControl;
        }

        public void setMD5HashValue(String MD5HashValue)
        {
            this.MD5HashValue = MD5HashValue;
        }

        public void setSize(long size)
        {
            this.size = size;
        }

        public void setLastmodifiedTime(DateTimeOffset time)
        {
            this.lastmodifiedTime = time;
        }
    }
}
