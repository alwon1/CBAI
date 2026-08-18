
## 2024-05-18 - Added loading spinner to async submit button
**Learning:** In Fluent UI Blazor, simply setting `Disabled="@submitting"` on `FluentButton` is insufficient for indicating asynchronous operations. The `Loading="@submitting"` parameter must also be explicitly set to provide immediate visual feedback (a loading spinner) and prevent duplicate submissions.
**Action:** Always link the `Loading` parameter alongside the `Disabled` parameter when working with async form submissions using `FluentButton` in this project.
