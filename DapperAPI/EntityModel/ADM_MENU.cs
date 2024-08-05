using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_MENU:CustomAttributes
    {
        [Key]
        [PrimaryKey]
        public int MENU_ID { get; set; }
        public string MENU_SCR_NAME { get; set; }
        public string MENU_ACTION_TYPE { get; set; }
        public string MENU_ACTION { get; set; }
        public int? MENU_PARENT_ID { get; set; }
        public int? MENU_DISP_SEQ_NO { get; set; }
        public string MENU_HELP_TEXT_1 { get; set; }
        public string MENU_HELP_TEXT_2 { get; set; }
        public string MENU_PARAMETER_1 { get; set; }
        public string MENU_PARAMETER_2 { get; set; }
        public string MENU_PARAMETER_3 { get; set; }
        public string MENU_PARAMETER_4 { get; set; }
        public string MENU_PARAMETER_5 { get; set; }
        public string MENU_PARAMETER_6 { get; set; }
        public string MENU_PARAMETER_7 { get; set; }
        public string MENU_PARAMETER_8 { get; set; }
        public string MENU_PARAMETER_9 { get; set; }
        public string MENU_PARAMETER_10 { get; set; }
        public string MENU_CR_UID { get; set; }
        public DateTime MENU_CR_DT { get; set; }
        public string MENU_UPD_UID { get; set; }
        public DateTime? MENU_UPD_DT { get; set; }
        public string MENU_REMARKS { get; set; }
    }
}
