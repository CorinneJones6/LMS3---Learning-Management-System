﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {           
                var classes = from e in db.Enrolleds
                  join c in db.Classes on e.Class equals c.ClassId
                  join course in db.Courses on c.Listing equals course.CatalogId
                  where e.Student == uid
                  select new
                  {
                      subject = course.Department,
                      number = course.Number,
                      name = course.Name,
                      season = c.Season,
                      year = c.Year,
                      grade = e.Grade ?? "--"
                  };

            return Json(classes.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var assignments = from e in db.Enrolleds
                            join c in db.Classes on e.Class equals c.ClassId
                            join course in db.Courses on c.Listing equals course.CatalogId
                            join ac in db.AssignmentCategories on c.ClassId equals ac.InClass
                            join a in db.Assignments on ac.CategoryId equals a.Category
                            where course.Department == subject && course.Number == num &&
                                    c.Season == season && c.Year == year &&
                                    e.Student == uid
                            select new
                            {
                                aname = a.Name,
                                cname = ac.Name,
                                due = a.Due,
                                score = (from s in db.Submissions
                                        where s.Assignment == a.AssignmentId && s.Student == uid
                                        select (int?)s.Score).FirstOrDefault()
                            };

            return Json(assignments);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, string category, string asgname, string uid, string contents)
        {

            var course = db.Courses.SingleOrDefault(c => c.Department == subject && c.Number == num);
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }

            var cls = db.Classes.SingleOrDefault(c => c.Listing == course.CatalogId && c.Season == season && c.Year == year);
            if (cls == null)
            {
                return Json(new { success = false, message = "Class not found" });
            }

            var categoryName = db.AssignmentCategories.SingleOrDefault(ac => ac.InClass == cls.ClassId && ac.Name == category);
            if (categoryName == null)
            {
                return Json(new { success = false, message = "Assignment category not found" });
            }

            var assignment = db.Assignments.SingleOrDefault(a => a.Category == categoryName.CategoryId && a.Name == asgname);
            if (assignment == null)
            {
                return Json(new { success = false, message = "Assignment not found" });
            }

            var submission = db.Submissions.SingleOrDefault(s => s.Assignment == assignment.AssignmentId && s.Student == uid);
            if (submission == null)
            {
                submission = new Submission
                {
                    Assignment = assignment.AssignmentId,
                    Student = uid,
                    Time = DateTime.Now,
                    Score = 0,
                    SubmissionContents = contents
                };
                db.Submissions.Add(submission);
            }
            else
            {
                submission.Time = DateTime.Now;
                submission.SubmissionContents = contents;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var cls = (from c in db.Classes
                    join course in db.Courses on c.Listing equals course.CatalogId
                    where course.Department == subject && course.Number == num &&
                            c.Season == season && c.Year == year
                    select c).FirstOrDefault();

            if (cls == null || db.Enrolleds.Any(e => e.Class == cls.ClassId && e.Student == uid))
            {
                return Json(new { success = false });
            }

            var enrollment = new Enrolled
            {
                Class = cls.ClassId,
                Student = uid,
                Grade = "--"
            };

            db.Enrolleds.Add(enrollment);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            // Retrieve grades for the student
            var grades = from e in db.Enrolleds
                        where e.Student == uid && e.Grade != null
                        select e.Grade;

            if (!grades.Any())
            {
                return Json(new { gpa = 0.0 });
            }

            double totalPoints = 0.0;
            int count = 0;

            foreach (var grade in grades)
            {
                double point = GetGradePoint(grade);
                totalPoints += point;
                count++;
            }

            double gpa = count == 0 ? 0.0 : totalPoints / count;
            return Json(new { gpa = gpa });
        }

        private double GetGradePoint(string grade)
        {
            switch (grade.ToUpper())
            {
                case "A": return 4.0;
                case "A-": return 3.7;
                case "B+": return 3.3;
                case "B": return 3.0;
                case "B-": return 2.7;
                case "C+": return 2.3;
                case "C": return 2.0;
                case "C-": return 1.7;
                case "D+": return 1.3;
                case "D": return 1.0;
                case "D-": return 0.7;
                case "E": return 0.0;
                default: return 0.0;
            }
        }     
        /*******End code to modify********/

    }
}

