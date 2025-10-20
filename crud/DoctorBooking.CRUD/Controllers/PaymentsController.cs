using DoctorBooking.CRUD.Db;
using DoctorBooking.CRUD.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorBooking.CRUD.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
        {
            _service = service;
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
        public IActionResult Create()
        {
            // Service will provide ViewData in controller in real app; keep simple here.
            var appointments = new List<SelectListItem>();
            ViewData["AppointmentId"] = new SelectList(appointments, "Value", "Text");
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
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var payment = await _service.GetByIdAsync(id.Value);
            if (payment == null) return NotFound();
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
