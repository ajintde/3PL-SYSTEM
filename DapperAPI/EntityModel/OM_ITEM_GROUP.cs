


using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class OM_ITEM_GROUP : CustomAttributes
    {
 
            [MaxLength(10)]
            [Display(Name = "IG_CODE")]
            [Key]
            [PrimaryKey]
            public string IG_CODE { get; set; }
            [MaxLength(60)]
            [Display(Name = "IG_NAME")]
            public string IG_NAME { get; set; }
            [MaxLength(30)]
            [Display(Name = "IG_SHORT_NAME")]
            public string IG_SHORT_NAME { get; set; }
            [MaxLength(6)]
            [Display(Name = "IG_COMP_CODE")]
            [ForeignKey("FM_DIVISION", "DIVN_COMP_CODE", typeof(FM_DIVISION))]
            public string? IG_COMP_CODE { get; set; }
            [MaxLength(12)]
            [Display(Name = "IG_DIVN_CODE")]
            [ForeignKey("FM_DIVISION", "DIVN_CODE", typeof(FM_DIVISION))]
            public string? IG_DIVN_CODE { get; set; }
            [Display(Name = "IG_FRZ_FLAG")]
            public string IG_FRZ_FLAG { get; set; }
            [Display(Name = "IG_CR_DT")]
            public DateTime IG_CR_DT { get; set; }
            [MaxLength(20)]
            [Display(Name = "IG_CR_UID")]
            public string IG_CR_UID { get; set; }
            [MaxLength(20)]
            [Display(Name = "IG_UPD_UID")]
            public string? IG_UPD_UID { get; set; }
            [Display(Name = "IG_UPD_DT")]
            public DateTime? IG_UPD_DT { get; set; }
       
    }
}
