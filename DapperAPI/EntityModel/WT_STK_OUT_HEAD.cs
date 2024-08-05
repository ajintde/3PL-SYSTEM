using ServiceStack;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static DapperAPI.EntityModel.CustomAttributes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DapperAPI.EntityModel
{
    public class WT_STK_OUT_HEAD
    {
        [PrimaryKey]
        [Key]
        [Required]
        [SequenceKey]
        public long STOH_SYS_ID { get; set; }

        public long? STOH_ERPH_SYS_ID { get; set; }

        public long? STOH_TASK_ID { get; set; }

        public string STOH_COMP_CODE { get; set; }

        public string STOH_DIVN_CODE { get; set; }

        public string STOH_TXN_TYPE { get; set; }

        public string STOH_TXN_CODE { get; set; }
        //[JsonPropertyName("Doc_No")]
        public long STOH_DOC_NO { get; set; }
       
        public DateTime STOH_DOC_DT { get; set; }

        public DateTime? STOH_ACNT_YEAR_MTH { get; set; }

        public string STOH_AR_CODE { get; set; }

        public string STOH_FM_LOCN_CODE { get; set; }

        public string? STOH_FM_LOCN_NAME { get; set; }

        public string? STOH_TO_LOCN_CODE { get; set; }

        public string? STOH_TO_LOCN_NAME { get; set; }

        public string? STOH_CUST_CODE { get; set; }

        public string? STOH_CUST_NAME { get; set; }

        public string? STOH_CUST_COUNTRY { get; set; }

        public string? STOH_CUST_CITY { get; set; }

        public string? STOH_CUST_AREA { get; set; }
        [Column(TypeName ="date")]
        public DateTime? STOH_DEL_DT { get; set; }

        public string? STOH_RES_CODE { get; set; }

        public string? STOH_REMARKS { get; set; }

        public string? STOH_URGENT_YN { get; set; }

        public long? STOH_PRINT_STATUS { get; set; }

        public string STOH_STATUS { get; set; }

        public string STOH_APR_STATUS { get; set; }

        public string STOH_CR_UID { get; set; }

        public DateTime STOH_CR_DT { get; set; }

        public string? STOH_UPD_UID { get; set; }

        public DateTime? STOH_UPD_DT { get; set; }

        public string? STOH_REG_CODE { get; set; }

        public string? STOH_CUST_ADD_1 { get; set; }

        public string? STOH_CUST_ADD_2 { get; set; }

        public string? STOH_CUST_ADD_3 { get; set; }

        public string? STOH_DEL_ADD_1 { get; set; }

        public string? STOH_DEL_ADD_2 { get; set; }

        public string? STOH_DEL_ADD_3 { get; set; }

        public string STOH_ENTITY { get; set; }

        public string? STOH_ERP_TXN_CODE { get; set; }

        public long? STOH_ERP_DOC_NO { get; set; }

        public string? STOH_REF_FROM { get; set; }

        public string? STOH_SM_CODE { get; set; }

        public string? STOH_SKU_TYPE { get; set; }

        public double? STOH_SKU_COST { get; set; }

        public double? STOH_PCK_COST { get; set; }

        public double? STOH_NET_COST { get; set; }

        public long? STOH_OLD_SYS_ID { get; set; }

        public string? STOH_DWLD_YN { get; set; }

        public double? STOH_DISC_PERC { get; set; }

        public double? STOH_FC_DISC_VAL { get; set; }

        public string? STOH_CURR_CODE { get; set; }

        public double? STOH_EXGE_RATE { get; set; }

        public string? STOH_APR_UID { get; set; }

        public string STOH_CHRG_YN { get; set; }

        public DateTime? STOH_APR_DT { get; set; }

        public string? STOH_LPO_NO { get; set; }

        public long? STOH_REF_SYS_ID { get; set; }

        public string? STOH_MUL_TYPE { get; set; }

        public string? STOH_REF_TXN_CODE { get; set; }

        public long? STOH_REF_DOC_NO { get; set; }

        public string? STOH_SIT_CODE { get; set; }

        public long? STOH_AGRMT_ID { get; set; }

        public string? STOH_AGRMT_CODE { get; set; }

        public long? STOH_AGRMT_NO { get; set; }

        public string? STOH_HOLD_STATUS { get; set; }



        [NotMapped]
        public List<WT_STK_OUT_ITEM> WT_STK_OUT_HEAD_WT_STK_OUT_ITEM { get; set; } = new List<WT_STK_OUT_ITEM>();
    }
}
