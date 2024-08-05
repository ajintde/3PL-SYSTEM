using ServiceStack;
using System.Diagnostics.CodeAnalysis;

namespace DapperAPI.EntityModel
{

    public class CompanyLookup
    {
        
        public string? COMP_CODE { get; set; }
        
        public string? COMP_NAME { get; set; }
        
        public string? strAdd { get; set; }
        
        public string? sortby { get; set; }
        public string userId { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }


    }
    public class ItemLookup
    {
        public string? ITEM_CODE { get; set; }

        public string? ITEM_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }


    }


    public class ItemGroupLookup
    {
        public string? IG_CODE { get; set; }

        public string? IG_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }

    }



    public class ItemSubGroupLookup
    {
        public string? ISG_CODE { get; set; }

        public string? ISG_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }

    }

    public class CommonLookup
    {
        public string? CM_CODE { get; set; }

        public string? CM_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }

    }

   

    public class ItemUomLookup
    {

        public string? UOM_CODE { get; set; }

        public string? UOM_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }

    }

    public class SpplierLookup
    {

        public string? SUPP_CODE { get; set; }

        public string? SUPP_NAME { get; set; }

        public string? strAdd { get; set; }

        public string? sortby { get; set; }
        public int pageNum { get; set; }
        public int pageSize { get; set; }

    }
}
