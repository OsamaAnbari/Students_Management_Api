﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using Students_Management_Api;
using Students_Management_Api.Models;

namespace Students_Management_Api.Controllers
{
    [Authorize(Roles = "1")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly LibraryContext _context;

        public StudentsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudent()
        {
          if (_context.Student == null)
          {
              return NotFound();
          }
            return await _context.Student.ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
          if (_context.Student == null)
          {
              return NotFound();
          }
            var student = await _context.Student.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Student.Attach(student);
            _context.Entry(student).Property(x => x.Firstname).IsModified = true;
            _context.Entry(student).Property(x => x.Surname).IsModified = true;
            _context.Entry(student).Property(x => x.Phone).IsModified = true;
            _context.Entry(student).Property(x => x.Tc).IsModified = true;
            _context.Entry(student).Property(x => x.Faculty).IsModified = true;
            _context.Entry(student).Property(x => x.Department).IsModified = true;
            _context.Entry(student).Property(x => x.Year).IsModified = true;
            _context.Entry(student).Property(x => x.birth).IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
          if (_context.Student == null)
          {
              return Problem("Entity set 'LibraryContext.Student'  is null.");
          }
            User user = new User
            {
                Username = student.Tc,
                Password = BCrypt.Net.BCrypt.HashPassword(student.Tc, workFactor: 10),
                Role = 3
            };

            student.User = user;

            _context.User.Add(user);
            _context.Student.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.Id }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (_context.Student == null)
            {
                return NotFound();
            }
            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(student.UserId);
            if (user == null)
            {
                return NotFound("User is not found");
            }

            _context.Student.Remove(student);
            _context.User.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentExists(int id)
        {
            return (_context.Student?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
