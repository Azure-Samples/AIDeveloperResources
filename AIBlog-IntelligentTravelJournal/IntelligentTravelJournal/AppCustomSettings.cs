// Created using https://json2csharp.com/
// Renated Root class to RootCustomSettings

// RootCustomSettings myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

using System.Text.Json.Serialization;

public class SampleDataFolders
{
    [JsonPropertyName("RootFolder")]
    public string RootFolder { get; set; }

    [JsonPropertyName("PhotosToTagFolder")]
    public string PhotosToTagFolder { get; set; }

    [JsonPropertyName("PhotosToAnalyzeFolder")]
    public string PhotosToAnalyzeFolder { get; set; }

    [JsonPropertyName("VoiceMemosFolder")]
    public string VoiceMemosFolder { get; set; }

    [JsonPropertyName("IndexesFolder")]
    public string IndexesFolder { get; set; }

    [JsonPropertyName("TranscribedFileFolder")]
    public string TranscribedFileFolder { get; set; }

    [JsonPropertyName("BatchTranscribedFolder")]
    public string BatchTranscribedFolder { get; set; }

    [JsonPropertyName("ExtractedTagsFolder")]
    public string ExtractedTagsFolder { get; set; }

    [JsonPropertyName("TaggedPhotosFolder")]
    public string TaggedPhotosFolder { get; set; }

    [JsonPropertyName("TravelJournalsFolder")]
    public string TravelJournalsFolder { get; set; }

    [JsonPropertyName("AnalyzedImagesFolder")]
    public string AnalyzedImagesFolder { get; set; }
}

public class SampleIndividualFiles
{
    [JsonPropertyName("SampleVoiceMemoFile")]
    public string SampleVoiceMemoFile { get; set; }

    [JsonPropertyName("PhotoFileToProcess")]
    public string PhotoFileToProcess { get; set; }

    [JsonPropertyName("SampleTextIndexJsonFile")]
    public string SampleTextIndexJsonFile { get; set; }
}

public class AzureResourceGroupSettings
{
    [JsonPropertyName("ResourceGroupName")]
    public string ResourceGroupName { get; set; }

    [JsonPropertyName("SubscriptionId")]
    public string SubscriptionId { get; set; }
}

public class AzureBlobContainers
{
    [JsonPropertyName("InputVoiceMemoFiles")]
    public string InputVoiceMemoFiles { get; set; }

    [JsonPropertyName("BatchTranscribedJsonResults")]
    public string BatchTranscribedJsonResults { get; set; }

    [JsonPropertyName("ExtractedTranscribedTexts")]
    public string ExtractedTranscribedTexts { get; set; }

    [JsonPropertyName("TravelJournals")]
    public string TravelJournals { get; set; }

    [JsonPropertyName("TaggedPhotos")]
    public string TaggedPhotos { get; set; }
}

public class SpeechConfigSettings
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("Region")]
    public string Region { get; set; }
}

public class ComputerVisionSettings
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("Region")]
    public string Region { get; set; }

    [JsonPropertyName("Endpoint")]
    public string Endpoint { get; set; }
}
public class TextAnalyticsSettings
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("Endpoint")]
    public string Endpoint { get; set; }
}


public class TranslatorConfigSettings
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("Endpoint")]
    public string Endpoint { get; set; }
}
public class SearchConfigSettings
{
    [JsonPropertyName("CongnitiveKey")]
    public string CongnitiveKey { get; set; }

    [JsonPropertyName("CognitiveEndpoint")]
    public string CognitiveEndpoint { get; set; }

    [JsonPropertyName("ServiceQueryKey")]
    public string ServiceQueryKey { get; set; }

    [JsonPropertyName("ServiceKey")]
    public string ServiceKey { get; set; }

    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("ServiceUrl")]
    public string ServiceUrl { get; set; }

    [JsonPropertyName("IndexName")]
    public string IndexName { get; set; }
}


public class AzureStorageAccount
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("ConnectionString")]
    public string ConnectionString { get; set; }
}

public class RootCustomSettings
{
    [JsonPropertyName("SampleDataFolders")]
    public SampleDataFolders SampleDataFolders { get; set; }

    [JsonPropertyName("SampleIndividualFiles")]
    public SampleIndividualFiles SampleIndividualFiles { get; set; }

    [JsonPropertyName("AzureResourceGroupSettings")]
    public AzureResourceGroupSettings AzureResourceGroupSettings { get; set; }

    [JsonPropertyName("AzureBlobContainers")]
    public AzureBlobContainers AzureBlobContainers { get; set; }

    [JsonPropertyName("SpeechConfigSettings")]
    public SpeechConfigSettings SpeechConfigSettings { get; set; }

    [JsonPropertyName("ComputerVisitionSettings")]
    public ComputerVisionSettings ComputerVisionSettings { get; set; }

    [JsonPropertyName("TextAnalyticsSettings")]
    public TranslatorConfigSettings TextAnalyticsSettings { get; set; }

    [JsonPropertyName("TranslatorConfigSettings")]
    public TranslatorConfigSettings TranslatorConfigSettings { get; set; }

    [JsonPropertyName("SearchConfigSettings")]
    public SearchConfigSettings SearchConfigSettings { get; set; }

    [JsonPropertyName("AzureStorageAccount")]
    public AzureStorageAccount AzureStorageAccount { get; set; }
}

