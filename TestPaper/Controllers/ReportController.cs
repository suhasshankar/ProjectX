using Microsoft.Reporting.WebForms;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class ReportController : Controller
    {
        private string HomeDirectory { get; set; }

        public JsonResult GenerateStats()
        {
            Users usercookie = Session["usercookie"] as Users;
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Please login.Session expired!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                int currentUserId = usercookie.Id;
                var chart = new ReportDesign();
                ReportDesign.ScoresDataTable scores = chart.Scores;
                ReportDesign.SuccessRateDataTable successRate = chart.SuccessRate;
                ReportDataSourceComponents[] source = new ReportDataSourceComponents[] { };
                if (usercookie.RoleId == 2)
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        var log = dbContext.ReportCard.ToList();
                        foreach (ReportCard score in log)
                        {
                            scores.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                                dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                                dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                                score.TotalScore, score.UserId.ToString(),
                                dbContext.Users.Where(c => c.Id == currentUserId).Select(c => c.UserName).FirstOrDefault());
                        }
                        var optionsLog = dbContext.Scores.Select(s => s.Score).ToList();
                        double successRateCount = 0;
                        double FailRateCount = 0;
                        for (int i = 0; i < optionsLog.Count; i++)
                        {
                            if (optionsLog[i] != 0)
                                successRateCount++;
                            else
                                FailRateCount++;
                        }
                        successRate.AddSuccessRateRow(successRateCount / (successRateCount + FailRateCount), FailRateCount / (successRateCount + FailRateCount));
                        source = new[] { new ReportDataSourceComponents("Scores", scores), new ReportDataSourceComponents("SuccessRate", successRate) };
                    }
                }
                else
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        var log = dbContext.ReportCard.Where(s => s.UserId == currentUserId).ToList();
                        foreach (ReportCard score in log)
                        {
                            scores.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                                dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                                dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                                score.TotalScore, score.UserId.ToString(),
                                dbContext.Users.Where(c => c.Id == currentUserId).Select(c => c.UserName).FirstOrDefault());
                        }
                        var optionsLog = dbContext.Scores.Where(s => s.UserId == currentUserId).Select(s => s.Score).ToList();
                        double successRateCount = 0;
                        double FailRateCount = 0;
                        for (int i = 0; i < optionsLog.Count; i++)
                        {
                            if (optionsLog[i] != 0)
                                successRateCount++;
                            else
                                FailRateCount++;
                        }
                        successRate.AddSuccessRateRow((successRateCount / (successRateCount + FailRateCount)) * 100, (FailRateCount / (successRateCount + FailRateCount)) * 100);
                        source = new[] { new ReportDataSourceComponents("Scores", scores), new ReportDataSourceComponents("SuccessRate", successRate) };
                    }
                }

                string templateName = "DashBoardChart.rdlc";
                string path = GetTemplateFullPath(templateName);
                var reportOutput = PublishReportForTemplate("Excel", path, source);
                string tempReportName = Guid.NewGuid() + ".xls";
                WriteReportToFile(tempReportName, reportOutput);

                //now export chart to img
                Workbook workbook = new Workbook();
                workbook.LoadFromFile(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName), ExcelVersion.Version97to2003);
                Worksheet sheet = workbook.Worksheets[0];
                ExcelPicture ScoresPicture = sheet.Pictures[0];
                ExcelPicture SuccessRatePicture = sheet.Pictures[1];

                string scoresPath;
                string successRatePath;
                if (usercookie.RoleId == 2)
                {
                    scoresPath = Path.Combine(GetHomeDirectory(), "TempCharts", "AdminScoresChart.png");
                    successRatePath = Path.Combine(GetHomeDirectory(), "TempCharts", "AdminSuccessRateChart.png");
                }
                else
                {
                    scoresPath = Path.Combine(GetHomeDirectory(), "TempCharts", "UserScoresChart.png");
                    successRatePath = Path.Combine(GetHomeDirectory(), "TempCharts", "UserSuccessRateChart.png");
                }

                //Delete old score chart
                if (System.IO.File.Exists(scoresPath))
                    System.IO.File.Delete(scoresPath);

                //Delete old successRate chart
                if (System.IO.File.Exists(successRatePath))
                    System.IO.File.Delete(successRatePath);

                ScoresPicture.Picture.Save(scoresPath, ImageFormat.Png);
                SuccessRatePicture.Picture.Save(successRatePath, ImageFormat.Png);

                //Delete temp file
                if (System.IO.File.Exists(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName)))
                    System.IO.File.Delete(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));

                return Json(new { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        protected void WriteReportToFile(string tempReportName, byte[] report)
        {
            CreateOuputFolderIfNeeded();
            string path = Path.Combine(GetReportOutputFolderName(), tempReportName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            using (var fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(report, 0, report.Length);
            }
        }

        private void CreateOuputFolderIfNeeded()
        {
            var path = GetReportOutputFolderName();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public string GetReportOutputFolderName()
        {
            return Path.Combine(GetHomeDirectory(), "TempCharts");
        }

        protected byte[] PublishReportForTemplate(string outputFormat, string path, ReportDataSourceComponents[] data)
        {
            // Defines hearder type, margins, and page size.
            const string COMMON_DEVICE_INFO = "<DeviceInfo>" +
                    "<SimplePageHeaders>True</SimplePageHeaders>" +
                    "<MarginLeft>0.25in</MarginLeft>" +
                    "<MarginRight>0.25in</MarginRight>" +
                    "<PageHeight>8.5in</PageHeight>" +
                    "<PageWidth>11in</PageWidth>" +
                    "</DeviceInfo>";

            using (var localReport = new LocalReport())
            {
                localReport.ReportPath = path;
                for (int i = 0; i < data.Count(); i++)
                {
                    localReport.DataSources.Add(new ReportDataSource(data[i].DataSetName, data[i].Table));
                }
                string mimeType, encoding, fileNameExtension;
                string[] streamids;
                Warning[] warnings;

                return localReport.Render(outputFormat, COMMON_DEVICE_INFO, out mimeType, out encoding, out fileNameExtension,
                                          out streamids, out warnings);
            }
        }

        private string GetTemplateFullPath(string templateName)
        {
            const string DEPLOYMENT_FOLDER = @"ReportTemplates";
            string path = System.IO.Path.Combine(GetHomeDirectory(), DEPLOYMENT_FOLDER);
            if (Directory.Exists(path))
            {
                return System.IO.Path.Combine(path, templateName);
            }
            else
                return null;
        }

        public string GetHomeDirectory()
        {
            if (!string.IsNullOrWhiteSpace(HomeDirectory))
            {
                return HomeDirectory;
            }

            // First look for config file in a parent of the current directory, then in a
            //		parent of the executing assembly
            var uri = new System.Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            string path = Path.GetDirectoryName(uri.LocalPath);
            return path.Remove(path.LastIndexOf('\\'));
        }

        [HttpGet]
        public ActionResult GenerateReport()
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
        public FileResult ReportGenerator(Guid Id)
        {
            Users usercookie = Session["usercookie"] as Users;
            int currentUserId = usercookie.Id;
            var chart = new ReportDesign();
            ReportDesign.ScoresDataTable card = chart.Scores;
            ReportDataSourceComponents[] source = new ReportDataSourceComponents[] { };

            using (DBEntities dbContext = new DBEntities())
            {
                var log = dbContext.ReportCard.Where(s => s.TestId == Id).ToList();
                foreach (ReportCard score in log)
                {
                    card.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                        dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                        dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                        score.TotalScore, score.UserId.ToString(),
                        dbContext.Users.Where(c => c.Id == currentUserId).Select(c => c.UserName).FirstOrDefault());
                }
                source = new[] { new ReportDataSourceComponents("ReportCard", card) };
            }

            string templateName = "ReportCard.rdlc";
            string path = GetTemplateFullPath(templateName);
            var reportOutput = PublishReportForTemplate("pdf", path, source);
            string tempReportName = Guid.NewGuid() + ".pdf";
            WriteReportToFile(tempReportName, reportOutput);

            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));
            string fileName = "ReportCard.pdf";

            //Delete temp file
            if (System.IO.File.Exists(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName)))
                System.IO.File.Delete(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpGet]
        public FileResult AdminExamWiseReport(int CourseId, string type)
        {
            var chart = new ReportDesign();
            ReportDesign.ScoresDataTable card = chart.Scores;
            ReportDataSourceComponents[] source = new ReportDataSourceComponents[] { };

            using (DBEntities dbContext = new DBEntities())
            {
                var log = dbContext.ReportCard.Where(s => s.CourseId == CourseId).OrderByDescending(s => s.TotalScore).ToList();
                foreach (ReportCard score in log)
                {
                    card.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                        dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                        dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                        score.TotalScore, score.UserId.ToString(),
                        dbContext.Users.Where(c => c.Id == score.UserId).Select(c => c.UserName).FirstOrDefault());
                }
                source = new[] { new ReportDataSourceComponents("ExamWiseReport", card) };
            }

            string templateName = "ExamWiseReport.rdlc";
            string path = GetTemplateFullPath(templateName);
            var reportOutput = PublishReportForTemplate(type, path, source);
            string tempReportName = Guid.NewGuid() + "." + type;
            WriteReportToFile(tempReportName, reportOutput);

            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));
            string fileName = "ExamWiseReport." + type;

            //Delete temp file
            if (System.IO.File.Exists(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName)))
                System.IO.File.Delete(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpGet]
        public FileResult StudentWiseReport(int Id, string type)
        {
            var chart = new ReportDesign();
            ReportDesign.ScoresDataTable card = chart.Scores;
            ReportDataSourceComponents[] source = new ReportDataSourceComponents[] { };
            ReportDesign.SuccessRateDataTable successRate = chart.SuccessRate;

            using (DBEntities dbContext = new DBEntities())
            {
                var log = dbContext.ReportCard.Where(s => s.UserId == Id).ToList();
                foreach (ReportCard score in log)
                {
                    card.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                        dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                        dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                        score.TotalScore, score.UserId.ToString(),
                        dbContext.Users.Where(c => c.Id == Id).Select(c => c.UserName).FirstOrDefault());
                }
                var optionsLog = dbContext.Scores.Where(s => s.UserId == Id).Select(s => s.Score).ToList();
                double successRateCount = 0;
                double FailRateCount = 0;
                for (int i = 0; i < optionsLog.Count; i++)
                {
                    if (optionsLog[i] != 0)
                        successRateCount++;
                    else
                        FailRateCount++;
                }
                successRate.AddSuccessRateRow(successRateCount / (successRateCount + FailRateCount), FailRateCount / (successRateCount + FailRateCount));
                source = new[] { new ReportDataSourceComponents("StudentWiseReportScores", card),
                new ReportDataSourceComponents("StudentWiseReportSuccessRate", successRate)};
            }

            string templateName = "StudentWiseReport.rdlc";
            string path = GetTemplateFullPath(templateName);
            var reportOutput = PublishReportForTemplate(type, path, source);
            string tempReportName = Guid.NewGuid() + "." + type;
            WriteReportToFile(tempReportName, reportOutput);

            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));
            string fileName = "StudentWiseReport." + type;

            //Delete temp file
            if (System.IO.File.Exists(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName)))
                System.IO.File.Delete(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpGet]
        public FileResult CompleteReport()
        {
            Users usercookie = Session["usercookie"] as Users;
            int currentUserId = usercookie.Id;
            var chart = new ReportDesign();
            ReportDesign.ScoresDataTable scores = chart.Scores;
            ReportDesign.SuccessRateDataTable successRate = chart.SuccessRate;
            ReportDataSourceComponents[] source = new ReportDataSourceComponents[] { };
            if (usercookie.RoleId == 2)
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    var log = dbContext.ReportCard.ToList();
                    foreach (ReportCard score in log)
                    {
                        scores.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                            dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                            dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                            score.TotalScore, score.UserId.ToString(),
                            dbContext.Users.Where(c => c.Id == currentUserId).Select(c => c.UserName).FirstOrDefault());
                    }
                    var optionsLog = dbContext.Scores.Select(s => s.Score).ToList();
                    double successRateCount = 0;
                    double FailRateCount = 0;
                    for (int i = 0; i < optionsLog.Count; i++)
                    {
                        if (optionsLog[i] != 0)
                            successRateCount++;
                        else
                            FailRateCount++;
                    }
                    successRate.AddSuccessRateRow(successRateCount / (successRateCount + FailRateCount), FailRateCount / (successRateCount + FailRateCount));
                    source = new[] { new ReportDataSourceComponents("Scores", scores), new ReportDataSourceComponents("SuccessRate", successRate) };
                }
            }
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    var log = dbContext.ReportCard.Where(s => s.UserId == currentUserId).ToList();
                    foreach (ReportCard score in log)
                    {
                        scores.AddScoresRow(dbContext.Courses.Where(c => c.CourseId == score.CourseId).Select(c => c.CourseName).FirstOrDefault(),
                            dbContext.Papers.Where(c => c.PaperId == score.Paperid).Select(c => c.PaperName).FirstOrDefault(),
                            dbContext.Levels.Where(c => c.Level == score.LevelId).Select(c => c.LevelName).FirstOrDefault(),
                            score.TotalScore, score.UserId.ToString(),
                            dbContext.Users.Where(c => c.Id == currentUserId).Select(c => c.UserName).FirstOrDefault());
                    }
                    var optionsLog = dbContext.Scores.Where(s => s.UserId == currentUserId).Select(s => s.Score).ToList();
                    double successRateCount = 0;
                    double FailRateCount = 0;
                    for (int i = 0; i < optionsLog.Count; i++)
                    {
                        if (optionsLog[i] != 0)
                            successRateCount++;
                        else
                            FailRateCount++;
                    }
                    successRate.AddSuccessRateRow((successRateCount / (successRateCount + FailRateCount)) * 100, (FailRateCount / (successRateCount + FailRateCount)) * 100);
                    source = new[] { new ReportDataSourceComponents("Scores", scores), new ReportDataSourceComponents("SuccessRate", successRate) };
                }
            }

            string templateName = "DashBoardChart.rdlc";
            string path = GetTemplateFullPath(templateName);
            var reportOutput = PublishReportForTemplate("pdf", path, source);
            string tempReportName = Guid.NewGuid() + ".pdf";
            WriteReportToFile(tempReportName, reportOutput);

            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));
            string fileName = "CompleteReportCard.pdf";

            //Delete temp file
            if (System.IO.File.Exists(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName)))
                System.IO.File.Delete(Path.Combine(GetHomeDirectory(), "TempCharts", tempReportName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}