# Intelligent Travel

Intelligent Travel Journey mobile app demo for blog post [aka.ms/onyourtermsblog2](https://techcommunity.microsoft.com/t5/azure-ai/bg-p/AzureAIBlog?WT.mc_id=aiml-131555-ayyonet). 

## Getting Started

[![Intelligent Travel Journal App](../Assets/Images/intelligentTravelJournal.jpg)

### Prerequisites

To run this code, you will need:

-   [Azure Subscription](https://azure.microsoft.com/services/cognitive-services/?WT.mc_id=aiml-131555-ayyonet) to access [Azure Cognitive Services](https://docs.microsoft.com/azure/cognitive-services/?WT.mc_id=aiml1315555-ayyonet). If you don't have one, you can sign up for [Free Azure Credit](https://azure.microsoft.com/free/cognitive-services/?WT.mc_id=aim13155155-ayyonet).
-   [Visual Studio 2019+](https://visualstudio.microsoft.com/downloads/?WT.mc_id=aiml-131555-ayyonet).
-   [.NET Core 3.1 or above (for FotoFly)](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/intro?WT.mc_id=aiml-131555-ayyonet).

## Quickstart

Under Project [IntelligenetTravelJournal](./IntelligentTravelJournal) - Update [AppCustomSettings.json](./IntelligentTravelJournal/AppCustomSettings.json) as follows

1. Copy [SampleData](./IntelligentTravelJournal/SampleData) to a path of your interest. Alternatively you can directly use the folder from solution/project extrated location.
2. Change "SampleDataFolders"/RootFolder value to the SampleData location in step 1.

NOTE: You can leave all other folders and containers settings as is.

RECOMMENDATION: Create all the below in a single azure resource group for better management.
Storing the groupname and subscriptionId info so its useful in cleanup, as needed.

3. Record your target Resource group name and azure subscriptionid under AzureResourceGroupSettings.
   NOTE: The items in step 3 are not currently used.

4. Create a new resource "Speech Microsoft" in azure portal, copy Key and Region (for now endpoint is not required)
   Replace Key & Region fields in SpeechConfigSettings
   NOTE: Branch transcription does not work with Free pricing tier. So select Standard while trying out the feature

5. Create a new resource "Text Analytics Microsoft", and copy Key and Endpoint information
   Update these fields in TextAnalyticsSettings

6. Create new resource "Computer Vision Microsoft", and
   update key, region and endpoint informaton under ComputerVisionSettings

7. Create new resource "Translator Microsoft", and
   update key, endpoint settings under TranslatorConfigSettings
   NOTE: The service is not available in all regions. At times it gives permission errors.  
   This sample was tested with region set to "Global".

8. Create new resource "Azure Cognitive Search", and
   update the following under SearchConfigSettings
   Name to ServiceName (really not used)
   Get Url from overview section and update ServiceUrl
   Get admin Key and update ServiceKey
   Get query key and update ServiceQueryKey

9. Create a new resource "Cognitive Services Microsoft", and update
   its Key and endpoint to CognitiveKey, and CognitiveEndpoint settngs under SearchConfigSettings

10. Create a new resource "Azure Storage Account", and update
    Name, Key and [ConnectionString](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string?WT.mc_id=aiml-13155-ayyonet#configure-a-connection-string-for-an-azure-storage-account) set to Name, Key, ConnectionString settings respectively under
    AzureStorageAccount section. Learn more about [Creating a connection string](https://docs.microsoft.com/azure/storage/common/storage-configure-connection-string?WT.mc_id=aiml-13155-ayyonet#configure-a-connection-string-for-an-azure-storage-account).

11. Refer to [Publish a .NET console application using Visual Studio tutorial](https://docs.microsoft.com/dotnet/core/tutorials/publishing-with-visual-studio?WT.mc_id=aiml-0000-ayyonet) for detailed guidance on how to publish a console app.

## Reference Links

-   [Free Azure Credit](https://azure.microsoft.com/free/cognitive-services/?WT.mc_id=aim13155155-ayyonet)
-   [Azure AI Blog](https://techcommunity.microsoft.com/t5/azure-ai/bg-p/AzureAIBlog?WT.mc_id=aiml-131555-ayyonet)
-   [Speech SDK Samples](https://github.com/Azure-Samples/cognitive-services-speech-sdk?WT.mc_id=aiml-13155-ayyonet)
-   [Batch client, converting to a class library code sample](https://github.com/Azure-Samples/cognitive-services-speech-sdk/tree/master/samples/batch/csharp/batchclient?WT.mc_id=aiml-13155-ayyonet)
-   [Speech Recognizer Class Reference](https://docs.microsoft.com/dotnet/api/microsoft.cognitiveservices.speech.speechrecognizer?view=azure-dotnet&WT.mc_id=aiml-13155-ayyonet)
-   [Speech to Text Documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/index-speech-to-text?WT.mc_id=aiml-13155-ayyonet)
-   [How to use batch transcription](https://docs.microsoft.com/azure/cognitive-services/speech-service/batch-transcription?WT.mc_id=aiml-13155-ayyonet)
-   [Speech SDK Samples](https://github.com/Azure-Samples/cognitive-services-speech-sdk?WT.mc_id=aiml-13155-ayyonet)
-   [Batch client code sample](https://github.com/Azure-Samples/cognitive-services-speech-sdk/tree/master/samples/batch/csharp/batchclient?WT.mc_id=aiml-13155-ayyonet)
-   [Speech Recognizer API](https://docs.microsoft.com/dotnet/api/microsoft.cognitiveservices.speech.speechrecognizer?view=azure-dotnet&WT.mc_id=aiml-13155-ayyonet)
    https://docs.microsoft.com/en-us/dotnet/api/microsoft.cognitiveservices.speech.speechrecognizer?view=azure-dotnet
-   [Speech translation code sample](https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/quickstart/csharp/dotnet/translate-speech-to-text/helloworld/Program.cs?WT.mc_id=aiml-13155-ayyonet)

-   [Dowload FotoFly link (.zip file)](http://www.java2s.com/Open-Source/CSharp_Free_Code/Windows_Presentation_Foundation_Library/Download_Fotofly_Photo_Metadata_Library.htm) is at the end of the page.
-   [Add metadata to image blobs](https://docs.microsoft.com/azure/cognitive-services/computer-vision/tutorials/storage-lab-tutorial?WT.mc_id=aiml-13155-ayyonet)
-   [Store Application Data with Azure Storage](https://docs.microsoft.com/learn/modules/store-app-data-with-azure-blob-storage/?WT.mc_id=aiml-13155-ayyonet)
-   [Computer Vision Documentation](https://docs.microsoft.com/azure/cognitive-services/computer-vision/?WT.mc_id=aiml-13155-ayyonet)
-   [Quickstart: Use Computer Vision Client LIbrary](https://docs.microsoft.com/azure/cognitive-services/computer-vision/quickstarts-sdk/client-library?tabs=visual-studio&pivots=programming-language-csharp&WT.mc_id=aiml-13155-ayyonet)
-   [Quickstart Sample Code](https://github.com/Azure-Samples/cognitive-services-quickstart-code/blob/master/dotnet/ComputerVision/ComputerVisionQuickstart.cs?WT.mc_id=aiml-13155-ayyonet)
-   [Azure Search Documentation](https://docs.microsoft.com/azure/search/?WT.mc_id=aiml-13155-ayyonet)
-   [Search Client 11 - client library - using Azure.Search.Documents](https://docs.microsoft.com/dotnet/api/overview/azure/search.documents-readme?WT.mc_id=aiml-13155-ayyonet)
-   [Tutorial: Index from multiple data sources using the .NET SDK](https://docs.microsoft.com/azure/search/tutorial-multiple-data-sources?WT.mc_id=aiml-13155-ayyonet)
-   [Tutorial: AI-generated searchable content from Azure blobs using the .NET SDK](https://docs.microsoft.com/azure/search/cognitive-search-tutorial-blob-dotnet?WT.mc_id=aiml-13155-ayyonet)
-   [Azure Cognitive Search client library for .NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/search/Azure.Search.Documents?WT.mc_id=aiml-13155-ayyonet)
-   [Azure Cognitive Search .NET Samples](https://github.com/Azure-Samples/azure-search-dotnet-samples?WT.mc_id=aiml-13155-ayyonet)
-   [Azure Cognitive Search client library for .NET: Creating an Index](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/search/Azure.Search.Documents?WT.mc_id=aiml-13155-ayyonet#creating-an-index)

-   [Add metadata to image blobs](https://docs.microsoft.com/azure/cognitive-services/computer-vision/tutorials/storage-lab-tutorial?WT.mc_id=aiml-13155-ayyonet)
-   [Store Application Data with Azure Storage](https://docs.microsoft.com/learn/modules/store-app-data-with-azure-blob-storage/?WT.mc_id=aiml-13155-ayyonet)

-   [Dowload FotoFly link (.zip file)](http://www.java2s.com/Open-Source/CSharp_Free_Code/Windows_Presentation_Foundation_Library/Download_Fotofly_Photo_Metadata_Library.htm) is at the end of the page.
-   [Publish a .NET console application using Visual Studio tutorial](https://docs.microsoft.com/dotnet/core/tutorials/publishing-with-visual-studio?WT.mc_id=aiml-0000-ayyonet)
