namespace DapperAPI.EntityModel
{
    public class CommonResponse<T>
    {
        public bool ValidationSuccess { get; set; }

        public string? StatusCode { get; set; }
        public string SuccessString { get; set; }
        public string ErrorString { get; set; }
        public T ReturnCompleteRow { get; set; }
        public CommonResponse()
        {
            ValidationSuccess = true;
            StatusCode = "200";
        }
    }
}
