using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class BatchScan
    {
        /// <summary>
        /// The unique ID of the batch being scanned in the db.
        /// </summary>
        public int BatchID { get; set; }
        /// <summary>
        /// The batch being scanned.
        /// </summary>
        public Batch Batch { get; set; }
        /// <summary>
        /// The unique ID of the location of the batch when scannned.
        /// </summary>
        public int CurrentLocationID { get; set; }
        /// <summary>
        /// The location of the batch when scanned.
        /// </summary>
        public Location CurrentLocation { get; set; }
        /// <summary>
        /// The unique ID of the intended destination of the batch.
        /// </summary>
        public int IntendedDestinationID { get; set; }
        /// <summary>
        /// The intended destination of the batch.
        /// </summary>
        public Location IntendedDestination { get; set; }
        /// <summary>
        /// The unique ID of the actual destination of the batch(where it was actually routed).
        /// </summary>
        public int ActualDestinationID { get; set; }
        /// <summary>
        /// The actual destination of the batch(where it was actually routed).
        /// </summary>
        public Location ActualDestination { get; set; }
        /// <summary>
        /// The time of the scan.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
