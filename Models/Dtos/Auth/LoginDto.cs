using System.ComponentModel.DataAnnotations;

namespace DulceAtardecer.Models.Dtos.Auth;

public record LoginDto(
    [Required] string Username,
    [Required] string Password
);
