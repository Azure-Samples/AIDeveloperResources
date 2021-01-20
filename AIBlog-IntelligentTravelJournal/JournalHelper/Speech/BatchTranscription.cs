//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

namespace JournalHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    using BatchClient;

    public class BatchTranscription
    {

        // recordings and locale
        private const string Locale = "en-US";

        // For use of custom trained model:
        private static EntityReference CustomModel = null;
        //private static EntityReference CustomModel =
        //    new EntityReference { Self = new Uri($"https://{Region}.api.cognitive.microsoft.com/speechtotext/v3.0/models/<id of custom model>")};
        private const string DisplayName = "Simple batch transcription";

        public static async Task RunBatchTranscribeAsync(string key, string region, Uri[] audioUris, bool isContainerUrls=false)
        {
            // create the client object and authenticate
            using (var client = BatchClient.CreateApiV3Client(key, $"{region}.api.cognitive.microsoft.com"))
            {

                await TranscribeAsync(client, audioUris, isContainerUrls).ConfigureAwait(false);
            }
        }

        private async static Task TranscribeAsync(BatchClient client, Uri[] audioFiles, bool isContainerUrls)
        {
            Console.WriteLine("Deleting all existing completed transcriptions.");

            // get all transcriptions for the subscription
            PaginatedTranscriptions paginatedTranscriptions = null;
            do
            {
                if (paginatedTranscriptions == null)
                {
                    paginatedTranscriptions = await client.GetTranscriptionsAsync().ConfigureAwait(false);
                }
                else
                {
                    paginatedTranscriptions = await client.GetTranscriptionsAsync(paginatedTranscriptions.NextLink).ConfigureAwait(false);
                }

                // delete all pre-existing completed transcriptions. If transcriptions are still running or not started, they will not be deleted
                foreach (var transcriptionToDelete in paginatedTranscriptions.Values)
                {
                    // delete a transcription
                    await client.DeleteTranscriptionAsync(transcriptionToDelete.Self).ConfigureAwait(false);
                    Console.WriteLine($"Deleted transcription {transcriptionToDelete.Self}");
                }
            }
            while (paginatedTranscriptions.NextLink != null);

            // <transcriptiondefinition>
            var newTranscription = new Transcription
            {
                DisplayName = DisplayName,
                Locale = Locale,
                ContentUrls = audioFiles,
                //ContentContainerUrl = ContentAzureBlobContainer,
                Model = CustomModel,
                Properties = new TranscriptionProperties
                {
                    DestinationContainerUrl = null,
                    IsWordLevelTimestampsEnabled = false,
                    TimeToLive = TimeSpan.FromDays(1)
                }
            };

            if (isContainerUrls)
            {
                newTranscription.ContentUrls = null;
                newTranscription.ContentContainerUrl = audioFiles[0];
                newTranscription.Properties.DestinationContainerUrl = audioFiles[1];
            }

            newTranscription = await client.CreateTranscriptionAsync(newTranscription).ConfigureAwait(false);
            Console.WriteLine($"Created transcription {newTranscription.Self}");
            // </transcriptiondefinition>

            // get the transcription Id from the location URI
            var createdTranscriptions = new List<Uri> { newTranscription.Self };

            Console.WriteLine("Checking status.");

            // get the status of our transcriptions periodically and log results
            int completed = 0, running = 0, notStarted = 0;
            while (completed < 1)
            {
                completed = 0; running = 0; notStarted = 0;

                // get all transcriptions for the user
                paginatedTranscriptions = null;
                do
                {
                    // <transcriptionstatus>
                    if (paginatedTranscriptions == null)
                    {
                        paginatedTranscriptions = await client.GetTranscriptionsAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        paginatedTranscriptions = await client.GetTranscriptionsAsync(paginatedTranscriptions.NextLink).ConfigureAwait(false);
                    }

                    // delete all pre-existing completed transcriptions. If transcriptions are still running or not started, they will not be deleted
                    foreach (var transcription in paginatedTranscriptions.Values)
                    {
                        switch (transcription.Status)
                        {
                            case "Failed":
                            case "Succeeded":
                                // we check to see if it was one of the transcriptions we created from this client.
                                if (!createdTranscriptions.Contains(transcription.Self))
                                {
                                    // not created form here, continue
                                    continue;
                                }

                                completed++;

                                // if the transcription was successful, check the results
                                if (transcription.Status == "Succeeded")
                                {
                                    Console.WriteLine($"\t\tTranscirption {transcription.DisplayName} Succeeded.");

                                    // BUGBUG:  TODO:  The destination container public access level is set to OFF.
                                    // Hence the call to GetTranscriptionResultAsync(Uri) is failing with "resource not found" error.
                                    // so disabling the code below.

                                    // Alternatively, checking for isContainerUrls & use audioFiles[1] to get the container and change its public access level.
                                    if (false)
                                    {
                                        var paginatedfiles = await client.GetTranscriptionFilesAsync(transcription.Links.Files).ConfigureAwait(false);

                                        var resultFile = paginatedfiles.Values.FirstOrDefault(f => f.Kind == ArtifactKind.Transcription);
                                        var result = await client.GetTranscriptionResultAsync(new Uri(resultFile.Links.ContentUrl)).ConfigureAwait(false);
                                        Console.WriteLine("Transcription succeeded. Results: ");
                                        Console.WriteLine(JsonConvert.SerializeObject(result, SpeechJsonContractResolver.WriterSettings));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Transcription failed. Status: {0}", transcription.Properties.Error.Message);
                                }

                                break;

                            case "Running":
                                running++;
                                break;

                            case "NotStarted":
                                notStarted++;
                                break;
                        }
                    }

                    // for each transcription in the list we check the status
                    Console.WriteLine(string.Format("Transcriptions status: {0} completed, {1} running, {2} not started yet", completed, running, notStarted));
                }
                while (paginatedTranscriptions.NextLink != null);

                // </transcriptionstatus>
                // check again after 1 minute
                await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

     }
}
