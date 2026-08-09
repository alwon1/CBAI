## 2026-07-27 - Entity Framework N+1 Queries in Blazor Server
**Learning:** Using `Task.WhenAll` to perform concurrent database lookups against a scoped `DbContext` in Blazor Server can lead to EF Core concurrency exceptions and N+1 query performance bottlenecks.
**Action:** Always batch related data queries. Use `.Include()` in the main query for child collections, and batch ID lookups (e.g. `Where(u => ids.Contains(u.Id))`) instead of issuing separate queries inside loops or via `Task.WhenAll`.
