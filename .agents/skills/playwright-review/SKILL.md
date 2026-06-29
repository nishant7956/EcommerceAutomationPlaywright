---
name: playwright-review
description: >
  Pre-commit code review for the OrangeHRM Playwright framework at C:\WebTestPlaywright.
  Use this skill whenever the user asks to "review", "check before commit",
  "pre-commit review", or "review my changes" in this project.
---

# Playwright Framework Pre-Commit Review Skill

## Purpose
Review all staged or recently modified files in `C:\WebTestPlaywright` against
the framework conventions agreed during the design phase. Block the commit
mentally if any rule is violated — always explain what needs fixing and why.

---

## Review Checklist

### 1. Solution Structure
- All framework code lives under `OrangeHRM.Playwright.Framework/`
- All UI tests live under `OrangeHRM.Playwright.Tests/`
- All API tests live under `OrangeHRM.Playwright.Api/`
- No test code (NUnit attributes, test methods) exists inside the Framework project
- No browser/page dependencies exist inside the Api project unless using `IAPIRequestContext`

### 2. Naming Conventions
- Page classes: `{PageName}Page.cs` inside `Framework/Pages/`
- Component classes: `{Name}Component.cs` inside `Framework/Components/`
- Test classes: `{Feature}Tests.cs` inside appropriate `Tests/UI/{Module}/` folder
- Base classes: `BasePage.cs`, `BaseUiTest.cs`, `BaseApiTest.cs`
- Config: `TestSettings.cs` + `appsettings.json` inside `Framework/Config/`

### 3. Async Rules (CRITICAL)
- ALL page methods that touch `IPage` must be `async Task` or `async Task<T>`
- NEVER use `.GetAwaiter().GetResult()` or `.Result` — this causes deadlocks
- ALL test methods must be `async Task` (not `void`, not `Task<T>`)
- Constructors must remain synchronous — async initialization belongs in a factory method or `SetUpAsync`

### 4. Page Object Model Rules
- Every page class must inherit from `BasePage`
- Pages receive `IPage page` and `TestSettings settings` via constructor
- Navigation methods (`OpenAsync`, `GotoAsync`) must return `Task<SamePage>` or `Task<OtherPage>`
- Action methods (Click, Type, Select) must return `Task<SamePage>` for fluent chaining
- Query methods (`UserCountAsync`, `GetTextAsync`) must return `Task<T>` where T is the actual value type
- No raw `await page.Locator(...)` calls inside test methods — those belong in page/component classes

### 5. Locator Strategy
- Prefer role-based locators: `Page.GetByRole(AriaRole.Button, new() { Name = "Save" })`
- Prefer label locators: `Page.GetByLabel("Username")`
- CSS selectors are acceptable as fallback
- NEVER use XPath unless absolutely necessary (complex text matching)
- Locators should be defined as private `ILocator` properties in the page class, not inline in methods

### 6. Wait Strategy
- Do NOT add `await Task.Delay(...)` or `Thread.Sleep()` — use proper waits
- Built-in auto-wait handles visibility/enabled/stable for click/fill/type actions
- Use `WaitForAsync` from the `Waiter` class for complex conditions (count changes, toast appears)
- Never use `WaitForTimeoutAsync` except as an absolute last resort — always comment why

### 7. Configuration
- No hardcoded URLs, credentials, or timeouts in page or test files
- All configurable values must come from `TestSettings` (injected via constructor)
- `appsettings.json` holds defaults; environment variables override at runtime

### 8. Tracing / Failure Artifacts
- `TraceHelper.cs` manages all Playwright trace start/stop
- Traces must be saved on failure only by default
- Trace files must go to the path defined in `TestSettings.ArtifactsDirectory`
- Never commit trace zip files to the repository (`.gitignore` covers `playwright-traces/`)

### 9. Test Structure Rules
- Every UI test class must inherit from `BaseUiTest`
- Every API test class must inherit from `BaseApiTest`
- No `IPage` or `IBrowser` creation inside test methods — lifecycle is managed by `BaseUiTest`
- Use `[Category("UI")]`, `[Category("Smoke")]` etc. for test filtering in CI
- Use `[TestFixture]` on test classes
- Use FluentAssertions (`Should().Be(...)`) — never plain `Assert.That(...)` unless in a base class

### 10. Project File Hygiene
- No `<Compile Remove="..." />` overrides without a comment explaining why
- `appsettings.json` must have `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` set
- No direct `PackageReference` to `Selenium.*` in this solution

---

## How to Run the Review

1. Run `git diff --name-only HEAD` to see changed files
2. Read each changed file
3. Apply the checklist above to each file
4. Report: ✅ Passes / ⚠️ Warning / ❌ Violation — with file + line reference
5. If any ❌ exists, do NOT proceed with commit — explain the fix required
6. If all ✅, confirm: "All checks passed — safe to commit."

---

## Trigger Phrases
- "review before commit"
- "pre-commit review"  
- "review my changes"
- "check everything"
- "is this ready to commit"
