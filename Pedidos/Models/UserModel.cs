using System.ComponentModel.DataAnnotations;

namespace Pedidos.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty; 
    }
}
