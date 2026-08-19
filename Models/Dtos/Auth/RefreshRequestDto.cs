using System.ComponentModel.DataAnnotations;

namespace DulceAtardecer.Models.Dtos.Auth;

public record RefreshRequestDto(
    [Required] string RefreshToken
);
