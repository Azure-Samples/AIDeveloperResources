using Azure.AI.TextAnalytics;
using JournalHelper;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentTravelJournal
{
    class Scenarios
    {
        private RootCustomSettings customSettings;

        public Scenarios(RootCustomSettings rcs)
        {
            customSettings = rcs;
        }

        internal void TranscribeAndExtractTagsForAGivenFile()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                "Press Y to Continue with single file contigous transciption or any key to skip...",
                () =>
                {
                    string inputPath = HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.VoiceMemosFolder);
                    string inputFile = Path.Combine(inputPath, customSettings.SampleIndividualFiles.SampleVoiceMemoFile);

                    // transcribing speech - a single file case scenario
                    IndividualFileTranscribe.TranscribeSpeechFileAsync(
                        customSettings.SpeechConfigSettings.Key, customSettings.SpeechConfigSettings.Region,
                        inputFile,
                        HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.TranscribedFileFolder)).Wait();
                });

            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to extract tags from a single file  or any key to skip...",
                    () =>
                    {
                        string outPath = HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.TranscribedFileFolder);
                        string outFile = Path.Combine(outPath, customSettings.SampleIndividualFiles.SampleVoiceMemoFile) + ".txt";

                    TextAnalyticsClient textClient = TextAnalytics.GetClient(
                       customSettings.TextAnalyticsSettings.Key,
                       customSettings.TextAnalyticsSettings.Endpoint);

                        var tags = TextAnalytics.GetTags(textClient, outFile).ConfigureAwait(false).GetAwaiter().GetResult();

                        Console.WriteLine(string.Join(", ", tags));
                    });

        }


        // Scenario to illustrate Speech-to-Text apis
        internal void TranscribeVoiceMemosInAFolder(bool deleteExistingContainer=false)
        {
            //NOTE: for the following batch transcription we are passing the source and destination
            // blob container urls as an array and later interpret them based on boolean value passed.

            // upload voice memos to azure blob container
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue with uploading and branch transcribe or any key to skip...",
                    () => {
                        TranscribeVoiceMemosFolder(deleteExistingContainer).Wait();
                    });

            //HelperFunctions.PromptForConfirmationAndExecute(
            //        "Press Y to Continue with Batch Transcription for blob containers or any key to skip...",
            //        () =>
            //        {
            //            Helper.BatchTranscribeVoiceMemos(
            //                customSettings.AzureBlobContainers.InputVoiceMemoFiles,
            //                customSettings.AzureBlobContainers.BatchTranscribedJsonResults,
            //                customSettings.SpeechConfigSettings.Key,
            //                customSettings.SpeechConfigSettings.Region
            //                );
            //        });

            //HelperFunctions.PromptForConfirmationAndExecute(
            //        "Press Y to Continue to transcribe text from blob containers or any key to skip...",
            //        () =>
            //        {
            //            Helper.ExtractTranscribedTextfromJsonAsync(
            //                customSettings.AzureBlobContainers.BatchTranscribedJsonResults,
            //                customSettings.AzureBlobContainers.ExtractedTranscribedTexts,
            //                customSettings.SampleDataFolders.BatchTranscribedFolder, true).Wait();
            //        });

        }

    internal async Task TranscribeVoiceMemosFolder(bool deleteExistingContainer = false)
    {        
            Console.WriteLine("Uploading voice memos folder to blob container...");

            Helper.UploadFolderToContainer(
                HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.VoiceMemosFolder),
                customSettings.AzureBlobContainers.InputVoiceMemoFiles, deleteExistingContainer);

            Console.WriteLine("Branch Transcribing voice memos using containers...");
            //NOTE: Turn the pricing tier for Speech Service to standard for this below to work.

            await Helper.BatchTranscribeVoiceMemosAsync(
                customSettings.AzureBlobContainers.InputVoiceMemoFiles,
                customSettings.AzureBlobContainers.BatchTranscribedJsonResults,
                customSettings.SpeechConfigSettings.Key,
                customSettings.SpeechConfigSettings.Region
                );

            Console.WriteLine("Extract transcribed text files into another container and folder, delete the intermediate container with json files...");

            await Helper.ExtractTranscribedTextfromJsonAsync(
                customSettings.AzureBlobContainers.BatchTranscribedJsonResults,
                customSettings.AzureBlobContainers.InputVoiceMemoFiles,
                customSettings.AzureBlobContainers.ExtractedTranscribedTexts,
                HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.BatchTranscribedFolder), true);
        }

        // Scenario to illustrate TextAnalytics api - entity and keyphrase extractions
        internal void CreateTagsForFolderItems()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                   "Press Y to Continue to extract tags from a file folder  or any key to skip...",
                   () =>
                   {
                       Helper.CreateTagsForFolderItems(
                           customSettings.TextAnalyticsSettings.Key,
                           customSettings.TextAnalyticsSettings.Endpoint,
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.BatchTranscribedFolder),
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.ExtractedTagsFolder));
                   });


        }

        // Scenario to update photo file metadata with tags  - photo tagging
        internal void UpdateTagsToPhotoFiles()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to set tags to photos from a file folder  or any key to skip...",
                    () =>
                    {
                        Helper.SetPhotoTagsForFolder(
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.PhotosToTagFolder),
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.ExtractedTagsFolder),
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.TaggedPhotosFolder));
                    });
        }

        internal void CreateTravelJournal(string sourcefolder, string journalFileName)
        {
            if (!Directory.Exists(sourcefolder))
            {
                Console.WriteLine("Directory does not exist.  Journal is not created");
                return;
            }

            CreateTravelJournal(Directory.EnumerateFiles(sourcefolder), journalFileName);

        }
        // grouping related transcribed memos together
        internal void CreateTravelJournal(IEnumerable<string> files, string journalFileName)
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    $"Press Y to Continue to create travel journal {journalFileName} from input files or any key to skip...",
                    () =>
                    {
                        Helper.CreateTravelJournal(files,
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.TravelJournalsFolder),
                            journalFileName).Wait();
                    });
        }

        // TODO:  The following scenario is not working.  Investigate further.
        //  If it works then we can avoid uploading all voicememos to azure storage
        internal void BatchTranscribeUsingFiles()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                 "Press Y to Continue with Batch Transcription or local audio files or any key to skip...",
                 () =>
                 {
                     var audioUris = Helper.GetFileUris(HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.VoiceMemosFolder), "*.wav");
                     foreach (Uri uri in audioUris)
                         Console.WriteLine(uri.ToString());

                     try
                     {
                         BatchTranscription.RunBatchTranscribeAsync(
                             customSettings.SpeechConfigSettings.Key, customSettings.SpeechConfigSettings.Region,
                             audioUris.ToArray()).Wait();
                     }
                     catch (Exception ex)
                     {
                         Console.WriteLine("Please note file URIs are not working for Batch Transcription API");
                         Console.WriteLine(ex.Message);
                         Console.WriteLine();
                     }
                 });
        }

        internal void ProcessImageFile()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to process/analyze a image file or any key to skip...",
                    () =>
                    {
                        string inputImageFilePath = Path.Combine(
                                HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.PhotosToAnalyzeFolder),
                                customSettings.SampleIndividualFiles.PhotoFileToProcess
                            );

                        Helper.ProcessImageAsync(
                            customSettings.ComputerVisionSettings.Key,
                            customSettings.ComputerVisionSettings.Endpoint,
                            new Uri(new Uri("File://"), inputImageFilePath),
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.AnalyzedImagesFolder))
                            .Wait();
                    });

        }

        internal void ProcessImagesInFolder()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to process/analyze all image files in a folder or any key to skip...",
                    () =>
                    {
                        Helper.ProcessImagesFromFolderAsync(
                            customSettings.ComputerVisionSettings.Key,
                            customSettings.ComputerVisionSettings.Endpoint,
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.PhotosToAnalyzeFolder),
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.AnalyzedImagesFolder))
                            .Wait();
                    });

        }


        internal void ReadDetectAndTranslateImage()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to reading text from image using OCR, detecting language, and then translating a file or any key to skip...",
                    () =>
                    {
                        ReadImageOcrTextAndTranslate().Wait();
                    });

        }

        internal async Task ReadImageOcrTextAndTranslate(string toLanguage="en-US")
        {
            string inputImageFilePath = Path.Combine(
                    HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.PhotosToAnalyzeFolder),
                    customSettings.SampleIndividualFiles.PhotoFileToProcess
                );

            string fileNamePrefix = Path.GetFileName(inputImageFilePath);
            string outFolder = HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.AnalyzedImagesFolder);
            string outBaseFilePath = Path.Combine(outFolder, fileNamePrefix);

            // ensure destination Path exists
            Directory.CreateDirectory(outFolder);



            // OCR - text extraction

            // Get vision client

            Console.WriteLine($"Extracting Text using Vision OCR from {inputImageFilePath}...");

            ComputerVisionClient visionClient = ComputerVision.Authenticate(
                customSettings.ComputerVisionSettings.Endpoint,
                customSettings.ComputerVisionSettings.Key);

            string ocrFilePath = outBaseFilePath + "-ReadOcrResults.txt";

            var ocrResult = await ComputerVision.RecognizeTextFromImageLocal(visionClient, inputImageFilePath, false);
            var ocrLineTexts = ComputerVisionHelper.GetOcrResultLineTexts(ocrResult);
            await File.WriteAllLinesAsync(ocrFilePath, ocrLineTexts);

            Console.WriteLine($"Generated OCR output file {ocrFilePath}.");
            Console.WriteLine();

            // Detect Languages using Text Analytics Api
            TextAnalyticsClient textClient = TextAnalytics.GetClient(
                customSettings.TextAnalyticsSettings.Key,
                customSettings.TextAnalyticsSettings.Endpoint
                );


            Console.WriteLine("Detect the language from generated OCR text using TextAnalytics...");
            IEnumerable<string> sourceLanguages = await TextAnalytics.DetectLanguageBatchAsync(textClient, ocrLineTexts);

            //Console.WriteLine($"Detected languages Count: {sourceLanguages.Count()}");
            //Console.WriteLine($"Detected Languages: {string.Join(", ", sourceLanguages)}");
            Console.WriteLine();


            // Now translate the extracted text (OCR) to output language (here default is English)
            Console.WriteLine($"Now translate the generated OCR file to English {toLanguage}...");

            string ocrText = await File.ReadAllTextAsync(ocrFilePath);
            string translatedText = await JournalHelper.Translator.Translate.TranslateTextRequestAsync(
                customSettings.TranslatorConfigSettings.Key,
                customSettings.TranslatorConfigSettings.Endpoint,
                toLanguage,
                ocrText
                );

            string outTranslatedFilePath = outBaseFilePath + "-translated-" + toLanguage + ".json";

            if (!translatedText.StartsWith("["))
            {
                Console.WriteLine($"Storing the generated translation output to file: {outTranslatedFilePath}... ");
                var json = JObject.Parse(translatedText);
                Helper.WriteToJsonFile<JObject>(outTranslatedFilePath, json);

                if (json.ContainsKey("error"))
                {
                    Console.WriteLine($"\t\t\tTRANSLATOR ERROR: {json["error"]["code"]}");
                    Console.WriteLine($"\t\t\tMESSAGE: {json["error"]["message"]}");
                    return;
                }
            }

            string txtFile = outTranslatedFilePath + ".txt";
            Console.WriteLine($"Generating txt file with translated texts - {txtFile}");

            IEnumerable<string> texts = JournalHelper.Translator.Translate.GetTranslatedTexts(translatedText);
            await File.WriteAllLinesAsync(txtFile, texts);

            Console.WriteLine();
        }

        internal void CreateSearchIndexer()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to create Search Index and Indexer or any key to skip...",
                    () =>
                    {
                        CreateSearchIndexerAsync().Wait();
                    });
        }

        internal async Task CreateSearchIndexerAsync()
        {
            // first check for input files - transcribed texts 
            var container = Helper.GetBlobManager().GetBlobContainerAsync(
                customSettings.AzureBlobContainers.ExtractedTranscribedTexts
                );

            if (container == null)
            {
                // the voice memos are not transcribed. 
                await TranscribeVoiceMemosFolder();
            }

            // create search indexer
            string jsonFilePath = Path.Combine(
                HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.IndexesFolder),
                customSettings.SampleIndividualFiles.SampleTextIndexJsonFile);

            await Helper.CreateSearchIndexerAsync(
                customSettings.SearchConfigSettings.ServiceKey,
                customSettings.SearchConfigSettings.ServiceUrl,
                customSettings.SearchConfigSettings.CongnitiveKey,
                customSettings.SearchConfigSettings.IndexName,
                jsonFilePath,
                customSettings.AzureStorageAccount.ConnectionString,
                customSettings.AzureBlobContainers.ExtractedTranscribedTexts
                );
        }

        internal void DoSingleSearchAndCreateJournal()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to do Search for documents or any key to skip...",
                    () =>
                    {
                        Console.WriteLine("Input Search String.  Examples: mountain, stadium, lake, tower, summer+vacation, etc.");
                        Console.Write("QzxaueryString: ");
                        string searchString = Console.ReadLine();

                        Console.WriteLine("Input output Journal File name");
                        Console.WriteLine("You can leave it empty for default file name selection.");
                        Console.Write("FileName: ");
                        string outFileName = Console.ReadLine();

                        // fix outFileName as needed
                        string fileNamePrefix = "Journal";
                        if (string.IsNullOrEmpty(outFileName))
                        {
                            if (searchString.Split(" ").Count() == 1)
                                outFileName = fileNamePrefix + searchString;
                            else
                                outFileName = fileNamePrefix + DateTime.Now.ToString("dd-mm-yy hh-mm-ss");
                        }

                        var docs = Helper.SearchDocuments(
                            customSettings.SearchConfigSettings.ServiceUrl,
                            customSettings.SearchConfigSettings.ServiceQueryKey,
                            searchString,
                            customSettings.SearchConfigSettings.IndexName
                            );

                        Helper.CreateTravelJournal(searchString, docs,
                            HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.TravelJournalsFolder),
                            outFileName).Wait();
                    });

        }

        internal void DoSearch()
        {
            HelperFunctions.PromptForConfirmationAndExecute(
                    "Press Y to Continue to do Search and create Journals or any key to skip...",
                    () => { 

                        bool bContinue = false;

                        do
                        {
                            bContinue = false;
                            DoSingleSearchAndCreateJournal();

                            HelperFunctions.PromptForConfirmationAndExecute(
                                    "Press Y to do another search or any key to skip...",
                                    () => { bContinue = true; });
                        } while (bContinue);
                    });

        }
    }
}
