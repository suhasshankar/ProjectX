using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class AdminReportController : Controller
    {
        // GET: AdminReport
        public ActionResult ExamWiseReport()
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
                    var CourseIds = dbContext.Courses.Select(c => c.CourseId).ToList();
                    for (int i = 0; i < CourseIds.Count(); i++)
                    {
                        int id = CourseIds[i];
                        SelectListItem values = new SelectListItem();
                        values = dbContext.Courses.Where(c=>c.CourseId == id).Select(c => new SelectListItem
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

        public ActionResult StudentWiseReport()
        {
            if (Session["usercookie"] == null)
                return RedirectToAction("Authenticate", "Users");
            else
            {
                using (DBEntities dbContext = new DBEntities())
                {
                    Users user = new Users();
                    Users usercookie = Session["usercookie"] as Users;
                    List<SelectListItem> items = new List<SelectListItem>();
                    var userList = dbContext.Users.Where(u => u.RoleId != 2).Select(c => c.Id).ToList();
                    for (int i = 0; i < userList.Count(); i++)
                    {
                        int id = userList[i];
                        SelectListItem values = new SelectListItem();
                        values = dbContext.Users.Where(u =>u.Id == id).Select(c => new SelectListItem
                        {
                            Text = c.UserName,
                            Value = c.Id.ToString()
                        }).FirstOrDefault();
                        items.Add(values);
                    }
                    user.ListOfUsers = items;
                    return View(user);
                }
            }
        }
    }
}