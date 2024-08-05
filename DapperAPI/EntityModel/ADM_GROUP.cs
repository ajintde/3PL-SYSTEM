using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace DapperAPI.EntityModel
{
    public class ADM_GROUP
    {
        [Key]
        [PrimaryKey]
        public int GROUP_ID { get; set; }
        public string GROUP_DESC { get; set; }
        public string GROUP_CR_UID { get; set; }
        public DateTime GROUP_CR_DT { get; set; }
        public string GROUP_UPD_UID { get; set; }
        public DateTime? GROUP_UPD_DT { get; set; }
        public string GROUP_TYPE { get; set; }
    }
}
