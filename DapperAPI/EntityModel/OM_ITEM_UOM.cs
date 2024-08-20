


using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class OM_ITEM_UOM : CustomAttributes
    {
        [Key]
        [ForeignKey("OM_ITEM", "ITEM_CODE",typeof(OM_ITEM))]
        [MaxLength(40)]

        

        public string IU_ITEM_CODE { get; set; }

        [MaxLength(6)]

        
        [Key]
        [ForeignKey("OM_UOM", "UOM_CODE",typeof(OM_UOM))]
        public string IU_UOM_CODE { get; set; }

        

        public int IU_MAX_LOOSE { get; set; }

        

        public int IU_UNITS { get; set; }

        [MaxLength(2)]

        

        public string IU_FRZ_FLAG { get; set; }

        

        public DateTime IU_CR_DT { get; set; }

        [MaxLength(20)]

        

        public string IU_CR_UID { get; set; }

        

        public DateTime? IU_UPD_DT { get; set; }

        [MaxLength(20)]

        

        public string? IU_UPD_UID { get; set; }

        

        public double? IU_WIDTH { get; set; }

        

        public double? IU_LENGTH { get; set; }

        

        public double? IU_HEIGHT { get; set; }

        

        public double? IU_CBM { get; set; }

        [MaxLength(2)]

        

        public string? IU_BASE_UOM { get; set; }

        [MaxLength(2)]

        

        public string? IU_RPT_UOM { get; set; }


        
        public long? IU_WEIGHT { get; set; }

        [MaxLength(2)]

        

        public string? IU_HIGH_UOM { get; set; }

        [Key]
        [ForeignKey("OM_ITEM", "ITEM_COMP_CODE",typeof(OM_ITEM))]
        public string IU_COMP_CODE { get; set; }
    }
}
