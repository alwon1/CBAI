## 2026-07-27 - Added confirmation dialog for user deletion
**Learning:** Blazor's Fluent UI integration (`Microsoft.FluentUI.AspNetCore.Components`) makes it very easy to accidentally omit confirmation dialogs for destructive actions because inline click handlers are fast to write.
**Action:** Always check `FluentDataGrid` template columns with destructive actions (like Delete buttons) to ensure they are wired to a confirmation `FluentDialog` rather than executing the deletion logic immediately. Added an `AriaLabel` to the delete button as well to improve a11y.
