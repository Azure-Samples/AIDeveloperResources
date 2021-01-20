using Fotofly;
using System;
using System.Collections.Generic;
using System.Text;

namespace JournalHelper
{
    class ImageProperties
    {
        public static void SetPhotoTags(string inputFile, List<string> tags)
        {
            JpgPhoto jpgPhoto = new JpgPhoto(inputFile);

            // TODO:  BUGBUG:  Changing description is throwing exception. In the end it is trying to set null value
            //jpgPhoto.Metadata.Description = @"Tags updated on " + DateTime.Now.ToString();

            Console.WriteLine($"oldTags: {jpgPhoto.Metadata.Tags}");

            // not sure whether or not to preserve the old tags.  
            // Assuming we are not settings the same tags repeatedly, retaining original tags.
            //jpgPhoto.Metadata.Tags.Clear();

            jpgPhoto.Metadata.Tags.AddRange(tags);
            jpgPhoto.WriteMetadata();
        }

    }
}
