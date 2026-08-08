## 2026-07-27 - [Initialization]\n**Learning:** Just starting\n**Action:** None

## 2026-07-27 - [Blazor EF Core N+1 Optimization]
**Learning:** Using `Task.WhenAll` to fetch related data inside a loop (like audits or user details for a list of items) is a severe anti-pattern in EF Core with Blazor. It causes an N+1 query explosion and can easily lead to `InvalidOperationException` due to concurrent operations on the DbContext.
**Action:** Always prefer eager loading (e.g. `.Include()`) for related entity data, and batch queries (e.g. `Where(x => list.Contains(x.Id)).ToDictionaryAsync()`) for external dependencies like UserManager when displaying lists.
