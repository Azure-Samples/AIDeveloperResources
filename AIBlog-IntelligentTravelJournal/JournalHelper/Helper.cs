using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using JournalHelper.Search;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace JournalHelper
{
    public class Helper
    {

        static private BlobManager blobManager;

        static public void InitializeHelper(BlobManager bm)
        {
            blobManager = bm;
        }

        static public BlobManager GetBlobManager()
        {
            return blobManager;
        }
        // Get input WAV files from file store/folder
 
        static public IEnumerable<Uri> GetFileUris(string folderPath, string searchPattern="*.*", bool recurse=false)
        {
            List<Uri> audioUris = new List<Uri>();

            IEnumerable<string> audioFiles = Directory.EnumerateFiles(folderPath, searchPattern, recurse? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (string audioFile in audioFiles)
            {
                Uri uriAudio = new Uri(new Uri("file://"), audioFile);
                audioUris.Add(uriAudio);
            }
            return audioUris.ToArray();
        }

        static public async Task ExtractTranscribedTextfromJsonAsync(string sourceContainer, string blobFolder, string destContainer, string transcribedTextFolder, bool deleteSource=false)
        {
            // ensure the destination folder exists on file system
            Directory.CreateDirectory(transcribedTextFolder);


            var sContainer = await blobManager.GetBlobContainerAsync(sourceContainer);
            if (sContainer == null)
            {
                Console.WriteLine("ERROR:  Source container does not exist");
                return;
            }

            // get destination container, by creating a new one if not exist
            var dContainer = await blobManager.GetBlobContainerAsync(destContainer, true);

            var jsonBlobItems = await blobManager.GetBlobItemsInContainerAsync(sourceContainer);
            
            // NOTE: Hardcoded steps based on batch transcription results
            //  1. creates a root folder in cotainer quw(name is unknown) so pick the first one - hard coded
            CloudBlobDirectory blobDirectory = jsonBlobItems.FirstOrDefault<IListBlobItem>() as CloudBlobDirectory;
            
            // 2. under the root creates another folder with sourceContainer name
            jsonBlobItems = await blobManager.GetBlobItemsInFolderrAsync(blobDirectory);
            // filter with prefix name
            var query = from x in jsonBlobItems
                               where (x is CloudBlobDirectory) && ((CloudBlobDirectory)x).Prefix.EndsWith(blobFolder + "/")
                               select x;

            blobDirectory = query.FirstOrDefault<IListBlobItem>() as CloudBlobDirectory;

            // 3.  Now we can get the actual blobItems for us to process
            jsonBlobItems = await blobManager.GetBlobItemsInFolderrAsync(blobDirectory);
            foreach (var jsonItem in jsonBlobItems)
            {
                //NOTE:  Just in case, check for folders and skip
                if (jsonItem is CloudBlobDirectory) continue;

                //TODO: find a proper way to retrieve relative blob name from blobItem
                string blobName = Path.GetFileName(jsonItem.Uri.AbsolutePath);
                if (string.IsNullOrEmpty(blobName)) continue;
                Console.WriteLine(blobName);

                CloudBlockBlob blob = blobDirectory.GetBlockBlobReference($"{blobName}");
                // Get the blob file as text
                string contents = await blob.DownloadTextAsync();

                var jsonObject = JObject.Parse(contents);
                string transcribedText = jsonObject["combinedRecognizedPhrases"][0]["display"].ToString();
                string textFilePath = Path.Combine(transcribedTextFolder, Path.GetFileNameWithoutExtension(blobName)) + @".txt";
                File.WriteAllText(textFilePath, transcribedText);

                var destBlob = dContainer.GetBlockBlobReference(Path.GetFileName(textFilePath));
                await destBlob.UploadFromFileAsync(textFilePath);
            }

            // cleanup
            // delete the container that has the transcribed json result files due to branch transcription
            if (deleteSource)
            {
                await sContainer.DeleteAsync();
            }
        }

        static public async Task CreateTravelJournal(IEnumerable<string> inputFiles, string destFolder, string journalName)
        {
            string journalFilePath = Path.Combine(destFolder, journalName) + @".txt";
            Directory.CreateDirectory(destFolder);
            StringBuilder content = new StringBuilder();
            foreach (string filePath in inputFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileContent = await File.ReadAllTextAsync(filePath);
                
                content.AppendLine(fileName);
                content.AppendLine(fileContent);
                content.AppendLine();
                content.AppendLine();
            }
            await File.AppendAllTextAsync(journalFilePath, content.ToString());

        }

        static public async Task CreateTravelJournal(string searchString, IEnumerable<SearchDocument> inputDocs, string destFolder, string journalName)
        {
            string journalFilePath = Path.Combine(destFolder, journalName) + @".txt";
            Directory.CreateDirectory(destFolder);

            Console.WriteLine($"Writing the journal file {journalFilePath}...");

            // save search string for reference
            await File.WriteAllTextAsync(journalFilePath, $"Search String:  {searchString}");

            StringBuilder content = new StringBuilder();
            foreach (var doc in inputDocs)
            {
                JObject jsonObject = JObject.Parse(doc.ToString());

                string fileName = (string)jsonObject["metadata_storage_name"];
                string fileContent = (string)jsonObject["content"];

                content.AppendLine(fileName);
                content.AppendLine(fileContent);
                content.AppendLine();
                content.AppendLine();
            }
            await File.AppendAllTextAsync(journalFilePath, content.ToString());

        }


        // text analytics
        public static void CreateTagsForFolderItems(string key, string endpoint, string batchTranscribedFolder, string extractedTagsFolder)
        {
            if (!Directory.Exists(batchTranscribedFolder))
            {
                Console.WriteLine("Input folder for transcribed files does not exist");
                return;
            }

            // ensure destination folder path exists
            Directory.CreateDirectory(extractedTagsFolder);


            TextAnalyticsClient textClient = TextAnalytics.GetClient(key, endpoint);

            var contentFiles = Directory.EnumerateFiles(batchTranscribedFolder);
            foreach(var contentFile in contentFiles)
            {
                var tags = TextAnalytics.GetTags(textClient, contentFile).ConfigureAwait(false).GetAwaiter().GetResult();

                // generate output file with tags 
                string outFileName = Path.GetFileNameWithoutExtension(contentFile);
                outFileName += @"_tags.txt";
                string outFilePath = Path.Combine(extractedTagsFolder, outFileName);
                File.WriteAllLinesAsync(outFilePath, tags).Wait() ;
            }
        }


        public static void UploadFolderToContainer(string folder, string container, bool deleteExistingContainer=false)
        {
            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Source folder {folder} does not exist.");
                return;
            }

            blobManager.UploadDirectory(folder, container, deleteExistingContainer);

        }

        // speech sdk
        public static async Task BatchTranscribeVoiceMemosAsync(string sourceContainer, string destinationContainer, string speechKey, string speechRegion)
        {
            Uri sourceContainerUri = await blobManager.GetContainerSASUriWithoutStoredPolicyAsync(sourceContainer);
            if (sourceContainerUri == null)
            {
                Console.WriteLine($"Source Container does not exist");
                return;
            }

            // Get Service SAS Uri for destination folder  - otherwise transcription api fails
            Uri destinationContainerUri = await blobManager.GetContainerSASUriWithoutStoredPolicyAsync(destinationContainer, true);
            Console.WriteLine(destinationContainerUri.ToString());

            List<Uri> containerUris = new List<Uri>();
            containerUris.Add(sourceContainerUri);
            containerUris.Add(destinationContainerUri);

            await BatchTranscription.RunBatchTranscribeAsync(
                speechKey, speechRegion,
                containerUris.ToArray(), true);

        }

        public static void SetPhotoTagsForFile(string photoFilePath, string tagsFilePath)
        {
            List<string> tags = File.ReadAllLinesAsync(tagsFilePath).Result.ToList<string>();

            ImageProperties.SetPhotoTags(photoFilePath, tags);
        }

        // There is no relationship stored to correlate tags file name with photo file.
        // Here we are assuming the filename for photo was used to generate corresponding tagfilename.
        // If there are multiple voice memos for a single photo the filename prefix is common between them.
        // We retrieve all matching tag files, append the tags from all these sources and set tags to the photofile

        // also, we are copying the modified photofiles to a destination folder to preserve the input files.
        public static void SetPhotoTagsForFolder(string inputPhotosFolder, string tagsFolder, string OutPhotosFolder)
        {
            if (!Directory.Exists(inputPhotosFolder) || !Directory.Exists(tagsFolder))
            {
                Console.WriteLine("Either of the input photos folder or tags folder does not exist");
                return;
            }

            // make sure outFolder exist
            Directory.CreateDirectory(OutPhotosFolder);

            var photoFiles = Directory.EnumerateFiles(inputPhotosFolder);
            var photoTagFiles = Directory.EnumerateFiles(tagsFolder);
            foreach (var photoFile in photoFiles)
            {
                Console.WriteLine($"PhotoFileName:  {photoFile}");

                string filename = Path.GetFileNameWithoutExtension(photoFile);
                var query = from tagFile in photoTagFiles
                            where Path.GetFileName(tagFile).StartsWith(filename)
                            select tagFile;

                List<string> tags = new List<string>();
                foreach (var tagFile in query)
                {
                    Console.WriteLine($"\t\t TAGFILE: {tagFile}");

                    var tagsInFile = File.ReadAllLinesAsync(tagFile).GetAwaiter().GetResult().ToList<string>();
                    tags.AddRange(tagsInFile);
                }

                //string taggedPhotoFile = Path.Combine(OutPhotosFolder, Path.GetFileName(photoFile));
                string taggedPhotoFile = photoFile.Replace(inputPhotosFolder, OutPhotosFolder);
                File.Copy(photoFile, taggedPhotoFile, true);

                if (tags.Count > 0)
                {
                    ImageProperties.SetPhotoTags(taggedPhotoFile, tags);
                }

            }
        }

        public static async Task ProcessImageAsync(string key, string endpoint, Uri imageUri, string outFolder)
        {
            ComputerVisionClient client = ComputerVision.Authenticate(endpoint, key);
            await ProcessImageAsync(client, imageUri, outFolder);
        }

        public static async Task ProcessImagesFromFolderAsync(string key, string endpoint, string sourceFolder, string outFolder)
        {
            ComputerVisionClient client = ComputerVision.Authenticate(endpoint, key);
 
            IEnumerable<Uri> fileUris = GetFileUris(sourceFolder);
            foreach(var fileUri in fileUris)
            {
                await ProcessImageAsync(client, fileUri, outFolder);
            }
        }

        public static async Task ProcessImageAsync(ComputerVisionClient client, Uri imageUri, string outFolder)
        {
            string fileName = Path.GetFileName(imageUri.AbsolutePath);
            string outFilePath = Path.Combine(outFolder, fileName);
            Directory.CreateDirectory(outFolder);

            // Analyze
            var features = ComputerVision.GetSampleVisualFeatureTypes();
            var json = ComputerVisionHelper.GetJson(await ComputerVision.AnalyzeImageUri(client, imageUri, features));
            WriteToJsonFile<JObject>(outFilePath + "-AnalyzeResults.json", json);

            // Detect Domain Specific "landmarks as an example"
            var domainspecific = await ComputerVision.DetectDomainSpecific(client, imageUri, "landmarks");
            WriteToJsonFile<JObject>(outFilePath + "-DomainSpecific.json", domainspecific);

            // Read Image  - text detection
            var readResult = ComputerVisionHelper.GetReadResultLines(
                                await ComputerVision.ReadImageAsync(client, imageUri));
            await File.WriteAllLinesAsync(outFilePath + "-ReadResults.txt", readResult);


            // OCR - text extraction
            var ocrResult = await ComputerVision.RecognizeTextFromImageUri(client, imageUri, false);
            var ocrLineTexts = ComputerVisionHelper.GetOcrResultLineTexts(ocrResult);
            await File.WriteAllLinesAsync(outFilePath + "-ReadOcrResults.txt", ocrLineTexts);


        }

        public static async Task<string> TranslateFileContent(string key, string endpoint, string filePath, string toLanguages)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string inputText = await File.ReadAllTextAsync(filePath);

            string translatedText = await Translator.Translate.TranslateTextRequestAsync(key, endpoint, toLanguages, inputText);
            return translatedText;
        }

        public static async Task CreateSearchIndexerAsync(
                string serviceAdminKey, string searchSvcUrl,
                string cognitiveServiceKey,
                string indexName, string jsonFieldsFilePath,
                string blobConnectionString, string blobContainerName
                )
        {
            // Its a temporary arrangment.  This function is not complete
            IEnumerable<SearchField> fields = SearchHelper.LoadFieldsFromJSonFile(jsonFieldsFilePath);

            // create index
            var searchIndex = await Search.Search.CreateSearchIndexAsync(serviceAdminKey, searchSvcUrl, indexName, fields.ToList());

            // get indexer client
            var indexerClient = Search.Search.GetSearchIndexerClient(serviceAdminKey, searchSvcUrl);

            // create azure blob data source
            var dataSource = await Search.Search.CreateOrUpdateAzureBlobDataSourceAsync(indexerClient, blobConnectionString, indexName, blobContainerName);

            // create indexer

            // create skill set with minimal skills
            List<SearchIndexerSkill> skills = new List<SearchIndexerSkill>();
            skills.Add(Skills.CreateEntityRecognitionSkill());
            skills.Add(Skills.CreateLanguageDetectionSkill());

            var skillSet = await Search.Search.CreateOrUpdateSkillSetAsync(indexerClient,
                                    indexName + "-skillset", skills, cognitiveServiceKey);

            var indexer = await Search.Search.CreateIndexerAsync(indexerClient, dataSource, skillSet, searchIndex);

            // wait for some time to have indexer run and load documents
            Thread.Sleep(TimeSpan.FromSeconds(20));

            await Search.Search.CheckIndexerOverallStatusAsync(indexerClient, indexer);
        }


        public static async Task<IEnumerable<T>> SearchAsync<T>(
            string searchSvcUrl, string searchQueryKey,
            string searchQueryText, string indexName
            )
        {
            var searchClient = Search.SearchQuery.GetQueryClient(searchQueryKey, searchSvcUrl, indexName);

            var documents = await Search.SearchQuery.GetSearchDocumentsAsync<T>(searchClient, searchQueryText);
            return documents;
        }

        public static IEnumerable<SearchDocument> SearchDocuments(
            string searchSvcUrl, string searchQueryKey,
            string searchQueryText, string indexName
            )
        {
            var searchClient = Search.SearchQuery.GetQueryClient(searchQueryKey, searchSvcUrl, indexName);

            var documents = Search.SearchQuery.GetSearchDocuments(searchClient, searchQueryText);

            Console.WriteLine($"\t\tFound {documents.Count()} documents during search.");

            return documents;
        }

        internal void ExecuteWithExceptionHandling(Action action)
        {
            try
            {
                action();
            }
            catch (StorageException e)
            {
                //if ((int)ex.RequestInformation. != 409)
                //{
                //    throw;
                //}
                Console.WriteLine($"Exception: {e.RequestInformation.ErrorCode} {e.Message}");
                throw;
            }
        }

        internal void ExecuteWithExceptionHandlingNoRethrow(Action action)
        {
            try
            {
                action();
            }
            catch (StorageException e)
            {
                //if ((int)ex.RequestInformation. != 409)
                //{
                //    throw;
                //}
                Console.WriteLine($"Exception: {e.RequestInformation.ErrorCode} {e.Message}");
            }
        }

        internal bool ExecuteWithExceptionHandlingAndReturnValue(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (StorageException e)
            {
                //if ((int)ex.StatusCode == 409)
                //{
                //    return false;
                //}
                //throw;

                Console.WriteLine($"Exception: {e.RequestInformation.ErrorCode} {e.Message}");
                return false;

            }
        }

        internal static void PromptForConfirmationAndExecute(string message, Action action)
        {
            Console.WriteLine(message);
            ConsoleKeyInfo cki = Console.ReadKey();
            if (cki.Key.ToString().ToLower() == "y")
            {
                action();
            }
        }

        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static async void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
            if (append)
                await File.AppendAllTextAsync(filePath, contentsToWriteToFile);
            else
                await File.WriteAllTextAsync(filePath, contentsToWriteToFile);

        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public async static Task<T> ReadFromJsonFile<T>(string filePath) where T : new()
        {
            string contents = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(contents);
        }

    }
}
