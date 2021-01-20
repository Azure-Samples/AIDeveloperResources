# AIDeveloperResources

SETUP INSTRUCTIONS:

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
    Name, Key and ConnectionString set to Name, Key, ConnectionString settings respectively under
    AzureStorageAccount section
