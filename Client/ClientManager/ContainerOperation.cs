using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace client
{
    public class ContainerOperation
    {
       public void Container(string sas,string localPath)
       {
           Console.WriteLine("Container operation ");
           CloudBlobContainer container = new CloudBlobContainer(new Uri(sas)); //returning a reference to the container.
 
           //creating a list to store URIs returned by listing operation on the container
           List<Uri> blobUris = new List<Uri>();
           try
           {
               foreach(ICloudBlob blobListing in container.ListBlobs())
               {
                   blobUris.Add(blobListing.Uri);
               }
               Console.WriteLine("Operation successful " + sas);
               Console.WriteLine();
           }
           catch (StorageException x)
           {
               Console.WriteLine("Listing of the blob failed " + sas);
               Console.WriteLine(x.ToString());

           }

           //reading based on one of the reference of the blob in the container
           try
           {
               CloudBlockBlob blob = container.GetBlockBlobReference(blobUris[0].ToString());
               MemoryStream readContent = new MemoryStream();
               readContent.Position = 0;
               using(readContent)
               {
                   blob.DownloadToStream(readContent);
                   Console.WriteLine(readContent.Length);//this data will go in local drive path.

               }
               Console.WriteLine("Read Operation succeeded for SAS" + sas);
               Console.WriteLine();
           }
           catch 
           {
               Console.WriteLine("Download operation failed for SAS"+sas);
               Console.WriteLine();


           }


           }
       }
    }


