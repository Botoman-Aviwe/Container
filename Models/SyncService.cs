using PrescribingSystem.Data;
using System;

namespace PrescribingSystem.Models
{
    public class SyncService
    {
        private readonly ApplicationDbContext _context;

        public SyncService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SyncData()
        {
            // Example: simulate pulling from external DB
            var newData = new List<COPIED>
        {
            new COPIED
            {
                Sequence = 1,
                Container = "CONT123",
                KindMove = "Load",
                FromLocation = "A",
                ToLocation = "B",
                CreatedDate = DateTime.Now
            }
        };

            _context.COPIED.AddRange(newData);
            _context.SaveChanges();
        }
    }
}
