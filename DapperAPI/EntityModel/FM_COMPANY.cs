using ServiceStack.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class FM_COMPANY
    {
        [PrimaryKey]
        public string COMP_CODE { get; set; }
        public string COMP_NAME { get; set; }
        public string COMP_SHORT_NAME { get; set; }
        public string COMP_HEADER { get; set; }
        public string COMP_ADD_1 { get; set; }
        public string COMP_ADD_2 { get; set; }
        public string COMP_ADD_3 { get; set; }
        public string COMP_FRZ_FLAG { get; set; }
        public string COMP_CR_UID { get; set; }
        public DateTime? COMP_CR_DT { get; set; }
        public string COMP_UPD_UID { get; set; }
        public DateTime? COMP_UPD_DT { get; set; }
        public byte[]? COMP_LOGO { get; set; }
        public string COMP_TYPE { get; set; }
        public string COMP_TAX_REG_NO { get; set; }
    }
}
