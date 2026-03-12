using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("step")]
    public class Step
    {
        [Key]
        [Column("step_id")]
        public int StepId { get; set; }

        [Column("name_step")]
        public string NameStep { get; set; } = "";
    }
}
