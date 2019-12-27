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

namespace Tellyt.Controllers
{
  public class VideoDetail
  {
    public string Stream { get; set; }
    public string AccountHash { get; set; }
    public string Location { get; set; }
    public string Question { get; set; }
    public string Topic { get; set; }
    public string RecordedTime { get; set; }
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

    [HttpPost]
    public JsonResult GetPhotos()
    {
      var returnList = new List<PhotoResult>();
      var userId = CommonController.GetCurrentUserId();
      using (var db = new AmandaDevEntities())
      {
        foreach(var externalMedia in db.ExternalMedias.Where(e => e.UserId == userId && e.Type == "Photo"))
        {
          var thumbUrlRequest = new GetPreSignedUrlRequest()
          //var thumbUrl = 
        }
      }
    }

    [HttpPost]
    public JsonResult UploadPhoto(string fileName, HttpPostedFileBase uploadFile)
    {
      var uploadResult = new UploadReturnResult();

      int thumbHeight = 54;
      int thumbWidth;
      try
      {
        if (uploadFile.ContentLength > 0)
        {
          var normalMap = (Bitmap)Bitmap.FromStream(uploadFile.InputStream);
          thumbWidth = (thumbHeight * normalMap.Width) / normalMap.Height;
          var thumbMap = new Bitmap(normalMap, new Size(thumbWidth, thumbHeight));

          var key = Guid.NewGuid().ToString();
          var keyName = key + ".png";
          var thumbKeyName = key + "_thumb.png";

          using (MemoryStream memoryStream = new MemoryStream())
          {
            normalMap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            
            uploadResult = AmazonS3Uploader.UploadFile(keyName, memoryStream);
          }

          if (uploadResult.Success)
          {
            using (MemoryStream thumbMemoryStream = new MemoryStream())
            {
              thumbMap.Save(thumbMemoryStream, System.Drawing.Imaging.ImageFormat.Png);

              uploadResult = AmazonS3Uploader.UploadFile(thumbKeyName, thumbMemoryStream);
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
            var bucketName = ConfigurationManager.AppSettings["BucketName"];

            db.ExternalMedias.Add(new ExternalMedia
            {
              Bucket = bucketName,
              Key = keyName,
              ThumbKey = thumbKeyName,
              ThumbUrl = string.Empty,
              Url = string.Empty,
              Type = "Photo",
              LastModified = DateTime.Now,
              UserId = CommonController.GetCurrentUserId()
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

    [HttpPost]
    public string SaveVideo(string location, string hash, string stream, string video, int questionId, string duration)
    {
      try
      {
        //Transform seconds into hh:mm:ss
        TimeSpan time = TimeSpan.FromSeconds(Convert.ToDouble(duration));
        var recordedTime = time.ToString(@"mm\:ss");
        using (var db = new AmandaDevEntities())
        {
          var answeredQuestion = db.Questions.Where(q => q.Id == questionId).ToList();
          db.Videos.Add(new Video
          {
            AccountHash = hash,
            Stream = stream,
            ExternalVideoId = Convert.ToInt32(video),
            Location = location,
            Questions = answeredQuestion,
            UserId = CommonController.GetCurrentUserId(),
            RecordedTime = recordedTime
          });
          db.SaveChanges();
        }
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
      return "success";
    }

    public string GetUserTopicsAndQuestions()
    {
      return GetTopicsAndQuestions();
    }

  }
}