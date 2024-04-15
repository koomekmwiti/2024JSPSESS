using System.ComponentModel.DataAnnotations;

namespace ESS.ViewModels
{
	public class SelectLeaveTypeVM
	{
		[Display(Name = "ID")]
		public Guid Id { get; set; }

		[Display(Name = "Leave Type")]
		public string LeaveType { get; set; }

    }
}
