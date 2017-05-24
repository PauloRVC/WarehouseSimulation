using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Location
    {
        /// <summary>
        /// The unique ID for this location in the DB.
        /// </summary>
        public int LocationID { get; set; }
        /// <summary>
        /// How this location is referred to in the production DB.
        /// </summary>
        public string ScannerIndicator { get; set; }
        /// <summary>
        /// The batch scans taken place at this location.
        /// </summary>
        public List<BatchScan> CurrentLocationBatchScans { get; set; }
        /// <summary>
        /// The batch scans where this location was the intended destination.
        /// </summary>
        public List<BatchScan> IntendedDestinationBatchScans { get; set; }
        /// <summary>
        /// The batch scans where this location was the actual destination.
        /// </summary>
        public List<BatchScan> ActualDestinationBatchScans { get; set; }
    }
}
