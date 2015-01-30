using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Common
    {
        public Boolean validateEmail(string email)
        {
            RegexUtilities util = new RegexUtilities();
            if (!util.IsValidEmail(email))
            {
                return false;
            }
            return true;
        }

        public Boolean isEmailPwdNull(string email, string password)
        {
            if (email == "" || password == "")
            {
                return true;
            }
            return false;
        }
    }
}
