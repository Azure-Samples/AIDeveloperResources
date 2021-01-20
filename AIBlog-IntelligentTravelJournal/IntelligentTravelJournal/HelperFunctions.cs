using JournalHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentTravelJournal
{
    class HelperFunctions
    {

        // Load JSON
        static internal RootCustomSettings LoadCustomSettingsJson()
        {
            using (StreamReader r = new StreamReader("AppCustomSettings.json"))
            {
                string json = r.ReadToEnd();
                RootCustomSettings customSettings = JsonConvert.DeserializeObject<RootCustomSettings>(json);
                return customSettings;
            }
        }

        static internal string GetSampleDataFullPath(string subPath)
        {
            return Path.Combine(Program.customSettings.SampleDataFolders.RootFolder, subPath);
        }

        internal static void PromptForConfirmationAndExecute(string message, Action action)
        {
            Console.WriteLine(message);
            ConsoleKeyInfo cki = Console.ReadKey();
            Console.WriteLine();
            if (cki.Key.ToString().ToLower() == "y")
            {
                action();
            }
        }

        internal static async Task CleanUp(RootCustomSettings customSettings)
        {

            Console.WriteLine("Deleting Search Index related resources...");
            await JournalHelper.Search.SearchHelper.DeleteSearchResourcesAsync(
                customSettings.SearchConfigSettings.ServiceKey,
                customSettings.SearchConfigSettings.ServiceUrl,
                customSettings.SearchConfigSettings.IndexName
               );



            Console.WriteLine("Deleting all blob containers...");

            // delete azure blob containers
            await Helper.GetBlobManager().DeleteBlobContainerAsync(Program.customSettings.AzureBlobContainers.BatchTranscribedJsonResults);
            await Helper.GetBlobManager().DeleteBlobContainerAsync(Program.customSettings.AzureBlobContainers.ExtractedTranscribedTexts);
            await Helper.GetBlobManager().DeleteBlobContainerAsync(Program.customSettings.AzureBlobContainers.InputVoiceMemoFiles);
            await Helper.GetBlobManager().DeleteBlobContainerAsync(Program.customSettings.AzureBlobContainers.TaggedPhotos);
            await Helper.GetBlobManager().DeleteBlobContainerAsync(Program.customSettings.AzureBlobContainers.TravelJournals);

            Console.WriteLine("Deleting all output directories used...");

            // delete output Folders
            Directory.Delete(Program.customSettings.SampleDataFolders.BatchTranscribedFolder, true);
            Directory.Delete(Program.customSettings.SampleDataFolders.ExtractedTagsFolder, true);
            Directory.Delete(Program.customSettings.SampleDataFolders.TaggedPhotosFolder, true);
            Directory.Delete(Program.customSettings.SampleDataFolders.TranscribedFileFolder, true);
            Directory.Delete(Program.customSettings.SampleDataFolders.TravelJournalsFolder, true);
            Directory.Delete(Program.customSettings.SampleDataFolders.AnalyzedImagesFolder, true);


        }
    }
}
