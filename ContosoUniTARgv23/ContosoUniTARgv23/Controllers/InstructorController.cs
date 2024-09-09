using ContosoUniTARgv23.Data;
using ContosoUniTARgv23.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniTARgv23.Controllers
{
    public class InstructorController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorController
            (
            SchoolContext context
            )
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id, int? courseId)
        {
            var viewModel = new InstructorIndexData();
            
            viewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Enrollments)
                            .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                        .ThenInclude(i => i.Department)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorId"] = id.Value;
                Instructor instructor = viewModel.Instructors
                    .Where(i => i.Id == id.Value).Single();
                viewModel.Courses = instructor.CourseAssignments
                    .Select(s => s.Course);
            }

            if (id != null)
            {
                ViewData["InstructorId"] = id.Value;
                Instructor instructor = viewModel.Instructors
                    .Where(i => i.Id == id.Value)
                    .Single();

                viewModel.Courses = instructor.CourseAssignments
                    .Select(s => s.Course);
            }

            if (courseId != null)
            {
                ViewData["CourseId"] = courseId;
                viewModel.Enrollments = viewModel.Courses
                    .Where(x => x.CourseId == courseId)
                    .Single()
                    .Enrollments;
            }

            return View(viewModel);
        }
    }
}
