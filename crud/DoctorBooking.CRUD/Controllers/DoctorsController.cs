using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _service;
        private readonly IUserService _userService;

        public DoctorsController(IDoctorService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        // GET: Doctors
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetAllAsync());
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var doctor = await _service.GetByIdAsync(id.Value);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // GET: Doctors/Create
        public async Task<IActionResult> Create()
        {
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name");
            return View();
        }

        // POST: Doctors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Specialization")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(doctor);
                return RedirectToAction(nameof(Index));
            }
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", doctor.UserId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var doctor = await _service.GetByIdAsync(id.Value);
            if (doctor == null) return NotFound();
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", doctor.UserId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Specialization")] Doctor doctor)
        {
            if (id != doctor.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(doctor);
                }
                catch (Exception)
                {
                    if ((await _service.GetByIdAsync(doctor.Id)) == null) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var users = await _userService.GetAllAsync();
            ViewData["UserId"] = new SelectList(users.Select(u => new { u.Id, u.Name }), "Id", "Name", doctor.UserId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var doctor = await _service.GetByIdAsync(id.Value);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
