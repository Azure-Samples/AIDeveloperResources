using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JournalHelper.Search
{
    public class Skills
    {
        public static OcrSkill CreateOcrSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("image")
            {
                Source = "/document/normalized_images/*"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("text")
            {
                TargetName = "text"
            });

            OcrSkill ocrSkill = new OcrSkill(inputMappings, outputMappings)
            {
                Description = "Extract text (plain and structured) from image",
                Context = "/document/normalized_images/*",
                DefaultLanguageCode = OcrSkillLanguage.En,
                ShouldDetectOrientation = true
            };

            return ocrSkill;
        }

        public static MergeSkill CreateMergeSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/content"
            });
            inputMappings.Add(new InputFieldMappingEntry("itemsToInsert")
            {
                Source = "/document/normalized_images/*/text"
            });
            inputMappings.Add(new InputFieldMappingEntry("offsets")
            {
                Source = "/document/normalized_images/*/contentOffset"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("mergedText")
            {
                TargetName = "merged_text"
            });

            MergeSkill mergeSkill = new MergeSkill(inputMappings, outputMappings)
            {
                Description = "Create merged_text which includes all the textual representation of each image inserted at the right location in the content field.",
                Context = "/document",
                InsertPreTag = " ",
                InsertPostTag = " "
            };

            return mergeSkill;
        }

        public static LanguageDetectionSkill CreateLanguageDetectionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/merged_text"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("languageCode")
            {
                TargetName = "languageCode"
            });

            LanguageDetectionSkill languageDetectionSkill = new LanguageDetectionSkill(inputMappings, outputMappings)
            {
                Description = "Detect the language used in the document",
                Context = "/document"
            };

            return languageDetectionSkill;
        }

        public static SplitSkill CreateSplitSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/merged_text"
            });
            inputMappings.Add(new InputFieldMappingEntry("languageCode")
            {
                Source = "/document/languageCode"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("textItems")
            {
                TargetName = "pages",
            });

            SplitSkill splitSkill = new SplitSkill(inputMappings, outputMappings)
            {
                Description = "Split content into pages",
                Context = "/document",
                TextSplitMode = TextSplitMode.Pages,
                MaximumPageLength = 4000,
                DefaultLanguageCode = SplitSkillLanguage.En
            };

            return splitSkill;
        }

        public static EntityRecognitionSkill CreateEntityRecognitionSkill()
        {
            List<InputFieldMappingEntry> inputMappings = new List<InputFieldMappingEntry>();
            inputMappings.Add(new InputFieldMappingEntry("text")
            {
                Source = "/document/pages/*"
            });

            List<OutputFieldMappingEntry> outputMappings = new List<OutputFieldMappingEntry>();
            outputMappings.Add(new OutputFieldMappingEntry("organizations")
            {
                TargetName = "organizations"
            });

            EntityRecognitionSkill entityRecognitionSkill = new EntityRecognitionSkill(inputMappings, outputMappings)
            {
                Description = "Recognize organizations",
                Context = "/document/pages/*",
                DefaultLanguageCode = EntityRecognitionSkillLanguage.En
            };
            entityRecognitionSkill.Categories.Add(EntityCategory.Organization);

            return entityRecognitionSkill;
        }
    }
}
