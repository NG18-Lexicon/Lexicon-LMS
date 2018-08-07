﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Lexicon_LMS.Models;
using Microsoft.AspNet.Identity;

namespace Lexicon_LMS.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Activities
        public ActionResult Index()
        {
            return View(db.Activities.ToList());
        }

        // GET: Activities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // GET: Activities/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create(int? id)
        {
            Module targetModule = db.Modules.Where(module => module.ID == id).FirstOrDefault();
            ViewBag.ModuleLabel = targetModule.Course.CourseName + " (" + targetModule.Course.CourseCode + ") - " + targetModule.Description;
            Activity model = new Activity();
            if (id != null)
            {
                model.ModuleID = targetModule.ID;
            }
            return View(model);
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Create([Bind(Include = "ID,Name,Deadline,ModuleID")] Activity activity, HttpPostedFileBase upload)
        {

            if (ModelState.IsValid)
            {
                Module targetModule = db.Modules.Where(module => module.ID == activity.ModuleID).FirstOrDefault();
                activity.Module = targetModule;


                if (upload != null && upload.ContentLength > 0)
                {
                    Document file = Upload(upload, activity.ID);

                    activity.Documents = new List<Document>();
                    activity.Documents.Add(file);

                }

                if (Request["notify"] != null)
                {
                    foreach (ApplicationUser u in activity.Module.Course.CourseParticipants)
                    {
                        var alert = NewActivityAlert(activity, db.Users.Find(User.Identity.GetUserId()));
                        alert.RecipientID = u.Id;
                        u.Notifications.Add(alert);
                    }
                }

                if (Request["assignment"] != null)
                {
                    Assignment assignment = new Assignment
                    {
                        Name = activity.Name,
                        Module = targetModule,
                        Deadline = activity.Deadline,
                        Documents = new List<Document>()
                };

                    targetModule.ModuleActivities.Add(assignment);
                    db.Activities.Add(assignment);
                    db.SaveChanges();
                }
                else
                { 
                //course.Teacher = db.Users.Where(u => u.Id == course.TeacherID).FirstOrDefault();
                targetModule.ModuleActivities.Add(activity);
                db.Activities.Add(activity);
                db.SaveChanges();
                }

                return RedirectToAction("Details", "Courses", new { id = targetModule.Course.ID });
            }

            //ViewBag.CourseCode = new SelectList(db.Courses, "ID", "CourseCode", module.CourseCode);
            return View(activity);
        }

        public Document Upload (HttpPostedFileBase upload, int activityID)
        {
            var originalFilename = Path.GetFileName(upload.FileName);
            var user = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var activity = db.Activities.Find(activityID);

            string fileId = Guid.NewGuid().ToString().Replace("-", "");

            var fileName = user.Forename + "_" + user.Surname + "_" + fileId + "_" + originalFilename;

            var path = Path.Combine(Server.MapPath("~/Uploads"),activity.Module.Course.CourseName, activity.Module.ModuleTitle, activity.Name);
            var save = Path.Combine(Server.MapPath("~/Uploads"), fileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            upload.SaveAs(save);

            var file = new Document
            {
                FileName = fileName,
                DisplayName = originalFilename,
                UploadDate = DateTime.Now,
                ActivityID = activityID,
                Filepath = path,
                User = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault()
            };

            return (file);
        }

        [Authorize]
        public ActionResult Download(string filePath, string fileName, string saveName)
        {
            string fullName = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().CodeBase, filePath, fileName);

            if (!System.IO.File.Exists(fullName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "File was not found");
            }

            string contentType = MimeMapping.GetMimeMapping(filePath);
            byte[] fileBytes = GetFile(fullName);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = saveName,
                Inline = false
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(
             fileBytes,
             contentType
             );
        }

        private byte[] GetFile(string fullName)
        {

            //is null check filepath
            //https://stackoverflow.com/questions/3597179/file-download-in-asp-net-mvc-2

            FileStream fs = System.IO.File.OpenRead(fullName);

            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new IOException(fullName);

            return data;
        }

        // GET: Activities/Edit/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit([Bind(Include = "ID,Name,Deadline,ModuleID")] Activity activity)
        {
            if (ModelState.IsValid)
            {
                db.Entry(activity).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        public ActionResult SubmitAssignment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAssignment([Bind(Include = "ID,Name,ModuleID")] Assignment assignment, HttpPostedFileBase upload)
        {
            if (upload != null && upload.ContentLength > 0)
            {

                Assignment targetAssignment = db.Activities.OfType<Assignment>().Where(a => a.ID == assignment.ID).FirstOrDefault();

                var file = Upload(upload, assignment.ID);

                file.UserAssignment = true;
                targetAssignment.Documents.Add(file);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = assignment.ID });
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult UploadActDoc(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);

            if (activity == null)
            {
                return HttpNotFound();
            }
            return PartialView(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadActDoc([Bind(Include = "ID")] Activity activity, HttpPostedFileBase upload)
        {
            var file = Upload(upload, activity.ID);
            Activity targetActiviy = db.Activities.Find(activity.ID);

            targetActiviy.Documents.Add(file);
            db.SaveChanges();
            return RedirectToAction("Details", new { activity.ID });
        }

        // GET: Activities/Delete/5
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteConfirmed(int id)
        {
            Activity activity = db.Activities.Find(id);
            db.Activities.Remove(activity);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult DeleteDoc(int id)
        {
            Document doc = db.Documents.Find(id);
            var tgtActivity = db.Activities.Find(doc.ActivityID);
            string fullPath = doc.Filepath + "/" + doc.FileName;

            tgtActivity.Documents.Remove(doc);
            System.IO.File.Delete(fullPath);
            db.Documents.Remove(doc);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = tgtActivity.ID });
        }

        public Notification NewActivityAlert(Activity activity, ApplicationUser user)
        {
            var notif = new Notification();
            notif.Subject = user.FullName + " has added a new activity";
            notif.Body = "A new activity (" + activity.Name + ") has been added to \"" + activity.Module.Description + "\" module in " + activity.Module.Course.CourseName;
            notif.Sender = user;
            notif.Recipients = activity.Module.Course.CourseParticipants.ToList();
            notif.DateSent = DateTime.Now;

            return notif;
        }

    }
}
