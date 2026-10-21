
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrescribingSystem.Data;
using PrescribingSystem.Models;
using PrescribingSystem.Service;


namespace PrescribingSystem.Controllers
{
    [Authorize(Roles = "Hatchman")]
    public class HatchmanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AccessDbService _accessDb;
        private readonly UserManager<ApplicationUser> _userManager;

        public HatchmanController(
            ApplicationDbContext context,
            AccessDbService accessDb,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _accessDb = accessDb;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movedContainers = await _context.MovedContainers
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var receivedContainers = movedContainers
                .Where(x => x.ReceivedDate.HasValue)
                .ToList();

            var fast = receivedContainers.Count(x =>
                (x.ReceivedDate.Value - x.CreatedDate).TotalHours <= 1);

            var medium = receivedContainers.Count(x =>
                (x.ReceivedDate.Value - x.CreatedDate).TotalHours > 1 &&
                (x.ReceivedDate.Value - x.CreatedDate).TotalHours <= 4);

            var slow = receivedContainers.Count(x =>
                (x.ReceivedDate.Value - x.CreatedDate).TotalHours > 4);

            var averageHours = receivedContainers.Any()
                ? Math.Round(receivedContainers.Average(x =>
                    (x.ReceivedDate.Value - x.CreatedDate).TotalHours), 2)
                : 0;

            ViewBag.Fast = fast;
            ViewBag.Medium = medium;
            ViewBag.Slow = slow;
            ViewBag.AverageHours = averageHours;

            return View(movedContainers);
        }

        //// Hatchman dashboard
        //[HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    var movedContainers = await _context.MovedContainers
        //        .AsNoTracking()
        //        .OrderByDescending(x => x.CreatedDate)
        //        .ToListAsync();

        //    return View(movedContainers);
        //}

        // Display all moved containers
        public async Task<IActionResult> MovedContainers()
        {
            var moved = await _context.MovedContainers
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(moved);
        }

        // Mark a container as received
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkContainerReceived(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid container ID.");
            }

            var container = await _context.MovedContainers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (container == null)
            {
                return NotFound("Container not found.");
            }

            // Prevent overwriting the original received timestamp.
            if (container.ReceivedDate.HasValue)
            {
                TempData["InfoMessage"] =
                    $"Container {container.Container} has already been received.";

                return RedirectToAction(nameof(MovedContainers));
            }

            container.ReceivedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Container {container.Container} was marked as received successfully.";

            return RedirectToAction(nameof(MovedContainers));
        }

        public async Task<IActionResult> ContainerDetails(int id)
        {
            var container = await _context.MovedContainers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (container == null)
                return NotFound();

            return View(container);
        }
        public async Task<IActionResult> ContainerDetails2(int id)
        {
            var container = await _context.MovedContainers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (container == null)
                return NotFound();

            return View(container);
        }

    }
}
