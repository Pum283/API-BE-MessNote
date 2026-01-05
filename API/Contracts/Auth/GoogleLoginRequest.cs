using System.ComponentModel.DataAnnotations;

namespace API.Contracts.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}

