## 2024-10-18 - [Missing Account Lockout on Login]
**Vulnerability:** The `SignInManager.PasswordSignInAsync` method was called with `lockoutOnFailure: false`, meaning that failed login attempts were not counted toward account lockout.
**Learning:** This prevented the application from mitigating brute-force password guessing attacks. This is a common missed configuration in the default scaffolding.
**Prevention:** Always verify that `lockoutOnFailure` is explicitly set to `true` in any password-based authentication endpoints.
