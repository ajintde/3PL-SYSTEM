using ServiceStack;
using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DapperAPI.EntityModel
{
    public class WT_STK_OUT_ITEM : CustomAttributes
    {
        [Key]
        [PrimaryKey]
        [SequenceKey]
        public long STOI_SYS_ID { get; set; }
        [ForeignKey("WT_STK_OUT_HEAD", "STOH_SYS_ID", typeof(WT_STK_OUT_HEAD))]
        public long STOI_STOH_SYS_ID { get; set; }

        public long? STOI_ERPI_SYS_ID { get; set; }

        public long? STOI_TSK_SYS_ID { get; set; }

        public string STOI_ITEM_CODE { get; set; }

        public string? STOI_UOM_CODE { get; set; }

        public long STOI_QTY { get; set; }

        public long STOI_QTY_LS { get; set; }

        public long STOI_QTY_BU { get; set; }

        public long? STOI_JOBI_QTY_BU { get; set; }

        public long? STOI_STRI_QTY_BU { get; set; }

        public double? STOI_RATE { get; set; }

        public double? STOI_DISC_PERC { get; set; }

        public double? STOI_FC_DISC_VAL { get; set; }

        public double? STOI_FC_VAL { get; set; }

        public string? STOI_FOC_FLAG { get; set; }

        public string? STOI_RES_CODE { get; set; }

        public DateTime? STOI_PREF_EXP_DT { get; set; }

        public string? STOI_ITEM_DESC_1 { get; set; }

        public string? STOI_ITEM_DESC_2 { get; set; }

        public string? STOI_ITEM_DESC_3 { get; set; }

        public string? STOI_REMARKS { get; set; }

        public string? STOI_PCK_STATUS_EDITED { get; set; }

        public string STOI_CR_UID { get; set; }

        public DateTime STOI_CR_DT { get; set; }

        public string? STOI_UPD_UID { get; set; }

        public DateTime? STOI_UPD_DT { get; set; }

        public long? STOI_PREF_SHLF_LIFE { get; set; }

        public DateTime? STOI_FNSH_DT { get; set; }

        public string? STOI_MUL_ITEM_CODE { get; set; }

        public long? STOI_WII_QTY { get; set; }

        public long? STOI_SHIP_QTY_BU { get; set; }

        public string? STOI_PICK_TYPE { get; set; }

        


    }
}
