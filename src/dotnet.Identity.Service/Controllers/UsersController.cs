using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Identity.Contracts;
using dotnet.Identity.Service.Dtos;
using dotnet.Identity.Service.Entities;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static IdentityServer4.IdentityServerConstants;

namespace dotnet.Identity.Service.Controllers
{
    [ApiController]
    [Route("users")]
    //Policy for securing APIs that live on the auth's own server
    [Authorize(Policy = LocalApi.PolicyName, Roles = Roles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPublishEndpoint publishEndpoint;// { get; set; }

        public UsersController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint)
        {
            this.userManager = userManager;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get()
        {
            var users = userManager.Users.ToList().Select(user => user.AsDto());

            return Ok(users);
        }

        // /users/{123}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            return user.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.Email = updateUserDto.Email;
            user.UserName = updateUserDto.Email;
            user.Okubo = updateUserDto.Okubo;

            await userManager.UpdateAsync(user);
            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, user.Okubo));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            await userManager.DeleteAsync(user);
            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, 0));
            return NoContent();
        }

    }
}