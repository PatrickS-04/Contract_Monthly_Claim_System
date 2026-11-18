namespace PROG6212PoE_ST10440987.Models
{
    public class ReportSummaryViewModel
    {
        public int TotalHoursWorked { get; set; }
        public double TotalAmountDue { get; set; }
        public string ReportDate { get; set; }

        public List<LecturerReportSummary> lecturerReportSummaries { get; set; } = new List<LecturerReportSummary>();
    }

    public class LecturerReportSummary
    {
        public string LecturerId { get; set; }
        public string LecturerName { get; set; }
        public string LecturerEmail { get; set; }
        public double HourlyRate { get; set; }

        public double OverallHoursWorked { get; set; }
        public double OverallAmountDue { get; set; }

        public List<ApprovedClaim> ApprovedClaims { get; set; }
    }

    public class ApprovedClaim
    {
        public int ClaimId { get; set; }
        public DateTime dateOfClaim { get; set; }
        public int HoursWorked { get; set; }
        public double totalForClaim { get; set; }
    }
}
