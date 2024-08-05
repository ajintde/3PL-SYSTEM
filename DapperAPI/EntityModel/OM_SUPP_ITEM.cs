using System.ComponentModel.DataAnnotations;


namespace DapperAPI.EntityModel
{
   

    public class OM_SUPP_ITEM : CustomAttributes
    {
        [MaxLength(6)]
        [Display(Name = "SI_COMP_CODE")]
        [ForeignKey("OM_ITEM", "ITEM_COMP_CODE", typeof(OM_ITEM))]
        public string SI_COMP_CODE { get; set; }
        [MaxLength(12)]
        [Display(Name = "SI_SUPP_CODE")]
        [Key]
        [PrimaryKey]
        public string SI_SUPP_CODE { get; set; }
        [MaxLength(40)]
        [Display(Name = "SI_SUPP_ITEM_CODE")]
        public string? SI_SUPP_ITEM_CODE { get; set; }
        [MaxLength(40)]
        [Display(Name = "SI_ITEM_CODE")]
        [Key]
        [ForeignKey("OM_ITEM", "ITEM_CODE", typeof(OM_ITEM))]
        public string SI_ITEM_CODE { get; set; }
        [Display(Name = "SI_PERC")]
        public double? SI_PERC { get; set; }
        [Display(Name = "SI_FRZ_FLAG")]
        public string SI_FRZ_FLAG { get; set; }
        [Display(Name = "SI_CR_DT")]
        public DateTime SI_CR_DT { get; set; }
        [MaxLength(20)]
        [Display(Name = "SI_CR_UID")]
        public string SI_CR_UID { get; set; }
        [Display(Name = "SI_UPD_DT")]
        public DateTime? SI_UPD_DT { get; set; }
        [MaxLength(20)]
        [Display(Name = "SI_UPD_UID")]
        public string? SI_UPD_UID { get; set; }
        [MaxLength(40)]
        [Display(Name = "SI_SUPP_BAR_CODE")]
        public string? SI_SUPP_BAR_CODE { get; set; }
    }

}
