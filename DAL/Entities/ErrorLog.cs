using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ErrorLog
    {
        [JsonIgnore]
        public int id { get; set; }
        public string? methodName { get; set; }
        public string? errorMsg { get; set; }
        [JsonIgnore]
        public DateTime? loggedDate { get; set; }


    }
}
