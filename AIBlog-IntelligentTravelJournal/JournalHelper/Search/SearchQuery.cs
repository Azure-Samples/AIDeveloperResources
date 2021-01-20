using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace JournalHelper.Search
{
    public class SearchQuery
    {

        public static SearchClient GetQueryClient(string key, string serviceUrl, string indexName)
        {
            // Create a client
            AzureKeyCredential credential = new AzureKeyCredential(key);
            Uri endpointUri = new Uri(serviceUrl);
            SearchClient client = new SearchClient(endpointUri, indexName, credential);
            return client;
        }

        public static IEnumerable<SearchDocument> GetSearchDocuments(SearchClient client, string queryString, SearchOptions options = null)
        {
            SearchResults<SearchDocument> response = client.Search<SearchDocument>(queryString, options);

            var documents = response.GetResults().Select(result => result.Document);

            return documents;
        }

        public static async Task<IEnumerable<T>> GetSearchDocumentsAsync<T>(SearchClient client, string query, SearchOptions options=null)
        {
            
            List<T> documents = new List<T>();

            SearchResults<T> response = await client.SearchAsync<T>(query);
            await foreach (SearchResult<T> result in response.GetResultsAsync())
            {
                documents.Add(result.Document);
            }

            //documents = client.GetDocumentAsync<T>();
            return documents;

        }

        public static void PrintDocumentDetails(IEnumerable<SearchDocument> documents)
        {
            foreach (SearchDocument document in documents)
            {
                // Print out the title and job description (we'll see below how to
                // use C# objects to make accessing these fields much easier)
                string title = (string)document["business_title"];
                string description = (string)document["job_description"];
                Console.WriteLine($"{title}\n{description}\n");
            }
        }
    }
}
