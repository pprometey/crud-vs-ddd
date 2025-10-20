using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IPatientService _service;
        private readonly IUserService _userService;

        public PatientsController(IPatientService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _service.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // GET: Patients/Create
        public async Task<IActionResult> Create()
        {
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name");
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,DateOfBirth")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(patient);
                return RedirectToAction(nameof(Index));
            }
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", patient.UserId);
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _service.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", patient.UserId);
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,DateOfBirth")] Patient patient)
        {
            if (id != patient.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(patient);
                }
                catch (Exception)
                {
                    if ((await _service.GetByIdAsync(patient.Id)) == null) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", patient.UserId);
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var patient = await _service.GetByIdAsync(id.Value);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
