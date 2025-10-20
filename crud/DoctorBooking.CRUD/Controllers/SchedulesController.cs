using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class SchedulesController : Controller
    {
        private readonly IScheduleService _service;
        private readonly IDoctorService _doctorService;

        public SchedulesController(IScheduleService service, IDoctorService doctorService)
        {
            _service = service;
            _doctorService = doctorService;
        }

        // GET: Schedules
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _service.GetByIdAsync(id.Value);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        // GET: Schedules/Create
        public async Task<IActionResult> Create()
        {
            var doctors = await _doctorService.GetAllAsync();
            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name");
            return View();
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DoctorId,Date,StartTime,EndTime,IsAvailable")] Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(schedule);
                return RedirectToAction(nameof(Index));
            }
            var doctors = await _doctorService.GetAllAsync();
            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", schedule.DoctorId);
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _service.GetByIdAsync(id.Value);
            if (schedule == null) return NotFound();
            var doctors = await _doctorService.GetAllAsync();
            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", schedule.DoctorId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DoctorId,Date,StartTime,EndTime,IsAvailable")] Schedule schedule)
        {
            if (id != schedule.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(schedule);
                }
                catch (Exception)
                {
                    if ((await _service.GetByIdAsync(schedule.Id)) == null) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var doctors = await _doctorService.GetAllAsync();
            ViewData["DoctorId"] = new SelectList(doctors.Select(d => new { d.Id, Name = d.User?.Name ?? string.Empty }), "Id", "Name", schedule.DoctorId);
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var schedule = await _service.GetByIdAsync(id.Value);
            if (schedule == null) return NotFound();
            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
