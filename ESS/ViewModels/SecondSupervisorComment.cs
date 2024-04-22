using SupervisorCommentsServiceReference;
using System.ComponentModel.DataAnnotations;

namespace ESS.ViewModels
{
    public class SecondSupervisorComment
    {		
		public int No { get; set; }
		[Display(Name = "Question")]
		public string? Question { get; set; }

		[Display(Name = "key")]
		public string? key { get; set; }

		[Display(Name = "comments_on_Performance")]
		public string? comments_on_Performance { get; set; }

		[Display(Name = "appraisal_No")]
		public string? appraisal_No { get; set; }

		[Display(Name = "person")]
		public Person person { get; set; }

		[Display(Name = "personFieldSpecified")]
		public bool personFieldSpecified { get; set; }

		public string? Table { get; set; }
	}
}
