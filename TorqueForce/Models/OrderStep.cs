using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("order_step")]
    public class OrderStep
    {
        [Key]
        [Column("order_step_id")]
        public int OrderStepId { get; set; }

        [Column("customer_order_id")]
        public int CustomerOrderId { get; set; }

        [Column("step_id")]
        public int StepId { get; set; }

        [Column("date_start")]
        public DateTime DateStart { get; set; }

        [Column("date_end")]
        public DateTime? DateEnd { get; set; }
    }
}
