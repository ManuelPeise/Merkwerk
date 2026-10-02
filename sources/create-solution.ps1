# =====================================================================
#  Merkwerk – Projektmappe anlegen (LP-001)
#  Ausfuehren in PowerShell:  cd D:\WorkBench\Merkwerk\sources
#                             .\create-solution.ps1
#  Voraussetzung: .NET 10 SDK  (dotnet --version  ->  10.x)
#  Die vorhandene, leere Merkwerk.slnx wird befuellt.
# =====================================================================
$ErrorActionPreference = "Stop"
$sln = "Merkwerk.slnx"

# ---------- 01 Web ----------------------------------------------------
# Blazor Web App mit Server- UND WebAssembly-Teil (Rendermodus je Seite)
dotnet new blazor -n Merkwerk.Web --interactivity Auto --empty
#   -> erzeugt Merkwerk.Web (Host, Erwachsene) und Merkwerk.Web.Client (Kinder, WASM)

# ---------- 02 Service ------------------------------------------------
# Klassenbibliothek mit API-Controllern; wird von Merkwerk.Web gehostet
dotnet new classlib -n Merkwerk.Service

# ---------- 03 Logic --------------------------------------------------
dotnet new classlib -n Merkwerk.Logic
dotnet new classlib -n Merkwerk.Logic.Shared      # Pruefregeln, Generatoren (auch WASM)

# ---------- 04 Data ---------------------------------------------------
dotnet new classlib -n Merkwerk.Data.Context     # Entities, DbContext, Migrationen
dotnet new classlib -n Merkwerk.Data.Accessor    # Repositories, Unit of Work

# ---------- 05 Shared -------------------------------------------------
dotnet new classlib -n Merkwerk.Shared            # DTOs, Enums, Konstanten

# ---------- 06 Tests --------------------------------------------------
dotnet new xunit -n Merkwerk.Logic.Tests
dotnet new xunit -n Merkwerk.Logic.Shared.Tests
dotnet new xunit -n Merkwerk.Web.Tests
dotnet new xunit -n Merkwerk.Data.IntegrationTests
dotnet new xunit -n Merkwerk.Service.IntegrationTests
dotnet new xunit -n Merkwerk.Architecture.Tests

# ---------- Projekte in Projektmappen-Ordner haengen -----------------
dotnet sln $sln add --solution-folder "01 Web"     Merkwerk.Web/Merkwerk.Web.csproj Merkwerk.Web.Client/Merkwerk.Web.Client.csproj
dotnet sln $sln add --solution-folder "02 Service" Merkwerk.Service/Merkwerk.Service.csproj
dotnet sln $sln add --solution-folder "03 Logic"   Merkwerk.Logic/Merkwerk.Logic.csproj Merkwerk.Logic.Shared/Merkwerk.Logic.Shared.csproj
dotnet sln $sln add --solution-folder "04 Data"    Merkwerk.Data.Context/Merkwerk.Data.Context.csproj Merkwerk.Data.Accessor/Merkwerk.Data.Accessor.csproj
dotnet sln $sln add --solution-folder "05 Shared"  Merkwerk.Shared/Merkwerk.Shared.csproj
dotnet sln $sln add --solution-folder "06 Tests"   `
    Merkwerk.Logic.Tests/Merkwerk.Logic.Tests.csproj `
    Merkwerk.Logic.Shared.Tests/Merkwerk.Logic.Shared.Tests.csproj `
    Merkwerk.Web.Tests/Merkwerk.Web.Tests.csproj `
    Merkwerk.Data.IntegrationTests/Merkwerk.Data.IntegrationTests.csproj `
    Merkwerk.Service.IntegrationTests/Merkwerk.Service.IntegrationTests.csproj `
    Merkwerk.Architecture.Tests/Merkwerk.Architecture.Tests.csproj

# ---------- Projektverweise (Abhaengigkeitsregeln, ADR 010) ----------
# Shared            -> (nichts)
# Logic.Shared      -> Shared
dotnet add Merkwerk.Logic.Shared reference Merkwerk.Shared
# Data.Context      -> Shared
dotnet add Merkwerk.Data.Context reference Merkwerk.Shared
# Data.Accessor     -> Data.Context, Shared
dotnet add Merkwerk.Data.Accessor reference Merkwerk.Data.Context Merkwerk.Shared
# Logic             -> Logic.Shared, Shared, Data.Accessor   (NIE direkt Data.Context)
dotnet add Merkwerk.Logic reference Merkwerk.Logic.Shared Merkwerk.Shared Merkwerk.Data.Accessor
# Service           -> Logic, Shared
dotnet add Merkwerk.Service reference Merkwerk.Logic Merkwerk.Shared
# Web.Client (WASM) -> Logic.Shared, Shared   (NIE Logic oder Data.*!)
dotnet add Merkwerk.Web.Client reference Merkwerk.Logic.Shared Merkwerk.Shared
# Web (Host)        -> Web.Client (vom Template gesetzt), Service, Logic, Data.Accessor + Data.Context (nur DI)
dotnet add Merkwerk.Web reference Merkwerk.Service Merkwerk.Logic Merkwerk.Data.Accessor Merkwerk.Data.Context

# Tests
dotnet add Merkwerk.Logic.Tests reference Merkwerk.Logic
dotnet add Merkwerk.Logic.Shared.Tests reference Merkwerk.Logic.Shared
dotnet add Merkwerk.Web.Tests reference Merkwerk.Web Merkwerk.Web.Client
dotnet add Merkwerk.Data.IntegrationTests reference Merkwerk.Data.Accessor Merkwerk.Data.Context
dotnet add Merkwerk.Service.IntegrationTests reference Merkwerk.Web
dotnet add Merkwerk.Architecture.Tests reference Merkwerk.Web Merkwerk.Web.Client Merkwerk.Service Merkwerk.Logic Merkwerk.Logic.Shared Merkwerk.Data.Context Merkwerk.Data.Accessor Merkwerk.Shared

# ---------- Service braucht ASP.NET Core (Controller) ----------------
# In Merkwerk.Service.csproj von Hand ergaenzen:
#   <ItemGroup>
#     <FrameworkReference Include="Microsoft.AspNetCore.App" />
#   </ItemGroup>

# ---------- Leere Class1.cs entfernen ---------------------------------
Get-ChildItem -Recurse -Filter Class1.cs | Remove-Item

# ---------- Pruefen ---------------------------------------------------
dotnet build $sln
Write-Host "Fertig. Merkwerk.slnx in Visual Studio neu laden." -ForegroundColor Green
