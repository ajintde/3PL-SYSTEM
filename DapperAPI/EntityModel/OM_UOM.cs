



using ServiceStack.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    
    public class OM_UOM:CustomAttributes
    {
        [PrimaryKey]
        [Key]
        public string UOM_CODE { get; set; }
        public string UOM_NAME { get; set; }
        public string UOM_SHORT_NAME { get; set; }
        [DefaultValue("N")]
        public string UOM_FRZ_FLAG { get; set; }
        //[DefaultValue(DateTime.Now)]
        public DateTime UOM_CR_DT {  get; set; }
        [DefaultValue("API")]
        public string UOM_CR_UID { get; set; }
        public DateTime? UOM_UPD_DT { get; set; }
        public string? UOM_UPD_UID { get; set; }

        //public override string StoreProcedureName => "SP_UOM";

        ////    public override string inserSqlTemplate => $"INSERT INTO OM_UOM(UOM_CODE, UOM_NAME, UOM_SHORT_NAME, UOM_FRZ_FLAG, UOM_CR_DT, UOM_CR_UID, UOM_MOD_DT, UOM_MOD_UID) " +
        ////$"VALUES(@UOM_CODE, @UOM_NAME, @UOM_SHORT_NAME, @UOM_FRZ_FLAG, @UOM_CR_DT, @UOM_CR_UID, @UOM_MOD_DT, @UOM_MOD_UID); "; //+
        //////$"SELECT CAST(SCOPE_IDENTITY() AS int);";

        //public override string updateSqlTemplate => "";

        //public override string tableName => "";

        //public override string primaryColumnName => "";

        //public override string inserSqlTemplate => "";

        //"SP_UOM";
    }
}
