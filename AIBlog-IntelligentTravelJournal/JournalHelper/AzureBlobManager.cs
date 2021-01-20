using Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JournalHelper
{
    /// <summary>
    /// Blob storage manager class
    /// </summary>
    public class BlobManager
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _blobClient;

        private readonly string sasPolicyName = "TravelJournalPolicy";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobManager" /> class.
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string in app.config or web.config file.</param>
        public BlobManager(string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);

            _blobClient = _account.CreateCloudBlobClient();
            _blobClient.DefaultRequestOptions = GetBlobRequestOptionsForClient();
        }

        public BlobManager(string accountName, string accountKey)
        {

            var storageCredentials = new StorageCredentials(accountName, accountKey);
            _account = new CloudStorageAccount(storageCredentials, true);

            _blobClient = _account.CreateCloudBlobClient();
            _blobClient.DefaultRequestOptions = GetBlobRequestOptionsForClient();
        }

        static private BlobRequestOptions GetBlobRequestOptionsForClient()
        {
            BlobRequestOptions DefaultRequestOptions = new BlobRequestOptions
            {
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 4),
                LocationMode = LocationMode.PrimaryThenSecondary,
                MaximumExecutionTime = TimeSpan.FromSeconds(20)
            };

            return DefaultRequestOptions;
        }
    

        static public CloudBlobClient GetBlobClient(string accountName, string accountKey)
        {
            var storageCredentials = new StorageCredentials(accountName, accountKey);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            cloudBlobClient.DefaultRequestOptions = GetBlobRequestOptionsForClient();

            return cloudBlobClient;
        }

        public Uri GetContainerUri(string containerName, bool createNew=false)
        {
            var container = GetBlobContainerAsync(containerName, createNew).GetAwaiter().GetResult();
            if (container == null) return null;

            return container.Uri;
        }

        public async Task<Uri> GetContainerSASUriAsync(string containerName, bool createNew=false)
        {
            var container = await GetBlobContainerAsync(containerName, createNew);
            if (container == null) return null;

            // create the stored policy we will use, with the relevant permissions and expiry time
            var storedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read |
                              SharedAccessBlobPermissions.Write |
                              SharedAccessBlobPermissions.List
            };

            // get the existing permissions (alternatively create new BlobContainerPermissions())
            var permissions = await container.GetPermissionsAsync();

            permissions.PublicAccess = BlobContainerPublicAccessType.Container;

            // optionally clear out any existing policies on this container
            permissions.SharedAccessPolicies.Clear();
            // add in the new one
            permissions.SharedAccessPolicies.Add(sasPolicyName, storedPolicy);
            // save back to the container
            await container.SetPermissionsAsync(permissions);

            // Now we are ready to create a shared access signature based on the stored access policy
            var containerSignature = container.GetSharedAccessSignature(null, sasPolicyName);
            // create the URI a client can use to get access to just this container
            var uri = container.Uri + containerSignature;
            return new Uri(uri);
        }
        public async Task<Uri> GetContainerSASUriWithoutStoredPolicyAsync(string containerName, bool createNew = false)
        {
            var container = await GetBlobContainerAsync(containerName, createNew);
            if (container == null) return null;

            // create the stored policy we will use, with the relevant permissions and expiry time
            var storedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read |
                              SharedAccessBlobPermissions.Write |
                              SharedAccessBlobPermissions.List
            };

            // get the existing permissions (alternatively create new BlobContainerPermissions())
            var permissions = await container.GetPermissionsAsync();

            permissions.PublicAccess = BlobContainerPublicAccessType.Container;

            //// optionally clear out any existing policies on this container
            //permissions.SharedAccessPolicies.Clear();
            //// add in the new one
            //permissions.SharedAccessPolicies.Add(sasPolicyName, storedPolicy);
            //// save back to the container
            //await container.SetPermissionsAsync(permissions);

            // Now we are ready to create a shared access signature based on the stored access policy
            var containerSignature = container.GetSharedAccessSignature(storedPolicy);
            // create the URI a client can use to get access to just this container
            var uri = container.Uri + containerSignature;
            return new Uri(uri);
        }

        public async Task RevokeSharedAccessPolicyAsync(string containerName)
        {
            var container = await GetBlobContainerAsync(containerName, false);
            if (container == null) return;

            var permissions = await container.GetPermissionsAsync();
            permissions.SharedAccessPolicies.Remove(sasPolicyName);
            await container.SetPermissionsAsync(permissions);
        }
        public async Task DeleteBlobContainerAsync(string containerName)
        {
            var container = await GetBlobContainerAsync(containerName);
            if (container != null)
            {
                await container.DeleteAsync();
            }
        }

        public async Task<CloudBlobContainer> GetBlobContainerAsync(string containerName, bool createNew = false)
        {
            var container = _blobClient.GetContainerReference(containerName);
            if (createNew)
            {
                await container.CreateIfNotExistsAsync();
            }
            else
            {
                bool containerExists = await container.ExistsAsync();
                if (!containerExists)
                {
                    Console.WriteLine("Error: Blob container does not exist: {0} ", containerName);
                    return null;
                }
            }
            return container;

        }
        public async Task<IEnumerable<IListBlobItem>> GetBlobItemsInContainerAsync(string sourceContainer)
        {
            var container = await GetBlobContainerAsync(sourceContainer);
            var blobItems = new List<IListBlobItem>();
            if (container == null)
            {
                Console.WriteLine("Source Container does note exist");
                return blobItems;  // return empty collection.
            }

            BlobContinuationToken currentToken = null;
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var blobResultSegment = await container.ListBlobsSegmentedAsync(currentToken);
                blobItems.AddRange(blobResultSegment.Results);
                blobContinuationToken = blobResultSegment.ContinuationToken;
            } while (blobContinuationToken != null);

            return blobItems;
        }

        public async Task<IEnumerable<IListBlobItem>> GetBlobItemsInFolderrAsync(IListBlobItem folderItem)
        {
            var blobItems = new List<IListBlobItem>();
            if (!(folderItem is CloudBlobDirectory))
            {
                return blobItems;
            }
                
            CloudBlobDirectory cloudBlobDirectory = (CloudBlobDirectory)folderItem;

            BlobContinuationToken currentToken = null;
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var blobResultSegment = await cloudBlobDirectory.ListBlobsSegmentedAsync(currentToken);
                blobItems.AddRange(blobResultSegment.Results);
                blobContinuationToken = blobResultSegment.ContinuationToken;
            } while (blobContinuationToken != null);

            return blobItems;
        }

        public IEnumerable<Uri> GetBlobUrisAsync(IEnumerable<IListBlobItem> blobItems)
        {
            List<Uri> jsonBlobUris = new List<Uri>();
            foreach (var blobItem in blobItems)
            {
                jsonBlobUris.Add(blobItem.Uri);
            }
            return jsonBlobUris;
        }

        public async Task<IEnumerable<Uri>> GetBlobsInContainerAsync(string sourceContainer)
        {
            var blobItems = await GetBlobItemsInContainerAsync(sourceContainer);

            List<Uri> jsonBlobUris = new List<Uri>();
            foreach (var blobItem in blobItems)
            {
                jsonBlobUris.Add(blobItem.Uri);
            }
            return jsonBlobUris;
        }

        /// <summary>
        /// Updates or created a blob in Azure blobl storage
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="blobName">Name of the blob.</param>
        /// <param name="content">The content of the blob.</param>
        /// <returns></returns>
        public void PutBlob(string containerName, string blobName, string filePath)
        {

            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            blob.UploadFromFileAsync(filePath).Wait();

        }

        /// <summary>
        /// Creates the container in Azure blobl storage
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>True if contianer was created successfully</returns>
        public async Task<bool> CreateContainer(string containerName, bool deleteExisting=false)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            if (deleteExisting)
            {
                Console.WriteLine($"Deleting the container {containerName}");
                await container.DeleteIfExistsAsync();
                Thread.Sleep(new TimeSpan(0, 1, 0));
            }
            bool created = await container.CreateIfNotExistsAsync();
            return created;
        }

        /// <summary>
        /// Checks if a container exist.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>True if conainer exists</returns>
        public async Task<bool> DoesContainerExist(string containerName)
        {
            CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
            return await container.ExistsAsync(); //.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // list containers in storage account
        public  IEnumerable<CloudBlobContainer> ListContainers()
        {
            var blobContainers = new List<CloudBlobContainer>();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var containerSegment = _blobClient.ListContainersSegmentedAsync(blobContinuationToken).GetAwaiter().GetResult();
                blobContainers.AddRange(containerSegment.Results);
                blobContinuationToken = containerSegment.ContinuationToken;
            } while (blobContinuationToken != null);
            
            return blobContainers;
        }

        /// <summary>
        /// Uploads the directory to blobl storage
        /// </summary>
        /// <param name="sourceDirectory">The source directory name.</param>
        /// <param name="containerName">Name of the container to upload to.</param>
        /// <returns>True if successfully uploaded</returns>
        public void UploadDirectory(string sourceDirectory, string containerName, bool deleteExistingContainer=false)
        {
            if (!Directory.Exists(sourceDirectory))
                return;

            bool created = CreateContainer(containerName, deleteExistingContainer).GetAwaiter().GetResult();

            UploadDirectory(sourceDirectory, containerName, string.Empty);
        }

        private void UploadDirectory(string sourceDirectory, string containerName, string prefixAzureFolderName)
        {

            var folder = new DirectoryInfo(sourceDirectory);
            var files = folder.GetFiles();
            foreach (var fileInfo in files)
            {
                string blobName = fileInfo.Name;
                if (!string.IsNullOrEmpty(prefixAzureFolderName))
                {
                    blobName = prefixAzureFolderName + "/" + blobName;
                }
                PutBlob(containerName, blobName, fileInfo.FullName);
            }

            var subFolders = folder.GetDirectories();
            foreach (var directoryInfo in subFolders)
            {
                var prefix = directoryInfo.Name;
                if (!string.IsNullOrEmpty(prefixAzureFolderName))
                {
                    prefix = prefixAzureFolderName + "/" + prefix;
                }
                UploadDirectory(directoryInfo.FullName, containerName, prefix);
            }

        }

   }
}
