using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _service;
        private readonly IAppointmentService _appointmentService;

        public PaymentsController(IPaymentService service, IAppointmentService appointmentService)
        {
            _service = service;
            _appointmentService = appointmentService;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _service.GetByIdAsync(id.Value);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // GET: Payments/Create
        public async Task<IActionResult> Create()
        {
            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(
                appointments.Select(a => new
                {
                    a.Id,
                    Name = $"{a.Doctor?.User?.Name ?? "Doctor"} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                }),
                "Id",
                "Name"
            );

            return View();
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AppointmentId,Amount,Status,PaymentDate")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(payment);
                return RedirectToAction(nameof(Index));
            }

            // repopulate appointments select if validation fails
            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(
                appointments.Select(a => new
                {
                    a.Id,
                    Name = $"{a.Doctor?.User?.Name ?? "Doctor"} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                }),
                "Id",
                "Name",
                payment.AppointmentId
            );

            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _service.GetByIdAsync(id.Value);
            if (payment == null) return NotFound();

            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(
                appointments.Select(a => new
                {
                    a.Id,
                    Name = $"{a.Doctor?.User?.Name ?? "Doctor"} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                }),
                "Id",
                "Name",
                payment.AppointmentId
            );

            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppointmentId,Amount,Status,PaymentDate")] Payment payment)
        {
            if (id != payment.Id) return NotFound();
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(payment);
                return RedirectToAction(nameof(Index));
            }

            // repopulate appointments select if validation fails
            var appointments = await _appointmentService.GetAllAsync();
            ViewData["AppointmentId"] = new SelectList(
                appointments.Select(a => new
                {
                    a.Id,
                    Name = $"{a.Doctor?.User?.Name ?? "Doctor"} {a.Schedule.Date:yyyy-MM-dd} {a.Schedule.StartTime:HH:mm} - {a.Schedule.EndTime:HH:mm}"
                }),
                "Id",
                "Name",
                payment.AppointmentId
            );

            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _service.GetByIdAsync(id.Value);
            if (payment == null) return NotFound();
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
