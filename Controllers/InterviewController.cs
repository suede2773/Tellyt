using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Tellyt.Utilities;
using System.Drawing;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Tellyt.Utilities;
using System.Threading.Tasks;

namespace Tellyt.Controllers
{

  public class RelatedPhotoDetail
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public string ThumbUrl { get; set; }
  }

  public class VideoDetail
  {
    public string Stream { get; set; }
    public string AccountHash { get; set; }
    public string Location { get; set; }
    public string Question { get; set; }
    public string Topic { get; set; }
    public string RecordedTime { get; set; }
    public string Type { get; set; }
    public string LastModifiedText { get; set; }
  }
  public class Topic
  {
    public Topic()
    {
      TopicQuestions = new List<TopicQuestion>();
    }
    public int TopicId { get; set; }
    public string TopicName { get; set; }
    public List<TopicQuestion> TopicQuestions { get; set; }
    public int QuestionCount { get; set; }
    public int AnsweredCount { get; set; }
  }

  public class ThumbnailInput
  {
    public string keyName { get; set; }
    public string inputFileType { get; set; }
    public string thumbnailSize { get; set; }
  }

  public class ConvertVideoInput
  {
    public string keyName { get; set; }
    public string inputFileType { get; set; }
    public string thumbnailSize { get; set; }
    public string outputFileType { get; set; }
  }

  public class VideoInput
  {
    public string keyName { get; set; }
    public string inputFileType { get; set; }
    public string outputFileType { get; set; }
    public string thumbnailSize { get; set; }
  }

  public class ValidationResult
  {
    public string duration { get; set; }
    public bool valid { get; set; }
    public string message { get; set; }
    public string keyName { get; set; }
  }

  public class UploadDataValuesModel
  {
    public string questionId { get; set; }
    public string duration { get; set; }
    public string fileName { get; set; }
  }

  public class PhotoResult
  {
    public string Url { get; set; }
    public string ThumbUrl { get; set; }
  }

  public class TopicQuestion
  {
    public int TopicId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionAudioLocation { get; set; }
    public bool IsAnswered { get; set; }
    public string RecordedTime { get; set; }
  }

  public class InterviewController : Controller
  {
    // GET: Interview
    [Authorize]
    public ActionResult Index()
    {
      ViewBag.AccountHash = "3a607b4a17256a0676680aca650548be";

      ViewBag.UserId = CommonController.GetCurrentUserId();// "eaa747d0-60f5-40b5-8257-a25397ef2524"; //hardcode for now
      return View();
    }

    public string GetTopicsAndQuestions()
    {
      var userId = CommonController.GetCurrentUserId();
      var topics = new List<Topic>();
      var topicQuestions = new List<TopicQuestion>();
      using (var db = new AmandaDevEntities())
      {
        foreach (var topic in db.Topics)
        {
          topics.Add(new Topic
          {
            TopicName = topic.Name,
            TopicId = topic.Id
          });

        }

        topicQuestions = db.Database
            .SqlQuery<TopicQuestion>("[dbo].[GetQuestions] @UserId", new SqlParameter("UserId", userId)).ToList();
      }
      //Fill in the details
      foreach (var topic in topics)
      {
        topic.TopicQuestions = topicQuestions.Where(t => t.TopicId == topic.TopicId).ToList();
        topic.QuestionCount = topic.TopicQuestions.Count;
        topic.AnsweredCount = topic.TopicQuestions.Count(q => q.IsAnswered);
      }
      Session["TopicsAndQuestions"] = topics;
      return JsonConvert.SerializeObject(topics);
    }

    //[HttpPost]
    //public JsonResult GetPhotos()
    //{
    //  var returnList = new List<PhotoResult>();
    //  var userId = CommonController.GetCurrentUserId();
    //  using (var db = new AmandaDevEntities())
    //  {
    //    foreach(var externalMedia in db.ExternalMedias.Where(e => e.UserId == userId && e.Type == "Photo"))
    //    {
    //      //var thumbUrlRequest = new GetPreSignedUrlRequest().....
    //      //var thumbUrl = 
    //    }
    //  }
    //}

    private async Task SaveVideo(string formValues, HttpPostedFileBase uploadFile)
    {
      var uploadResult = new UploadReturnResult();

      var azureUploader = new AzureUploader();
      var videoCreationSuccess = false;

      var uploadFormValues = JsonConvert.DeserializeObject<UploadDataValuesModel>(formValues);
      var b = new BinaryReader(uploadFile.InputStream);
      var byteData = b.ReadBytes(uploadFile.ContentLength);
      var memoryStream = new MemoryStream(byteData);
      memoryStream.Seek(0, SeekOrigin.Begin);

      using (var fileStream = memoryStream)
      {
        uploadResult = await azureUploader.UploadWebcamVideo(uploadFormValues.fileName, fileStream);
        if (uploadResult.Success)
        {
          var videoEncoderUrl = ConfigurationManager.AppSettings["VideoEncoderUrl"] + "/convertvideo";
          var httpClient = new HttpClient();
          httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

          var convertVideoInput = new ConvertVideoInput
          { inputFileType = "webm", outputFileType = "mp4", keyName = uploadFormValues.fileName, thumbnailSize = "720x405" };
          var postBody = JsonConvert.SerializeObject(convertVideoInput);

          var convertVideoResponse = httpClient.PostAsync(videoEncoderUrl, new StringContent(postBody, Encoding.UTF8, "application/json")).Result;
          if (convertVideoResponse.IsSuccessStatusCode)
          {
            var resultString = convertVideoResponse.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ValidationResult>(resultString);

            if (result.valid)
            {
              var durationSeconds = Convert.ToDouble(result.duration);
              //Convert to hours:minutes:seconds
              TimeSpan time = TimeSpan.FromSeconds(durationSeconds);
              var recordedTime = time.ToString(@"mm\:ss");

              var questionNumber = Convert.ToInt32(uploadFormValues.questionId);

              using (var db = new AmandaDevEntities())
              {
                var answeredQuestion = db.Questions.Where(q => q.Id == questionNumber).ToList();
                db.Videos.Add(new Video
                {
                  AccountHash = "videos",
                  Stream = uploadFormValues.fileName,
                  ExternalVideoId = 0,
                  Location = "tellyt.blob.core.windows.net",
                  Questions = answeredQuestion,
                  UserId = CommonController.GetCurrentUserId(),
                  RecordedTime = recordedTime,
                  type = "MP4",
                  LastModified = DateTime.Now
                });
                db.SaveChanges();
              }
            }
          }
        }
      }
    }

    [HttpPost]
    public async Task<JsonResult> SaveWebcamVideo(string formValues, HttpPostedFileBase uploadFile)
    {
      if (uploadFile.ContentLength > 0)
      {
        SaveVideo(formValues, uploadFile);

        return Json(new
        {
          Successful = true,
          ErrorMessage = ""
        });
      }
      else
      {
        return Json(new
        {
          Successful = false,
          ErrorMessage = "The video you saved was empty."
        });
      }
    }




    [HttpPost]
    public JsonResult UploadPhoto(string formValues, HttpPostedFileBase uploadFile)
    {
      var uploadResult = new UploadReturnResult();
      var azureUploader = new AzureUploader();
      int thumbHeight = 54;
      int fullHeight = 600;
      int thumbWidth;
      int fullWidth;
      try
      {
        var uploadFormValues = JsonConvert.DeserializeObject<UploadDataValuesModel>(formValues);

        if (uploadFile.ContentLength > 0)
        {
          var normalMap = (Bitmap)Bitmap.FromStream(uploadFile.InputStream);

          thumbWidth = (thumbHeight * normalMap.Width) / normalMap.Height;
          var thumbMap = new Bitmap(normalMap, new Size(thumbWidth, thumbHeight));

          //fullWidth = (fullHeight * normalMap.Width) / normalMap.Height;
          //var fullMap = new Bitmap(normalMap, new Size(fullWidth, fullHeight));

          var key = Guid.NewGuid().ToString();
          var keyName = key + ".png";
          var thumbKeyName = key + "_thumb.png";
         
          var imageStream = new MemoryStream();
          normalMap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
          imageStream.Position = 0;
          using (var fileStream = imageStream)
          {
            uploadResult = azureUploader.UploadPhoto(keyName, fileStream);
          }

          if (uploadResult.Success)
          {
            var thumbImageStream = new MemoryStream();
            thumbMap.Save(thumbImageStream, System.Drawing.Imaging.ImageFormat.Png);
            thumbImageStream.Position = 0;
            using (var thumbFileStream = thumbImageStream)
            {
              uploadResult = azureUploader.UploadPhoto(thumbKeyName, thumbFileStream);
            }
          }

          if(!uploadResult.Success)
          {
            return Json(new
            {
              Successful = false,
              ErrorMessage = uploadResult.Message
            });
          }

          using (var db = new AmandaDevEntities())
          {
            var questions = new List<Question> {new Question {Id = Convert.ToInt32(uploadFormValues.questionId)}};
            db.ExternalMedias.Add(new ExternalMedia
            {
              Key = keyName,
              ThumbKey = thumbKeyName,
              ThumbUrl = "https://tellyt.blob.core.windows.net/photos/" + thumbKeyName,
              Url = "https://tellyt.blob.core.windows.net/photos/" + keyName,
              Type = "Photo",
              LastModified = DateTime.Now,
              UserId = CommonController.GetCurrentUserId(),
              MediaType = "Image",
              Questions = questions,
              Name = keyName
            });
            db.SaveChanges();
          }

          return Json(new
          {
            Successful = uploadResult.Success,
            ErrorMessage = uploadResult.Message
          });
        }
        else
        {
          return Json(new
          {
            Successful = false,
            ErrorMessage = "The file you uploaded was empty."
          });
        }
      }
      catch (Exception ex)
      {
        return Json(new
        {
          Successful = false,
          ErrorMessage = "There was an error uploading your file. Please contact technical support and provide the following error message: " + ex.Message
        });
      }
    }

    [HttpPost]
    public string GetUserRelatedPhotos(int topicId, int questionId)
    {
      var userId = CommonController.GetCurrentUserId();
      var relatedPhotoList = new List<RelatedPhotoDetail>();

      using (var db = new AmandaDevEntities())
      {
        relatedPhotoList = db.Database
          .SqlQuery<RelatedPhotoDetail>("[dbo].[GetTopicPhotos] @UserId, @TopicId, @QuestionId",
            new SqlParameter("@UserId", userId), new SqlParameter("@TopicId", topicId),
            new SqlParameter("@QuestionId", questionId)).ToList();
      }

      return JsonConvert.SerializeObject(relatedPhotoList);
    }

    [HttpPost]
    public string GetUserVideos(int topicId, int questionId)
    {
      var userId = CommonController.GetCurrentUserId();
      var userQuestions = new List<VideoDetail>();

      using (var db = new AmandaDevEntities())
      {
        userQuestions = db.Database
            .SqlQuery<VideoDetail>("[dbo].[GetTopicVideos] @UserId, @TopicId, @QuestionId", new SqlParameter("@UserId", userId), new SqlParameter("@TopicId", topicId), new SqlParameter("@QuestionId", questionId)).ToList();  
      }
      return JsonConvert.SerializeObject(userQuestions);
    }

    [HttpPost]
    public string GetTopicVideos(string topic)
    {
      var videoDetails = new List<VideoDetail>();
      var userId = CommonController.GetCurrentUserId();

      using (var db = new AmandaDevEntities())
      {
        foreach (var video in db.Videos.Where(v => v.UserId == userId))
        {
          var questionText = string.Empty;
          var firstOrDefaultQuestion = video.Questions.FirstOrDefault();
          if (firstOrDefaultQuestion != null)
          {
            questionText = firstOrDefaultQuestion.QuestionText;
          }
          videoDetails.Add(new VideoDetail
          {
            AccountHash = video.AccountHash,
            Location = video.Location,
            Stream = video.Stream,
            Question = questionText
          });
        }
      }

      return JsonConvert.SerializeObject(videoDetails);
    }

    [HttpPost]
    public string GetVideos()
    {
      var videoDetails = new List<VideoDetail>();
      var userId = CommonController.GetCurrentUserId();
      using (var db = new AmandaDevEntities())
      {
        foreach (var video in db.Videos.Where(v => v.UserId == userId))
        {
          var questionText = string.Empty;
          var firstOrDefaultQuestion = video.Questions.FirstOrDefault();
          if (firstOrDefaultQuestion != null)
          {
            questionText = firstOrDefaultQuestion.QuestionText;
          }
            videoDetails.Add(new VideoDetail
            {
              AccountHash = video.AccountHash,
              Location = video.Location,
              Stream = video.Stream,
              Question = questionText
            });
        }
      }
      return JsonConvert.SerializeObject(videoDetails);
    }

    [HttpPost]
    public string DeleteVideo(string stream)
    {
      try
      {
        var userId = CommonController.GetCurrentUserId();
        using (var db = new AmandaDevEntities())
        {
          var currentVideo = db.Videos.First(v => v.UserId == userId && v.Stream == stream);
          var videoQuestions = currentVideo.Questions.ToList();

          foreach (var videoQuestion in videoQuestions)
          {
            currentVideo.Questions.Remove(videoQuestion);
          }
          db.SaveChanges();

          db.Videos.Remove(currentVideo);
          db.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
      return "success";
    }

    //[HttpPost]
    //public string SaveWebcamVideo()

    //[HttpPost]
    //public string SaveVideo(string location, string hash, string stream, string video, int questionId, string duration)
    //{
    //  try
    //  {
    //    //Transform seconds into hh:mm:ss
    //    TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(duration));
    //    var recordedTime = time.ToString(@"mm\:ss");
    //    using (var db = new AmandaDevEntities())
    //    {
    //      var answeredQuestion = db.Questions.Where(q => q.Id == questionId).ToList();
    //      db.Videos.Add(new Video
    //      {
    //        AccountHash = hash,
    //        Stream = stream,
    //        ExternalVideoId = 0,
    //        Location = location,
    //        Questions = answeredQuestion,
    //        UserId = CommonController.GetCurrentUserId(),
    //        RecordedTime = recordedTime,
    //        LastModified = DateTime.Now
    //      });
    //      db.SaveChanges();
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    return ex.Message;
    //  }
    //  return "success";
    //}

    public string GetUserTopicsAndQuestions()
    {
      return GetTopicsAndQuestions();
    }

  }
}