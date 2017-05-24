using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Order
    {
        /// <summary>
        /// The unique ID for this order used in the db.
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// The unique ID for the batch which this order belongs to.
        /// </summary>
        public int BatchID { get; set; }
        /// <summary>
        /// The batch this order belongs to.
        /// </summary>
        public Batch Batch { get; set; }
        /// <summary>
        /// The order-item relationships this order participates in.
        /// </summary>
        public List<OrderItem> OrderItems { get; set; }
    }
}
