using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Identity.ViewModels
{
    public class ForgotPasswordVM
    {
        [Required]
        [EmailAddress]
        [Remote("IsAnyEmail","Account",HttpMethod ="Post")]
        
        
        public string Email { get; set; }
    }
}
