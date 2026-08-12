## 2026-07-27 - Fluent UI Blazor Dialogs for Destructive Actions
**Learning:** Adding confirmation to destructive actions using `FluentDialog` is straight-forward but requires managing extra component state (e.g. `isDeleteUserDialogOpen`, `userToDelete`) alongside the component logic.
**Action:** When implementing destructive action modals, remember to manage both the boolean visibility state and the entity being targeted so that the modal context is clear and the operation acts correctly when confirmed.
