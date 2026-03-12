using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("part")]
    public class Part
    {
        [Key]
        [Column("part_id")]
        public int PartId { get; set; }

        [Column("part_name")]
        public string PartName { get; set; } = "";

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("brand_id")]
        public int BrandId { get; set; }

        [Column("car_model_id")]
        public int CarModelId { get; set; }

        [Column("price", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column("stock")]
        public int Stock { get; set; }
    }
}
