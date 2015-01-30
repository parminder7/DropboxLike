using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server.DBLikeExceptions
{
    public class CloudContainerNotFoundException : Exception
    {
        public CloudContainerNotFoundException() : base()
        { }
        public CloudContainerNotFoundException(string message) : base(message)
        { }
        public CloudContainerNotFoundException(string message, Exception innerException) : base(message, innerException)
        { }
        protected CloudContainerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}

