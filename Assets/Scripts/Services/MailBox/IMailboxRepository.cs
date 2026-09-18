using System.Collections.Generic;
using System.Threading.Tasks;

public interface IMailboxRepository
{
    Task<List<MailData>> LoadAsync(string userId);
    Task<MailboxClaimResult> ClaimAsync(string userId, IReadOnlyCollection<string> mailIds);
    Task DeleteAsync(string userId, IReadOnlyCollection<string> mailIds);
}
