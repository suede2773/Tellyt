using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;

namespace Tellyt.Utilities
{
  public class UploadReturnResult
  {
    public bool Success { get; set; }
    public string Message { get; set; }
  }

  public class AmazonS3Uploader
  {
    
    private static readonly Amazon.RegionEndpoint bucketRegion = Amazon.RegionEndpoint.USEast1;
    private static IAmazonS3 s3Client;

    public static UploadReturnResult UploadFile(string keyName, Stream uploadFileStream)
    {
      var bucketName = ConfigurationManager.AppSettings["BucketName"];

      s3Client = new AmazonS3Client(bucketRegion);

      try
      {
        var fileTransferUtility = new TransferUtility(s3Client);

        fileTransferUtility.Upload(uploadFileStream, bucketName, keyName);

        return new UploadReturnResult { Message = "Upload File Complete", Success = true };
      }
      catch (AmazonS3Exception e)
      {
        var errorMessage = "Error encountered on server. Message:'" + e.Message + "' when writing an object";
        return new UploadReturnResult { Message = "Upload File Complete", Success = true };
      }
      catch (Exception e)
      {
        var errorMessage = "Unknown encountered on server. Message:'" + e.Message + "' when writing an object";
        return new UploadReturnResult { Message = "Upload File Complete", Success = true };
      }
    }

    public static async Task UploadFileAsync(string keyName, Stream uploadFileStream)
    {
      s3Client = new AmazonS3Client(bucketRegion);
      var bucketName = ConfigurationManager.AppSettings["BucketName"];
      try
      {
        var fileTransferUtility = new TransferUtility(s3Client);

        await fileTransferUtility.UploadAsync(uploadFileStream, bucketName, keyName);

        Console.WriteLine("Upload file complete");
      }
      catch (AmazonS3Exception e)
      {
        Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
      }
      catch (Exception e)
      {
        Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
      }
    }


    //private string bucketName = "your-amazon-s3-bucket";
    //private string keyName = "the-name-of-your-file";
    //private string filePath = "C:\\Users\\yourUserName\\Desktop\\myImageToUpload.jpg";

    //public void UploadFile(string keyName)
    //{

    //  var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

    //  try
    //  {
    //    PutObjectRequest putRequest = new PutObjectRequest
    //    {
    //      BucketName = bucketName,
    //      Key = keyName,
    //      FilePath = filePath,
    //      ContentType = "text/plain"
    //    };

    //    PutObjectResponse response = client.PutObject(putRequest);
    //  }
    //  catch (AmazonS3Exception amazonS3Exception)
    //  {
    //    if (amazonS3Exception.ErrorCode != null &&
    //        (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
    //        ||
    //        amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
    //    {
    //      throw new Exception("Check the provided AWS Credentials.");
    //    }
    //    else
    //    {
    //      throw new Exception("Error occurred: " + amazonS3Exception.Message);
    //    }
    //  }
    //}
  }
}