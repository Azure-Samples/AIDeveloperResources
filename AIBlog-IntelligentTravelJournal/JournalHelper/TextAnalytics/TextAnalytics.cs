using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JournalHelper
{
    public class TextAnalytics
    {
        public static TextAnalyticsClient GetClient(string key, string endpoint)
        {
            AzureKeyCredential credentials = new AzureKeyCredential(key);
            Uri endpointUri = new Uri(endpoint);

            return new TextAnalyticsClient(endpointUri, credentials);
        }

        public static IEnumerable<string> EntityRecognition(TextAnalyticsClient client, string content)
        {
            var response = client.RecognizeEntities(content);
            Console.WriteLine("Named Entities:");
            List<string> entities = new List<string>();
            foreach (var entity in response.Value)
            {
//                Console.WriteLine($"\tText: {entity.Text},\tCategory: {entity.Category},\tSub-Category: {entity.SubCategory}");
                entities.Add(entity.Text);
            }
            return entities;
        }

        static IEnumerable<string> KeyPhraseExtraction(TextAnalyticsClient client, string content)
        {
            var response = client.ExtractKeyPhrases(content);

            // Printing key phrases
            Console.WriteLine("Key phrases:");
            List<string> keyPhrases = new List<string>();
            foreach (string keyphrase in response.Value)
            {
//                Console.WriteLine($"\t{keyphrase}");
                keyPhrases.Add(keyphrase);
            }
            return keyPhrases;
        }

        static public async Task<IEnumerable<string>> GetTags(TextAnalyticsClient client, string inputTextFilePath)
        {

            string inputContent = await File.ReadAllTextAsync(inputTextFilePath);

            var entities = EntityRecognition(client, inputContent);
            var phrases = KeyPhraseExtraction(client, inputContent);

            var tags = new List<string>();
            tags.AddRange(entities);
            tags.AddRange(phrases);

            return tags;
        }

        public static async Task<string> DetectLanguageAsync(TextAnalyticsClient client, string content)
        {
            DetectedLanguage detectedLanguage = await client.DetectLanguageAsync(content);
            Console.WriteLine("Language:");
            Console.WriteLine($"\t{detectedLanguage.Name},\tISO-6391: {detectedLanguage.Iso6391Name}\n");
            return detectedLanguage.Iso6391Name;
        }

        public static async Task<IEnumerable<string>> DetectLanguageBatchAsync(TextAnalyticsClient client, IEnumerable<string> documents)
        {
            var detectedLanguages = await client.DetectLanguageBatchAsync(documents);
            Console.WriteLine("Languages:");

            List<string> languages = new List<string>();
            foreach (var result in detectedLanguages.Value)
            {
                DetectedLanguage dl = result.PrimaryLanguage;
                
                Console.WriteLine($"\t{dl.Name},\tISO-6391: {dl.Iso6391Name}, \tConfidenceScore: {dl.ConfidenceScore}");
                languages.Add(dl.Iso6391Name);
            }
            Console.WriteLine();

            return languages;
        }
    }
}

