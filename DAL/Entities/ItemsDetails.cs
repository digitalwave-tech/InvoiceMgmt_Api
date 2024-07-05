using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DAL.Entities
{
    public partial class ItemsDetails
    {
        [JsonIgnore]
        public int? invoiceNo { get; set; }
        public string? itemName { get; set; }
        public int? quantity { get; set; }
        public decimal? price { get; set; }
        public decimal? Amount { get; set; }
        [JsonIgnore]
        public virtual Invoices? InvoiceNoNavigation { get; set; }
    }
}
