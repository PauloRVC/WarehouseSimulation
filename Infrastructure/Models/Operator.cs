using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Operator
    {
        public int OperatorID { get; set; }

        public string OperatorID_Text { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
