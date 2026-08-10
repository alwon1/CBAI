## 2026-07-27 - Task.WhenAll with EF Core DbContext
**Learning:** In Blazor Server architecture, EF Core `DbContext` is scoped per circuit. Using `Task.WhenAll` for concurrent queries (like `UserManager` lookups or `DbContext` queries) causes EF Core concurrency exceptions and N+1 performance bottlenecks.
**Action:** Always batch data fetching (e.g., using `.Contains()`) or eager-load using `.Include()` instead of using `Task.WhenAll` on scoped contexts.
