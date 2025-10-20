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
    public class PaymentsController : Controller
    {
        private readonly MedicalBookingContext _context;

        public PaymentsController(MedicalBookingContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Schedule)
                .AsNoTracking();

            return View(await payments.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Schedule)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            var appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                .AsNoTracking()
                .ToList()
                .Select(a => new
                {
                    a.Id,
                    Text = $"{(a.Doctor?.User?.Name ?? "Doctor")} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["AppointmentId"] = new SelectList(appointments, "Id", "Text");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppointmentId,Amount,Status,PaymentDate")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                .AsNoTracking()
                .ToList()
                .Select(a => new
                {
                    a.Id,
                    Text = $"{(a.Doctor?.User?.Name ?? "Doctor")} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["AppointmentId"] = new SelectList(appointments, "Id", "Text", payment.AppointmentId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            var appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                .AsNoTracking()
                .ToList()
                .Select(a => new
                {
                    a.Id,
                    Text = $"{(a.Doctor?.User?.Name ?? "Doctor")} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["AppointmentId"] = new SelectList(appointments, "Id", "Text", payment.AppointmentId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentId,Amount,Status,PaymentDate")] Payment payment)
        {
            if (id != payment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var appointments = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Schedule)
                .AsNoTracking()
                .ToList()
                .Select(a => new
                {
                    a.Id,
                    Text = $"{(a.Doctor?.User?.Name ?? "Doctor")} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                })
                .ToList();

            ViewData["AppointmentId"] = new SelectList(appointments, "Id", "Text", payment.AppointmentId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Schedule)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null) _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}
