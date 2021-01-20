using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JournalHelper.Search
{
    public class SearchHelper
    {

        // HACK:  Find a good and elegant way of loading existing Index Json using serializer method
        public static IEnumerable<SearchField> LoadFieldsFromJSonFile(string jsonFile)
        {
            string jsonText = File.ReadAllTextAsync(jsonFile).ConfigureAwait(false).GetAwaiter().GetResult();
            JObject json = JObject.Parse(jsonText);

            List<SearchField> sFields = new List<SearchField>();
            foreach (var field in json["fields"])
            {
                sFields.Add(CreateSearchField(field));
            }
            return sFields;
        }

        public static SearchField CreateSearchField(JToken field)
        {

            SearchField sField = new SearchableField((string)field["name"], field.GetType().IsArray);
            if (field["facetable"] != null)
                sField.IsFacetable = (bool)field["facetable"];
            if (field["filterable"] != null)
                sField.IsFilterable = (bool)field["filterable"];
            if (field["retrievable"] != null)
                sField.IsHidden = !(bool)field["retrievable"];
            if (field["sortable"] != null)
                sField.IsSortable = (bool)field["sortable"];
            if (field["key"] != null)
            {
                sField.IsKey = (bool)field["key"];
            }
            if (field["searchable"] != null)
                sField.IsSearchable = (bool)field["searchable"];
            // sField.SearchAnalyzerName =  field["searchAnalyzer"] == null ? "" : (string)field["searchAnalyzer"];
            // sField.IndexAnalyzerName = field["indexAnalyzer"] == null ? "" : (string)field["indexAnalyzer"];
            // sField.AnalyzerName = (string)field["analyzer"];

            return sField;
        }

        public static async Task DeleteSearchResourcesAsync(string key, string svcUrl, string indexName)
        {
            var indexClient = Search.GetSearchIndexClient(key, svcUrl);
            await DeleteIndexIfExistsAsync(indexName, indexClient);

            var indexerClient = Search.GetSearchIndexerClient(key, svcUrl);
            
            var datasourceNames = await indexerClient.GetDataSourceConnectionNamesAsync();
            foreach (var datasourcename in datasourceNames.Value)
            {
                await indexerClient.DeleteDataSourceConnectionAsync(datasourcename);
            }

            var skillSetNames = await indexerClient.GetSkillsetNamesAsync();
            foreach(var skillsetname in skillSetNames.Value)
            {
                await indexerClient.DeleteSkillsetAsync(skillsetname);
            }

            await DeleteIndexerIfExistsAsync(indexName + "-indexer", indexerClient);
        }

        public static async Task DeleteIndexIfExistsAsync(string name, SearchIndexClient idxclient)
        {
            Console.WriteLine($"Deleting search index {name}...");

            idxclient.GetIndexNames();
            {
                await idxclient.DeleteIndexAsync(name);
            }
        }

        public static async Task DeleteDataSourceIfExistsAsync(string name, SearchIndexerClient idxclient)
        {
            Console.WriteLine($"Deleting search indexer datasource {name}...");

            await idxclient.GetDataSourceConnectionNamesAsync();
            {
                await idxclient.DeleteDataSourceConnectionAsync(name);
            }
        }

        public static async Task DeleteSkillSetIfExistsAsync(string name, SearchIndexerClient idxclient)
        {
            Console.WriteLine($"Deleting search indexer datasource {name}...");

            await idxclient.GetSkillsetNamesAsync();
            {
                await idxclient.DeleteSkillsetAsync(name);
            }
        }

        public static async Task DeleteIndexerIfExistsAsync(string name, SearchIndexerClient idxclient)
        {
            Console.WriteLine($"Deleting search indexer {name}...");

            await idxclient.GetIndexerNamesAsync();
            {
                await idxclient.DeleteIndexerAsync(name);
            }
        }


    }
}
