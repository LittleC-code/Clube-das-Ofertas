using ClubeDasOfertas.Web.Data;
using ClubeDasOfertas.Web.Domain;

namespace ClubeDasOfertas.Web.Services;

public sealed class ReviewService(AppRepository repository)
{
    public async Task ApproveAsync(Guid itemId, UserAccount user, string comment, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetCampaignItemAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException("Item nao encontrado.");

        var blockers = item.BlockingReasons
            .Where(x => !x.Contains("pendente", StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Contains("Revisao rejeitada", StringComparison.OrdinalIgnoreCase))
            .ToList();

        await repository.UpdateItemReviewAsync(
            itemId,
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
