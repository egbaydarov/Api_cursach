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
        const string accountURL = @"https://localhost:8081";
        const string accountKey = @"C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        const string databaseId = @"IDO";
        const string collectionId = @"content";

        private Uri collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

        private DocumentClient client;

        public ContentManager()
        {
            client = new DocumentClient(new System.Uri(accountURL), accountKey);
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
        public async Task CreateContentDocumentAsync(string userId)
        {
            List<Note> notes = new List<Note>();
            
            await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                new Content(
                    userId.RemoveGuidDelimiters(),
                    notes));
        }

        public async Task AddNoteAsync(string userId, Note note)
        {
            string documentId = userId.RemoveGuidDelimiters();
            var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId,collectionId),new FeedOptions {EnableCrossPartitionQuery = true,MaxItemCount = 1})
                .Where(content => content.Id.Equals(documentId))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query != null)
            {
                query.notes.Add(note);
                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);
            }
            else throw new ApplicationException("Incorrect User Data");
        }

        public async Task DeleteNoteAsync(string userId, Note note)
        {
            string documentId = userId.RemoveGuidDelimiters();
            var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId))
                          .Where(content => content.Id.Equals(documentId))
                          .AsEnumerable()
                          .FirstOrDefault();
            query.notes.RemoveAll(x => x.ImageReference.Equals(note.ImageReference) && x.Description.Equals(note.Description));
            await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);
        }

        public async Task ReplaceNoteAsync(string userId, string oldnoteref, Note newNote)
        {
            string documentId = userId.RemoveGuidDelimiters();
            var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId))
                          .Where(content => content.Id.Equals(documentId))
                          .AsEnumerable()
                          .FirstOrDefault();
            query.notes.RemoveAll(x => x.ImageReference.Equals(oldnoteref));
            query.notes.Add(newNote);
            await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, query.Id), query);
        }
        public List<Note> GetNotes(string userId)
        {
            var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId))
                .Where(content => content.Id.Equals(userId.RemoveGuidDelimiters()))
                .AsEnumerable()
                .FirstOrDefault();
            if (query == null)
                throw new ApplicationException("Incorrect user ID");
            else
                return query.notes;
        }
        public Note GetSingleNote(string userId, string notename)
        {
            string documentId = userId.RemoveGuidDelimiters();
            var query = client.CreateDocumentQuery<Content>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId))
                          .Where(content => content.Id.Equals(documentId))
                          .AsEnumerable()
                          .FirstOrDefault()
                          .notes
                          .Where(x => x.ImageReference.Equals(notename))
                          .AsEnumerable()
                          .FirstOrDefault();
            if (query != null)
                return query;
            else
                throw new ApplicationException("Incorrect Note Data.");

        }
    }
}
