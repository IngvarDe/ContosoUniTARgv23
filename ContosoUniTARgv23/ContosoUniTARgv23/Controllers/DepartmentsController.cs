using ContosoUniTARgv23.Data;
using ContosoUniTARgv23.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniTARgv23.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly SchoolContext _context;

        public DepartmentsController
            (
                SchoolContext context
            )
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _context.Departments
                .Include(d => d.Administrator)
                .ToListAsync();

            return View(result);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if ( id == null )
            {
                return NotFound();
            }

            string query = "SELECT * FROM Department WHERE DepartmentId = {0}";
            var department = await _context.Departments
                .FromSqlRaw(query, id)
                .Include(d => d.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if ( department == null ) 
            { 
                return NotFound();
            }

            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName", department.InstructorId);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion, Department model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var domain = await _context.Departments
                .Include(d => d.Administrator)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if(domain == null)
            {
                Department deleteDepartment = new Department();
                await TryUpdateModelAsync(deleteDepartment);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The department was deleted by another user.");
                ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName", deleteDepartment.InstructorId);
                return View(deleteDepartment);
            }

            _context.Entry(domain).Property("RowVersion").OriginalValue = rowVersion;

            domain.Name = model.Name;
            domain.Budget = model.Budget;
            domain.StartDate = model.StartDate;

            _context.Update(domain);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            //if (await TryUpdateModelAsync<Department>(
            //    departmentToUpdate,
            //    "",
            //    s => s.Name, s => s.StartDate, s => s.Budget, s => s.InstructorId))
            //{
                //try
                //{
                //    await _context.SaveChangesAsync();
                //    return RedirectToAction(nameof(Index));
                //}
                //catch (DbUpdateConcurrencyException ex)
                //{
                //    var exceptionEntry = ex.Entries.Single();
                //    var clientValues = (Department)exceptionEntry.Entity;
                //    var databaseEntry = exceptionEntry.GetDatabaseValues();

                //    if(databaseEntry == null)
                //    {
                //        ModelState.AddModelError(string.Empty,
                //            "Unabel to save changes. The department was deleted by another user.");
                //    }
                //    else
                //    {
                //        var databaseValues = (Department)databaseEntry.ToObject();

                //        if(databaseValues.Name != clientValues.Name)
                //        {
                //            ModelState.AddModelError("Name", $"Current value: {databaseValues.Name}");
                //        }
                //        if (databaseValues.Budget != clientValues.Budget)
                //        {
                //            ModelState.AddModelError("Budget", $"Current value: {databaseValues.Budget}");
                //        }
                //        if (databaseValues.StartDate != clientValues.StartDate)
                //        {
                //            ModelState.AddModelError("StartDate", $"Current value: {databaseValues.StartDate}");
                //        }
                //        if (databaseValues.InstructorId != clientValues.InstructorId)
                //        {
                //            Instructor databaseInstructor = await _context.Instructors
                //                .FirstOrDefaultAsync(i => i.Id == databaseValues.InstructorId);
                //            ModelState.AddModelError("InstructorId", $"Current value: {databaseValues.InstructorId}");
                //        }

                //        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                //            + "was modified by another user after you got the original value. The "
                //            + "edit operation was canceled and the current values in the database "
                //            + "have been displayed. If you still want to edit this record, click "
                //            + "the Save button again. Otherwise click the Back to List hyperlink.");
                //        departmentToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                //        ModelState.Remove("RowVersion");
                //    }
                //}
            //}

            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName", domain.InstructorId);
            return View(domain);
        }

        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(x => x.Administrator)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }

                return NotFound();
            }

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Department department)
        {
            try
            {
                if (await _context.Departments.AnyAsync(m => m.DepartmentId == department.DepartmentId))
                {
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {

                return RedirectToAction(nameof(Delete), new { concurrencyError = true, id = department.DepartmentId });
            }
        }

        public IActionResult Create()
        {
            ViewData["InstructorID"] = new SelectList(_context.Instructors, "Id", "FullName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "Id", "FullName", department.InstructorId);

            return View(department);
        }
    }
}
