using System.ComponentModel.DataAnnotations;

namespace DulceAtardecer.Models.Dtos.Auth;

public record RegisterDto(
    [Required] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string Nombre
);
