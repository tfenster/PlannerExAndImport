using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

// see https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Token-cache-serialization
namespace PlannerExAndImport
{
    class EncryptedFileCache : TokenCache
    {
        private static readonly object fileLock = new object();
        private string cacheFilePath;

        // Initializes the cache against a local file.
        // If the file is already present, it loads its content in the ADAL cache
        public EncryptedFileCache(string filePath = @".\TokenCache.dat")
        {
            cacheFilePath = filePath;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            lock (fileLock)
            {
                this.Deserialize(File.Exists(cacheFilePath) ?
                     ProtectedData.Unprotect(File.ReadAllBytes(cacheFilePath), null,
                                             DataProtectionScope.CurrentUser)
                                                            : null);
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            File.Delete(cacheFilePath);
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (fileLock)
            {
                this.Deserialize(File.Exists(cacheFilePath) ?
                                 ProtectedData.Unprotect(File.ReadAllBytes(cacheFilePath), null,
                                                         DataProtectionScope.CurrentUser)
                                                            : null);
            }
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                lock (fileLock)
                {
                    // reflect changes in the persistent store
                    File.WriteAllBytes(cacheFilePath, ProtectedData.Protect(this.Serialize(),
                                       null, DataProtectionScope.CurrentUser));
                    // once the write operation took place, restore the HasStateChanged bit to false
                    this.HasStateChanged = false;
                }
            }
        }
    }
}