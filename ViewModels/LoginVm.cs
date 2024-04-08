using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class LoginVm
    {
        [Required]
        [Display(Name ="User Name")]
        [StringLength(200)]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Remebmer Me?")]

        public bool RememberMe { get; set; }
     

    }
}
