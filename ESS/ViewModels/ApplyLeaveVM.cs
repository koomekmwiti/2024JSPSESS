using System.ComponentModel.DataAnnotations;

namespace ESS.ViewModels
{
    public class ApplyLeaveVM
    {
        [Display(Name = "ID")]
        public Guid Id { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "Number of Days")]
        public int NumberOfDays { get; set; } = 0;

        [Display(Name = "Reliever")]
        public string RelieverCode { get; set; }
    }
}
