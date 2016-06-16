using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using RawlinsonITUpload.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RawlinsonITUploadToQueue.Controllers
{
    public class UploadController : Controller
    {
        private CloudQueue requestQueue;

        private static CloudBlobContainer blobContainer; // GET: Upload
        public UploadController()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            //blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            blobContainer = blobClient.GetContainerReference("inputfiles");
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            requestQueue = queueClient.GetQueueReference("processrequest");
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(HttpPostedFileBase file)
        {
            CloudBlockBlob fileBlob = null;
            if (file != null && file.ContentLength > 0)
            { fileBlob = await AddToStorage(file);
                }
            if (fileBlob != null)
            {
                BlobInformation blobInfo = new BlobInformation() { BlobUri = new Uri(fileBlob.Uri.ToString()) };
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                await requestQueue.AddMessageAsync(queueMessage);
                Trace.TraceInformation("Created queue message for file", blobInfo.BlobNameWithoutExtension);
                TempData["ResultMessage"] = "Upload completed successfully";
            }
            else
            { TempData["ResultMessage"] = "File empty"; }
            return View();
        }


        private async Task<CloudBlockBlob> AddToStorage(HttpPostedFileBase file) {
            Trace.TraceInformation("Uploading file {0}", file.FileName);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            // Retrieve reference to a blob. 
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.
            using (var fileStream = file.InputStream)
            {
                await blob.UploadFromStreamAsync(fileStream);
            }

            Trace.TraceInformation("Uploaded file to {0}", blob.Uri.ToString());

            return blob;



        }



    }
}