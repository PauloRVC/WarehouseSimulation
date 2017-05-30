using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Batch
    {
        /// <summary>
        /// The unique key for the batch in the db.
        /// </summary>
        public int BatchID { get; set; }
        /// <summary>
        /// The unique batch indicator used in the production db.
        /// </summary>
        public string BatchIndicator { get; set; }
        
        /// <summary>
        /// The orders that belong to this batch.
        /// </summary>
        public List<Order> Orders { get; set; }
        /// <summary>
        /// The scans that have taken place on this batch.
        /// </summary>
        public List<BatchScan> BatchScans { get; set; }
    }
}
