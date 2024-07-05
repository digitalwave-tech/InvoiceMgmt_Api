using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DAL.Entities
{
    public partial class Invoices
    {
        [JsonIgnore]
        public int? InvoiceNo { get; set; }
        [JsonIgnore]
        public Guid? CompanyId { get; set; }
        [JsonIgnore]
        public int? CustomerId { get; set; }
        [JsonIgnore]
        public DateTime? InvoiceDate { get; set; }
        [JsonIgnore]
        public string? CustomerName {  get; set; }
        public decimal? SubTotal { get; set; }
        public decimal GstRate { get; set; }
        public decimal TotalAmt { get; set; }   
        [JsonIgnore]
        public virtual UserAccount? Company { get; set; }
        [JsonIgnore]
        public virtual Customers? Customers { get; set; }

        public virtual ICollection<ItemsDetails> Details { get; set; } 
    }
}
