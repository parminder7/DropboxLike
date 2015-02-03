using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//INCOMPLETED CLASS IMPLEMENTATION
namespace Server.BlobStorageManager
{
    public class BlobFileHandler
    {
        CloudBlockBlob cloudBlob;
        public Boolean initialise(String containerName, String blobName){
            CloudBlobContainer container = BlobStorage.getCloudBlobContainer(containerName);
            try
            {
                if (container.Exists())
                {
                    cloudBlob = container.GetBlockBlobReference(blobName);
                 
                    if (cloudBlob.Exists())
                    {
                        cloudBlob.FetchAttributes();
                        return true;
                    }
                    else
                    {
                        throw new DBLikeExceptions.CloudBlobNotFoundException();
                    }
                }
                else
                {
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }
            }
            catch (Exception) 
            {
                throw new DBLikeExceptions.CloudBlobNotFoundException();
            }
        }
        public Boolean getIsBlobDeleted(String containerName, String blobName)
        {
            if (initialise(containerName, blobName))
            {
                if (cloudBlob.Properties.ContentType.Equals("file/deleted")) 
                {
                    return true;
                } 
                else if (cloudBlob.Properties.ContentType.Equals("file/active")) 
                {
                    return false;
                } 
                else 
                {
                    throw new InvalidOperationException("invalid content-type information; probably older file");
                }
            } else {
                throw new DBLikeExceptions.CloudBlobNotFoundException();
            }
        }

        public DateTimeOffset getBlobLastModifiedTime(String containerName, String blobName)
        {
            if (initialise(containerName, blobName))
            {
                if (cloudBlob.Metadata.ContainsKey("ClientModifyTime"))
                {
                    string longTicksStr = cloudBlob.Metadata["ClientModifyTime"];
                    try
                    {
                        long ticks = long.Parse(longTicksStr);
                        return (DateTimeOffset)new DateTime(ticks, DateTimeKind.Utc);
                    }
                    catch (FormatException)
                    {
                        return (DateTimeOffset)cloudBlob.Properties.LastModified;
                    }
                }
                else
                {
                    return (DateTimeOffset)cloudBlob.Properties.LastModified;
                }
            }
            else
            {
                throw new DBLikeExceptions.CloudBlobNotFoundException();
            }
        }

        public long getBlobSize(String containerName, String blobName) 
        {
            if (initialise(containerName, blobName))
            {
                return cloudBlob.Properties.Length;
            }
            else 
            {
                return -1;
            }
        }

        public String getCurrentVersion(String containerName, String blobName)
        {
            if (initialise(containerName, blobName))
            {
                return cloudBlob.Properties.ETag;
            }
            else
            {
                return null;
            }
        }

        public String getBlobMD5HashValue(String containerName, String blobName)
        {
            if (initialise(containerName, blobName))
            {
                return cloudBlob.Metadata["HashValue"];
            }
            else
            {
                return null;
            }
        }

        public String getFileMD5(Stream fstream)
        {
            byte[] hash;
            fstream.Seek(0, SeekOrigin.Begin);
            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(fstream);
            }

            fstream.Seek(0, SeekOrigin.Begin);

            return Convert.ToBase64String(hash);
        }
    }
}
