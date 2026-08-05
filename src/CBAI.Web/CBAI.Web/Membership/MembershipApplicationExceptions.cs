namespace CBAI.Web.Membership;

/// <summary>
/// Thrown when a requested state change is not valid for the application's current
/// <see cref="MembershipApplicationStatus"/> (e.g. deciding a Draft, or resubmitting a
/// Submitted/decided application).
/// </summary>
public class InvalidMembershipApplicationTransitionException(string message) : InvalidOperationException(message);

/// <summary>
/// Thrown when a submission names a sponsor who does not meet the eligibility rules — see
/// <see cref="IMembershipApplicationService.IsSponsorEligibleAsync"/>.
/// </summary>
public class SponsorIneligibleException(string message) : InvalidOperationException(message);

/// <summary>
/// Thrown when a decision is attributed to an account that does not exist or is not authorized
/// to decide membership applications.
/// </summary>
public class DecisionMakerUnauthorizedException(string message) : UnauthorizedAccessException(message);
