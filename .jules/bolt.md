## 2026-07-27 - N+1 Query in Review Applications
**Learning:** Blazor Server with EF Core circuits can fail on N+1 queries using `Task.WhenAll` due to concurrency exceptions or perform poorly. Memory explicitly warns: "Using `Task.WhenAll` for concurrent database queries (e.g., `UserManager` lookups or `DbContext` queries) causes EF Core concurrency exceptions and N+1 performance bottlenecks. Data fetching should be batched (e.g., using `.Contains()`) or eager-loaded using `.Include()` instead."
**Action:** Use batched `.Contains()` for UserManager lookups to avoid N+1 queries.
