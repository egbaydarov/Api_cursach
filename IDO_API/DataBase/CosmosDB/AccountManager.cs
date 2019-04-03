using IDO_API.Models;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.DataBase.CosmosDB
{
    class AccountManager
    {
        static AccountManager defaultInstance = new AccountManager();

        const string accountURL = @"https://apponedb.documents.azure.com:443/";
        const string accountKey = @"vgmfdRZISOYPuyhVE1gf7DiM6Ky0ImzF6Mm3ftLf2Fqnfb5RqgTDOzRqKTGdSMkwp7lSf4sXKxOLw2jPlUNZNw==";
        const string databaseId = @"appOne";
        const string collectionId = @"accounts";

        private Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        private DocumentClient client;

        public AccountManager()
        {
            client = new DocumentClient(new System.Uri(accountURL), accountKey);
        }

        public static AccountManager DefaultManager
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
        public async Task UpadateAccountInfoAsync(User oldAccount, User newAccount)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                              .Where(user => user.Nickname.Equals(oldAccount.Nickname))
                              .AsEnumerable()
                              .FirstOrDefault();
            if (query != null && query.Password.Equals(oldAccount.Password))
            {
                await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                    newAccount);
            }
        }
        public async Task CreateAccountAsync(User newAccount)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(newAccount.Nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query == null)
            {
                await client.CreateDocumentAsync(collectionLink, newAccount);
            }
            else
            {
                throw new ApplicationException("Account Already Exists.");
            }

        }
        public User TryGetAccountData(User account)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(account.Nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query != null && query.Password.Equals(account.Password))
            {
                return query;
            }
            else
                throw new ApplicationException("Incorrect Login/Password.");
        }
    }
}
