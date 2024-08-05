namespace DapperAPI.EntityModel
{



    public class ExcelImport
    {
        public int Total_rows { get; set; }
        public int Upload_rows { get; set; }
        public int Error_rows { get; set; }
        public List <ImportErr> errRaw { get; set; }

    }

    public class ImportErr
    {
        
        public int Error_rownum { get; set; }
        public string ErrorMessage { get; set; }
    }
}
