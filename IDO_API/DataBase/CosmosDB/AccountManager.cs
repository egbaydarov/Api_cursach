using IDO_API.DataBase.Hashing;
using IDO_API.Models;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<short> UpadateAccountInfoAsync(string oldPass, User user)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                      .Where(acc => acc.Id.Equals(user.Id))
                                      .AsEnumerable()
                                      .FirstOrDefault();

                if (query == null || !query.Password.Equals(oldPass))
                    throw new ApplicationException("Incorrect ID or Password.");

                var checkForUserExistQuery = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(acc => acc.Nickname.Equals(user.Nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();

                if (checkForUserExistQuery != null)
                    throw new ApplicationException("Nickname already taken.");

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                        user);
                return 0;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public async Task<short> CreateAccountAsync(User newAccount)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Equals(newAccount.Nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();


                if (query != null)
                    throw new ApplicationException("Account Already Exists.");

                await client.CreateDocumentAsync(collectionLink, newAccount);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }

        }
        public User GetAccountData(string nickname, string password)
        {
            try
            {
                string passHash = SHA.GenerateSaltedHashBase64(password);
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Equals(nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();
                if (query == null || !query.Password.Equals(passHash))
                    throw new ApplicationException("Incorrect Login/Password.");
                query.Password = password;
                return query;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
        public string GetAccountId(string username)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                 .Where(user => user.Nickname.Equals(username))
                                 .AsEnumerable()
                                 .FirstOrDefault();
                if (query == null)
                    throw new ApplicationException("Incorrect Nickname.");
                return query.Id;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        internal List<User> FindUserByNickname(string searchdata)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Contains(searchdata))
                                  .ToList();

                if (query == null)
                    throw new ApplicationException("Users Not Found");

                for (int i = 0; i < query.Count; i++)
                {
                    query[i].Password = null;
                }

                return query;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        public bool IsValidAcccount(string nickname, string passwordUnhased)
        {
            try
            {
                string password = SHA.GenerateSaltedHashBase64(passwordUnhased);
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Equals(nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();

                return query != null && query.Password.Equals(password);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<short> Follow(string nickname, string passwordUnhashed, string follownick)
        {
            try
            {
                string password = SHA.GenerateSaltedHashBase64(passwordUnhashed);
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
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<short> UnFollow(string nickname, string passwordUnhashed, string follownick)
        {
            try
            {

                string password = SHA.GenerateSaltedHashBase64(passwordUnhashed);
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
                    throw new ApplicationException("Cant unfollow");

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                        query);

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, follow.Id),
                        follow);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public User GetProtectedAccountData(string nickname)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Equals(nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();
                if (query == null)
                    throw new ApplicationException("Wrong Nickname.");
                query.Password = null;
                return query;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<short> AddGoal(string userNickname, string nickname, string description)
        {
            try
            {
                string id = GetAccountId(userNickname);
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(acc => acc.Id.Equals(id))
                                  .AsEnumerable()
                                  .FirstOrDefault();
                //TEMP!!!
                if (query.Goals == null)
                    query.Goals = new List<Goal>();

                if (query.Goals.Where(x => x.Nickname.Equals(nickname) && x.Description.Equals(description)).AsEnumerable()
                                  .FirstOrDefault() != null)
                    throw new ApplicationException("Goal Already Added");

                query.Goals.Add(new Goal { Description = description, Nickname = nickname });

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                        query);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<short> RemoveGoal(string userNickname, string nickname, string description)
        {
            try
            {
                string id = GetAccountId(userNickname);
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(acc => acc.Id.Equals(id))
                                  .AsEnumerable()
                                  .FirstOrDefault();

                if (query.Goals.Where(x => x.Nickname.Equals(nickname) && x.Description.Equals(description)).AsEnumerable()
                                  .FirstOrDefault() == null)
                    throw new ApplicationException("Goal doesn't exist");

                query.Goals.RemoveAll(x => x.Nickname.Equals(nickname) && x.Description.Equals(description));

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                        query);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public async Task<short> UploadAvatar(string nickname, string password, string avatarreference)
        {
            try
            {
                var query = client.CreateDocumentQuery<User>(collectionLink, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true })
                                  .Where(user => user.Nickname.Equals(nickname))
                                  .AsEnumerable()
                                  .FirstOrDefault();

                if (query == null)
                    throw new ApplicationException("Wrong users data");

                query.Avatar = avatarreference;

                await client.ReplaceDocumentAsync(
                        UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id),
                        query);

                return 0;

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
