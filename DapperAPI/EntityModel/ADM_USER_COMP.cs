using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_USER_COMP :CustomAttributes
    {
        [ForeignKey("ADM_USER", "USER_ID", typeof(ADM_USER))]
        [Key]
        public string UC_USER_ID { get; set; }
        [Key]
        public string UC_COMP_CODE { get; set; }
        public string UC_CR_UID { get; set; }
        public DateTime? UC_CR_DT { get; set; }
        public decimal UC_COMP_DEF { get; set; }
    }
}
