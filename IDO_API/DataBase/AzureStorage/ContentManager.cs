using IDO_API.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.DataBase.AzureStorage
{
    public class ContentManager
    {
        static string storageConnectionString = "";
        static  CloudBlobClient cloudBlobClient;


        static ContentManager defaultInstance = new ContentManager();
        public ContentManager()
        {
            try
            {
                cloudBlobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.Message}");
            }
        }

        public static ContentManager DefaultManager
        {
            get
            {
                return defaultInstance;
            }
            private set
            {
                defaultInstance = value;
            }
        }


        public async Task UploadAchievementImageAsync(string containerReference, string blobReference, Stream image)
        {
            var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
            await blob.UploadFromStreamAsync(image);
        }
        public async Task CreateContainerAsync(string containerReference)
        {
            await cloudBlobClient.GetContainerReference(containerReference).CreateAsync();
        }
        
        public async Task DeleteAchievementImageAsync(string containerReference, string blobReference)
        {
            var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
            await blob.DeleteAsync();
        }
        public async Task DownloadAchievementImageAsync(string containerReference, string blobReference, Stream outputstream)
        {
            var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
            blob.DownloadToStream(outputstream);
        }
    }
}



