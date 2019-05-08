using IDO_API.Extensions;
using IDO_API.Models;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDO_API.DataBase.CosmosDB
{
    public class ContentManager
    {
        static ContentManager defaultInstance = new ContentManager();
#if DEBUG
        const string accountURL = @"https://localhost:8081";
        const string accountKey = @"C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        const string databaseId = @"IDO";
        const string collectionId = @"content";
#else
        const string accountURL = @"https://pidstagram.documents.azure.com:443/";
        const string accountKey = @"JYfaV28KzEQEOr8jzJdwojr3TBb6eu9PBvLbL0sj0quyZahWE3TeuWyFAhFQj3RotcvgQo9cj91ZEzmkMonXOg==";
        const string databaseId = @"IDO";
        const string collectionId = @"content";
#endif

        private Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        private DocumentClient client;

        public ContentManager()
        {
            try
            {
                client = new DocumentClient(new System.Uri(accountURL), accountKey);
                client.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database() { Id = databaseId });
                client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId),
                     new Microsoft.Azure.Documents.DocumentCollection()
                     {
                         Id = collectionId,
                         PartitionKey = new Microsoft.Azure.Documents.PartitionKeyDefinition() { Paths = new System.Collections.ObjectModel.Collection<string>(new List<string> { "/" + collectionId }) }
                     });
                client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId),
                     new Microsoft.Azure.Documents.DocumentCollection()
                     {
                         Id = "reports",
                         PartitionKey = new Microsoft.Azure.Documents.PartitionKeyDefinition() { Paths = new System.Collections.ObjectModel.Collection<string>(new List<string> { "/" + "reps" }) }
                     });
            }
            catch (Exception)
            {

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
        public async Task<short> CreateContentDocumentAsync(string userId)
        {
            try
            {
                List<Note> notes = new List<Note>();

                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    new Content(
                        userId.RemoveGuidDelimiters(),
                        notes));
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<short> AddNoteAsync(string userId, Note note)
        {
            try
            {
                string documentId = userId.RemoveGuidDelimiters();
                var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 1 })
                    .Where(content => content.Id.Equals(documentId))
                              .AsEnumerable()
                              .FirstOrDefault();

                if (query == null)
                    throw new ApplicationException("Incorrect Note Data");

                query.notes.Add(note);
                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<short> DeleteNoteAsync(string userId, string note)
        {
            try
            {
                string documentId = userId.RemoveGuidDelimiters();

                var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 1 })
                              .Where(content => content.Id.Equals(documentId))
                              .AsEnumerable()
                              .FirstOrDefault();

                if (query == null)
                    throw new ApplicationException("Incorrect Note Data");

                query.notes.RemoveAll(x => x.ImageReference.Equals(note));

                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<short> ReplaceNoteAsync(string userId, string oldnoteref, Note newNote)
        {
            try
            {
                string documentId = userId.RemoveGuidDelimiters();

                var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId))
                              .Where(content => content.Id.Equals(documentId))
                              .AsEnumerable()
                              .FirstOrDefault();

                if (query == null)
                    throw new ApplicationException("Incorrect Note Data");

                query.notes.RemoveAll(x => x.ImageReference.Equals(oldnoteref));
                query.notes.Add(newNote);

                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }
        }
        public List<Note> GetNotes(string userId)
        {
            try
            {
                var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 1 })
                        .Where(content => content.Id.Equals(userId.RemoveGuidDelimiters()))
                        .AsEnumerable()
                        .FirstOrDefault();
                if (query == null)
                    throw new ApplicationException("Incorrect user ID");

                return query.notes;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }

        public List<Note> GetAllNotes()
        {
            try
            {
                var query = client.CreateDocumentQuery<Content>(
                        collectionLink,
                        new FeedOptions { MaxItemCount = 100 });
                List<Note> notes = new List<Note>();
                foreach (var i in query)
                    notes.AddRange(i.notes);
                return notes;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<bool?> AddOrDeleteRespectFromUser(string userFromNickname, string notename, string userToId)
        {
            try
            {
                bool IsRespected;

                string documentId = userToId.RemoveGuidDelimiters();

                var noteQuery = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 1 })
                              .Where(content => content.Id.Equals(documentId))
                              .AsEnumerable()
                              .FirstOrDefault()
                              .notes
                              .Where(x => x.ImageReference.Equals(notename))
                              .AsEnumerable()
                              .FirstOrDefault();

                var contentQuery = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), new FeedOptions { EnableCrossPartitionQuery = true, MaxItemCount = 1 })
                              .Where(content => content.Id.Equals(documentId))
                              .AsEnumerable()
                              .FirstOrDefault();

                int index = contentQuery.notes.FindIndex((x) => x.ImageReference.Equals(noteQuery.ImageReference));

                if (noteQuery == null || contentQuery == null || index == -1)
                    throw new ApplicationException("Incorrect Note Name");

                if (contentQuery.notes[index].Lukasers.IndexOf(userFromNickname) == -1)
                {
                    noteQuery.LukasCount++;
                    noteQuery.Lukasers.Add(userFromNickname);
                    IsRespected = true;
                }
                else
                {
                    noteQuery.LukasCount--;
                    noteQuery.Lukasers.Remove(userFromNickname);
                    IsRespected = false;
                }
                contentQuery.notes[index] = noteQuery;
                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, contentQuery.Id), contentQuery);
                return IsRespected;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return null;
            }
        }
        public async Task<short> ReportBug(string message)
        {
            try
            {

                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, "reports"),
                        new Report() { Message = message});
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

    }
}
