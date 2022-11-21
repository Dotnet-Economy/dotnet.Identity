using System.Threading.Tasks;
using MassTransit;
using dotnet.Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using dotnet.Identity.Service.Entities;
using dotnet.Identity.Service.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;
using dotnet.Common.Settings;

namespace dotnet.Identity.Service.Consumers
{
    public class ComotOkuboConsumer : IConsumer<ComotOkubo>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ComotOkubo> logger;
        private readonly Counter<int> okuboDonComotCounter;
        public ComotOkuboConsumer(UserManager<ApplicationUser> userManager, ILogger<ComotOkubo> logger, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.logger = logger;

            var settings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            Meter meter = new(settings.ServiceName);
            okuboDonComotCounter = meter.CreateCounter<int>("OkuboDonComot");
        }

        public async Task Consume(ConsumeContext<ComotOkubo> context)
        {

            var message = context.Message;
            var user = await userManager.FindByIdAsync(message.UserId.ToString());
            
            logger.LogInformation(
                "Comot {Amount} Okubo from User:{UserId}. CorrelationId:{CorrelationId}",
                message.Okubo,
                message.UserId,
                context.Message.CorrelationId
            );

            //User no exist
            if (user == null) throw new UnknownUserException(message.UserId);
            if (user.MessageIds.Contains(context.MessageId.Value))
            {
                await context.Publish(new OkuboDonComot(message.CorrelationId));
                return;
            }

            user.Okubo -= message.Okubo;
            //Okubo no dey
            if (user.Okubo < 0)
            {
                logger.LogError(
                "Insufficient Funds to comot {Amount} Okubo from User:{UserId}. CorrelationId:{CorrelationId}.",
                message.Okubo,
                message.UserId,
                context.Message.CorrelationId
            );
                throw new InsufficientFundsException(message.UserId, message.Okubo);
            }

            user.MessageIds.Add(context.MessageId.Value);

            await userManager.UpdateAsync(user);

            var okuboDonComotTask = context.Publish(new OkuboDonComot(message.CorrelationId));
            var userUpdatedTask = context.Publish(new UserUpdated(user.Id, user.Email, user.Okubo));

            okuboDonComotCounter.Add(1, new System.Collections.Generic.KeyValuePair<string, object>(nameof(message.CorrelationId), message.CorrelationId));

            await Task.WhenAll(userUpdatedTask, okuboDonComotTask);
        }

    }
}