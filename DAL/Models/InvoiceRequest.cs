using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{

    public class InvoiceRequest
    {
        public Guid? companyId { get; set; }

        public string? customerName { get; set; }
        public decimal? subtotal { get; set; }
        public decimal? gstRate { get; set; }
        public decimal? totalAmt { get; set; }

        public virtual ICollection<ItemsDetails> Details { get; set; }

    }

   

}
