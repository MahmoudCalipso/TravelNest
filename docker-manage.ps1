# TravelNest Docker Management Script for Windows
# Usage: .\docker-manage.ps1 [command]

param(
    [Parameter(Position = 0)]
    [string]$Command = "help",
    [Parameter(Position = 1)]
    [string]$BackupFile
)

$composeFile = "docker-compose.yml"
$projectName = "travelnest"
$apiContainer = "travelnest-api"
$dbContainer = "travelnest-db"

# Function to print colored output
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

# Function to check if Docker is running
function Check-Docker {
    try {
        docker ps > $null 2>&1
    }
    catch {
        Print-Error "Docker is not running"
        exit 1
    }
}

# Start services
function Start-Services {
    Print-Status "Starting TravelNest services..."
    docker-compose -f $composeFile up -d
    Print-Status "Services started successfully"
    Start-Sleep -Seconds 3
    Show-Status
}

# Stop services
function Stop-Services {
    Print-Status "Stopping TravelNest services..."
    docker-compose -f $composeFile down
    Print-Status "Services stopped"
}

# Build images
function Build-Images {
    Print-Status "Building Docker images..."
    docker-compose -f $composeFile build
    Print-Status "Build completed"
}

# Rebuild images
function Rebuild-Images {
    Print-Status "Rebuilding Docker images (no cache)..."
    docker-compose -f $composeFile build --no-cache
    Print-Status "Rebuild completed"
}

# Show logs
function Show-Logs {
    docker-compose -f $composeFile logs -f
}

# Show API logs
function Show-ApiLogs {
    docker-compose -f $composeFile logs -f api
}

# Show database logs
function Show-DbLogs {
    docker-compose -f $composeFile logs -f postgres
}

# Restart services
function Restart-Services {
    Print-Status "Restarting all services..."
    docker-compose -f $composeFile restart
    Print-Status "Services restarted"
}

# Restart API
function Restart-Api {
    Print-Status "Restarting API..."
    docker-compose -f $composeFile restart api
    Print-Status "API restarted"
}

# Restart database
function Restart-Db {
    Print-Status "Restarting database..."
    docker-compose -f $composeFile restart postgres
    Print-Status "Database restarted"
}

# Show service status
function Show-Status {
    docker-compose -f $composeFile ps
}

# Check health
function Check-Health {
    Print-Status "Checking service health..."
    
    # API health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing -ErrorAction Stop
        Write-Host "✓ API is healthy" -ForegroundColor Green
        $response.Content | ConvertFrom-Json | ConvertTo-Json
    }
    catch {
        Write-Host "✗ API is not responding" -ForegroundColor Red
    }
    
    # Database health
    try {
        docker exec $dbContainer pg_isready > $null 2>&1
        Write-Host "✓ Database is healthy" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Database is not responding" -ForegroundColor Red
    }
}

# Clean up
function Clean-All {
    Print-Warning "This will stop and remove all services and volumes!"
    $confirm = Read-Host "Are you sure? (yes/no)"
    
    if ($confirm -eq "yes") {
        Print-Status "Removing all services and volumes..."
        docker-compose -f $composeFile down -v
        Print-Status "Cleanup completed"
    }
    else {
        Print-Status "Cancelled"
    }
}

# Database backup
function Backup-Db {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = "travelnest_backup_$timestamp.sql"
    Print-Status "Creating database backup: $backupFile"
    docker exec $dbContainer pg_dump -U travelnest_user -d travelnest | Out-File -FilePath $backupFile -Encoding ASCII
    Print-Status "Backup created: $backupFile"
}

# Database restore
function Restore-Db {
    if (-not $BackupFile) {
        Print-Error "Please provide backup file path"
        Write-Host "Usage: .\docker-manage.ps1 db-restore -BackupFile <path-to-backup>"
        exit 1
    }
    
    if (-not (Test-Path $BackupFile)) {
        Print-Error "Backup file not found: $BackupFile"
        exit 1
    }
    
    Print-Warning "This will restore the database from backup!"
    $confirm = Read-Host "Are you sure? (yes/no)"
    
    if ($confirm -eq "yes") {
        Print-Status "Restoring database from: $BackupFile"
        Get-Content $BackupFile | docker exec -i $dbContainer psql -U travelnest_user -d travelnest
        Print-Status "Database restored successfully"
    }
    else {
        Print-Status "Cancelled"
    }
}

# Database shell
function Open-DbShell {
    Print-Status "Connecting to database..."
    docker exec -it $dbContainer psql -U travelnest_user -d travelnest
}

# Run migrations
function Run-Migrations {
    Print-Status "Running database migrations..."
    docker exec $apiContainer dotnet ef database update --project TravelNest.Infrastructure
    Print-Status "Migrations completed"
}

# Print help
function Print-Help {
    Write-Host @"
TravelNest Docker Management Script

Usage: .\docker-manage.ps1 [command] [options]

Commands:
    up              Start all services
    down            Stop all services
    build           Build images
    rebuild         Rebuild images (no cache)
    logs            Show real-time logs
    logs-api        Show API logs
    logs-db         Show database logs
    restart         Restart all services
    restart-api     Restart API service
    restart-db      Restart database service
    ps              Show running services
    health          Check service health
    clean           Stop and remove all (including volumes)
    db-backup       Create database backup
    db-restore      Restore database from backup (-BackupFile required)
    db-shell        Connect to database shell
    migrate         Run database migrations
    help            Show this help message

Examples:
    .\docker-manage.ps1 up
    .\docker-manage.ps1 logs
    .\docker-manage.ps1 db-backup
    .\docker-manage.ps1 db-restore -BackupFile travelnest_backup_20240115_120000.sql
"@
}

# Main script logic
Check-Docker

switch ($Command.ToLower()) {
    "up" { Start-Services }
    "down" { Stop-Services }
    "build" { Build-Images }
    "rebuild" { Rebuild-Images }
    "logs" { Show-Logs }
    "logs-api" { Show-ApiLogs }
    "logs-db" { Show-DbLogs }
    "restart" { Restart-Services }
    "restart-api" { Restart-Api }
    "restart-db" { Restart-Db }
    "ps" { Show-Status }
    "health" { Check-Health }
    "clean" { Clean-All }
    "db-backup" { Backup-Db }
    "db-restore" { Restore-Db }
    "db-shell" { Open-DbShell }
    "migrate" { Run-Migrations }
    "help" { Print-Help }
    default {
        Print-Error "Unknown command: $Command"
        Print-Help
        exit 1
    }
}
