# CBAI
Blazor Web App with Auto Interactive render mode, Individual Accounts auth, SQLite database, Aspire orchestration, MSTest, and devcontainer support

## Demo/seed data

When enabled, the app seeds a fixed set of Identity roles (`Administrator`, `Staff`,
`BoardMember`, `Sponsor`, `Member`) and demo accounts on startup. This is controlled by the
`SeedData` configuration section and is **opt-in** (`Enabled: false` by default), so it never
runs in Production unless explicitly turned on. `appsettings.Development.json` sets
`Enabled: true` for local development.

Known accounts (demo-only credentials — never used in production):

| Email | Password | Role |
| --- | --- | --- |
| admin@example.com | Admin123! | Administrator |
| staff@example.com | Staff123! | Staff |
| board@example.com | Board123! | BoardMember |
| sponsor@example.com | Sponsor123! | Sponsor |
| member@example.com | Member123! | Member |

A configurable number of additional `Member`-role accounts (default 40) are also generated
with realistic-looking names/emails via [Bogus](https://github.com/bchavez/Bogus), using a
fixed random seed so the same demo population is produced every time the seeder runs against
an empty database. Configure via the `SeedData` section:

```json
"SeedData": {
  "Enabled": true,
  "BogusUserCount": 40,
  "RandomSeed": 20260805
}
```
