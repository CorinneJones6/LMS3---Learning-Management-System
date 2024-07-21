using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var students = from e in db.Enrolleds
                           where e.ClassNavigation.ListingNavigation.Department == subject && e.ClassNavigation.ListingNavigation.Number == num &&
                                 e.ClassNavigation.Season == season && e.ClassNavigation.Year == year
                           select new
                           {
                               fName = e.StudentNavigation.FName,
                               lName = e.StudentNavigation.LName,
                               uid = e.StudentNavigation.UId,
                               dob = e.StudentNavigation.Dob,
                               grade = e.Grade
                           };

            return Json(students);
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var assignments = from a in db.Assignments
                              where a.CategoryNavigation.InClassNavigation.ListingNavigation.Department == subject && a.CategoryNavigation.InClassNavigation.ListingNavigation.Number == num &&
                                    a.CategoryNavigation.InClassNavigation.Season == season && a.CategoryNavigation.InClassNavigation.Year == year &&
                                    (category == null || a.CategoryNavigation.Name == category)
                              select new
                              {
                                  aname = a.Name,
                                  cname = a.CategoryNavigation.Name,
                                  due = a.Due,
                                  submissions = a.Submissions.Count
                              };

            return Json(assignments);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignmentCategories = from a in db.AssignmentCategories
                              where a.InClassNavigation.ListingNavigation.Department == subject && 
                                    a.InClassNavigation.ListingNavigation.Number == num &&
                                    a.InClassNavigation.Season == season && 
                                    a.InClassNavigation.Year == year
                              select new
                              {
                                  name = a.Name,
                                  weight = a.Weight
                              };

            return Json(assignmentCategories); 
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
             var cls = db.Classes.FirstOrDefault(c => c.ListingNavigation.Department == subject && c.ListingNavigation.Number == num &&
                                                     c.Season == season && c.Year == year);

            if (cls == null || db.AssignmentCategories.Any(ac => ac.InClassNavigation.ClassId == cls.ClassId && ac.Name == category))
            {
                return Json(new { success = false });
            }

            AssignmentCategory assignmentCategory = new()
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = cls.ClassId
            };

            db.AssignmentCategories.Add(assignmentCategory);
            db.SaveChanges();

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var cls = db.Classes.FirstOrDefault(c => c.ListingNavigation.Department == subject && 
                                                    c.ListingNavigation.Number == num &&
                                                    c.Season == season && 
                                                    c.Year == year);

            if (cls == null)
            {
                return Json(new { success = false });
            }

            var categoryEntity = db.AssignmentCategories.FirstOrDefault(ac => ac.InClass == cls.ClassId && ac.Name == category);

            if (categoryEntity == null)
            {
                return Json(new { success = false, message = "Category does not exist" });
            }

            var assignment = new Assignment
            {
                Name = asgname,
                MaxPoints = (uint)asgpoints,
                Due = asgdue,
                Contents = asgcontents,
                Category = categoryEntity.CategoryId
            };

            db.Assignments.Add(assignment);
            db.SaveChanges();

            // When a professor creates a new assignment, update the grades of all students in the class
            UpdateStudentGrades((int)cls.ClassId);

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
                var submissions = from s in db.Submissions
                      join a in db.Assignments on s.Assignment equals a.AssignmentId
                      join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                      join c in db.Classes on ac.InClass equals c.ClassId
                      join st in db.Students on s.Student equals st.UId
                      join co in db.Courses on c.Listing equals co.CatalogId
                      where co.Department == subject && co.Number == num &&
                            c.Season == season && c.Year == year &&
                            ac.Name == category && a.Name == asgname
                      select new
                      {
                          fname = st.FName,
                          lname = st.LName,
                          uid = st.UId,
                          time = s.Time,
                          score = s.Score
                      };

            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var result = (from s in db.Submissions
                  join a in db.Assignments on s.Assignment equals a.AssignmentId
                  join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                  join c in db.Classes on ac.InClass equals c.ClassId
                  join co in db.Courses on c.Listing equals co.CatalogId
                  where co.Department == subject && co.Number == num &&
                        c.Season == season && c.Year == year &&
                        ac.Name == category && a.Name == asgname &&
                        s.Student == uid
                  select new { Submission = s, ClassID = c.ClassId }).FirstOrDefault();

            if (result == null || result.Submission == null)
            {
                return Json(new { success = false });
            }

            result.Submission.Score = (uint)score;
            db.SaveChanges();

            UpdateStudentGrades((int)result.ClassID);

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {            
             var classes = from c in db.Classes
                            where c.TaughtBy == uid
                            select new
                            {
                                subject = c.ListingNavigation.Department,
                                number = c.ListingNavigation.Number,
                                name = c.ListingNavigation.Name,
                                season = c.Season,
                                year = c.Year
                            };

            return Json(classes);
        }

        private void UpdateStudentGrades(int classId)
        {
            var enrollments = db.Enrolleds.Where(e => e.Class == classId).ToList();
            foreach (var enrollment in enrollments)
            {
                enrollment.Grade = CalculateGrade(enrollment.Student, classId);
            }
            db.SaveChanges();
        }

        private string CalculateGrade(string studentId, int classId)
        {
            var categories = db.AssignmentCategories.Where(ac => ac.InClass == classId).ToList();
            if (!categories.Any()) return "--";

            double totalWeight = categories.Sum(c => c.Weight);
            double totalScore = 0.0;

            foreach (var category in categories)
            {
                var assignments = db.Assignments.Where(a => a.Category == category.CategoryId).ToList();
                if (!assignments.Any()) continue;

                // Calculate the percentage of total points earned in the category
                double categoryMaxPoints = assignments.Sum(a => (double)a.MaxPoints); // Ensure MaxPoints is cast to double for calculation
                double categoryEarnedPoints = assignments.Sum(a => a.Submissions
                    .Where(s => s.Student == studentId)
                    .Select(s => (double)s.Score).DefaultIfEmpty(0.0).Sum());

                double categoryPercentage = categoryEarnedPoints / categoryMaxPoints;
                // Multiply the percentage by the category weight
                totalScore += categoryPercentage * category.Weight;
            }

            // Compute the scaling factor to make all category weights add up to 100%
            double scalingFactor = 100.0 / totalWeight;
            // Multiply the total score by the scaling factor
            double finalScore = totalScore * scalingFactor;
            // Convert the class percentage to a letter grade
            return ConvertToLetterGrade(finalScore);
        }

        private string ConvertToLetterGrade(double score)
        {
            if (score >= 93) return "A";
            if (score >= 90) return "A-";
            if (score >= 87) return "B+";
            if (score >= 83) return "B";
            if (score >= 80) return "B-";
            if (score >= 77) return "C+";
            if (score >= 73) return "C";
            if (score >= 70) return "C-";
            if (score >= 67) return "D+";
            if (score >= 63) return "D";
            if (score >= 60) return "D-";
            return "E";
        }
        
        /*******End code to modify********/
    }
}

