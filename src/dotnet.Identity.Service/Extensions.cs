using dotnet.Identity.Service.Dtos;
using dotnet.Identity.Service.Entities;

namespace dotnet.Identity.Service
{
    public static class Extensions
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(user.Id, user.UserName, user.Email, user.Okubo, user.CreatedOn);
        }
    }
}