using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using RawlinsonITUpload.Domain;
namespace RawlinsonITUploadToQueueWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("processrequest")] BlobInformation blobInfo, TextWriter logger)
        {
            logger.WriteLine("Processing file " + blobInfo.BlobNameWithoutExtension);
        }
    }
}
