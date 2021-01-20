using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JournalHelper
{
    public class IndividualFileTranscribe
    {
        public static async Task TranscribeSpeechFileAsync(string key, string region, string filePath, string outFolder)
        {
            var stopRecognition = new TaskCompletionSource<int>();
            StringBuilder transcribedText = new StringBuilder();

            var speechConfig = SpeechConfig.FromSubscription(key, region);

            using (var audioConfig = AudioConfig.FromWavFileInput(filePath))
            {
                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {

                    recognizer.Recognizing += (s, e) =>
                    {
                        //Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            Console.WriteLine($"RECOGNIZED: Text={e.Result.Text}");

                            transcribedText.AppendLine(e.Result.Text);
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                            stopRecognition.TrySetResult(1);
                        }
                        else if (e.Reason == CancellationReason.EndOfStream)
                        {
                            Console.WriteLine($"CANCELED:  But Success.  End of speech reached!");
                            stopRecognition.TrySetResult(0);
                        }

                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        stopRecognition.TrySetResult(0);
                    };


                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync();

                    // Waits for completion. Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync();

                }

            }


            // copy the transcribed text to destination folder
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string outFileName = fileName + ".txt";

            Directory.CreateDirectory(outFolder);
            string outFile = Path.Combine(outFolder, outFileName);

            await File.WriteAllTextAsync(outFile, transcribedText.ToString());
        }

    }
}
