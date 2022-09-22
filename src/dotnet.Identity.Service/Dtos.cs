using System;
using System.ComponentModel.DataAnnotations;

namespace dotnet.Identity.Service.Dtos
{
    public record UserDto(
        Guid Id,
        string Username,
        string Email,
        decimal Okubo,
        DateTimeOffset CreatedDate
    );

    public record UpdateUserDto(
        [Required]
        [EmailAddress]
        string Email,

        [Range(0, 1000000)]
        decimal Okubo
    );
}