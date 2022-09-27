using System.Threading.Tasks;
using MassTransit;
using dotnet.Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using dotnet.Identity.Service.Entities;
using dotnet.Identity.Service.Exceptions;

namespace dotnet.Identity.Service.Consumers
{
    public class ComotOkuboConsumer : IConsumer<ComotOkubo>
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ComotOkuboConsumer(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task Consume(ConsumeContext<ComotOkubo> context)
        {
            var message = context.Message;
            var user = await userManager.FindByIdAsync(message.UserId.ToString());
            //User no exist
            if (user == null) throw new UnknownUserException(message.UserId);

            user.Okubo -= message.Okubo;
            //Okubo no dey
            if (user.Okubo < 0) throw new InsufficientFundsException(message.UserId, message.Okubo);

            await userManager.UpdateAsync(user);
            await context.Publish(new OkuboDonComot(message.CorrelationId));
        }

    }
}