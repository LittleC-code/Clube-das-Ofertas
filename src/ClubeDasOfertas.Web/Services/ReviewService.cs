using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;

namespace ClubeDasOfertas.Web.Services;

public sealed class ReviewService(AppRepository repository)
{
    public async Task ApproveAsync(Guid itemId, UserAccount user, string comment, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetCampaignItemAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException("Item nao encontrado.");

        await ApproveItemAsync(item, user, comment, cancellationToken);
    }

    public async Task<int> ApproveManyAsync(IReadOnlyList<Guid> itemIds, UserAccount user, string comment, CancellationToken cancellationToken = default)
    {
        var approved = 0;

        foreach (var itemId in itemIds.Distinct())
        {
            var item = await repository.GetCampaignItemAsync(itemId, cancellationToken);
            if (item is null || !(item.ReviewRequired || item.ReviewStatus is ReviewStatus.Pending or ReviewStatus.Rejected))
            {
                continue;
            }

            await ApproveItemAsync(item, user, comment, cancellationToken);
            approved++;
        }

        return approved;
    }

    private async Task ApproveItemAsync(CampaignItem item, UserAccount user, string comment, CancellationToken cancellationToken)
    {
        var blockers = item.BlockingReasons
            .Where(x => !x.Contains("pendente", StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Contains("Revisao rejeitada", StringComparison.OrdinalIgnoreCase))
            .ToList();

        await repository.UpdateItemReviewAsync(
            item.Id,
            blockers.Count > 0,
            blockers.Count > 0 ? ReviewStatus.Pending : ReviewStatus.Approved,
            blockers,
            cancellationToken);

        await repository.AddReviewDecisionAsync(new ReviewDecision(
            Guid.NewGuid(),
            item.CampaignId,
            item.Id,
            user.Id,
            ReviewStatus.Approved,
            comment.Trim(),
            DateTimeOffset.UtcNow), cancellationToken);

        await repository.AddAuditAsync(user.Id, user.Email, "Aprovou revisao", "CampaignItem", item.Id, item.DescriptionTabloid, cancellationToken);
    }

    public async Task RejectAsync(Guid itemId, UserAccount user, string comment, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetCampaignItemAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException("Item nao encontrado.");

        var blockers = item.BlockingReasons.ToList();
        blockers.Add("Revisao rejeitada");

        await repository.UpdateItemReviewAsync(itemId, true, ReviewStatus.Rejected, blockers.Distinct().ToList(), cancellationToken);
        await repository.AddReviewDecisionAsync(new ReviewDecision(
            Guid.NewGuid(),
            item.CampaignId,
            item.Id,
            user.Id,
            ReviewStatus.Rejected,
            comment.Trim(),
            DateTimeOffset.UtcNow), cancellationToken);

        await repository.AddAuditAsync(user.Id, user.Email, "Rejeitou revisao", "CampaignItem", item.Id, item.DescriptionTabloid, cancellationToken);
    }
}
