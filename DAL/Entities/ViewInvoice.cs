using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ViewInvoice
    {
        public DateTime? InvoiceDate { get; set; }
        public string? CustomerName { get; set; }
        public int? InvoiceNo { get; set; }
        public decimal? NetAmt { get; set; }
    }
    public class ViewInvoiceList
    {
        public List<ViewInvoice> viewInvoices { get; set; }

    }
}
