# TravelNest Solution Health Check Script for Windows
# This script verifies all components of the solution

$ErrorActionPreference = "Continue"

# Counters
$passed = 0
$failed = 0
$warnings = 0

# Helper functions
function Print-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Print-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Print-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Print-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host $Title -ForegroundColor Blue
    Write-Host "========================================" -ForegroundColor Blue
    Write-Host ""
}

function Check-Passed {
    param(
        [string]$Name,
        [string]$Details
    )
    Write-Host "✓ $Name" -ForegroundColor Green
    if ($Details) {
        Write-Host "  Details: $Details"
    }
    $script:passed++
}

function Check-Failed {
    param(
        [string]$Name,
        [string]$Details
    )
    Write-Host "✗ $Name" -ForegroundColor Red
    if ($Details) {
        Write-Host "  Details: $Details"
    }
    $script:failed++
}

function Check-Warning {
    param(
        [string]$Name,
        [string]$Details
    )
    Write-Host "⚠ $Name" -ForegroundColor Yellow
    if ($Details) {
        Write-Host "  Details: $Details"
    }
    $script:warnings++
}

# ============================================
# System Checks
# ============================================
Print-Header "1. System Requirements Check"

# Check Docker
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $dockerVersion = docker --version
    Check-Passed "Docker installed" $dockerVersion
}
else {
    Check-Failed "Docker is not installed"
}

# Check Docker Desktop
try {
    if (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue) {
        Check-Passed "Docker Desktop is running"
    }
    else {
        Check-Warning "Docker Desktop may not be running"
    }
}
catch {
    Check-Warning "Could not verify Docker Desktop status"
}

# Check Docker Compose
if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
    $composeVersion = docker-compose --version
    Check-Passed "Docker Compose installed" $composeVersion
}
else {
    Check-Failed "Docker Compose is not installed"
}

# Check .NET SDK
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnetVersion = dotnet --version
    Check-Passed ".NET SDK installed" $dotnetVersion
}
else {
    Check-Warning ".NET SDK not installed (required for local development)"
}

# Check Git
if (Get-Command git -ErrorAction SilentlyContinue) {
    $gitVersion = git --version
    Check-Passed "Git installed" $gitVersion
}
else {
    Check-Warning "Git is not installed"
}

# ============================================
# Project Structure Checks
# ============================================
Print-Header "2. Project Structure Check"

# Check solution file
if (Test-Path "TravelNest.sln") {
    Check-Passed "Solution file found: TravelNest.sln"
}
else {
    Check-Failed "Solution file not found: TravelNest.sln"
}

# Check project files
$projects = @(
    "TravelNest.API/TravelNest.API.csproj",
    "TravelNest.Application/TravelNest.Application.csproj",
    "TravelNest.Domain/TravelNest.Domain.csproj",
    "TravelNest.Infrastructure/TravelNest.Infrastructure.csproj"
)

foreach ($project in $projects) {
    if (Test-Path $project) {
        Check-Passed "Project found: $project"
    }
    else {
        Check-Failed "Project not found: $project"
    }
}

# ============================================
# Docker Configuration Checks
# ============================================
Print-Header "3. Docker Configuration Check"

# Check Dockerfile
if (Test-Path "Dockerfile") {
    Check-Passed "Dockerfile found"
}
else {
    Check-Failed "Dockerfile not found"
}

# Check docker-compose.yml
if (Test-Path "docker-compose.yml") {
    Check-Passed "docker-compose.yml found"
}
else {
    Check-Failed "docker-compose.yml not found"
}

# Check .env file
if (Test-Path ".env") {
    Check-Passed ".env file found"
}
else {
    Check-Failed ".env file not found"
    Write-Host "  Run: Copy-Item .env.example -Destination .env"
}

# Check .dockerignore
if (Test-Path ".dockerignore") {
    Check-Passed ".dockerignore found"
}
else {
    Check-Warning ".dockerignore not found (recommended)"
}

# ============================================
# Configuration Checks
# ============================================
Print-Header "4. Configuration Check"

