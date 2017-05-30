using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class OrderItem
    {
        /// <summary>
        /// The unique ID for the Order corresponding to this order-item relation.
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// The order corresponding to this order-item relation.
        /// </summary>
        public Order Order { get; set; }
        /// <summary>
        /// The unique ID for the item corresponding to this order-item relation.
        /// </summary>
        public int ItemID { get; set; }
        /// <summary>
        /// The item corresponding to this order-item relation.
        /// </summary>
        public Item Item { get; set; }
        /// <summary>
        /// The time which this item belonging to this order was picked.
        /// </summary>
        public DateTime? PickTimestamp { get; set; }
        /// <summary>
        /// The time which this item belonging to this order was put.
        /// </summary>
        public DateTime? PutTimestamp { get; set; }
    }
}
