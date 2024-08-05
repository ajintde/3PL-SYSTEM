namespace DapperAPI.EntityModel
{
    public class SearchRequest
    {
        public string? JsonModel { get; set; }
        public string? SortBy { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public string? CompanyCode { get; set; }
        public string User { get; set; }
        public string? WhereClause { get; set; }

        public string? ShowDetail { get; set; }
    }
}
