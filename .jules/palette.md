## 2024-05-18 - Missing loading spinners on async Fluent UI buttons
**Learning:** Fluent UI Blazor `FluentButton` components need explicit `Loading` property bound to state alongside `Disabled` property to show a loading spinner on async form submissions. Direct `Disabled` prevents multiple submissions but provides poor visual feedback on its own.
**Action:** Add `Loading="@submitting"` (or equivalent state) to any `FluentButton` that triggers an asynchronous operation and disables itself while processing.
