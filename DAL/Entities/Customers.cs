using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DAL.Entities
{
    public partial class Customers
    {
        public Customers()
        {
            Invoices = new HashSet<Invoices>();
        }
        [JsonIgnore]
        public int CustomerId { get; set; }
        [JsonIgnore]
        public Guid? CompanyId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? States { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? EmailId { get; set; }
        [JsonIgnore]
        public DateTime? CreatedOn { get; set; }
        [JsonIgnore]
        public virtual UserAccount? Company { get; set; }
        [JsonIgnore]
        public virtual ICollection<Invoices> Invoices { get; set; }
    }
}
