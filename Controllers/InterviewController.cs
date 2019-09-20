using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Tellyt.Controllers
{
  public class VideoDetail
  {
    public string Stream;
    public string Hash;
    public string Location;
    public string Question;
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

  //public class TopicOutput
  //{
  //  public bool Selected { get; set; }
  //  public string Name { get; set; }
  //}

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
              Hash = video.AccountHash,
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
    public string SaveVideo(string location, string hash, string stream, string video, int questionId)
    {
      try
      {
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
            UserId = CommonController.GetCurrentUserId()
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
      return Session["TopicsAndQuestions"] == null ? GetTopicsAndQuestions() : JsonConvert.SerializeObject((List<Topic>)Session["TopicsAndQuestions"]);
    }

  }
}