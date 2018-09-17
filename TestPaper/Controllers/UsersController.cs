using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectX.Models;

namespace ProjectX.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Authenticate()
        {
            return View();
        }

        public ActionResult Signup()
        {
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Authenticate(Users credentials)
        {
            using (DBEntities dbContext = new DBEntities())
            {
                if (ModelState.IsValid)
                {
                    List<Users> users = new List<Users>();
                    if (credentials.UserName.Contains("@"))
                        users = dbContext.Users.Where(l => l.UserName == credentials.EmailId && l.Password == credentials.Password).ToList();
                    else
                        users = dbContext.Users.Where(l => l.UserName == credentials.UserName && l.Password == credentials.Password).ToList();
                    if (users.Count == 0)
                    {
                        ViewData["MSG"] = "Sorry : user is not found !!";
                        return View();
                    }
                    else
                    {
                        Session["usercookie"] = users[0];
                        if (users[0].RoleId == 2)
                            return RedirectToAction("DashBoard", "Admin");
                        else
                            return RedirectToAction("DashBoard", "Student");
                    }
                }
                else
                {
                    ViewData["MSG"] = "Please Enter valid username and password.";
                    return View();
                }
            }
        }

        [HttpPost]
        public ActionResult Signup(Users details)
        {
            using (DBEntities dbContext = new DBEntities())
            {
                if (ModelState.IsValid)
                {
                    details.RoleId = 1; //always considered as Student or guest 
                    dbContext.Users.Add(details);
                    dbContext.SaveChanges();
                    ModelState.Clear();
                    ViewData["MSG"] = "Registered Successfully";
                    return RedirectToAction("Authenticate", "Users");
                }
                else
                    return View();
            }         
        }

        [HttpPost]
        public ActionResult ForgotPassword(Users details)
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateUser(Users user)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities dbcontext = new DBEntities())
                    {
                        Users updatedUser = dbcontext.Users.Where(u => u.Id == user.Id).FirstOrDefault();
                        updatedUser.UserName = user.UserName;
                        updatedUser.EmailId = user.EmailId;
                        updatedUser.Status = user.Status;
                        updatedUser.RoleId = user.RoleId;
                        dbcontext.SaveChanges();
                        return Json(new { success = true, responseText = "Updated " + updatedUser.UserName + " Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch
                {
                    return Json(new { success = false, responseText = "Failed to update. Try again later or contact administrator." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult DeleteUser(int Id)
        {
            if (Session["usercookie"] == null)
                return Json(new { success = false, responseText = "Session Expired. Please Users Again!!!." }, JsonRequestBehavior.AllowGet);
            else
            {
                try
                {
                    using (DBEntities context = new DBEntities())
                    {
                        Users user = (from l in context.Users
                                      where l.Id == Id
                                      select l).FirstOrDefault();
                        context.Users.Remove(user);
                        context.SaveChanges();
                        return Json(new { success = true, responseText = "Deleted " + user.UserName + " Successfully." }, JsonRequestBehavior.AllowGet);
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