using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_GROUP_MENU_COMP:CustomAttributes
    {
        [Key]
        [ForeignKey("ADM_GROUP", "GROUP_ID", typeof(ADM_GROUP))]
        public int GMC_GROUP_ID { get; set; }
        [Key]
        [ForeignKey("ADM_MENU", "MENU_ID", typeof(ADM_MENU))]
        public int GMC_MENU_ID { get; set; }
        [Key]
        public string GMC_COMP_CODE { get; set; }
        public string GMC_CR_UID { get; set; }
        public DateTime GMC_CR_DT { get; set; }
        public string GMC_UPD_UID { get; set; }
        public DateTime? GMC_UPD_DT { get; set; }
    }
}
