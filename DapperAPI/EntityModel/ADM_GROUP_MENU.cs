using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_GROUP_MENU:CustomAttributes
    {
        [Key]
        [ForeignKey("ADM_MENU", "MENU_ID",typeof(ADM_MENU))]
        public int GM_MENU_ID { get; set; }
        [Key]
        [ForeignKey("ADM_GROUP", "GROUP_ID",typeof(ADM_GROUP))]
        public int GM_GROUP_ID { get; set; }
        public bool GM_ALLOW_INST { get; set; }
        public bool GM_ALLOW_UPDT { get; set; }
        public bool GM_ALLOW_DELT { get; set; }
        public bool GM_ALLOW_VIEW { get; set; }
        public bool GM_ALLOW_PRNT { get; set; }
        public bool GM_ALLOW_MAIL { get; set; }
        public bool GM_ALLOW_EXPT { get; set; }
        public string GM_CR_UID { get; set; }
        public DateTime GM_CR_DT { get; set; }
        public string GM_UPD_UID { get; set; }
        public DateTime? GM_UPD_DT { get; set; }
    }
}
