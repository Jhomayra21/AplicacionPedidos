using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Models
{
    public class OrdenModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        
        [ForeignKey("ClienteId")]
        public virtual UserModel? Cliente { get; set; }
        
        [Required]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";
        
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }
        
        public virtual ICollection<OrdenItem> Items { get; set; } = new List<OrdenItem>();
    }
}
