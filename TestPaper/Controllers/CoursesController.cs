using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class CoursesController : Controller
    {
        [HttpPost]
        public JsonResult UpdateCourse(Courses course)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Courses updatedCourse = dbcontext.Courses.Where(c => c.CourseId == course.CourseId).FirstOrDefault();
                        updatedCourse.CourseName = course.CourseName;
                        dbcontext.SaveChanges();
                        return Json(new { success = true, responseText = "Updated " + updatedCourse.CourseName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteCourse(int Id)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Courses course = dbcontext.Courses.Where(c => c.CourseId == Id).FirstOrDefault();
                        dbcontext.Courses.Remove(course);
                        dbcontext.SaveChanges();
                        return Json(new { success = true, responseText = "Deleted " + course.CourseName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult UpdatePaper(Papers paper)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Papers updatedPaper = dbcontext.Papers.Where(c => c.PaperId == paper.PaperId).FirstOrDefault();
                        updatedPaper.PaperName = paper.PaperName;
                        dbcontext.SaveChanges();
                        return Json(new { success = true, responseText = "Updated " + updatedPaper.PaperName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public ActionResult DeletePaper(int Id)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Papers paper = dbcontext.Papers.Where(c => c.PaperId == Id).FirstOrDefault();
                        dbcontext.Papers.Remove(paper);
                        dbcontext.SaveChanges();
                        return Json(new { success = true, responseText = "Deleted " + paper.PaperName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult AddCourse(string course)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    if(string.IsNullOrEmpty(course))
                    {
                        return Json(new { success = false, responseText = "Empty Course Name!!!" }, JsonRequestBehavior.AllowGet);
                    }
                    Courses addCourse = new Courses();
                    addCourse.CourseName = course;
                    using (DBEntities context = new DBEntities())
                    {
                        context.Courses.Add(addCourse);
                        context.SaveChanges();
                        return Json(new { success = true, responseText = "Added " + addCourse.CourseName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to Add. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetCourse()
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        var result = (from r in dbContext.Courses
                                      select new
                                      {
                                          r.CourseId,
                                          r.CourseName
                                      }).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetRoles()
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        var result = (from r in dbContext.Roles
                                      select new
                                      {
                                          r.RoleId,
                                          r.RoleName
                                      }).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
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
                try
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
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
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
                try
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
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpGet]
        public JsonResult GetOptionTypes()
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbContext = new DBEntities())
                    {
                        var result = (from r in dbContext.OptionTypes
                                      select new
                                      {
                                          r.TypeId,
                                          r.TypeName
                                      }).ToList();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}