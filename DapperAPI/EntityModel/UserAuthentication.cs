using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DapperAPI.EntityModel
{
    public class UserLogin
    {
        public string userId { get; set; }
        public string Password { get; set; }

        
    }

    public class authToken
    {
        public string accessToken { get; set; }
        public string RefreshToken { get; set; }


    }

    public class Tokens
    {
        [AllowNull]
        public string? AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public List<COMP_ACCESS> CompList { get; set; }
    }



    public class COMP_ACCESS
    {

        public string COMP_CODE { get; set; }
        public string COMP_NAME { get; set; }
    }

    public class UserRefreshTokens
    {
    

        [Required]
        public string UserName { get; set; }

        [Required]
        public string RefreshToken { get; set; }

    }


    public class TOCKEN_REFRESH
    {


        [Required]
        public string TK_USER_ID { get; set; }

        [Required]
        public string TK_REF_TOKEN { get; set; }

        public string TK_ACTIVE { get; set; }

    }


}
