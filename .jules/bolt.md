## 2026-08-05 - Fix N+1 query problem in ReviewApplications.razor
**Learning:** In the Blazor Server architecture of this project, the EF Core DbContext is scoped per circuit. Using Task.WhenAll for concurrent database queries (e.g., UserManager lookups or DbContext queries) causes EF Core concurrency exceptions and N+1 performance bottlenecks. Data fetching should be batched (e.g., using .Contains()) or eager-loaded using .Include() instead.
**Action:** Replace Task.WhenAll(items.Select(x => ...)) with eager loading (.Include()) and batched lookups (.Where(u => ids.Contains(u.Id))).
