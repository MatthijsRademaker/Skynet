using Arnold.SkyNet.Domain;

namespace Arnold.KnowledgeTest
{
    public class PremiumCalculator : IPremiumCalculator
    {
        public async Task<Premium?> CalculatePremiumAsync(
            Guid customerId,
            CancellationToken cancellationToken
        )
        {
            return null;
        }
    }
}