if (Test-Path ".env") {
    $envContent = Get-Content ".env"
    
    # Check database config
    if ($envContent -match "DB_NAME=" -and $envContent -match "DB_USER=" -and $envContent -match "DB_PASSWORD=") {
        Check-Passed "Database configuration present"
    }
    else {
        Check-Warning "Incomplete database configuration in .env"
    }
    
    # Check JWT config
    if ($envContent -match "JWT_SECRET=" -and $envContent -match "JWT_ISSUER=") {
        Check-Passed "JWT configuration present"
    }
    else {
        Check-Warning "Incomplete JWT configuration in .env"
    }
}
else {
    Check-Failed ".env file not found, cannot check configuration"
}

# ============================================
# Docker Service Checks
# ============================================
Print-Header "5. Docker Service Status Check"

try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        Check-Passed "Docker daemon is running"
        
        # Check running containers
        $containers = docker-compose ps -q 2>&1
        $containerCount = ($containers | Measure-Object -Line).Lines
        
        if ($containerCount -gt 0) {
            Check-Passed "Services are running" "$containerCount containers"
            
            # Check API service
            $psOutput = docker-compose ps
            if ($psOutput -match "travelnest-api") {
                Check-Passed "API service is running"
            }
            else {
                Check-Failed "API service is not running"
            }
            
            # Check Database service
            if ($psOutput -match "travelnest-db") {
                Check-Passed "Database service is running"
            }
            else {
                Check-Failed "Database service is not running"
            }
        }
        else {
            Check-Warning "No services running currently"
            Write-Host "  Run: docker-compose up -d"
        }
    }
}
catch {
    Check-Failed "Docker daemon is not running"
}

# ============================================
# API Health Checks
# ============================================
Print-Header "6. API Health Check"

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Check-Passed "API health endpoint responding"
        
        $jsonResponse = $response.Content | ConvertFrom-Json
        if ($jsonResponse.status -eq "Healthy") {
            Check-Passed "API health status: Healthy"
        }
        else {
            Check-Warning "API health status: $($jsonResponse.status)"
        }
    }
}
catch {
    Check-Warning "API health endpoint not responding"
    Write-Host "  API may not be running: docker-compose up -d api"
}

# Check Swagger UI
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/index.html" -UseBasicParsing -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Check-Passed "Swagger UI is accessible"
    }
}
catch {
    Check-Warning "Swagger UI not accessible"
}

# ============================================
# Database Checks
# ============================================
Print-Header "7. Database Checks"

try {
    $isReady = docker exec travelnest-db pg_isready -U travelnest_user 2>&1
    if ($LASTEXITCODE -eq 0) {
        Check-Passed "Database is responding"
        
        # Check if database exists
        $dbList = docker exec travelnest-db psql -U travelnest_user -lqt 2>&1
        if ($dbList -match "travelnest") {
            Check-Passed "Database 'travelnest' exists"
            
            # Count tables
            $tables = docker exec travelnest-db psql -U travelnest_user -d travelnest -c "\dt" 2>&1
            $tableCount = ($tables | Measure-Object -Line).Lines
            Check-Passed "Database contains tables" "$tableCount rows"
        }
        else {
            Check-Warning "Database 'travelnest' not found"
        }
    }
    else {
        Check-Failed "Database is not responding"
    }
}
catch {
    Check-Warning "Database container not running"
}

# ============================================
# File Integrity Checks
# ============================================
Print-Header "8. File Integrity Check"

$criticalFiles = @(
    "TravelNest.Infrastructure/DependencyInjection.cs",
    "TravelNest.API/Program.cs",
    "TravelNest.Infrastructure/Data/ApplicationDbContext.cs"
)

foreach ($file in $criticalFiles) {
    if (Test-Path $file) {
        Check-Passed "Critical file present: $file"
    }
    else {
        Check-Failed "Critical file missing: $file"
    }
}

# ============================================
# Summary
# ============================================
Print-Header "Summary"

$total = $passed + $failed + $warnings

Write-Host "Total Checks: $total"
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red
Write-Host "Warnings: $warnings" -ForegroundColor Yellow

if ($failed -eq 0) {
    Write-Host ""
    Write-Host "✓ Solution health check passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:"
    Write-Host "1. Start services: docker-compose up -d"
    Write-Host "2. View logs: docker-compose logs -f"
    Write-Host "3. Check health: curl http://localhost:5000/health"
    Write-Host "4. View Swagger: http://localhost:5000/swagger/index.html"
    Write-Host ""
    exit 0
}
else {
    Write-Host ""
    Write-Host "✗ Solution health check found issues!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please address the failures above and run this check again."
    Write-Host ""
    exit 1
}
