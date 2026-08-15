## 2024-05-24 - Loading states for FluentButton
**Learning:** When using Fluent UI Blazor (e.g., `FluentButton`), explicitly linking the `Loading` parameter (e.g., `Loading="@submitting"`) alongside the `Disabled` parameter is essential. Disabled state alone prevents multiple submissions but does not give visual feedback to the user that processing is happening.
**Action:** Always add `Loading="@submitting"` (or equivalent state variable) when `Disabled="@submitting"` is used on action buttons for asynchronous operations.
