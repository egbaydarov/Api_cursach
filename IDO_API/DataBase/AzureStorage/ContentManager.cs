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
                Console.Error.WriteLine(e.Message);
            }
        }

        public static ContentManager DefaultManager
        {
            get
            {
                return defaultInstance;
            }
            set
            {
                defaultInstance = value;
            }
        }


        public async Task<short> UploadImageAsync(string containerReference, string blobReference, Stream image)
        {
            try
            {
                var container = cloudBlobClient.GetContainerReference(containerReference);
                var blob = container.GetBlockBlobReference(blobReference);

                await blob.UploadFromStreamAsync(image);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public async Task<short> CreateContainerAsync(string containerReference)
        {
            try
            {
                await cloudBlobClient.GetContainerReference(containerReference).CreateAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        
        public async Task<short> DeleteImageAsync(string containerReference, string blobReference)
        {
            try
            {
                var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
                await blob.DeleteAsync();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public async Task<Stream> DownloadImageAsync(string containerReference, string blobReference)
        {

            try
            {
                var stream = new MemoryStream();
                var blob = await cloudBlobClient.GetContainerReference(containerReference).GetBlobReferenceFromServerAsync(blobReference);
                blob.DownloadToStream(stream);
                return stream;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
    }
}



