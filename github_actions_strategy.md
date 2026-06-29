# Multi-Repository CI/CD Strategy

This document outlines the industry-standard approach for configuring GitHub Actions when your application code and test automation framework live in two separate repositories.

## Architecture Overview

1. **Automatic Runs (Web App Repository)**
   - **Purpose:** Provide immediate feedback to developers when they push code changes to the application.
   - **Location:** `EcommerceWebApp/.github/workflows/ci.yml`
   - **Trigger:** `on: push` or `on: pull_request`
   - **Behavior:** The workflow automatically clones the Web App, builds it, starts it, clones the Playwright repository, and executes the full regression or smoke test suite.
   - **Benefit:** Developers don't have to leave their repository to see if their code changes broke existing functionality.

2. **Manual Runs (Playwright Repository)**
   - **Purpose:** Allow QA Automation Engineers to manually trigger specific tests or suites when they add new automation code, without waiting for a developer to push an application update.
   - **Location:** `ECommerce.Playwright/.github/workflows/manual-run.yml`
   - **Trigger:** `on: workflow_dispatch`
   - **Behavior:** The workflow displays a "Run Workflow" button in GitHub Actions with custom inputs (e.g., `TestCategory`, `TestName`). Upon execution, it clones the Web App, builds it, starts it, and runs *only* the specific tests requested.
   - **Benefit:** QA engineers can test their new automation scripts in isolation directly from their own repository.

## Future Implementation Guide

When we are ready to implement the **Manual Runs (Playwright Repository)** portion of this strategy, we will create a new file in the Playwright repository at `.github/workflows/manual-run.yml` with the following configuration:

```yaml
name: Manual Test Run (On-Demand)

on:
  workflow_dispatch:
    inputs:
      test_filter:
        description: 'Test Filter (e.g., TestCategory=Smoke, or Name=Can_Create_New_Product)'
        required: false
        default: ''

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    # 1. Get the Web App Code
    - name: Checkout Web App Code
      uses: actions/checkout@v4
      with:
        repository: nishant7956/EcommerceWebApp
        path: 'webapp'

    # 2. Build & Run the Web App
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Build and Start Web App
      run: |
        dotnet build
        dotnet run --no-build &
        timeout 30 bash -c 'until curl -s http://localhost:5130/ > /dev/null; do sleep 1; done'
      working-directory: ./webapp

    # 3. Get the Playwright Tests Code
    - name: Checkout Playwright Tests Code
      uses: actions/checkout@v4
      with:
        path: 'tests'

    # 4. Install Playwright and Run
    - name: Build Playwright Tests
      run: dotnet build ECommerce.Playwright.slnx
      working-directory: ./tests

    - name: Install Playwright Browsers
      run: pwsh tests/ECommerce.Playwright.Tests/bin/Debug/net8.0/playwright.ps1 install --with-deps

    - name: Run Playwright Tests (Filtered)
      run: |
        if [ -z "${{ github.event.inputs.test_filter }}" ]; then
          dotnet test ECommerce.Playwright.slnx --logger:"trx;LogFileName=test_results.trx"
        else
          dotnet test ECommerce.Playwright.slnx --filter "${{ github.event.inputs.test_filter }}" --logger:"trx;LogFileName=test_results.trx"
        fi
      working-directory: ./tests
      env:
        DOTNET_ENVIRONMENT: CI

    # 5. Upload Results
    - name: Upload Test Results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: test-results
        path: ./tests/TestResults/
```

> [!TIP]
> This `workflow_dispatch` feature is the direct equivalent to clicking the "Run" button with custom build configurations in TeamCity.
