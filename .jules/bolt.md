## 2024-05-19 - [Avoid Task.WhenAll on DB Contexts]
**Learning:** Using `Task.WhenAll` to execute concurrent database queries inside Blazor components over the same scoped EF Core `DbContext` causes EF Core concurrency exceptions and N+1 performance bottlenecks.
**Action:** When fetching data like User attributes or associated relational data in loops, prefer eager loading using `.Include()` on the main DbContext request, or use batched queries such as `.Where(u => ids.Contains(u.Id)).ToListAsync()`.
