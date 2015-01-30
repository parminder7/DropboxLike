using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server.DBLikeExceptions
{
    public class CloudBlobNotFoundException : Exception 
    {
        public CloudBlobNotFoundException() : base() 
        { }
        public CloudBlobNotFoundException(string message) : base(message)
        { }
        public CloudBlobNotFoundException(string message, Exception innerException) : base(message, innerException)
        { }
        protected CloudBlobNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}

