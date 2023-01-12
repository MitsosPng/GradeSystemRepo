﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GradeSystem.Models;

namespace GradeSystem.Controllers
{
    public class ProfessorsController : Controller
    {
        private readonly DBContext _context;

        public ProfessorsController(DBContext context)
        {
            _context = context;
        }

        // GET: Professor
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Professor") != null)
            {
                String username = HttpContext.Session.GetString("Professor");
                var professor = _context.Professors.FirstOrDefault(s => s.Username == username);

                return View(professor);
            }
            else
            {
                return RedirectToAction("UsersLogin", "Users");
            }
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.Any, NoStore = true)]
        public IActionResult SelectByLesson()
        {
            if (HttpContext.Session.GetString("Professor") != null)
            {
                String username = HttpContext.Session.GetString("Professor");

                ProfessorLessons(username);      
                
                return View();
            }
            else
            {
                return RedirectToAction("UsersLogin", "Users");
            }
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.Any, NoStore = true)]
        public async Task<IActionResult> SelectByLesson(int SelectedCourseId)
        {
            String username = HttpContext.Session.GetString("Professor");

            ProfessorLessons(username);

            var gradesByLesson = _context.CourseHasStudents.Include(s => s.Student).Include(s => s.Course).ThenInclude(s => s.Professor).Where(s => s.Course.Professor.Username.Equals(username) && s.GradeCourseStudent != null && s.IdCourse == SelectedCourseId );
            ViewBag.Selected = SelectedCourseId;

            return View(await gradesByLesson.ToListAsync());

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.Any, NoStore = true)]
        public async Task<IActionResult> InsertGrades()
        {
            if (HttpContext.Session.GetString("Professor") != null)
            {
                String username = HttpContext.Session.GetString("Professor");

                ProfessorLessons(username);

                return View();
            }
            else
            {
                return RedirectToAction("UsersLogin", "Users");
            }
        }

        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.Any, NoStore = true)]
        public async Task<IActionResult> InsertGrades(int SelectedCourseId,int? RegNum,int? grade)
        {
            String username = HttpContext.Session.GetString("Professor");

            ProfessorLessons(username);

            var gradesByLesson = _context.CourseHasStudents.Include(s => s.Student).Include(s => s.Course).ThenInclude(s => s.Professor).Where(s => s.Course.Professor.Username.Equals(username) && s.GradeCourseStudent == null && s.IdCourse == SelectedCourseId);
            ViewBag.Selected = SelectedCourseId;

            if (RegNum != null && grade != null)
            {

                CourseHasStudent crs = new CourseHasStudent();

                crs =_context.CourseHasStudents.FirstOrDefault(u=>u.RegistrationNumber==RegNum && u.IdCourse==SelectedCourseId);
                crs.GradeCourseStudent = grade;
                _context.Update(crs);
                _context.SaveChanges();
            }
            

            return View(await gradesByLesson.ToListAsync());

        }


        



        public void ProfessorLessons(String username) 
        {
            var lessons = _context.Courses.Include(s => s.Professor).Where(s => s.Professor.Username.Equals(username)).Select(s => new { s.IdCourse, s.CourseTitle });
            ViewBag.lessonsList = lessons.ToList();
        }

        // GET: Professors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Professors == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Afm == id);
            if (professor == null)
            {
                return NotFound();
            }

            return View(professor);
        }

        // GET: Professors/Create
        public IActionResult Create()
        {
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Username");
            return View();
        }

        // POST: Professors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Afm,Name,Surname,Department,Username")] Professor professor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Username", professor.Username);
            return View(professor);
        }

        // GET: Professors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Professors == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Username", professor.Username);
            return View(professor);
        }

        // POST: Professors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Afm,Name,Surname,Department,Username")] Professor professor)
        {
            if (id != professor.Afm)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfessorExists(professor.Afm))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Username"] = new SelectList(_context.Users, "Username", "Username", professor.Username);
            return View(professor);
        }

        // GET: Professors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Professors == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Afm == id);
            if (professor == null)
            {
                return NotFound();
            }

            return View(professor);
        }

        // POST: Professors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Professors == null)
            {
                return Problem("Entity set 'DBContext.Professors'  is null.");
            }
            var professor = await _context.Professors.FindAsync(id);
            if (professor != null)
            {
                _context.Professors.Remove(professor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfessorExists(int id)
        {
          return _context.Professors.Any(e => e.Afm == id);
        }
    }
}
