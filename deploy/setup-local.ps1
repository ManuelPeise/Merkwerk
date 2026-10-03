# =====================================================================
#  Merkwerk - local development environment (LP-004)
#  Run from the repository root:   .\deploy\setup-local.ps1
#
#  1. creates deploy\.env with random passwords (only if it doesn't exist yet)
#  2. starts the MySQL container (waits until it is healthy) and Mailpit
#  3. stores connection string and JWT key as user secrets of the Web project
#  4. checks the database (version, character set, collation)
#  Safe to run again: existing .env and data are kept.
# =====================================================================
$ErrorActionPreference = "Stop"

$repoRoot    = Split-Path $PSScriptRoot -Parent
$composeFile = Join-Path $PSScriptRoot "docker-compose.yml"
$envFile     = Join-Path $PSScriptRoot ".env"
$envExample  = Join-Path $PSScriptRoot ".env.example"
$webProject  = Join-Path $repoRoot "sources\Web.Core"

function New-Secret([int]$bytes = 24) {
    $buffer = [byte[]]::new($bytes)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()   # works in Windows PowerShell 5.1 and PowerShell 7
    $rng.GetBytes($buffer)
    $rng.Dispose()
    # URL-safe, no characters that break a connection string (; = ")
    return [Convert]::ToBase64String($buffer).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Invoke-Checked([scriptblock]$command, [string]$description) {
    & $command
    if ($LASTEXITCODE -ne 0) { throw "$description failed (exit code $LASTEXITCODE)" }
}

# ---------- 1. deploy\.env ---------------------------------------------
if (-not (Test-Path $envFile)) {
    Write-Host "Creating deploy\.env with random passwords ..." -ForegroundColor Cyan
    $content = Get-Content $envExample -Raw
    $content = $content -replace '(?m)^MERKWERK_HOST=.*$',    'MERKWERK_HOST=localhost'
    $content = $content -replace '(?m)^DB_PASSWORD=.*$',      "DB_PASSWORD=$(New-Secret)"
    $content = $content -replace '(?m)^DB_ROOT_PASSWORD=.*$', "DB_ROOT_PASSWORD=$(New-Secret)"
    $content = $content -replace '(?m)^JWT_SIGNING_KEY=.*$',  "JWT_SIGNING_KEY=$(New-Secret 48)"
    Set-Content -Path $envFile -Value $content -NoNewline
} else {
    Write-Host "deploy\.env exists - keeping it." -ForegroundColor DarkGray
}

# read KEY=VALUE pairs
$settings = @{}
Get-Content $envFile | Where-Object { $_ -match '^\s*[A-Z_]+=' } | ForEach-Object {
    $key, $value = $_ -split '=', 2
    $settings[$key.Trim()] = $value.Trim()
}

# ---------- 2. MySQL container ------------------------------------------
Write-Host "Starting MySQL and Mailpit containers ..." -ForegroundColor Cyan
Invoke-Checked { docker compose -f $composeFile --profile dev up -d db mailpit } "docker compose up"

$containerId = (docker compose -f $composeFile ps -q db).Trim()
$deadline = (Get-Date).AddMinutes(2)
do {
    Start-Sleep -Seconds 3
    $health = (docker inspect -f '{{.State.Health.Status}}' $containerId).Trim()
    Write-Host "  database: $health"
} while ($health -ne 'healthy' -and (Get-Date) -lt $deadline)
if ($health -ne 'healthy') { throw "MySQL did not become healthy within 2 minutes. Check: docker compose -f deploy/docker-compose.yml logs db" }

# ---------- 3. user secrets of the Web project ---------------------------
Write-Host "Storing user secrets for sources\Web.Core ..." -ForegroundColor Cyan
$connection = 'Server=localhost;Port=3306;Database=' + $settings['DB_NAME'] + ';User=' + $settings['DB_USER'] + ';Password=' + $settings['DB_PASSWORD']
Invoke-Checked { dotnet user-secrets set "ConnectionStrings:Default" $connection --project $webProject } "user-secrets (connection string)"
Invoke-Checked { dotnet user-secrets set "Auth:Jwt:SigningKey" $settings['JWT_SIGNING_KEY'] --project $webProject } "user-secrets (JWT key)"

# ---------- 4. check the database ----------------------------------------
Write-Host "Checking database ..." -ForegroundColor Cyan
$sql = 'SELECT VERSION() AS version, @@character_set_server AS charset, @@collation_server AS collation;'
$mysqlPassword = 'MYSQL_PWD=' + $settings['DB_PASSWORD']
$checkArgs = @('compose', '-f', $composeFile, 'exec', '-T', '-e', $mysqlPassword, 'db',
               'mysql', '-u', $settings['DB_USER'], '-D', $settings['DB_NAME'], '-e', $sql)
& docker @checkArgs
if ($LASTEXITCODE -ne 0) { throw "database check failed (exit code $LASTEXITCODE)" }

Write-Host ""
Write-Host "Local environment is ready." -ForegroundColor Green
Write-Host ('  MySQL:    localhost:3306, database ' + $settings['DB_NAME'] + ', user ' + $settings['DB_USER'])
Write-Host "  Mailpit:      http://localhost:8025 (SMTP localhost:1025) - every mail lands here"
Write-Host "  Secrets:      dotnet user-secrets list --project sources\Web.Core"
Write-Host "  Stop:         docker compose -f deploy/docker-compose.yml stop db mailpit"
Write-Host "  Reset (DATA LOSS): docker compose -f deploy/docker-compose.yml down -v"
