## 2026-07-27 - FluentButton Visual Feedback for Async Actions
**Learning:** When using Blazor's `FluentButton` for form submission with async handlers, simply setting `Disabled="submitting"` isn't sufficient for visual UX. Users need a visual indicator that the submission is processing. The loading state must be explicitly bound.
**Action:** When adding submitting states to `FluentButton` instances in the Blazor codebase, always bind `Loading="@submitting"` alongside `Disabled="@submitting"` to ensure a loading spinner is visible during form submission.
