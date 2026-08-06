## 2024-05-18 - N+1 Query in Blazor components using UserManager inside loop

**Learning:** Using `UserManager.GetRolesAsync` inside a loop in a Blazor page (such as `Users.razor`) leads to a severe N+1 query problem, as it fires a query for every user in the loop.
**Action:** When populating view models that require role information for a collection of users, use a single query against `DbContext.UserRoles` and `DbContext.Roles` to bulk fetch, then use `ToLookup()` to construct an in-memory dictionary for assigning roles in O(1) time.
