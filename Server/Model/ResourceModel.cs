using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Model
{
    class ResourceModel
    {
        private int rid;
        private String container;

        public int getRid()
        {
            return rid;
        }
        public String getContainer()
        {
            return container;
        }
        public void setRid(int rid)
        {
            this.rid = rid;
        }
        public void setContainer(String container)
        {
            this.container = container;
        }
        
    }
}
