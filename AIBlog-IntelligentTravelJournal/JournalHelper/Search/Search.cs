using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JournalHelper.Search
{
    public class Search
    {

        public static SearchIndexClient GetSearchIndexClient(string adminApiKey, string searchSvcUrl)
        {
            return new SearchIndexClient(new Uri(searchSvcUrl), new AzureKeyCredential(adminApiKey));
        }

        public static SearchIndexerClient GetSearchIndexerClient(string adminApiKey, string searchSvcUrl)
        {
            return new SearchIndexerClient(new Uri(searchSvcUrl), new AzureKeyCredential(adminApiKey));
        }

        //public static SearchIndex CreateSearchIndex(string key, string endpoint, string indexName, IList<SearchField> fields)
        //{
        //    // Create a service client
        //    AzureKeyCredential credential = new AzureKeyCredential(key);
        //    SearchIndexClient client = new SearchIndexClient(new Uri(endpoint), credential);

        //    // Create the index using FieldBuilder.
        //    SearchIndex index = new SearchIndex(indexName, fields);

        //    return client.CreateIndex(index).Value;



        public static async Task<SearchIndex> CreateSearchIndexAsync(string key, string endpoint, string indexName, IList<SearchField> fields)
        {
            // Create a service client
            AzureKeyCredential credential = new AzureKeyCredential(key);
            SearchIndexClient client = new SearchIndexClient(new Uri(endpoint), credential);

            // Create the index using FieldBuilder.
            SearchIndex index = new SearchIndex(indexName, fields);

            try
            {
                var result = await client.CreateOrUpdateIndexAsync(index);
                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create index\n Exception message: {0}\n", ex.Message);
                ExitProgram("Cannot continue without a search index");
            }
            return null;
        }

        private static void ExitProgram(string message)
        {
            Console.WriteLine("{0}", message);
            Console.WriteLine("Press any key to exit the program...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static async Task<SearchIndexerDataSourceConnection> CreateOrUpdateAzureBlobDataSourceAsync(SearchIndexerClient indexerClient,
                string blobConnectionString, string indexName, string blobContainerName)
        {
            SearchIndexerDataSourceConnection dataSource = new SearchIndexerDataSourceConnection(
                name: indexName + "-azureblobdatasource",
                type: SearchIndexerDataSourceType.AzureBlob,
                connectionString: blobConnectionString,
                container: new SearchIndexerDataContainer(blobContainerName))
            {
                Description = "Files to demonstrate cognitive search capabilities."
            };

            // The data source does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                await indexerClient.CreateOrUpdateDataSourceConnectionAsync(dataSource);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create or update the data source\n Exception message: {0}\n", ex.Message);
                ExitProgram("Cannot continue without a data source");
            }

            return dataSource;
        }

        public static async Task<SearchIndexerSkillset> CreateOrUpdateSkillSetAsync(SearchIndexerClient indexerClient, string skillSetName, IList<SearchIndexerSkill> skills, string cognitiveServicesKey)
        {

            SearchIndexerSkillset skillset = new SearchIndexerSkillset(skillSetName, skills)
            {
                Description =  "skillsets",

                CognitiveServicesAccount = new CognitiveServicesAccountKey(cognitiveServicesKey)
            };

            // Create the skillset in your search service.
            // The skillset does not need to be deleted if it was already created
            // since we are using the CreateOrUpdate method
            try
            {
                await indexerClient.CreateOrUpdateSkillsetAsync(skillset);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Failed to create the skillset\n Exception message: {0}\n", ex.Message);
                ExitProgram("Cannot continue without a skillset");
            }

            return skillset;
        }

        public static async Task<SearchIndexer> CreateIndexerAsync(SearchIndexerClient indexerClient, SearchIndexerDataSourceConnection dataSource, SearchIndexerSkillset skillSet, SearchIndex index)
        {
            IndexingParameters indexingParameters = new IndexingParameters()
            {
                MaxFailedItems = -1,
                MaxFailedItemsPerBatch = -1,
            };
            indexingParameters.IndexingParametersConfiguration = new IndexingParametersConfiguration();
            indexingParameters.IndexingParametersConfiguration.DataToExtract = BlobIndexerDataToExtract.ContentAndMetadata;
            indexingParameters.IndexingParametersConfiguration.ParsingMode = BlobIndexerParsingMode.Text;

            string indexerName = index.Name +"-indexer";
            SearchIndexer indexer = new SearchIndexer(indexerName, dataSource.Name, index.Name)
            {
                Description = index.Name + " Indexer",
                SkillsetName = skillSet.Name,
                Parameters = indexingParameters
            };

            FieldMappingFunction mappingFunction = new FieldMappingFunction("base64Encode");
            mappingFunction.Parameters.Add("useHttpServerUtilityUrlTokenEncode", true);

            indexer.FieldMappings.Add(new FieldMapping("metadata_storage_path")
            {
                TargetFieldName = "metadata_storage_path",
                MappingFunction = mappingFunction

            });

            //indexer.FieldMappings.Add(new FieldMapping("metadata_storage_name")
            //{
            //    TargetFieldName = "FileName"
            //});

            //indexer.FieldMappings.Add(new FieldMapping("content")5
            //{
            //    TargetFieldName = "Content"
            //});

            //indexer.OutputFieldMappings.Add(new FieldMapping("/document/pages/*/organizations/*")
            //{
            //    TargetFieldName = "organizations"
            //});
            //indexer.OutputFieldMappings.Add(new FieldMapping("/document/pages/*/keyPhrases/*")
            //{
            //    TargetFieldName = "keyPhrases"
            //});
            //indexer.OutputFieldMappings.Add(new FieldMapping("/document/languageCode")
            //{
            //    TargetFieldName = "languageCode"
            //});

            try
            {
                await indexerClient.GetIndexerAsync(indexer.Name);
                await indexerClient.DeleteIndexerAsync(indexer.Name);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                //if the specified indexer not exist, 404 will be thrown.
            }

            try
            {
                await indexerClient.CreateIndexerAsync(indexer);
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Failed to create the indexer\n Exception message: {0}\n", ex.Message);
                ExitProgram("Cannot continue without creating an indexer");
            }

            return indexer;
        }

        public static async Task CheckIndexerOverallStatusAsync(SearchIndexerClient indexerClient, SearchIndexer indexer)
        {
            try
            {
                var demoIndexerExecutionInfo = await indexerClient.GetIndexerStatusAsync(indexer.Name);

                switch (demoIndexerExecutionInfo.Value.Status)
                {
                    case IndexerStatus.Error:
                        ExitProgram("Indexer has error status. Check the Azure Portal to further understand the error.");
                        break;
                    case IndexerStatus.Running:
                        Console.WriteLine("Indexer is running");
                        break;
                    case IndexerStatus.Unknown:
                        Console.WriteLine("Indexer status is unknown");
                        break;
                    default:
                        Console.WriteLine("No indexer information");
                        break;
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Failed to get indexer overall status\n Exception message: {0}\n", ex.Message);
            }
        }
    }
}
