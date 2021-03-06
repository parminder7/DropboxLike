﻿using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure;

namespace Server.BlobStorageManager
{
    class BlobStorage
    {
        private static CloudBlobClient cloudBlobClient;
        //private static object initializeLock = new object();

        private static String accountName = "portalvhdsy2kypk56l5hn7";

        //private static Microsoft.WindowsAzure.Storage.Auth.StorageCredentials credentials;
        private static Microsoft.WindowsAzure.Storage.CloudStorageAccount account;
        
        
        /// <summary>
        /// This method do the housekeeping stuffs
        /// </summary>
        private static void initialize() {
            //Return reference to the storage account
            account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            
            //credentials = new StorageCredentials(accountName, accountKey);
            //account = new CloudStorageAccount(credentials, useHttps: true);

            cloudBlobClient = account.CreateCloudBlobClient();

        }

        /// <summary>
        /// This method returns the container reference
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public static CloudBlobContainer getCloudBlobContainer(String containerName) {
            initialize();

            CloudBlobContainer aContainer = cloudBlobClient.GetContainerReference(containerName);
            
            return aContainer;
        }

        /// <summary>
        /// This method returns the reference to the blob
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public static CloudBlockBlob getCloudBlob(String blobPath){
            //initialize();
            String uriString = "http://" + accountName + ".blob.core.windows.net";
            Uri rootUri = new Uri(uriString);
            Uri pathUri = new Uri(rootUri, blobPath);
            StorageUri uri = new StorageUri(pathUri);

            ICloudBlob result = cloudBlobClient.GetBlobReferenceFromServer(uri);
            if (result is CloudBlockBlob)
            {
                return (CloudBlockBlob)result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method returns true if specified container exists 
        /// But if container doesn't exists return CLOUDCONTAINERNOTFOUNDEXCEPTION
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Boolean isContainerExists(Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer container) {
            try {
                container.FetchAttributes();
                return true;
            }
            catch(StorageException ex){
                Console.WriteLine(ex);
                return false; 
                /*
                if (ex.ErrorCode == StorageErrorCode.ResourceNotFound)
                {
                    return false;
                }
                else {
                    throw;
                }*/
            }
        }

        /// <summary>
        /// This method chechs whether specified blob exists. 
        /// true, if true else false
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        public static Boolean isBlobExists(CloudBlockBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e);
                return false;
                /*if (e. == StorageException.ResourceNotFound)
                {
                    return false;
                }
                else
                {
                    throw;
                }*/
            }
        }

        /// <summary>
        /// This method returns the CloudBlobClient object
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        public static Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient getCloudBlobClient(String UID) {
            initialize();

            return cloudBlobClient;
        }
    }
}
