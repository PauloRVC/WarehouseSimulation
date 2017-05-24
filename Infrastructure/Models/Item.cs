using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Item
    {
        /// <summary>
        /// The unique ID used by the db.
        /// </summary>
        public int ItemID { get; set; }
        /// <summary>
        /// The unique indicator used in the production db.
        /// </summary>
        public string ItemIndicator { get; set; }
        /// <summary>
        /// The order-item relationships this item participates in.
        /// </summary>
        public List<OrderItem> OrderItems { get; set; }
    }
}
