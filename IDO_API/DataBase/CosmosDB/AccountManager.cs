using IDO_API.Models;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.DataBase.CosmosDB
{
    class AccountManager
    {
        static AccountManager defaultInstance = new AccountManager();

        
#if DEBUG
        const string accountURL = @"https://localhost:8081";
        const string accountKey = @"C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        const string databaseId = @"IDO";
        const string collectionId = @"accounts";
#else
        const string accountURL = @"https://pidstagram.documents.azure.com:443/";
        const string accountKey = @"JYfaV28KzEQEOr8jzJdwojr3TBb6eu9PBvLbL0sj0quyZahWE3TeuWyFAhFQj3RotcvgQo9cj91ZEzmkMonXOg==";
        const string databaseId = @"IDO";
        const string collectionId = @"accounts";
#endif

        private Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        private DocumentClient client;

        public AccountManager()
        {
            try
            {
                client = new DocumentClient(new System.Uri(accountURL), accountKey);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.Message}");
            }
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
        public async Task UpadateAccountInfoAsync(string oldPass, User user)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                              .Where(acc => acc.Id.Equals(user.Id))
                              .AsEnumerable()
                              .FirstOrDefault();
            if (query != null && query.Password.Equals(oldPass))
            {
                var query2 = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                              .Where(acc => acc.Nickname.Equals(user.Nickname))
                              .AsEnumerable()
                              .FirstOrDefault();
                if(query == null)
                await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                    user);
                else
                {
                    throw new ApplicationException("Nickname already taken.");
                }
            }
            else
            {
                throw new ApplicationException("Incorrect ID or Password.");
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
        public User GetAccountData(string l, string p)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(l))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query != null && query.Password.Equals(p))
            {
                return query;
            }
            else
                throw new ApplicationException("Incorrect Login/Password.");
        }
        public string GetAccountId(string username)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(username))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query != null)
            {
                return query.Id;
            }
            else
                throw new ApplicationException("Incorrect Nickname.");
        }

        internal List<User> SearchUser(string searchdata)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Contains(searchdata))
                          .ToList();
            if (query == null)
                throw new ApplicationException("Users Not Found");
            for(int i = 0; i < query.Count; i++)
            {
                query[i].Password = null;
            }
            return query;

        }

        public bool IsValidAcccount(string nickname, string password)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            return query != null && query.Password.Equals(password);
        }

        public async void Follow(string nickname, string password, string follownick)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            var follow = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(follownick))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query == null || !query.Password.Equals(password) || follow == null)
                throw new ApplicationException("Can't find user.");
            if (query.Follows.IndexOf(nickname) == -1)
            {
                query.Follows.Add(follow.Nickname);
                follow.Followers.Add(query.Nickname);
            }
            await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                    query);
            await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, follow.Id),
                    follow);
        }

        public async void UnFollow(string nickname, string password, string follownick)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            var follow = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(follownick))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query == null || !query.Password.Equals(password) || follow == null)
                throw new ApplicationException("Can't find user.");
            if (!query.Follows.Remove(follow.Nickname) && !follow.Followers.Remove(query.Nickname))
            {
                throw new ApplicationException("Cant unfollow");
            }
            
            await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                    query);
            await client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(databaseId, collectionId, follow.Id),
                    follow);
        }
        public User protectedAccountData(string nickname)
        {
            var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                          .Where(user => user.Nickname.Equals(nickname))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query == null)
                throw new ApplicationException("Wrong Nickname.");
            return query;
        }
    }
}
