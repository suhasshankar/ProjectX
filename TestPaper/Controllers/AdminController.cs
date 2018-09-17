using ProjectX.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace ProjectX.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public ActionResult DashBoard()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Authenticate", "Users");
        }

        [HttpGet]
        public ActionResult EditUsers()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
                return View(GetUsers());
        }

        [HttpPost]
        public JsonResult Revaluation(ReportCard card)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Please Login again" }, JsonRequestBehavior.AllowGet);
            else
            {
                using (DBEntities dbcontext = new DBEntities())
                {
                    ReportCard update = dbcontext.ReportCard.Where(s => s.ReportId == card.ReportId).FirstOrDefault();
                    if (update != null)
                    {
                        update.IsRevaluate = card.IsRevaluate;
                        dbcontext.SaveChanges();
                        if (card.IsRevaluate)
                            return Json(new { success = true, responseText = "Applied for Revalution. Please Wait for the Response." }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { success = true, responseText = "Cancelled Revalution Request" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { success = false, responseText = "Error while applying for Revaluation. Report Not Found." }, JsonRequestBehavior.AllowGet);
                }
            }

        }
        public ActionResult EditCourse()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                List<Courses> courses = new List<Courses>();
                using (DBEntities dbContext = new DBEntities())
                {
                    courses = dbContext.Courses.ToList();
                    return View(courses);
                }
            }
        }

        public ActionResult EditPaper()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                List<Papers> Papers = new List<Papers>();
                using (DBEntities dbContext = new DBEntities())
                {
                    Papers = dbContext.Papers.ToList();
                    for (int i = 0; i < Papers.Count; i++)
                    {
                        int courseId = Convert.ToInt32(Papers[i].CourseId);
                        Papers[i].CourseName = dbContext.Courses.Where(c => c.CourseId == courseId).Select(c => c.CourseName).FirstOrDefault().ToString();
                    }
                    return View(Papers);
                }
            }
        }

        [HttpGet]
        public ActionResult SetQuestions()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddUsers()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUsers(Users users)
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (DBEntities dbContext = new DBEntities())
                        {
                            dbContext.Users.Add(users);
                            dbContext.SaveChanges();
                            ModelState.Clear();
                            ViewData["MSG"] = "Added " + users.UserName + " Successfully";
                            return View();
                        }
                    }
                    else
                    {
                        ViewData["MSG"] = "Please fill all the details!!.";
                        return View();
                    }
                }
                catch
                {
                    ViewData["MSG"] = "Failed to Register user!!.Please try again later";
                    return RedirectToAction("AddUsers", "Users");
                }
            }
        }

        [HttpGet]
        public ActionResult EvaluateTests()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                try
                {
                    List<EvaluateTests> pending = new List<EvaluateTests>();
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        var evaluateUsers = dbcontext.StudentTestLog.Select(s => new { s.CourseId, s.UserId, s.TestId }).Distinct().ToList();
                        for (int i = 0; i < evaluateUsers.Count; i++)
                        {
                            EvaluateTests test = new EvaluateTests();
                            test.TestId = evaluateUsers[i].TestId;
                            test.Id = evaluateUsers[i].UserId;
                            test.CourseId = evaluateUsers[i].CourseId;
                            test.UserName = dbcontext.Users.Where(u => u.Id == test.Id).Select(s => s.UserName).FirstOrDefault();
                            test.CourseName = dbcontext.Courses.Where(c => c.CourseId == test.CourseId).Select(c => c.CourseName).FirstOrDefault();
                            test.lastReviewed = dbcontext.Scores.Where(c => c.TestId == test.TestId).Select(c => c.LastUpdatedDate).FirstOrDefault();
                            if (string.IsNullOrEmpty(test.lastReviewed.ToString()))
                                test.status = "Pending for Review";
                            else
                                test.status = "Done";

                            bool ReportCard = dbcontext.ReportCard.Where(r => r.TestId == test.TestId).Select(r => r.IsRevaluate).FirstOrDefault();
                            if (ReportCard)
                                test.isRevalidate = true;
                            else
                                test.isRevalidate = false;
                            pending.Add(test);
                        }
                        pending.Insert(0, new EvaluateTests());
                        return View(pending);
                    }
                }
                catch (Exception ex)
                {
                    ViewData["MSG"] = "Failed to Evaluate Test!!.Please try again later.Error: " + ex.Message;
                    return RedirectToAction("DashBoard", "Admin");
                }
            }
        }

        [HttpGet]
        public ActionResult ValidateTest(Guid testId)
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                using (DBEntities dbcontext = new DBEntities())
                {
                    var pendingItems = dbcontext.StudentTestLog.Where(s => s.TestId == testId && s.MultiLineAnswer != null).ToList();
                    List<ValidateTest> items = new List<ValidateTest>();
                    foreach (StudentTestLog log in pendingItems)
                    {
                        ValidateTest question = new ValidateTest();
                        question.UserId = log.UserId;
                        question.TestId = testId;
                        question.Question = dbcontext.Questions.Where(q => q.QId == log.QId).Select(s => s.Question).FirstOrDefault().ToString();
                        question.MultiLineAnswer = log.MultiLineAnswer;
                        question.QId = log.QId;
                        question.Score = dbcontext.Scores.Where(s => s.QId == log.QId && s.TestId == log.TestId).Select(s => s.Score).FirstOrDefault();
                        items.Add(question);
                    }
                    return View(items.ToList());
                }
            }
        }

        [HttpPost]
        public JsonResult Validate(ValidateTest score)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Please Login again" }, JsonRequestBehavior.AllowGet);
            else
            {
                using (DBEntities dbcontext = new DBEntities())
                {
                    var duplicate = dbcontext.Scores.Where(s => s.TestId == score.TestId && s.QId == score.QId).ToList();
                    if (duplicate.Count > 0)
                    {
                        foreach (Scores delete in duplicate)
                        {
                            dbcontext.Scores.Remove(delete);
                            dbcontext.SaveChanges();
                        }
                    }
                    Scores log = new Scores();
                    log.UserId = score.UserId;
                    log.TestId = score.TestId;
                    log.QId = score.QId;
                    log.Score = score.Score;
                    log.LastUpdatedDate = DateTime.UtcNow;
                    dbcontext.Scores.Add(log);
                    dbcontext.SaveChanges();

                    StudentTestLog updateLog = dbcontext.StudentTestLog.Where(s => s.TestId == score.TestId && s.QId == score.QId).FirstOrDefault();
                    updateLog.isCorrected = true;
                    dbcontext.SaveChanges();

                    return Json(new { success = true, responseText = "Updated Score for User Id " + log.UserId + " with score " + log.Score + " for QId " + log.QId }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult SubmitScore(Guid testId)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Please Select option." }, JsonRequestBehavior.AllowGet);
            else
            {
                using (DBEntities dbcontext = new DBEntities())
                {
                    int userId = 0;
                    var pendingItems = dbcontext.StudentTestLog.Where(s => s.TestId == testId).ToList();
                    foreach (StudentTestLog log in pendingItems)
                    {
                        int score = 0;
                        Scores scoreCard = new Scores();
                        userId = log.UserId;
                        if (string.IsNullOrEmpty(log.MultiLineAnswer))
                        {
                            var isCorrect = dbcontext.Answers.Where(a => a.QId == log.QId && a.CorrectOption == log.SelectedOption).FirstOrDefault();
                            if (isCorrect != null)
                            {
                                score++;
                                scoreCard.Score = 1;
                            }
                            else
                                scoreCard.Score = 0;
                            log.isCorrected = true;
                            dbcontext.SaveChanges();

                            //Delete existing duplicate test id if already present
                            var DuplicateScoreList = dbcontext.Scores.Where(s => s.TestId == testId && s.QId == log.QId).ToList();
                            if (DuplicateScoreList.Count >= 1)
                            {
                                foreach (Scores deletescore in DuplicateScoreList)
                                {
                                    dbcontext.Scores.Remove(deletescore);
                                    dbcontext.SaveChanges();
                                }
                            }
                            //Add fresh score to the board
                            scoreCard.QId = log.QId;
                            scoreCard.TestId = testId;
                            scoreCard.UserId = log.UserId;
                            scoreCard.LastUpdatedDate = DateTime.UtcNow;
                            dbcontext.Scores.Add(scoreCard);
                            dbcontext.SaveChanges();
                        }
                    }

                    //Delete existing duplicate test id if already present
                    var DuplicateCardList = dbcontext.ReportCard.Where(s => s.TestId == testId).ToList();
                    if (DuplicateCardList.Count >= 1)
                    {
                        foreach (ReportCard deleteCard in DuplicateCardList)
                        {
                            dbcontext.ReportCard.Remove(deleteCard);
                            dbcontext.SaveChanges();
                        }
                    }

                    //Add fresh report card to the board
                    ReportCard card = new ReportCard();
                    card.TestId = testId;
                    card.UserId = userId;
                    card.CourseId = pendingItems[0].CourseId;
                    card.Paperid = pendingItems[0].PaperId;
                    card.LevelId = pendingItems[0].Level;
                    card.LastUpdated = DateTime.UtcNow;
                    card.IsRevaluate = false;
                    card.TotalScore = dbcontext.Scores.Where(s => s.TestId == testId).Select(s => s.Score).Sum();
                    dbcontext.ReportCard.Add(card);
                    dbcontext.SaveChanges();

                    return Json(new { success = true, responseText = "Score Captured!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult _LeftAdminPanel()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AssignTest()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                try
                {
                    List<Users> users = new List<Users>();
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        users = (dbcontext.Users.Where(l => l.RoleId != 2).ToList());
                        users.Insert(0, new Users());
                        var courseList = dbcontext.Courses.ToList();
                        for (int i = 0; i < users.Count; i++)
                        {
                            List<int> TestTypeIds = new List<int>();
                            int id = users[i].Id;
                            var userCourses = dbcontext.AllocateCourse.Where(a => a.UserId == id).Select(a => new { a.CourseId, a.TestType }).ToList();
                            users[i].courses = courseList.Select(c => new SelectListItem
                            {
                                Text = c.CourseName,
                                Value = c.CourseId.ToString()
                            }).ToList();
                            for (int m = 0; m < courseList.Count; m++)
                                TestTypeIds.Add(0);

                            if (userCourses.Count > 0)
                            {
                                for (int j = 0; j < userCourses.Count; j++)
                                {
                                    for (int k = 0; k < courseList.Count; k++)
                                    {
                                        if (userCourses[j].CourseId == Convert.ToInt32(users[i].courses[k].Value))
                                        {
                                            users[i].courses[k].Selected = true;
                                            TestTypeIds[k] = userCourses[j].TestType;
                                            break;
                                        }
                                    }
                                }
                            }
                            users[i].TestType = TestTypeIds;
                        }
                    }
                    return View(users);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, responseText = "Failed to get Allocated Courses. Try again later or Contact Administrator. Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult AssignTest(List<AllocateCourse> result)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Login Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    if (result.Count == 0)
                        return Json(new { success = false, responseText = "Please Assign Course and Select Type of Exam." }, JsonRequestBehavior.AllowGet);
                    else
                    {
                        using (DBEntities dbcontext = new DBEntities())
                        {
                            foreach (AllocateCourse list in result)
                            {
                                var deleteList = dbcontext.AllocateCourse.Where(a => a.UserId == list.UserId).ToList();
                                foreach (AllocateCourse delete in deleteList)
                                {
                                    dbcontext.AllocateCourse.Remove(delete);
                                    dbcontext.SaveChanges();
                                }
                            }
                            foreach (AllocateCourse list in result)
                            {
                                AllocateCourse userList = new AllocateCourse();
                                userList.UserId = list.UserId;
                                userList.CourseId = list.CourseId;
                                userList.TestType = list.TestType;
                                dbcontext.AllocateCourse.Add(userList);
                                dbcontext.SaveChanges();
                            }
                            return Json(new { success = true, responseText = "Assigned Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, responseText = "Failed to Assign. Try again later or Contact Administrator. Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult EnableDisableUsers()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
                return View(GetUsers());
        }
        [HttpPost]
        public JsonResult EnableDisableUsers(Users user)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Login Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Users updatedUser = dbcontext.Users.Where(u => u.Id == user.Id).FirstOrDefault();
                        updatedUser.Status = user.Status;
                        dbcontext.SaveChanges();
                        if (user.Status)
                            return Json(new { success = true, responseText = "Activated User " + updatedUser.UserName + " Successfully." }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { success = true, responseText = "Deactivated User " + updatedUser.UserName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to Reset. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public ActionResult ResetPwd()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
                return View(GetUsers());
        }

        public List<Users> GetUsers()
        {
            List<Users> users = new List<Users>();
            using (DBEntities dbcontext = new DBEntities())
            {
                users = (dbcontext.Users.ToList());
            }
            users.Insert(0, new Users());
            return users;
        }

        [HttpPost]
        public JsonResult ResetPassword(Users user)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Login Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Users updatedUser = dbcontext.Users.Where(u => u.Id == user.Id).FirstOrDefault();
                        updatedUser.IsReset = user.IsReset;
                        dbcontext.SaveChanges();
                        if (user.IsReset)
                            return Json(new { success = true, responseText = "Password Reset for " + updatedUser.UserName + " was sent Successfully." }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { success = true, responseText = "Cancelled Request for password Reset for the " + updatedUser.UserName }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to Reset. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult SetQuestions(FormCollection question)
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                int QId;
                try
                {
                    if (question.Count > 0 && Convert.ToInt32(question[1]) != 0 && Convert.ToInt32(question[5]) != 0 && !question[5].ToString().Contains("-"))
                    {
                        using (DBEntities dbContext = new DBEntities())
                        {
                            Questions Question = new Questions();
                            Question.Question = question[3].ToString();
                            Question.OptionType = Convert.ToInt32(question[4]);
                            Question.CourseId = Convert.ToInt32(question[0]);
                            Question.PaperId = Convert.ToInt32(question[1]);
                            Question.Level = Convert.ToInt32(question[2]);
                            Question.Duration = Convert.ToInt32(question[5]);
                            dbContext.Questions.Add(Question);
                            dbContext.SaveChanges();
                            QId = Question.QId;

                            try
                            {
                                for (int i = 0; i < question.Count; i++)
                                {
                                    if (question.Keys[i].ToString().Contains("Option"))
                                    {
                                        if (!string.IsNullOrEmpty(question.Keys[i].ToString().Trim()))
                                        {
                                            Options option = new Options();
                                            option.QId = QId;
                                            option.OptionText = question.GetValue(question.Keys[i].ToString()).AttemptedValue;
                                            option.Comprehensive = "";
                                            dbContext.Options.Add(option);
                                            dbContext.SaveChanges();
                                            string key = "Option" + question[question.Count - 1].ToString();
                                            if (question.Keys[i].ToString() == key)
                                            {
                                                Answers correctOption = new Answers();
                                                correctOption.QId = QId;
                                                correctOption.CorrectOption = option.OptionId;
                                                correctOption.LastUpdate = DateTime.UtcNow;
                                                Users usercookie = Session["usercookie"] as Users;
                                                correctOption.LastUpdatedBy = usercookie.UserName;
                                                dbContext.Answers.Add(correctOption);
                                                dbContext.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ViewData["MSG"] = "Error Occurred while adding options!!! Please contact admin. Error: " + ex.Message;
                                dbContext.Questions.Remove(Question);
                                dbContext.SaveChanges();
                            }
                            ViewData["MSG"] = "Question Added successfully";
                            return View();
                        }
                    }
                    else
                    {
                        ViewData["MSG"] = "Please Fill all the details";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ViewData["MSG"] = ex.Message;
                    return View();
                }
            }
        }
    }
}