using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPTAPIClient.Models
{
    public record OEEReportViewModel
    {
        public long SectionId { get; set; }
        public string Section { get; set; }
        public string Building { get; set; }
        public string Machine { get; set; }
        public string ChildMachine { get; set; }
        public long ChildMachineId { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Availability { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Performance { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Quality { get; set; }
        [Column(TypeName = "decimal(18, 4)")]
        public decimal OEE { get; set; }
        public DateTime? ReportDate { get; set; }
        public string Remarks { get; set; }
        public string SmartorManual { get; set; }
        public long MachineTypeId { get; set; }
    }
}
