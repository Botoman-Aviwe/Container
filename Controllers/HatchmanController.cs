using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrescribingSystem.Data;
using PrescribingSystem.Service;

namespace PrescribingSystem.Controllers
{
    public class HatchmanController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly AccessDbService _accessDb;

        public HatchmanController(ApplicationDbContext context, AccessDbService accessDb,
 UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _accessDb = accessDb;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MovedContainers()
        {
            var moved = await _context.MovedContainers
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(moved);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkContainerReceived(int id)
        {
            var container = await _context.MovedContainers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (container == null)
            {
                return NotFound();
            }

            // Record the exact time the button was clicked
            container.ReceivedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MovedContainers));
        }
    }
}
