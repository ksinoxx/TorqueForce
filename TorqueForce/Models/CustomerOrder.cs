using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("customer_order")]
    public class CustomerOrder
    {
        [Key]
        [Column("customer_order_id")]
        public int CustomerOrderId { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("order_date")]
        public DateTime OrderDate { get; set; }

        [Column("total_price", TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }
    }
}
