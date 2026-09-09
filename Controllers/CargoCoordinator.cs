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
    public class CargoCoordinator : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly AccessDbService _accessDb;

        // ✅ Constructor Injection
        public CargoCoordinator(ApplicationDbContext context, AccessDbService accessDb,
    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _accessDb = accessDb;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var loads = _context.COPIED.Where(x => x.KindMove == "Load").ToList();
            var discharge = _context.COPIED.Where(x => x.KindMove == "Discharge").ToList();

            var model = new DashboardViewModel
            {
                TotalLoad = loads.Count,
                TotalDischarge = discharge.Count,

                Dates = _context.COPIED
                    .Select(x => x.CreatedDate.ToString("yyyy-MM-dd"))
                    .Distinct()
                    .ToList(),

                LoadCounts = loads
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(g => g.Count())
                    .ToList(),

                DischargeCounts = discharge
                    .GroupBy(x => x.CreatedDate.Date)
                    .Select(g => g.Count())
                    .ToList(),

                // 👇 ADD THIS
                LoadList = loads,
                DischargeList = discharge
            };

            return View(model);
        }
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
        // 📦 CONTAINER TRACKING
        // ============================================
        public async Task<IActionResult> Tracking(string search)
        {
            var query = _context.COPIED
                .Where(x => x.KindMove == "Load");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Container.Contains(search));
            }

            var result = await query
                .OrderBy(x => x.Sequence)
                .ToListAsync();

            return View(result);
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




