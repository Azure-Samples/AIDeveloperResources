using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace JournalHelper
{
    public class ComputerVisionHelper
    {

        public static JObject GetJson(ImageAnalysis results)
        {
            var jsonString = JsonConvert.SerializeObject(results);
            var json = JObject.Parse(jsonString);
            return json;
        }

        public static void PrintImageAnalysisResults(ImageAnalysis results)
        {
            // Sunmarizes the image content.
            Console.WriteLine("Summary:");
            foreach (var caption in results.Description.Captions)
            {
                Console.WriteLine($"{caption.Text} with confidence {caption.Confidence}");
            }
            Console.WriteLine();

            // Display categories the image is divided into.
            Console.WriteLine("Categories:");
            foreach (var category in results.Categories)
            {
                Console.WriteLine($"{category.Name} with confidence {category.Score}");
            }
            Console.WriteLine();

            // Image tags and their confidence score
            Console.WriteLine("Tags:");
            foreach (var tag in results.Tags)
            {
                Console.WriteLine($"{tag.Name} {tag.Confidence}");
            }
            Console.WriteLine();

            // Objects
            Console.WriteLine("Objects:");
            foreach (var obj in results.Objects)
            {
                Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                  $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
            }
            Console.WriteLine();

            // Detected faces, if any.
            Console.WriteLine("Faces:");
            foreach (var face in results.Faces)
            {
                Console.WriteLine($"A {face.Gender} of age {face.Age} at location {face.FaceRectangle.Left}, {face.FaceRectangle.Top}, " +
                  $"{face.FaceRectangle.Left + face.FaceRectangle.Width}, {face.FaceRectangle.Top + face.FaceRectangle.Height}");
            }
            Console.WriteLine();

            // Adult or racy content, if any.
            Console.WriteLine("Adult:");
            Console.WriteLine($"Has adult content: {results.Adult.IsAdultContent} with confidence {results.Adult.AdultScore}");
            Console.WriteLine($"Has racy content: {results.Adult.IsRacyContent} with confidence {results.Adult.RacyScore}");
            Console.WriteLine();

            // Well-known brands, if any.
            Console.WriteLine("Brands:");
            foreach (var brand in results.Brands)
            {
                Console.WriteLine($"Logo of {brand.Name} with confidence {brand.Confidence} at location {brand.Rectangle.X}, " +
                  $"{brand.Rectangle.X + brand.Rectangle.W}, {brand.Rectangle.Y}, {brand.Rectangle.Y + brand.Rectangle.H}");
            }
            Console.WriteLine();

            // Celebrities in image, if any.
            Console.WriteLine("Celebrities:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Celebrities != null)
                {
                    foreach (var celeb in category.Detail.Celebrities)
                    {
                        Console.WriteLine($"{celeb.Name} with confidence {celeb.Confidence} at location {celeb.FaceRectangle.Left}, " +
                          $"{celeb.FaceRectangle.Top},{celeb.FaceRectangle.Height},{celeb.FaceRectangle.Width}");
                    }
                }
            }
            Console.WriteLine();

            // Popular landmarks in image, if any.
            Console.WriteLine("Landmarks:");
            foreach (var category in results.Categories)
            {
                if (category.Detail?.Landmarks != null)
                {
                    foreach (var landmark in category.Detail.Landmarks)
                    {
                        Console.WriteLine($"{landmark.Name} with confidence {landmark.Confidence}");
                    }
                }
            }
            Console.WriteLine();

            // Identifies the color scheme.
            Console.WriteLine("Color Scheme:");
            Console.WriteLine("Is black and white?: " + results.Color.IsBWImg);
            Console.WriteLine("Accent color: " + results.Color.AccentColor);
            Console.WriteLine("Dominant background color: " + results.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color: " + results.Color.DominantColorForeground);
            Console.WriteLine("Dominant colors: " + string.Join(",", results.Color.DominantColors));
            Console.WriteLine();

            // Detects the image types.
            Console.WriteLine("Image Type:");
            Console.WriteLine("Clip Art Type: " + results.ImageType.ClipArtType);
            Console.WriteLine("Line Drawing Type: " + results.ImageType.LineDrawingType);
            Console.WriteLine();

        }

        public static void PrintDetectResult(DetectResult results)
        {
            Console.WriteLine("Printing DetectResult objects...");
            foreach (var obj in results.Objects)
            {
                Console.WriteLine($"{obj.ObjectProperty} with confidence {obj.Confidence} at location {obj.Rectangle.X}, " +
                  $"{obj.Rectangle.X + obj.Rectangle.W}, {obj.Rectangle.Y}, {obj.Rectangle.Y + obj.Rectangle.H}");
            }
            Console.WriteLine();

        }

        public static void PrintImageDomainSpecificContent(JObject results, string domains)
        {
            if (String.IsNullOrEmpty(domains))
            {
                domains = "landmarks,celebrities";
            }

            string[] individualDomains = domains.Split(",");

            foreach (var domain in individualDomains)
            {
                Console.WriteLine($"Detected results in domain {domain}...");

                foreach (var result in results[$"{domain}"])
                {
                    Console.WriteLine($"Name: {result["name"]}  with Confidence: {result["confidence"]}");
                }
                Console.WriteLine();
            }

        }

        public static IEnumerable<string> GetReadResultLines(ReadOperationResult results)
        {

            var readResults = results.AnalyzeResult.ReadResults;
            var lineTexts = readResults.SelectMany(result => result.Lines)
                .Select(line =>  new string(line.Text));

            return lineTexts;
        }

        public static string GetOcrResultText(OcrResult ocrResult)
        {
            var words = ocrResult.Regions.SelectMany(region => region.Lines)
                .SelectMany(line => line.Words)
                .Select(word => word.Text);

            string text = string.Join(" ", words);
            Console.WriteLine($"it says: {text}");

            return text;
        }

        public static IEnumerable<string> GetOcrResultLineTexts(OcrResult ocrResult)
        {
            List<string> LineTexts = new List<string>();

            var lines = ocrResult.Regions.SelectMany(region => region.Lines);

            foreach (OcrLine line in lines)
            {
                var wordTexts = line.Words.Select(word => word.Text);
                LineTexts.Add(String.Join(" ", wordTexts));
            }

            return LineTexts;
        }

    }
}
