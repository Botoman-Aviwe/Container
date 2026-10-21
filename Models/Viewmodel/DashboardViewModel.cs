namespace PrescribingSystem.Models.Viewmodel
{
    public class DashboardViewModel
    {
        public int TotalLoad { get; set; }

        public int TotalDischarge { get; set; }

        public List<string> Dates { get; set; } = new();

        public List<int> LoadCounts { get; set; } = new();

        public List<int> DischargeCounts { get; set; } = new();

        public List<COPIED> LoadList { get; set; } = new();

        public List<COPIED> DischargeList { get; set; } = new();

        public List<COPIED> CopiedList { get; set; } = new();
    }
}
