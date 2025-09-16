using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Models
{
    public class ProductoModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }
        
        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a 0")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }
    }
}
