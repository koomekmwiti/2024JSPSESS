using System.ComponentModel.DataAnnotations;

namespace ESS.Models
{
	public class DeftAddLeave
	{
		[Display(Name = "ID")]
        [Key]
        public Guid Id { get; set; }

		[Display(Name = "Employer Code")]
		public string EmpCode { get; set; } = string.Empty;

        [Display(Name = "Application No")]
        public string ApplicationNo { get; set; } = string.Empty;

        [Display(Name = "Leave Type")]
		public string? LeaveType { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
		public DateTime? StartDate { get; set; }

		[Display(Name = "Number of Days")]
		public int? NumberOfDays { get; set; } = 0;

        [Display(Name = "Is Used")]
        public bool IsUsed { get; set; } = false;

        [Display(Name = "Date Created")]
        public DateTime Created {  get; set; } = DateTime.Now;
	}
}
