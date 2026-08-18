## 2024-05-24 - [Enforce Account Lockout]
**Vulnerability:** The application was not enforcing account lockouts on failed login attempts because `lockoutOnFailure` was set to `false` in `Login.razor`, exposing the app to brute force attacks.
**Learning:** Default templates or copy-pasted code might disable security features like account lockout for convenience during development, leaving them vulnerable in production.
**Prevention:** Always verify that security-related flags like `lockoutOnFailure` are enabled for authentication endpoints. Ensure this is part of the code review process.
