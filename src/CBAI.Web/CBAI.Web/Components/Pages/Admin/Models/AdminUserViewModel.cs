using CBAI.Web.Membership;

namespace CBAI.Web.Components.Pages.Admin.Models;

public class AdminUserViewModel
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsEnabled { get; set; } = true;

    // Member data if the user is a member
    public bool IsMember { get; set; }
    public MemberStatus? MemberStatus { get; set; }
    public string? MembershipTypeName { get; set; }

    // Additional info for UI
    public string RolesDisplay => string.Join(", ", Roles);
}
