using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using NuGet.DependencyResolver;

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
                   where e.ClassNavigation.Season == season &&
                         e.ClassNavigation.Year == year &&
                         e.ClassNavigation.ListingNavigation.Department == subject &&
                         e.ClassNavigation.ListingNavigation.Number == num
                   select new
                   {
                       fname = e.StudentNavigation.FName,
                       lname = e.StudentNavigation.LName,
                       uid = e.StudentNavigation.UId,
                       dob = e.StudentNavigation.Dob,
                       grade = e.Grade
                   };

            return Json(students.ToList());
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
           var assignments = from cl in db.Classes
                      join co in db.Courses on cl.Listing equals co.CatalogId
                      join ca in db.AssignmentCategories on cl.ClassId equals ca.InClass
                      join a in db.Assignments on ca.CategoryId equals a.Category
                      where co.Department == subject && co.Number == num &&
                            cl.Season == season && cl.Year == year &&
                            (category == null || ca.Name == category)
                      select new
                      {
                          aname = a.Name,
                          cname = ca.Name,
                          due = a.Due,
                          submissions = a.Submissions.Count()
                      };

            return Json(assignments.ToList());
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
            var categories = from cl in db.Classes
                    join co in db.Courses on cl.Listing equals co.CatalogId
                    join ac in db.AssignmentCategories on cl.ClassId equals ac.InClass
                    where co.Department == subject && co.Number == num &&
                           cl.Season == season && cl.Year == year
                    select new
                    {
                        name = ac.Name,
                        weight = ac.Weight
                    };
            

            return Json(categories.ToList());
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
            var courseClass = db.Classes
                .Where(c => c.Season == season && c.Year == year &&
                            c.ListingNavigation.Department == subject &&
                            c.ListingNavigation.Number == num)
                .FirstOrDefault();

            if (courseClass == null)
            {
                //no class
                return Json(new { success = false });
            }

            //check already exist
            var existingCategory = db.AssignmentCategories
                .Where(ac => ac.InClass == courseClass.ClassId && ac.Name == category)
                .FirstOrDefault();

            if (existingCategory != null)
            {
                return Json(new { success = false });
            }

            
            var newCategory = new AssignmentCategory
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = courseClass.ClassId
            };

            // new cat for context
            db.AssignmentCategories.Add(newCategory);

            Debug.WriteLine (newCategory);

          
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
            var courseClass = db.Classes
                .Where(c => c.Season == season && c.Year == year &&
                            c.ListingNavigation.Department == subject &&
                            c.ListingNavigation.Number == num)
                .FirstOrDefault();

            if (courseClass == null){
                return Json(new { success = false });
            }

            var assignmentCategory = db.AssignmentCategories
                .Where(ac => ac.InClass == courseClass.ClassId && ac.Name == category)
                .FirstOrDefault();

            if (assignmentCategory == null){
                return Json(new { success = false });
            }

            // ---- make new assign ---
            var newAssignment = new Assignment
            {
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = assignmentCategory.CategoryId,
                CategoryNavigation = assignmentCategory
            };
            //add changes
            db.Assignments.Add(newAssignment);
            //save changes
            db.SaveChanges();
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
            // find class
            var courseClass = db.Classes
                .Where(c => c.Season == season && c.Year == year &&
                            c.ListingNavigation.Department == subject &&
                            c.ListingNavigation.Number == num)
                .FirstOrDefault();

            if (courseClass == null){          
                return Json(new List<object>());
            }
            // find assignmentcat
            var assignmentCategory = db.AssignmentCategories
                .Where(ac => ac.InClass == courseClass.ClassId && ac.Name == category)
                .FirstOrDefault();

            if (assignmentCategory == null){
                return Json(new List<object>());
            }

            var assignment = db.Assignments
                .Where(a => a.Category == assignmentCategory.CategoryId && a.Name == asgname)
                .FirstOrDefault();

            if (assignment == null){
                return Json(new List<object>());
            }

            //get all submissions of particular assignment
            var submissions = from s in db.Submissions
                              where s.Assignment == assignment.AssignmentId
                              select new
                              {
                                  fname = s.StudentNavigation.FName,
                                  lname = s.StudentNavigation.LName,
                                  uid = s.StudentNavigation.UId,
                                  time = s.Time,
                                  score = s.Score
                              };

            return Json(submissions.ToList());
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
            var courseClass = db.Classes
                    .Where(c => c.Season == season && c.Year == year &&
                                c.ListingNavigation.Department == subject &&
                                c.ListingNavigation.Number == num)
                    .FirstOrDefault();

                if (courseClass == null){
                    return Json(new { success = false });
                }

                // find assigncat
                var assignmentCategory = db.AssignmentCategories
                    .Where(ac => ac.InClass == courseClass.ClassId && ac.Name == category)
                    .FirstOrDefault();

                if (assignmentCategory == null){
                    return Json(new { success = false });
                }

                var assignment = db.Assignments
                    .Where(a => a.Category == assignmentCategory.CategoryId && a.Name == asgname)
                    .FirstOrDefault();

                if (assignment == null){
                    return Json(new { success = false });
                }
                //find per student - use UID !!
                var submission = db.Submissions
                    .Where(s => s.Assignment == assignment.AssignmentId && s.Student == uid)
                    .FirstOrDefault();

                if (submission == null)
                {
                    return Json(new { success = false });
                }
                //new one save and update
                submission.Score = (uint)score;
                db.SaveChanges();

                //update student grade here
                UpdateStudentGrades(courseClass.ClassId);
                
                return Json(new { success = true });
        }

        public void UpdateStudentGrades(uint classId)
        {

            var enrollments = db.Enrolleds.Where(e => e.Class == classId).ToList();

            foreach (var enrollment in enrollments)
            {
                var classAssignments = db.AssignmentCategories
                                        .Where(ac => ac.InClass == enrollment.Class)
                                        .SelectMany(ac => ac.Assignments)
                                        .ToList();

                // skip if the there are no assignments in the category
                if (!classAssignments.Any())
                    continue; 

                var validCategories = db.AssignmentCategories
                                        .Where(ac => ac.InClass == enrollment.Class && ac.Assignments.Any())
                                        .ToList();

                double totalValidWeights = validCategories.Sum(ac => ac.Weight);
                double scalingFactor = totalValidWeights > 0 ? 100 / totalValidWeights : 0;

                double earnedPoints = 0.0;
                double maxPossiblePoints = 100;

                foreach (var category in validCategories)
                {
                    var assignments = db.Assignments.Where(a => a.Category == category.CategoryId).ToList();
                    double categoryEarnedPoints = 0.0;
                    double categoryMaxPoints = 0.0;

                    foreach (var assignment in assignments)
                    {
                        double assignmentMaxPoints = assignment.MaxPoints;
                        categoryMaxPoints += assignmentMaxPoints;

                        var submission = db.Submissions.SingleOrDefault(s => s.Assignment == assignment.AssignmentId && s.Student == enrollment.Student);
                        if (submission != null)
                        {
                            categoryEarnedPoints += submission.Score;
                        }
                    }

                    if (assignments.Count > 0)
                    {
                        double categoryWeightAdjusted = category.Weight * scalingFactor;
                        double categoryEarnedPercentage = categoryMaxPoints > 0 ? categoryEarnedPoints / categoryMaxPoints : 0.0;
                        earnedPoints += categoryEarnedPercentage * categoryWeightAdjusted;
                    }
                }

                string letterGrade = getLetterGrade(earnedPoints, maxPossiblePoints);
                enrollment.Grade = letterGrade;

                db.SaveChanges();
            }
        }

        public static string getLetterGrade(double cumulativePoints, double totalPoints)
        {
            double gradePoint = cumulativePoints / totalPoints * 100;

            return gradePoint switch
            {
                _ when gradePoint >= 93 => "A",
                _ when gradePoint >= 90 => "A-",
                _ when gradePoint >= 87 => "B+",
                _ when gradePoint >= 83 => "B",
                _ when gradePoint >= 80 => "B-",
                _ when gradePoint >= 77 => "C+",
                _ when gradePoint >= 73 => "C",
                _ when gradePoint >= 70 => "C-",
                _ when gradePoint >= 67 => "D+",
                _ when gradePoint >= 63 => "D",
                _ when gradePoint >= 60 => "D-",
                _ => "F"
            };
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

            return Json(classes.ToList());      
        }


        
        /*******End code to modify********/
    }
}

