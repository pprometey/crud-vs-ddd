using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _service;
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IScheduleService _scheduleService;

        public AppointmentsController(IAppointmentService service, IDoctorService doctorService, IPatientService patientService, IScheduleService scheduleService)
        {
            _service = service;
            _doctorService = doctorService;
            _patientService = patientService;
            _scheduleService = scheduleService;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _service.GetByIdAsync(id.Value);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            var doctors = await _doctorService.GetAllAsync();
            var patients = await _patientService.GetAllAsync();
            var schedules = await _scheduleService.GetAllAsync();

            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name");
            ViewData["PatientId"] = new SelectList(patients.Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty }), "Id", "Name");
            ViewData["ScheduleId"] = new SelectList(schedules.Select(s => new { s.Id, Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}" }), "Id", "Name");

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DoctorId,PatientId,ScheduleId,ScheduledTime,Status")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(appointment);
                return RedirectToAction(nameof(Index));
            }

            // repopulate selects
            var doctors = await _doctorService.GetAllAsync();
            var patients = await _patientService.GetAllAsync();
            var schedules = await _scheduleService.GetAllAsync();

            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients.Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty }), "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules.Select(s => new { s.Id, Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}" }), "Id", "Name", appointment.ScheduleId);

            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _service.GetByIdAsync(id.Value);
            if (appointment == null) return NotFound();

            var doctors = await _doctorService.GetAllAsync();
            var patients = await _patientService.GetAllAsync();
            var schedules = await _scheduleService.GetAllAsync();

            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients.Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty }), "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules.Select(s => new { s.Id, Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}" }), "Id", "Name", appointment.ScheduleId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,PatientId,ScheduleId,ScheduledTime,Status")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(appointment);
                }
                catch (Exception)
                {
                    if (!AppointmentExists(appointment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var doctors = await _doctorService.GetAllAsync();
            var patients = await _patientService.GetAllAsync();
            var schedules = await _scheduleService.GetAllAsync();

            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(patients.Select(p => new { p.Id, Name = p.User?.Name ?? string.Empty }), "Id", "Name", appointment.PatientId);
            ViewData["ScheduleId"] = new SelectList(schedules.Select(s => new { s.Id, Name = $"{s.Date:yyyy-MM-dd} {s.StartTime:HH:mm} - {s.EndTime:HH:mm}" }), "Id", "Name", appointment.ScheduleId);

            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var appointment = await _service.GetByIdAsync(id.Value);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _service.GetByIdAsync(id).Result != null;
        }
    }
}
