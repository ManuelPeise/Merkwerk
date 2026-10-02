# =====================================================================
#  Merkwerk – create the remaining projects of the solution (LP-001)
#  Run in PowerShell:   cd D:\WorkBench\Merkwerk\sources
#                       .\create-solution.ps1
#  Requires the .NET 10 SDK.  Data.Database and Data.Accessor already exist
#  and are only wired up, not recreated.
#  Project names have no "Merkwerk." prefix (decision 02.10.2026).
# =====================================================================
$ErrorActionPreference = "Stop"
$sln = "Merkwerk.slnx"

function Invoke-Dotnet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($args -join ' ') failed (exit code $LASTEXITCODE)" }
}

# ---------- 01 Web -----------------------------------------------------
# Blazor Web App with server AND WebAssembly part (render mode per page).
# Generated into a temp folder first because the template nests its output.
Invoke-Dotnet new blazor -n Web --interactivity Auto --empty -o _tmp_web
Move-Item _tmp_web\Web        .\Web
Move-Item _tmp_web\Web.Client .\Web.Client
Remove-Item _tmp_web -Recurse -Force

# ---------- 02 Service -------------------------------------------------
Invoke-Dotnet new classlib -n Service

# ---------- 03 Logic ---------------------------------------------------
Invoke-Dotnet new classlib -n Logic
Invoke-Dotnet new classlib -n Logic.Shared

# ---------- 05 Shared --------------------------------------------------
Invoke-Dotnet new classlib -n Shared

# ---------- 06 Tests ---------------------------------------------------
foreach ($t in "Logic.Tests", "Logic.Shared.Tests", "Web.Tests", "Data.IntegrationTests", "Service.IntegrationTests", "Architecture.Tests") {
    Invoke-Dotnet new xunit -n $t
}

# ---------- Add projects to solution folders ---------------------------
Invoke-Dotnet sln $sln add --solution-folder "01 Web"     Web/Web.csproj Web.Client/Web.Client.csproj
Invoke-Dotnet sln $sln add --solution-folder "02 Service" Service/Service.csproj
Invoke-Dotnet sln $sln add --solution-folder "03 Logic"   Logic/Logic.csproj Logic.Shared/Logic.Shared.csproj
Invoke-Dotnet sln $sln add --solution-folder "05 Shared"  Shared/Shared.csproj
Invoke-Dotnet sln $sln add --solution-folder "06 Tests" `
    Logic.Tests/Logic.Tests.csproj Logic.Shared.Tests/Logic.Shared.Tests.csproj Web.Tests/Web.Tests.csproj `
    Data.IntegrationTests/Data.IntegrationTests.csproj Service.IntegrationTests/Service.IntegrationTests.csproj `
    Architecture.Tests/Architecture.Tests.csproj
# Data.Database and Data.Accessor are already in "04 Data".

# ---------- Project references (dependency rules, AGENTS.md §3) --------
Invoke-Dotnet add Logic.Shared  reference Shared
Invoke-Dotnet add Data.Database reference Shared
Invoke-Dotnet add Data.Accessor reference Data.Database Shared
Invoke-Dotnet add Logic         reference Logic.Shared Shared Data.Accessor      # never Data.Database directly
Invoke-Dotnet add Service       reference Logic Shared
Invoke-Dotnet add Web.Client    reference Logic.Shared Shared                    # never Logic or Data.*
Invoke-Dotnet add Web           reference Service Logic Data.Accessor Data.Database   # Data.* for DI only

Invoke-Dotnet add Logic.Tests              reference Logic
Invoke-Dotnet add Logic.Shared.Tests       reference Logic.Shared
Invoke-Dotnet add Web.Tests                reference Web Web.Client
Invoke-Dotnet add Data.IntegrationTests    reference Data.Accessor Data.Database
Invoke-Dotnet add Service.IntegrationTests reference Web
Invoke-Dotnet add Architecture.Tests       reference Web Web.Client Service Logic Logic.Shared Data.Database Data.Accessor Shared

# ---------- Clean up empty template classes ---------------------------
Get-ChildItem -Recurse -Filter Class1.cs | Remove-Item

# ---------- Build -----------------------------------------------------
Invoke-Dotnet build $sln
Write-Host "Done. Reload Merkwerk.slnx in Visual Studio." -ForegroundColor Green
Write-Host "Manual step: add <FrameworkReference Include=""Microsoft.AspNetCore.App"" /> to Service/Service.csproj." -ForegroundColor Yellow
