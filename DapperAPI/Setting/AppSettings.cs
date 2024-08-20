namespace DapperAPI.Setting
{
    public class AppSettings
    {
        public StatusCodes StatusCodes { get; set; }
        public SuccessStrings SuccessStrings { get; set; }
    }

    public class StatusCodes
    {
        public string Success { get; set; }
        public string Created { get; set; }
        public string NotFound { get; set; }
        public string Error { get; set; }
    }

    public class SuccessStrings
    {
        public string UpdateSuccess { get; set; }
        public string UpdateNotFound { get; set; }
        public string PrimaryKeyError { get; set; }
        public string PrimaryKeyNotFound { get; set; }
        public string UpdateDateNotFound { get; set; }
        public string CompNotFound { get; set; }

        public string NoData { get; set; }
        public string InsertFail { get; set; }
        public string InsertSuccess { get; set; }
        public string DeleteSuccess { get; set; }
        public string DeleteNotFound { get; set; }
    }
}
