using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class StudentReportController : Controller
    {
        // GET: StudentReport
        public ActionResult report()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CalculateScore()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    Users usercookie = Session["usercookie"] as Users;
                    Courses SelectedCourse = Session["SelectedCourse"] as Courses;
                    Papers SelectedPaper = Session["SelectedPaper"] as Papers;
                    Levels SelectedLevel = Session["SelectedLevel"] as Levels;
                    Guid TestId = new Guid(Session["TestId"].ToString());
                    List<StudentTestLog> result = dbContext.StudentTestLog.Where(q => q.CourseId == SelectedCourse.CourseId && 
                                                    q.PaperId == SelectedPaper.PaperId && 
                                                    q.Level == SelectedLevel.Level && 
                                                    q.isCorrected == false &&
                                                    q.TestId == TestId).ToList();

                    Scores scoreCard = new Scores();
                    int score = 0;
                    foreach(StudentTestLog answer in result)
                    {
                        var isCorrect = dbContext.Answers.Where(a => a.QId == answer.QId && a.CorrectOption == answer.SelectedOption).FirstOrDefault();
                        if (isCorrect != null)
                            score++;
                        answer.isCorrected = true;
                        dbContext.SaveChanges();
                    }

                    List<Scores> checkDuplicates = dbContext.Scores.Where(s => s.TestId == TestId).ToList();
                    if (checkDuplicates.Count >= 1)
                    {
                        scoreCard = dbContext.Scores.Where(s => s.TestId == TestId).FirstOrDefault();
                        scoreCard.TestId = TestId;
                        scoreCard.UserId = usercookie.Id;
                        scoreCard.LastUpdatedDate = DateTime.UtcNow;
                        scoreCard.Score = score;
                        dbContext.Scores.Add(scoreCard);
                        dbContext.SaveChanges();
                    }
                    else if (checkDuplicates.Count == 0)
                    {
                        scoreCard.TestId = TestId;
                        scoreCard.UserId = usercookie.Id;
                        scoreCard.LastUpdatedDate = DateTime.UtcNow;
                        scoreCard.Score = score;
                        dbContext.Scores.Add(scoreCard);
                        dbContext.SaveChanges();
                    }
                    return View(scoreCard);
                }
            }
        }
    }
}