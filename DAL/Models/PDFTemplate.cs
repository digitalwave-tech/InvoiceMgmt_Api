using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{

    public class PDFTemplate
    {
        public Companydetails companyDetails { get; set; }
        //public Customerdetail[] customerdetails { get; set; }
    }

    public class Companydetails
    {
        public Guid? companyid { get; set; }
        public string? companyName { get; set; }
        public string? companyaddress { get; set; }
        public string? companyphoneNo { get; set; }
        public string? companyemail { get; set; }
        public string? companycity { get; set; }
        public string? companystate { get; set; }
        public string? companypincode { get; set; }
        public Customerdetail[] customerdetails { get; set; }
    }

    public class Customerdetail
    {
        public string? customername { get; set; }
        public string? custoomeraddress { get; set; }
        public string? customerphoneNo { get; set; }
        public string? customeremail { get; set; }
        public string? customercity { get; set; }
        public string? customerstate { get; set; }
        public string? customerpincode { get; set; }
        public Itemdetail[] itemDetails { get; set; }
    }

    public class Itemdetail
    {
        public int? invoiceNo { get; set; }
        public string? invocieDate { get; set; }
        public string? itemName { get; set; }
        public int? quantity { get; set; }
        public decimal? price { get; set; }
        public decimal? totalAmount { get; set; }
        public decimal? NetAmount { get; set; }

    }

}
