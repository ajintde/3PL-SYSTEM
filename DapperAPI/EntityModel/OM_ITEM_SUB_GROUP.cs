using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class OM_ITEM_SUB_GROUP : CustomAttributes
    {
        [MaxLength(10)]
        [Display(Name = "ISG_CODE")]
        [Key]
        [PrimaryKey]
        public string ISG_CODE { get; set; }
        [MaxLength(10)]
        [Display(Name = "ISG_IG_CODE")]
        [Key]
        [ForeignKey("OM_ITEM_GROUP", "IG_CODE", typeof(OM_ITEM_GROUP))]
        public string ISG_IG_CODE { get; set; }
        [MaxLength(60)]
        [Display(Name = "ISG_NAME")]
        public string ISG_NAME { get; set; }
        [MaxLength(30)]
        [Display(Name = "ISG_SHORT_NAME")]
        public string ISG_SHORT_NAME { get; set; }
        [Display(Name = "ISG_FRZ_FLAG")]
        public string ISG_FRZ_FLAG { get; set; }
        [Display(Name = "ISG_CR_DT")]
        public DateTime ISG_CR_DT { get; set; }
        [MaxLength(20)]
        [Display(Name = "ISG_CR_UID")]
        public string ISG_CR_UID { get; set; }
        [MaxLength(20)]
        [Display(Name = "ISG_UPD_UID")]
        public string? ISG_UPD_UID { get; set; }
        [Display(Name = "ISG_UPD_DT")]
        public DateTime? ISG_UPD_DT { get; set; }

    }
}