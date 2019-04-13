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
#if DEBUG
        static string storageConnectionString = @"UseDevelopmentStorage=true";

#else
        static string storageConnectionString = @"DefaultEndpointsProtocol=https;AccountName=mirrorstorage2;AccountKey=rwyHKda1stz2mtGP39HCyO3KVJgy2DzO8GOhlg/lmgl1fJBH9EoFr1eOHkSIXU/tCPbvshnQ5+oHAHv8NrGm+A==;EndpointSuffix=core.windows.net";

#endif
        static CloudBlobClient cloudBlobClient;

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
            var container = cloudBlobClient.GetContainerReference(containerReference);
            var blob = container.GetBlockBlobReference(blobReference);

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
        public async Task<Stream> DownloadAchievementImageAsync(string containerReference, string blobReference)
        {
            var stream = new MemoryStream();
            var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
            blob.DownloadToStream(stream);
            return stream;
        }
    }
}



