using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PrescribingSystem.Data;
using PrescribingSystem.Models;
using PrescribingSystem.Models.Viewmodel;
using PrescribingSystem.Service;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json;
using Document = QuestPDF.Fluent.Document;


namespace PrescribingSystem.Controllers
{
    [Authorize(Roles = "CargoCoordinator")]
    public class CargoCoordinator : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly AccessDbService _accessDb;
        private readonly ILogger<CargoCoordinator> _logger;

        // ✅ Constructor Injection
        public CargoCoordinator(ApplicationDbContext context, AccessDbService accessDb,
    UserManager<ApplicationUser> userManager, ILogger<CargoCoordinator> logger)
        {
            _context = context;
            _accessDb = accessDb;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var copiedData = _context.COPIED
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            var loads = copiedData
                .Where(x => x.KindMove == "Load")
                .ToList();

            var discharge = copiedData
                .Where(x => x.KindMove == "Discharge")
                .ToList();

            var model = new DashboardViewModel
            {
                TotalLoad = loads.Count,
                TotalDischarge = discharge.Count,

                CopiedList = copiedData,

                LoadList = loads,

                DischargeList = discharge,

                Dates = copiedData
                    .Select(x => x.CreatedDate.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToList(),

                LoadCounts = loads
                    .GroupBy(x => x.CreatedDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),

                DischargeCounts = discharge
                    .GroupBy(x => x.CreatedDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList()
            };

            return View(model);
        }
        //public IActionResult Index()
        //{
        //    var loads = _context.COPIED.Where(x => x.KindMove == "Load").ToList();
        //    var discharge = _context.COPIED.Where(x => x.KindMove == "Discharge").ToList();

        //    var model = new DashboardViewModel
        //    {
        //        TotalLoad = loads.Count,
        //        TotalDischarge = discharge.Count,

        //        Dates = _context.COPIED
        //            .Select(x => x.CreatedDate.ToString("yyyy-MM-dd"))
        //            .Distinct()
        //            .ToList(),

        //        LoadCounts = loads
        //            .GroupBy(x => x.CreatedDate.Date)
        //            .Select(g => g.Count())
        //            .ToList(),

        //        DischargeCounts = discharge
        //            .GroupBy(x => x.CreatedDate.Date)
        //            .Select(g => g.Count())
        //            .ToList(),

        //        // 👇 ADD THIS
        //        LoadList = loads,
        //        DischargeList = discharge
        //    };

        //    return View(model);
        //}
        public async Task<IActionResult> CargoView()
        {
            var loads = await _context.COPIED
                .Where(x => x.KindMove == "Load")
                .OrderBy(x => x.Sequence)
                .ToListAsync();

            return View(loads);
        }
      
        
     
        public IActionResult Dashboard()
        {
            var loads = _context.COPIED
                .Where(x => x.KindMove == "Load")
                .ToList();

            var model = new DashboardViewModel
            {
                TotalLoad = loads.Count,

                Dates = loads
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(g => g.Key.ToString("yyyy-MM-dd"))
                    .ToList(),

                LoadCounts = loads
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(g => g.Count())
                    .ToList(),

                LoadList = loads
            };

            return View(model);
        }
        // ============================================
        // 🚚 MOVE CONTAINER TO VESSEL (Hatchman Action)
        // ============================================

        [HttpPost]
        public async Task<IActionResult> MoveContainer(int id)
        {
            var container = await _context.COPIED.FindAsync(id);

            if (container == null)
                return NotFound();

            // Mark as moved (toggle logic)
            container.KindMove = "Moved";

            // OPTIONAL: Add to another table (archive/history)
            var movedContainer = new MovedContainer
            {
                Sequence = container.Sequence,
                Container = container.Container,
                FromLocation = container.FromLocation,
                ToLocation = container.ToLocation,
                CreatedDate = DateTime.Now
            };

            _context.MovedContainers.Add(movedContainer);

            // Remove from current table (optional depending on your logic)
            _context.COPIED.Remove(container);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CargoView));
        }
        public async Task<IActionResult> MovedContainers()
        {
            var moved = await _context.MovedContainers
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(moved);
        }

        [Authorize(Roles = "Hatchman")]
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

            // Do not overwrite the original received timestamp.
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


        // ============================================
        // 📊 REPORTS
        // ============================================
        public IActionResult Reports()
        {
            var loads = _context.COPIED
                .Where(x => x.KindMove == "Load")
                .ToList();

            var report = loads
                .GroupBy(x => x.ToLocation)
                .Select(g => new
                {
                    Location = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return View(report);
        }

       


    }

}




