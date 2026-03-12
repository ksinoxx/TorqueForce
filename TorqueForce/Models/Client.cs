using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorqueForce.Models
{
    [Table("client")]
    public class Client
    {
        [Key]
        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("name_client")]
        public string NameClient { get; set; } = "";

        [Column("phone")]
        public string Phone { get; set; } = "";

        [Column("email")]
        public string? Email { get; set; }
    }
}
