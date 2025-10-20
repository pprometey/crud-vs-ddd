using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoctorBooking.CRUD.Db;

namespace DoctorBooking.CRUD.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly MedicalBookingContext _context;

        public AppointmentsController(MedicalBookingContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var medicalBookingContext = _context.Appointments
                .Include(a => a.Doctor)
                     .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                     .ThenInclude(d => d.User)
                .Include(a => a.Schedule);
            return View(await medicalBookingContext.ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                     .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                     .ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Project to simple id/name pairs so SelectList text shows user-friendly strings
            var doctors = _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .ToList()
                .Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty })
                .ToList();

            var patients = _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .ToList()
                .Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty })
                .ToList();

            var schedules = _context.Schedules
                .AsNoTracking()
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name");
            ViewData["PatientId"] = new SelectList(patients, "Id", "Name");
            ViewData["ScheduleId"] = new SelectList(schedules, "Id", "Name");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DoctorId,PatientId,ScheduleId,ScheduledTime,Status")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var doctors = _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .ToList()
                .Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty })
                .ToList();

            var patients = _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .ToList()
                .Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty })
                .ToList();

            var schedules = _context.Schedules
                .AsNoTracking()
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules, "Id", "Name", appointment.ScheduleId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var doctors = _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .ToList()
                .Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty })
                .ToList();

            var patients = _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .ToList()
                .Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty })
                .ToList();

            var schedules = _context.Schedules
                .AsNoTracking()
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules, "Id", "Name", appointment.ScheduleId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,PatientId,ScheduleId,ScheduledTime,Status")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
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

            var doctors = _context.Doctors
                .Include(d => d.User)
                .AsNoTracking()
                .ToList()
                .Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty })
                .ToList();

            var patients = _context.Patients
                .Include(p => p.User)
                .AsNoTracking()
                .ToList()
                .Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty })
                .ToList();

            var schedules = _context.Schedules
                .AsNoTracking()
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["DoctorId"] = new SelectList(doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients, "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules, "Id", "Name", appointment.ScheduleId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
