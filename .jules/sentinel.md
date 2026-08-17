## 2026-07-27 - [Enable Account Lockout]
**Vulnerability:** The application was not locking out users after failed login attempts, making it vulnerable to brute force and credential stuffing attacks.
**Learning:** `SignInManager.PasswordSignInAsync` was called with `lockoutOnFailure: false` in `Login.razor`, explicitly bypassing the default lockout behavior.
**Prevention:** Ensure `lockoutOnFailure: true` is set when validating user credentials with Identity.
