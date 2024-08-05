

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DapperAPI.EntityModel.CustomAttributes;

namespace DapperAPI.EntityModel
{
    public class OM_ITEM : BaseEntity
    {
        //public override string StoreProcedureName => "SP_ITEM";
        [MaxLength(40)]

        [Display(Name = "ITEM_CODE")]
        [Key]
        [PrimaryKey]
        [SequenceKey]
        public string ITEM_CODE { get; set; }

        [MaxLength(120)]


        
        public string ITEM_NAME { get; set; }

        [MaxLength(60)]

        

        public string ITEM_SHORT_NAME { get; set; }

        [MaxLength(4000)]

        

        public string? ITEM_LONG_NAME_1 { get; set; }

        [MaxLength(4000)]

        

        public string? ITEM_LONG_NAME_2 { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_IG_CODE { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_ISG_CODE { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_IT_CODE { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_IST_CODE { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_EQMT_CODE { get; set; }

        [MaxLength(10)]

        

        public string? ITEM_SEQMT_CODE { get; set; }

        [MaxLength(10)]

        

        public string ITEM_MAKE_CODE { get; set; }

        [MaxLength(6)]

        

        public string? ITEM_UOM_CODE { get; set; }

        

        public string ITEM_CATEGORY { get; set; }

        

        public short? ITEM_SHELF_MTH { get; set; }

        [MaxLength(2)]

        

        public string ITEM_ABC { get; set; }

        [MaxLength(2)]

        

        public string ITEM_VED { get; set; }

        

        public int? ITEM_MIN_STK { get; set; }

        

        public int? ITEM_MAX_STK { get; set; }

        

        public int? ITEM_RORD_LVL { get; set; }

        

        public int? ITEM_RORD_QTY { get; set; }

        

        public short? ITEM_LEAD_TIME { get; set; }

        

        public double? ITEM_GROSS_WT { get; set; }

        

        public double? ITEM_NET_WT { get; set; }

        

        public double? ITEM_VOL { get; set; }

        

        public double? ITEM_PUR_PRICE { get; set; }

        [MaxLength(2)]

        

        public string ITEM_BATCH_YN { get; set; }

        [MaxLength(2)]

        

        public string ITEM_SUPERSEDED_YN { get; set; }

        [MaxLength(2)]

        

        public string ITEM_SNO_YN { get; set; }

        [MaxLength(2)]

        

        public string ITEM_FRZ_FLAG { get; set; }

        

        public DateTime ITEM_CR_DT { get; set; }

        [MaxLength(20)]

        

        public string ITEM_CR_UID { get; set; }

        

        public DateTime? ITEM_UPD_DT { get; set; }

        [MaxLength(20)]

        

        public string? ITEM_UPD_UID { get; set; }

        

        public string ITEM_MULTI_FLAG { get; set; }

        [MaxLength(2)]

        

        public string ITEM_PROD_YN { get; set; }

        

        public double? ITEM_TEMP_MIN { get; set; }

        

        public double? ITEM_TEMP_MAX { get; set; }

        [MaxLength(12)]

        

        public string? ITEM_SEL_TYPE { get; set; }

        [MaxLength(2)]

        

        public string? ITEM_TRANS_YN { get; set; }

        [MaxLength(160)]

        

        public string? ITEM_PARENT_NAME { get; set; }

        [MaxLength(30)]

        

        public string ITEM_STORE_TYPE { get; set; }

        [MaxLength(40)]

        

        public string? ITEM_NTDE_CODE { get; set; }
        [PrimaryKey]
        [Key]
        public string ITEM_COMP_CODE {  get; set; }

        [NotMapped]
        public List<OM_ITEM_UOM> OM_ITEM_OM_ITEM_UOM { get; set;}= new List<OM_ITEM_UOM>();

        
    }
}
