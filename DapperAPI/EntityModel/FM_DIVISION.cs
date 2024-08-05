using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class FM_DIVISION : CustomAttributes
    {

        [MaxLength(6)]
        [Display(Name = "DIVN_COMP_CODE")]
        [Key]
        [PrimaryKey]
        public string DIVN_COMP_CODE { get; set; }
        [MaxLength(12)]
        [Display(Name = "DIVN_CODE")]
        [Key]
        [PrimaryKey]
        public string DIVN_CODE { get; set; }
        [MaxLength(60)]
        [Display(Name = "DIVN_NAME")]
        public string DIVN_NAME { get; set; }
        [MaxLength(30)]
        [Display(Name = "DIVN_SHORT_NAME")]
        public string DIVN_SHORT_NAME { get; set; }
        [MaxLength(20)]
        [Display(Name = "DIVN_HEADER")]
        public string DIVN_HEADER { get; set; }
        [MaxLength(30)]
        [Display(Name = "DIVN_INCHARGE")]
        public string? DIVN_INCHARGE { get; set; }
        [MaxLength(2)]
        [Display(Name = "DIVN_FRZ_FLAG")]
        public string DIVN_FRZ_FLAG { get; set; }
        [MaxLength(20)]
        [Display(Name = "DIVN_CR_UID")]
        public string DIVN_CR_UID { get; set; }
        [Display(Name = "DIVN_CR_DT")]
        public DateTime DIVN_CR_DT { get; set; }
        [MaxLength(20)]
        [Display(Name = "DIVN_UPD_UID")]
        public string? DIVN_UPD_UID { get; set; }
        [Display(Name = "DIVN_UPD_DT")]
        public DateTime? DIVN_UPD_DT { get; set; }


        }
    }