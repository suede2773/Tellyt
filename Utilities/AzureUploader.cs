using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Tellyt.Utilities
{

  public class UploadReturnResult
  {
    public bool Success { get; set; }
    public string Message { get; set; }
  }

  public class AzureUploader
  {
    private const string connectionString =
      "DefaultEndpointsProtocol=https;AccountName=tellyt;AccountKey=tpmgk/PVSzZ2qm8IZcA5/1QsMFcc++wWICOXiXFT2/oa9hLaj0BXbSWxIrW05MobconSfk5k+y9l1z/s5n9p7A==;EndpointSuffix=core.windows.net";

    public async Task<UploadReturnResult> UploadWebcamVideo(string keyName, MemoryStream stream)
    {
      var uploadReturnResult = new UploadReturnResult { Message = string.Empty, Success = false };
      try
      {
        var storageacc = CloudStorageAccount.Parse(connectionString);
        var blobClient = storageacc.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference("videos");
        var videoBlob = container.GetBlockBlobReference(keyName + ".webm");
        videoBlob.Properties.ContentType = "video/webm";
        videoBlob.UploadFromStream(stream);
        uploadReturnResult.Success = true;

      }
      catch (Exception e)
      {
        uploadReturnResult.Message = e.Message;
        uploadReturnResult.Success = false;
      }
      return uploadReturnResult;
    }

    public UploadReturnResult UploadPhoto(string keyName, MemoryStream stream)
    {
      var uploadReturnResult = new UploadReturnResult {Message = string.Empty, Success = false};
      try
      {
        var storageacc = CloudStorageAccount.Parse(connectionString);
        var blobClient = storageacc.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference("photos");
        var photoBlob = container.GetBlockBlobReference(keyName);
        photoBlob.Properties.ContentType = "image/png";
        photoBlob.UploadFromStream(stream);
        uploadReturnResult.Success = true;
      }
      catch (Exception e)
      {
        uploadReturnResult.Message = e.Message;
        uploadReturnResult.Success = false;
      }
      return uploadReturnResult;
    }

    //public UploadReturnResult UploadPhoto(string keyName, MemoryStream stream)
    //{
    //  var uploadResult = new UploadReturnResult{Success = false, Message = string.Empty};
    //  try
    //  {
    //    var streamBytes = GetStreamBytes(stream);
    //    var containerName = "photos";
    //    var method = "PUT";
    //    string requestUri = $"https://{StorageAccountName}.blob.core.windows.net/{containerName}/{keyName}";
    //    var request = (HttpWebRequest) WebRequest.Create(requestUri);
    //    request.ContentType = "image/jpeg";
    //    request.ContentLength = streamBytes.Length;

    //    string now = DateTime.UtcNow.ToString("R");

    //    request.Headers.Add("x-ms-date", now);
    //    request.Headers.Add("x-ms-version", "2015-12-11");
    //    request.Headers.Add("x-ms-blob-type", "BlockBlob");
    //    request.Headers.Add("Authorization",
    //      AzureStorageAuthenticationHelper.AuthorizationHeader(method, now, request, StorageAccountName,
    //        StorageAccountKey, containerName, keyName));

    //    using (Stream requestStream = request.GetRequestStream())
    //    {
    //      requestStream.Write(streamBytes, 0, streamBytes.Length);
    //    }

    //    var response = (HttpWebResponse) request.GetResponse();
    //    if (response.StatusCode == HttpStatusCode.OK)
    //    {
    //      uploadResult.Success = true;
    //      uploadResult.Message = "Image Uploaded.";
    //    }

    //    //return uploadResult;
    //  }
    //  catch (Exception e)
    //  {
    //    uploadResult.Success = false;
    //    uploadResult.Message = e.Message;
    //  }
    //  return uploadResult;
    //}

    public byte[] GetStreamBytes(MemoryStream stream)
    {
      using (var memoryStream = new MemoryStream())
      {
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
      }
    }

  }
}