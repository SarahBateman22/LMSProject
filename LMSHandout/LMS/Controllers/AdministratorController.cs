using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            if(db.Departments.Any(x => x.Name == name)){
                return Json(new { success = false});
            }
            
            Department d = new Department();

            d.Name = name;
            d.Subject = subject;

            db.Departments.Add(d);
            db.SaveChanges();

            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            
            var courses = from c in db.Courses where c.Department == subject
                      select new 
                      {
                          number = c.Number,
                          name = c.Name
                      };

            var allCourses = courses.ToList();

            return Json(allCourses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {

            var profs = from p in db.Professors where p.WorksIn == subject
                      select new 
                      {
                          fname = p.FName,
                          lname = p.LName,
                          uid = p.UId
                      };

            var allProfs = profs.ToList();

            return Json(allProfs);
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {           
            if(db.Courses.Any(x => x.Name == name)){
                return Json(new { success = false});
            }
            
            Course c = new Course();

            c.Name = name;
            c.Number = (uint)number;
            c.Department = subject;

            db.Courses.Add(c);
            db.SaveChanges();

            return Json(new { success = true});

        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {            

            //step 1: ensure the course that the class belongs to exists
            Course courseChecker = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == number);
            if (courseChecker == null)
            {
                return Json(new { success = false, message = "Course does not exist." });
            }

            //step 2: check if there is already a class that matches in the table
            bool existingClass = db.Classes.Any(c => 
                c.Season == season &&
                c.Year == year &&
                c.TaughtBy == instructor && 
                c.Location == location);

            if (existingClass)
            {
                return Json(new { success = false, message = "Class offering for this course already exists." });
            }

            //step 3: check for location conflicts at the same time
            bool locationConflict = db.Classes.Any(cls => 
                cls.Location == location &&
                cls.Season == season &&
                cls.Year == year &&
                ((cls.StartTime <= TimeOnly.FromDateTime(end) && cls.StartTime >= TimeOnly.FromDateTime(start)) || 
                (cls.EndTime >= TimeOnly.FromDateTime(start) && cls.EndTime <= TimeOnly.FromDateTime(end))));

            if (locationConflict)
            {
                return Json(new { success = false, message = "Another class occupies this location during the specified times." });
            }

            //step 4: if it hasn't hit a failure up to here, create a new class and add it
            Class nc = new Class();

            nc.Season = season;
            nc.Year = (uint)year;
            nc.Location = location;
            nc.StartTime = TimeOnly.FromDateTime(start);
            nc.EndTime = TimeOnly.FromDateTime(end);
            nc.TaughtBy = instructor;

            db.Classes.Add(nc);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /*******End code to modify********/

    }
}

