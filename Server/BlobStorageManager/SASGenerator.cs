using System.IO;    
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace Server.BlobStorageManager
{
    class SASGenerator
    {
        //THIS CLASS IS YET TO BE IMPLEMENTED FOR IMPOSING
        //PERMISSIONS ON BLOB CONTAINER FOR CLIENT ACCESS

        public string getContainerSASURI(CloudBlobContainer container) {
            SharedAccessBlobPolicy SASrestrictions = new SharedAccessBlobPolicy();

            //Permission for container expires in 15 mintues
            SASrestrictions.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15);

            //Since we're using SAS for download, Only read permission will be given
            SASrestrictions.Permissions = SharedAccessBlobPermissions.Read;

            //SAS Identifier
            string SASIden = container.GetSharedAccessSignature(SASrestrictions);

            //Container URI along with SAS identifier
            string ContainerSASURI = container.Uri + SASIden;

            return ContainerSASURI;
        }

        public string getBlobSASURI(CloudBlockBlob blob)
        {
            SharedAccessBlobPolicy SASrestrictions = new SharedAccessBlobPolicy();

            //Permission for container expires in 15 mintues
            SASrestrictions.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(15);

            //Since we're using SAS for download, Only read permission will be given
            SASrestrictions.Permissions = SharedAccessBlobPermissions.Read;

            //SAS Identifier
            string SASIden = blob.GetSharedAccessSignature(SASrestrictions);

            //Blob URI along with SAS identifier
            string BlobSASURI = blob.Uri + SASIden;

            return BlobSASURI;
        }
    }
}
