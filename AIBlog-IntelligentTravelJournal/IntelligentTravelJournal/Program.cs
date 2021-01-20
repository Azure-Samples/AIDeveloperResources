using JournalHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace IntelligentTravelJournal
{
    class Program
    {

        static internal RootCustomSettings customSettings = HelperFunctions.LoadCustomSettingsJson();
        static internal BlobManager blobManager = new BlobManager(customSettings.AzureStorageAccount.ConnectionString);


        static void Main(string[] args)
        {
            Console.WriteLine("================================================");
            Console.WriteLine("This is an AI Project, an Intelligent travel journal!");
            Console.WriteLine("Demostrate Microsoft Cognitive services focusing on Speech, text analytics and search");
            Console.WriteLine("================================================");

            //Load custom settings, moved it to global so to access in helper.cs
            //RootCustomSettings customSettings = Helper.LoadCustomSettingsJson();

            Helper.InitializeHelper(blobManager);

            Scenarios scenarios = new Scenarios(customSettings);

            //Uri uri = Helper.GetBlobManager().GetContainerSASUriAsync(customSettings.AzureBlobContainers.InputVoiceMemoFiles).ConfigureAwait(false).GetAwaiter().GetResult();

            
            scenarios.TranscribeVoiceMemosInAFolder();

            scenarios.CreateTagsForFolderItems();

            scenarios.UpdateTagsToPhotoFiles();

            
            // aggregate all the transcriptions into one complete travel journal
            string inputFolder = HelperFunctions.GetSampleDataFullPath(customSettings.SampleDataFolders.BatchTranscribedFolder);
            scenarios.CreateTravelJournal( inputFolder,  @"MyCompleteTravelJournal");
            

            scenarios.ProcessImagesInFolder();

            scenarios.ReadDetectAndTranslateImage();

            scenarios.CreateSearchIndexer();

            scenarios.DoSearch();

            HelperFunctions.PromptForConfirmationAndExecute(
                $"Press Y to Continue to cleanup all output generated so far or any key to skip...",
                () =>
                {
                    HelperFunctions.CleanUp(customSettings).Wait();
                });


            Console.WriteLine("Press Any Key to Continue to End...");
            Console.ReadKey();

        }

    }
}
