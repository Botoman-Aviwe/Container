namespace PrescribingSystem.Models.Viewmodel
{
    public class DashboardViewModel
    {
        public int TotalLoad { get; set; }
        public int TotalDischarge { get; set; }

        public List<string> Dates { get; set; }
        public List<int> LoadCounts { get; set; }
        public List<int> DischargeCounts { get; set; }

        // 👇 ADD THESE
        public List<COPIED> LoadList { get; set; }
        public List<COPIED> DischargeList { get; set; }
    }
}
