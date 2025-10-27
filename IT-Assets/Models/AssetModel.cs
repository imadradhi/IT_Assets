using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT_Assets.Models
{
    public class AssetModel
    {
        public string Id { get; set; }     // Firebase key
        public string Code { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public string ReceiptForm { get; set; }
        public string UpdatedBy { get; set; }
        public string Note { get; set; } 
    }
}

