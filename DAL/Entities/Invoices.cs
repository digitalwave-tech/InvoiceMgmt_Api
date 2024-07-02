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
        public string? CustomerName { get; set; }
        [JsonIgnore]
        public DateTime? InvoiceDate { get; set; }
        [JsonIgnore]
        public Guid? CompanyId { get; set; }
        [JsonIgnore]
        public virtual UserAccount? Company { get; set; }

        public virtual ICollection<ItemsDetails> Details { get; set; } 
    }
}
