using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("order_part")]
    public class OrderPart
    {
        [Key]
        [Column("order_part_id")]
        public int OrderPartId { get; set; }

        [Column("customer_order_id")]
        public int CustomerOrderId { get; set; }

        [Column("part_id")]
        public int PartId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }
}
