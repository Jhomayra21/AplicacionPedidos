using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Models
{
    public class OrdenItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Orden")]
        public int OrdenId { get; set; }
        
        [ForeignKey("OrdenId")]
        public virtual OrdenModel? Orden { get; set; }
        
        [Required]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }
        
        [ForeignKey("ProductoId")]
        public virtual ProductoModel? Producto { get; set; }
        
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }
    }
}
