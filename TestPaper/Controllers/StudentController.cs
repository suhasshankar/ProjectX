using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class StudentController : Controller
    {
        [HttpGet]
        public ActionResult DashBoard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetCourse()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    Courses course = new Courses();
                    Users usercookie = Session["usercookie"] as Users;
                    List<SelectListItem> items = new List<SelectListItem>();
                    var courseList = dbContext.AllocateCourse.Where(a => a.UserId == usercookie.Id).Select(a => a.CourseId).ToList();
                    foreach (int CId in courseList)
                    {
                        SelectListItem values = new SelectListItem();
                        values = dbContext.Courses.Where(c => c.CourseId == CId).Select(c => new SelectListItem
                        {
                            Text = c.CourseName,
                            Value = c.CourseId.ToString()
                        }).FirstOrDefault();
                        items.Add(values);
                    }
                    course.ListOfCourses = items;
                    return View(course);
                }
            }
        }
        [HttpGet]
        public JsonResult GetPaper(Papers paper)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    var result = (from p in dbContext.Papers
                                  where p.CourseId == paper.CourseId
                                  select new
                                  {
                                      p.PaperId,
                                      p.PaperName
                                  }).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetLevels()
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    var result = (from p in dbContext.Levels
                                  select new
                                  {
                                      p.Level,
                                      p.LevelName
                                  }).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult QuestionIterator(SelectedPaper result)
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                if (result.CourseId != 0 && result.levelId != 0 && result.paperId != 0)
                {
                    DBEntities dbContext = new DBEntities();
                    if (Session["SelectedCourse"] == null && Session["SelectedPaper"] == null && Session["SelectedLevel"] == null && Session["SelectedCourse"] == null)
                    {
                        Session["SelectedCourse"] = (from c in dbContext.Courses where c.CourseId == result.CourseId select c).ToList().FirstOrDefault();
                        Session["SelectedPaper"] = (from p in dbContext.Papers where p.PaperId == result.paperId select p).ToList().FirstOrDefault();
                        Session["SelectedLevel"] = (from l in dbContext.Levels where l.Level == result.levelId select l).ToList().FirstOrDefault();
                    }

                    if (Session["TestId"] == null)
                        Session["TestId"] = Guid.NewGuid(); //create testId for new test

                    Users usercookie = Session["usercookie"] as Users;
                    Courses SelectedCourse = Session["SelectedCourse"] as Courses;
                    Papers SelectedPaper = Session["SelectedPaper"] as Papers;
                    Levels SelectedLevel = Session["SelectedLevel"] as Levels;
                    Guid TestId = new Guid(Session["TestId"].ToString());

                    var questions = dbContext.GetQuestions(SelectedCourse.CourseId, SelectedPaper.PaperId, usercookie.Id, SelectedLevel.Level).ToList();
                    if (questions.Count > 0)
                    {
                        List<GetQuestions_Result> QuestionList = new List<GetQuestions_Result>();
                        int outercount = 0;
                        int innercount = 0;
                        while (true)
                        {
                            if (outercount >= questions.Count)
                                break;
                            GetQuestions_Result outernode = questions[outercount];
                            int QId = outernode.QId;
                            innercount = 0;
                            GetQuestions_Result FinalQuestion = new GetQuestions_Result();
                            FinalQuestion.QId = outernode.QId;
                            FinalQuestion.PaperId = outernode.PaperId;
                            FinalQuestion.CourseId = outernode.CourseId;
                            FinalQuestion.Question = outernode.Question;
                            FinalQuestion.Duration = outernode.Duration;
                            FinalQuestion.OptionType = outernode.OptionType;
                            List<choices> choicelist = new List<choices>();
                            foreach (GetQuestions_Result innernode in questions)
                            {
                                choices choice = new choices();
                                if (QId == innernode.QId)
                                {
                                    choice.Comprehensive = "";
                                    choice.choiceId = innernode.OptionId;
                                    choice.choiceText = innernode.OptionText;
                                    choicelist.Add(choice);
                                    innercount++;
                                }
                                else
                                    continue;
                            }
                            FinalQuestion.options = choicelist;
                            QuestionList.Add(FinalQuestion);
                            outercount = outercount + innercount;
                        }
                        return View(RandomizeQuestions(QuestionList));
                    }
                    else
                        return RedirectToAction("CalculateScore");
                }
                else
                {
                    ViewData["MSG"] = "Please select valid options to proceed.";
                    return RedirectToAction("GetCourse");
                }
            }
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
                    Scores scoreCard = new Scores();

                    var testType = dbContext.AllocateCourse.Where(a => a.UserId == usercookie.Id && a.CourseId == SelectedCourse.CourseId).FirstOrDefault();

                    StudentTestLog checkIfAlreadyTaken = dbContext.StudentTestLog.Where(s => s.UserId == usercookie.Id && s.CourseId == SelectedCourse.CourseId
                       && s.PaperId == SelectedPaper.PaperId && s.Level == SelectedLevel.Level).FirstOrDefault();

                    var isCorrected = dbContext.Scores.Where(s => s.TestId == checkIfAlreadyTaken.TestId).FirstOrDefault();

                    scoreCard.CourseName = dbContext.Courses.Where(c => c.CourseId == checkIfAlreadyTaken.CourseId).Select(s => s.CourseName).FirstOrDefault().ToString();
                    scoreCard.paperName = dbContext.Papers.Where(c => c.PaperId == checkIfAlreadyTaken.PaperId).Select(s => s.PaperName).FirstOrDefault().ToString();
                    scoreCard.LevelName = dbContext.Levels.Where(c => c.Level == checkIfAlreadyTaken.Level).Select(s => s.LevelName).FirstOrDefault().ToString();
                    scoreCard.userName = dbContext.Users.Where(c => c.Id == usercookie.Id).Select(s => s.UserName).FirstOrDefault().ToString();

                    if (testType.TestType == 1 && isCorrected == null)
                    {
                        List<StudentTestLog> result = dbContext.StudentTestLog.Where(q => q.CourseId == SelectedCourse.CourseId &&
                                                        q.PaperId == SelectedPaper.PaperId &&
                                                        q.Level == SelectedLevel.Level &&
                                                        q.isCorrected == false &&
                                                        q.TestId == TestId).ToList();

                        int score = 0;
                        foreach (StudentTestLog answer in result)
                        {
                            var isCorrect = dbContext.Answers.Where(a => a.QId == answer.QId && a.CorrectOption == answer.SelectedOption).FirstOrDefault();
                            if (isCorrect != null)
                                score++;
                            answer.isCorrected = true;
                            dbContext.SaveChanges();
                        }
                        //Delete existing duplicate test id if already present
                        var DuplicateList = dbContext.Scores.Where(s => s.TestId == TestId).ToList();
                        if (DuplicateList.Count >= 1)
                        {
                            foreach (Scores deletescore in DuplicateList)
                            {
                                dbContext.Scores.Remove(deletescore);
                                dbContext.SaveChanges();
                            }
                        }
                        //Add fresh score to the board
                        scoreCard.TestId = TestId;
                        scoreCard.UserId = usercookie.Id;
                        scoreCard.LastUpdatedDate = DateTime.UtcNow;
                        scoreCard.Score = score;
                        dbContext.Scores.Add(scoreCard);
                        dbContext.SaveChanges();

                        Session["TestId"] = null;
                        return View(scoreCard);
                    }
                    else if (checkIfAlreadyTaken != null)
                    {
                        Session["TestId"] = null;
                        scoreCard.Score = -1;
                        ViewData["msg"] = "Already Taken. Please contact admin if you did not attempted this paper.";
                        ViewData["testId"] = checkIfAlreadyTaken.TestId;
                        return View(scoreCard);
                    }
                    else
                    {
                        Session["TestId"] = null;
                        scoreCard.Score = -1;
                        ViewData["msg"] = "Review Under Progress. You will be notified soon!!.";
                        return View(scoreCard);
                    }
                }
            }
        }

        public List<GetQuestions_Result> RandomizeQuestions(List<GetQuestions_Result> QuestionList)
        {
            Random random = new Random();
            int position = random.Next(QuestionList.Count);
            var question = QuestionList[position];
            QuestionList.Clear();
            QuestionList.Add(question);
            return QuestionList;
        }

        [HttpPost]
        public JsonResult CaptureResult(SelectOption optionSelected)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                if (optionSelected.QId != 0)
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        try
                        {
                            Users usercookie = Session["usercookie"] as Users;
                            Courses SelectedCourse = Session["SelectedCourse"] as Courses;
                            Papers SelectedPaper = Session["SelectedPaper"] as Papers;
                            Levels SelectedLevel = Session["SelectedLevel"] as Levels;
                            Guid TestId = new Guid(Session["TestId"].ToString());

                            var UpdateAnswers = dbContext.StudentTestLog.Where(s => s.UserId == usercookie.Id && s.TestId == TestId && s.QId == optionSelected.QId).ToList();
                            foreach (StudentTestLog deletelog in UpdateAnswers)
                            {
                                dbContext.StudentTestLog.Remove(deletelog);
                                dbContext.SaveChanges();
                            }
                            StudentTestLog log = new StudentTestLog();
                            log.TestId = TestId;
                            log.CourseId = SelectedCourse.CourseId;
                            log.PaperId = SelectedPaper.PaperId;
                            log.UserId = usercookie.Id;
                            log.QId = optionSelected.QId;
                            log.Level = SelectedLevel.Level;
                            log.SelectedOption = optionSelected.SelectedOption;
                            log.MultiLineAnswer = optionSelected.Comprehensive;
                            log.isCorrected = false;

                            dbContext.StudentTestLog.Add(log);
                            dbContext.SaveChanges();
                            ModelState.Clear();
                            return Json(new { success = true, responseText = "Result Captured. Please Continue with next questions." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, responseText = "Error Occured. Please re-attempt the same question again or contact administrator. Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, responseText = "Please Select option." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult GetScores()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                List<getscores> testlog = new List<getscores>();
                using (DBEntities dbContext = new DBEntities())
                {
                    Users usercookie = Session["usercookie"] as Users;
                    var log = dbContext.ReportCard.Where(s => s.UserId == usercookie.Id).ToList();
                    foreach (ReportCard score in log)
                    {
                        getscores list = new getscores();
                        list.ReportId = score.ReportId;
                        list.CourseName = dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault();
                        list.paperName = dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault();
                        list.LevelName = dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault();
                        list.score = score.TotalScore;
                        list.isRevalidate = score.IsRevaluate;
                        testlog.Add(list);
                    }
                }
                return View(testlog);
            }
        }

        [HttpGet]
        public ActionResult GetAnswers()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    Users usercookie = Session["usercookie"] as Users;
                    var TestId = dbContext.StudentTestLog.Where(s => s.UserId == usercookie.Id).Select(s => s.TestId).Distinct().ToList();
                    List<courseSelectedTaken> courseList = new List<courseSelectedTaken>();
                    for (int i = 0; i < TestId.Count; i++)
                    {
                        Guid id = TestId[i];
                        var results = (from s in dbContext.StudentTestLog
                                       where s.TestId == id
                                       select s).FirstOrDefault();
                        courseSelectedTaken course = new courseSelectedTaken();
                        int courseId = results.CourseId;
                        int PaperId = results.PaperId;
                        int Level = results.Level;
                        course.TestId = results.TestId;
                        var isCorrected = dbContext.StudentTestLog.Where(s => s.TestId == id & s.isCorrected == false).ToList();
                        if (isCorrected.Count > 0)
                            course.isCorrected = false;
                        else
                            course.isCorrected = true;
                        course.CourseName = dbContext.Courses.Where(c => c.CourseId == courseId).Select(c => c.CourseName).FirstOrDefault().ToString();
                        course.PaperName = dbContext.Papers.Where(c => c.PaperId == PaperId).Select(c => c.PaperName).FirstOrDefault().ToString();
                        course.Level = dbContext.Levels.Where(c => c.Level == Level).Select(c => c.LevelName).FirstOrDefault().ToString();
                        courseList.Add(course);
                    }
                    return View(courseList);
                }
            }
        }

        [HttpGet]
        public ActionResult ShowAnswers(string Id)
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                if (!string.IsNullOrEmpty(Id))
                {
                    try
                    {
                        Guid TId = new Guid(Id);
                        using (DBEntities dbContext = new DBEntities())
                        {
                            int total = 0;
                            var log = dbContext.StudentTestLog.Where(s => s.TestId == TId).Select(s => new { s.QId, s.MultiLineAnswer, s.SelectedOption }).ToList();
                            List<ShowAnswersList> list = new List<ShowAnswersList>();
                            for (int i = 0; i < log.Count; i++)
                            {
                                ShowAnswersList answer = new ShowAnswersList();
                                int id = log[i].QId;
                                Nullable<int> SelectedOption = log[i].SelectedOption;
                                answer.Question = (from Q in dbContext.Questions
                                                   where Q.QId == id
                                                   select Q.Question).FirstOrDefault();

                                if (!string.IsNullOrEmpty(log[i].MultiLineAnswer)) //mulitline type
                                {
                                    answer.SelectedOption = log[i].MultiLineAnswer;
                                    answer.CorrectOption = "Reviewed";
                                }
                                else //Options type
                                {
                                    answer.SelectedOption = (from o in dbContext.Options
                                                             where o.OptionId == SelectedOption
                                                             select o.Comprehensive).FirstOrDefault();

                                    answer.CorrectOption = (from o in dbContext.Options
                                                            where o.OptionId == (from a in dbContext.Answers
                                                                                 where a.QId == id
                                                                                 select a.CorrectOption).FirstOrDefault()
                                                            select o.OptionText).FirstOrDefault();
                                }

                                answer.Score = dbContext.Scores.Where(s => s.TestId == TId && s.QId == id).Select(s => s.Score).FirstOrDefault();

                                total = total + answer.Score;
                                ViewData["total"] = total;
                                list.Add(answer);
                            }
                            return View(list);
                        }
                    }
                    catch
                    {
                        return RedirectToAction("GetAnswers");
                    }
                }
                else
                    return RedirectToAction("GetAnswers");
            }
        }
    }
}