using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

// Install Newtonsoft.Json with NuGet
using Newtonsoft.Json;


namespace JournalHelper.Translator
{

    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator Text API.
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }

    public class Translate
    {

        // toLanguages is comma separated list of languages
        internal static string ConstructTranslateRoute(string toLanguages)
        {
            string route = @"/translate?api-version=3.0";

            foreach (var lang in toLanguages.Split(","))
            {
                route += $"&to={lang}";
            }
            return route;
        }

        // Async call to the Translator Text API
        public static async Task<string> TranslateTextRequestAsync(string subscriptionKey, string endpoint, string toLanguages, string inputText)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);
            string route = ConstructTranslateRoute(toLanguages);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                // Read response as a string.

                string result = await response.Content.ReadAsStringAsync();
                return result;
            }

        }

        public static void PrintTranslationResults(string translatedText)
        {
            TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(translatedText);
            // Iterate over the deserialized results.
            foreach (TranslationResult o in deserializedOutput)
            {
                // Print the detected input languge and confidence score.
                Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
                // Iterate over the results and print each translation.
                foreach (Translation t in o.Translations)
                {
                    Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                }
            }
        }

        public static IEnumerable<string> GetTranslatedTexts(string result)
        {
            TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
            List<string> texts = new List<string>();
            foreach (TranslationResult o in deserializedOutput)
            {
                foreach (Translation t in o.Translations)
                {
                    texts.Add(t.Text);
                }
            }
            return texts;
        }
    }
}
