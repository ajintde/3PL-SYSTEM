using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_USER:CustomAttributes
    {
        [PrimaryKey]
        [Key]
        public string USER_ID { get; set; }
        public string USER_PWD { get; set; }
        [ForeignKey("ADM_GROUP", "GROUP_ID", typeof(ADM_GROUP))]
        public string USER_GROUP_ID { get; set; }
        public string USER_DESC { get; set; }
        public string USER_CHANGE_PWD_FLAG { get; set; } = "Y"; // Default value 'Y'
        public string USER_FRZ_FLAG { get; set; } = "N"; // Default value 'N'
        public string USER_START_MENU_ID { get; set; }
        public string USER_ASK_PASSWD_FLAG { get; set; }
        public string USER_TEL_NO { get; set; }
        public string USER_TEL_EXTN { get; set; }
        public string USER_FAX_NO { get; set; }
        public string USER_CR_UID { get; set; }
        public DateTime USER_CR_DT { get; set; } = DateTime.Now; // Default value using current date/time
        public string USER_UPD_UID { get; set; }
        public DateTime? USER_UPD_DT { get; set; }
        public int USER_SESSION { get; set; } = 0; // Default value 0
        public int USER_RPT_SESSION { get; set; } = 0; // Default value 0
        public string USER_EMAILID { get; set; }
        public DateTime? USER_LAST_PWD_CHG_DT { get; set; }
        public long? USER_EMP_SYS_ID { get; set; }
        public string USER_DIS_CN { get; set; }
        public string USER_LOCK_YN { get; set; } = "N"; // Default value 'N'
        public string USER_TYPE { get; set; }
    }
}
