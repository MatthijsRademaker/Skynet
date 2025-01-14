using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;
using Azure.Messaging.ServiceBus;

namespace Arnold.PaycheckProtector
{
    public class PremiumCalculator(
        ILogger<PremiumCalculator> logger,
        ServiceBusClient serviceBusClient,
        HttpClient httpClient
    ) : IPremiumCalculator
    {
        ServiceBusSender sender = serviceBusClient.CreateSender("customer");

        public async Task<Premium?> CalculatePremiumAsync(
            Guid customerId,
            CancellationToken cancellationToken
        )
        {
            var response = await httpClient.GetAsync("https+http://premiumcalcproxy/getPremium");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var premium = new Premium(decimal.Parse(content));

            logger.LogInformation(
                "Calculated premium for {customerId}: {premium}",
                customerId,
                premium
            );
            await sender.SendMessageAsync(
                new PremiumCalculatedCommand()
                {
                    Id = customerId,
                    Premium = premium,
                    Version = 1,
                }.ToServiceBusMessage(),
                cancellationToken
            );
            return premium;
        }
    }
}
