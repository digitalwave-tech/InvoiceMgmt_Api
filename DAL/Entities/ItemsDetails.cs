using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DAL.Entities
{
    public partial class ItemsDetails
    {
        [JsonIgnore]
        public int? InvoiceNo { get; set; }
        public string? ItemName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? GrossAmt { get; set; }
        public decimal? GSTRate { get; set; }
        [JsonIgnore]
        public decimal? NetAmt { get; set; }


        [JsonIgnore]
        public virtual Invoices? InvoiceNoNavigation { get; set; }
    }
}
