using System;
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
            var student = db.Students.FirstOrDefault(s => s.UId == uid);
            if (student == null){
                return Json(new List<object>());
            }

            var enrollments = db.Enrolleds
                .Where(e => e.Student == uid)
                .Select(e => new
                {
                    subject = e.ClassNavigation.ListingNavigation.Department,
                    number = e.ClassNavigation.ListingNavigation.Number,
                    name = e.ClassNavigation.ListingNavigation.Name,
                    season = e.ClassNavigation.Season,
                    year = e.ClassNavigation.Year,
                    grade = e.Grade ?? "--"
                }).ToList();

            return Json(enrollments); 
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
            var courseClass = db.Classes
                .Where(c => c.Season == season && c.Year == year &&
                            c.ListingNavigation.Department == subject &&
                            c.ListingNavigation.Number == num)
                .FirstOrDefault();

            if (courseClass == null){
                return Json(new List<object>());
            }

            // check enrollment
            var enrollment = db.Enrolleds
                .Where(e => e.Student == uid && e.Class == courseClass.ClassId)
                .FirstOrDefault();

            if (enrollment == null){
                return Json(new List<object>());
            }

            // all assignments in class
            var assignments = (from a in db.Assignments
                       join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                       where a.CategoryNavigation.InClass == courseClass.ClassId
                       select new
                       {
                           Assignment = a,
                           CategoryName = ac.Name
                       }).ToList();

            // all submissions from 1 student in 1 class
             var submissions = db.Submissions
                    .Where(s => s.Student == uid && assignments.Select(a => a.Assignment.AssignmentId).Contains(s.Assignment))
                    .ToList();

            // merge assignments and submissions
            var assignmentResults = assignments.Select(a => new {
                aname = a.Assignment.Name,
                cname = a.CategoryName, 
                due = a.Assignment.Due,
                score = submissions.FirstOrDefault(s => s.Assignment == a.Assignment.AssignmentId)?.Score ?? 0  // Safe navigation and coalescing to 0
            });

            return Json(assignmentResults.ToList());
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
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {         
            //find class  
            var listing = (from course in db.Courses
                      join class_ in db.Classes on course.CatalogId equals class_.Listing
                      where course.Department == subject && course.Number == num
                      select class_).FirstOrDefault();
        
            if (listing == null){
                return Json(new { success = false });
            }

            //find assignment
            var assignment = (from class_ in db.Classes
                          join cate in db.AssignmentCategories on class_.ClassId equals cate.InClass
                          join assignm in db.Assignments on cate.CategoryId equals assignm.Category
                          where class_.Season == season && class_.Year == year &&
                                cate.Name == category && assignm.Name == asgname
                          select assignm).SingleOrDefault();

            if (assignment == null) {
                return Json(new { success = false });
            }

            var submission = (from subm in db.Submissions
                            where subm.Student == uid && subm.Assignment == assignment.AssignmentId
                            select subm).SingleOrDefault();

            if (submission != null){
                submission.SubmissionContents = contents;
                submission.Time = DateTime.Now;
            }
            else{
                // make new submission
                submission = new Submission
                {
                    Student = uid,
                    Assignment = assignment.AssignmentId,
                    Time = DateTime.Now,
                    SubmissionContents = contents,
                    Score = 0
                };
                db.Submissions.Add(submission);
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
            var courseClass = db.Classes
                .Where(c => c.Season == season && c.Year == year &&
                            c.ListingNavigation.Department == subject &&
                            c.ListingNavigation.Number == num)
                .FirstOrDefault();

            if (courseClass == null){
                return Json(new { success = false });
            }

            // check not double enroll
            var enrollment = db.Enrolleds
                .Where(e => e.Student == uid && e.Class == courseClass.ClassId)
                .FirstOrDefault();

            if (enrollment != null){
                return Json(new { success = false });
            }
            // new enroll
            var newEnrollment = new Enrolled
            {
                Student = uid,
                Class = courseClass.ClassId,
                // if no grades are provided
                Grade = "--" 
            };
            db.Enrolleds.Add(newEnrollment);
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
            //TODO: check that this formula works as expected !!
            var gradePoints = new Dictionary<string, double>
            {
                { "A", 4.0 },
                { "A-", 3.7 },
                { "B+", 3.3 },
                { "B", 3.0 },
                { "B-", 2.7 },
                { "C+", 2.3 },
                { "C", 2.0 },
                { "C-", 1.7 },
                { "D+", 1.3 },
                { "D", 1.0 },
                { "D-", 0.7 },
                { "E", 0.0 }
            };

            // find all the student's enrollments
            var enrollments = db.Enrolleds
                .Where(e => e.Student == uid && e.Grade != "--")
                .Select(e => e.Grade)
                .ToList();

            if (!enrollments.Any()){
                return Json(new { gpa = 0.0 });
            }
            // calculate the GPA
            double totalPoints = 0.0;
            int count = 0;

            foreach (var grade in enrollments)
            {
                if (gradePoints.ContainsKey(grade))
                {
                    totalPoints += gradePoints[grade];
                    count++;
                }
            }
            double gpa = count > 0 ? totalPoints / count : 0.0;
            return Json(new { gpa });
        }
                
        /*******End code to modify********/

    }
}

