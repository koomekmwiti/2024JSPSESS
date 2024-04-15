using System.ComponentModel.DataAnnotations;

namespace ESS.ViewModels
{
    public class AppraisalCardVM
    {
        [Display(Name = "Appraisal Period")]
        public string Appraisal_Period { get; set; }

        [Display(Name = "Appraisal Type")]
        public string AppraisalType { get; set; }

    }
}
