using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DAL.Entities
{
    public partial class UserAccount
    {
        public UserAccount()
        {
            Customers = new HashSet<Customers>();
            Invoices = new HashSet<Invoices>();
        }
        [JsonIgnore]
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? States { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? EmailId { get; set; }
        [JsonIgnore]
        public DateTime? CreatedOn { get; set; }
        public string? GstNo { get; set; }
        [JsonIgnore]
        public virtual ICollection<Customers> Customers { get; set; }
        [JsonIgnore]
        public virtual ICollection<Invoices> Invoices { get; set; }
    }
}
