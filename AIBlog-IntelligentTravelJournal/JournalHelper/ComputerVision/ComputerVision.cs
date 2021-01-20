/* *****************************************************************
 * REFFERENCES
 * https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/quickstarts-sdk/client-library?tabs=visual-studio&pivots=programming-language-csharp
 * https://github.com/Azure-Samples/cognitive-services-quickstart-code/blob/master/dotnet/ComputerVision/ComputerVisionQuickstart.cs
 * https://csharp.hotexamples.com/examples/-/VisionServiceClient/RecognizeTextAsync/php-visionserviceclient-recognizetextasync-method-examples.html
 * 
 * :  
 * *****************************************************************
 */

using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace JournalHelper
{
    public class ComputerVision
    {

        /*
         * AUTHENTICATE
         * Creates a Computer Vision client used by each example.
         */
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public static IList<VisualFeatureTypes?> GetSampleVisualFeatureTypes()
        {
            //Creating a list that defines the features to be extracted from the image.

            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            return features;
        }

        public static async Task<ImageAnalysis> AnalyzeImageUri(ComputerVisionClient client, Uri imageUri, IList<VisualFeatureTypes?> features)
        {
            if (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp)
            {
                return await AnalyzeImageUrl(client, imageUri.ToString(), features);
            }
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                return await AnalyzeImageLocal(client, imageUri.AbsolutePath, features);
            }
            return null;

        }


        /* 
         * ANALYZE IMAGE - URL IMAGE
         * Analyze URL image. Extracts captions, categories, tags, objects, faces, racy/adult content,
         * brands, celebrities, landmarks, color scheme, and image types.
         */
        public static async Task<ImageAnalysis> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl, IList<VisualFeatureTypes?> features)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("ANALYZE IMAGE - URL");
            Console.WriteLine();


            Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();
            // Analyze the URL image 
            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, features);

            return results;
        }

        /*
            * ANALYZE IMAGE - LOCAL IMAGE
            * Analyze local image. Extracts captions, categories, tags, objects, faces, racy/adult content,
            * brands, celebrities, landmarks, color scheme, and image types.
        */
        public static async Task<ImageAnalysis> AnalyzeImageLocal(ComputerVisionClient client, string localImage, IList<VisualFeatureTypes?> features)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("ANALYZE IMAGE - LOCAL IMAGE");
            Console.WriteLine();


            Console.WriteLine($"Analyzing the local image {Path.GetFileName(localImage)}...");
            Console.WriteLine();

            using (Stream analyzeImageStream = File.OpenRead(localImage))
            {
                // Analyze the local image.
                ImageAnalysis results = await client.AnalyzeImageInStreamAsync(analyzeImageStream);
                return results;
            }

        }

        public static async Task<DetectResult> DetectObjectsUri(ComputerVisionClient client, Uri imageUri)
        {
            if (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp)
            {
                return await DetectObjectsUrl(client, imageUri.ToString());
            }
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                return await DetectObjectsLocal(client, imageUri.AbsolutePath);
            }
            return null;
        }

        /* 
        * DETECT OBJECTS - URL IMAGE
        */
        public static async Task<DetectResult> DetectObjectsUrl(ComputerVisionClient client, string urlImage)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("DETECT OBJECTS - URL IMAGE");
            Console.WriteLine();

            Console.WriteLine($"Detecting objects in URL image {Path.GetFileName(urlImage)}...");
            Console.WriteLine();
            // Detect the objects
            DetectResult detectObjectAnalysis = await client.DetectObjectsAsync(urlImage);

            // For each detected object in the picture, print out the bounding object detected, confidence of that detection and bounding box within the image
            Console.WriteLine("Detected objects:");
            Console.WriteLine();

            return detectObjectAnalysis;
        }
        /*
         * END - DETECT OBJECTS - URL IMAGE
         */

        /*
         * DETECT OBJECTS - LOCAL IMAGE
         * This is an alternative way to detect objects, instead of doing so through AnalyzeImage.
         */
        public static async Task<DetectResult> DetectObjectsLocal(ComputerVisionClient client, string localImage)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("DETECT OBJECTS - LOCAL IMAGE");
            Console.WriteLine();

            using (Stream stream = File.OpenRead(localImage))
            {
                Console.WriteLine($"Detecting objects in local image {Path.GetFileName(localImage)}...");
                Console.WriteLine();

                // Make a call to the Computer Vision service using the local file
                DetectResult results = await client.DetectObjectsInStreamAsync(stream);

                // For each detected object in the picture, print out the bounding object detected, confidence of that detection and bounding box within the image
                Console.WriteLine("Detected objects:");
                Console.WriteLine();

                return results;
            }
        }
        /*
         * DETECT DOMAIN-SPECIFIC CONTENT
         * Recognizes landmarks or celebrities in an image.
         */

        // supported domains "landmarks" and "celebrities"
        public static async Task<JObject> DetectDomainSpecific(ComputerVisionClient client, Uri imageUri, string domains)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("DETECT DOMAIN-SPECIFIC CONTENT");
            Console.WriteLine();

            JObject resultJsonUrl = null;

            // Http Urls
            if (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp)
            {
                // Detect the domain-specific content in a URL image.
                DomainModelResults resultsUrl = await client.AnalyzeImageByDomainAsync(domains, imageUri.ToString());
                // Display results.
                Console.WriteLine($"Detecting {domains} in the URL image {Path.GetFileName(imageUri.AbsolutePath)}...");

                var jsonUrl = JsonConvert.SerializeObject(resultsUrl.Result);
                resultJsonUrl = JObject.Parse(jsonUrl);
            }
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                // Detect the domain-specific content in a local image.
                using (Stream imageStream = File.OpenRead(imageUri.AbsolutePath))
                {
                    Console.WriteLine($"Detecting {domains} in the local file image {Path.GetFileName(imageUri.AbsolutePath)}...");

                    DomainModelResults resultsLocal = await client.AnalyzeImageByDomainInStreamAsync(domains, imageStream);

                    // return results.
                    var jsonLocal = JsonConvert.SerializeObject(resultsLocal.Result);
                    resultJsonUrl = JObject.Parse(jsonLocal);
                }
            }
            Console.WriteLine();

            return resultJsonUrl;
        }


        public static async Task<ReadOperationResult> ReadImageAsync(ComputerVisionClient client, Uri imageUri)
        {
            if (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp)
            {
                return await ReadFileUrl(client, imageUri.ToString());
            }
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                return await ReadFileLocal(client, imageUri.AbsolutePath);
            }
            return null;
        }

        /*
         * READ FILE - URL 
         * Extracts text. 
         */
        public static async Task<ReadOperationResult> ReadFileUrl(ComputerVisionClient client, string urlFile)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("READ FILE FROM URL");
            Console.WriteLine();

            // Read text from URL
            var textHeaders = await client.ReadAsync(urlFile, language: "en");
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);
            // </snippet_extract_call>

            // <snippet_read_response>
            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            return await GetReadOperationResult(client, operationId);

        }
        internal static async Task<ReadOperationResult> GetReadOperationResult(ComputerVisionClient client, string operationId)
        {
            ReadOperationResult results;

            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            return results;

        }

        public static async Task<ReadOperationResult> ReadFileLocal(ComputerVisionClient client, string localFile)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("READ FILE FROM LOCAL");
            Console.WriteLine();

            // Read text from URL
            var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile), language: "en");
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);
            // </snippet_extract_call>

            // <snippet_extract_response>
            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            return await GetReadOperationResult(client, operationId);
        }



        public static async Task<OcrResult> RecognizeTextFromImageUri(ComputerVisionClient client, Uri imageUri, bool detectOrientation, OcrLanguages language=OcrLanguages.En)
        {
            if (imageUri.Scheme == Uri.UriSchemeHttps || imageUri.Scheme == Uri.UriSchemeHttp)
            {
                return await RecognizeTextFromImageUrl(client, imageUri.ToString(), detectOrientation, language);
            }
            else if (imageUri.Scheme == Uri.UriSchemeFile)
            {
                return await RecognizeTextFromImageLocal(client, imageUri.AbsolutePath, detectOrientation, language);
            }
            return null;
        }

        /*
         * READ FILE - URL 
         * Extracts text. 
         */
        public static async Task<OcrResult> RecognizeTextFromImageUrl(ComputerVisionClient client, string urlFile, bool detectOrientation, OcrLanguages language=OcrLanguages.En)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("READ FILE FROM URL");
            Console.WriteLine();

            // Read text from URL
            var ocrResults = await client.RecognizePrintedTextAsync(detectOrientation, urlFile, language);
            return ocrResults;
        }

        public static async Task<OcrResult> RecognizeTextFromImageLocal(ComputerVisionClient client, string localFile, bool detectOrientation, OcrLanguages language=OcrLanguages.En)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine($"Recognize Printed Text (OCR) From a local image FILE: {Path.GetFileName(localFile)} ");
            Console.WriteLine();

            // Read text from URL
            var ocrResults = await client.RecognizePrintedTextInStreamAsync(detectOrientation, File.OpenRead(localFile), language);
            return ocrResults;
       }


    }
}
