using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Nodig voor [DatabaseGenerated]

namespace OdiseeConcerts.Models
{
    public abstract class BaseEntity
    {
        [Key] // Markeer Id als de primaire sleutel
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Zorg voor auto-incrementing
        public int Id { get; set; }
    }
}