using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    class UserModel
    {
        private int uid;
        private String emailid;
        private String password;
        private String fullname;    //new field added
        
        public int getUid()
        {
            return uid;
        }
        public String getEmailId()
        {
            return emailid;
        }
        public String getPassword()
        {
            return password;
        }
        public String getFullName()
        {
            return fullname;
        }
        public void setUid(int uid)
        {
            this.uid = uid;
        }
        public void setEmailId(String emailid)
        {
            this.emailid = emailid;
        }
        public void setPassword(String password)
        {
            this.password = password;
        }
        public void setFullName(String fullname)
        {
            this.fullname = fullname;
        }
    }
}