using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server.DBLikeExceptions
{
    public class CloudContainerAlreadyExistException : Exception
    {
        public CloudContainerAlreadyExistException() : base()
        { }
        public CloudContainerAlreadyExistException(string message) : base(message)
        { }
        public CloudContainerAlreadyExistException(string message, Exception innerException) : base(message, innerException)
        { }
        protected CloudContainerAlreadyExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
