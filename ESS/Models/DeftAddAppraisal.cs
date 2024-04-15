using System.ComponentModel.DataAnnotations;

namespace ESS.Models
{
    public class DeftAddAppraisal
    {
        [Display(Name = "ID")]
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Employer Code")]
        public string EmpCode { get; set; } = string.Empty;

        [Display(Name = "Appraisal No")]
        public string Appraisal_No { get; set; } = string.Empty;

        [Display(Name = "Appraisal Period")]
        public string? Appraisal_Period { get; set; } = string.Empty;

        [Display(Name = "Appraisal Type")]
        public string? AppraisalType { get; set; } = string.Empty;

        [Display(Name = "Is Used")]
        public bool IsUsed { get; set; } = false;

        [Display(Name = "Date Created")]
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
