## 2024-08-14 - Fluent UI Blazor Async Button Feedback
**Learning:** When using `FluentButton` in Blazor for async form submissions, the `Disabled` attribute alone prevents double-clicks but offers poor visual feedback. The button requires explicitly binding the `Loading` parameter alongside `Disabled` to render the internal loading spinner correctly.
**Action:** Always link the `Loading` parameter to the `submitting` state variable when setting `Disabled` on a `FluentButton` to ensure proper visual feedback during asynchronous operations.
