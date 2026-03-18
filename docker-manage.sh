#!/bin/bash

# TravelNest Docker Management Script
# Usage: ./docker-manage.sh [command]

set -e

COMPOSE_FILE="docker-compose.yml"
PROJECT_NAME="travelnest"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Help function
print_help() {
    cat << EOF
TravelNest Docker Management Script

Usage: ./docker-manage.sh [command]

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
    db-restore      Restore database from backup
    db-shell        Connect to database shell
    migrate         Run database migrations
    help            Show this help message

Examples:
    ./docker-manage.sh up
    ./docker-manage.sh logs
    ./docker-manage.sh db-backup
EOF
}

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker is not running${NC}"
        exit 1
    fi
}

# Function to print status
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Start services
start_services() {
    print_status "Starting TravelNest services..."
    docker-compose -f "$COMPOSE_FILE" up -d
    print_status "Services started successfully"
    sleep 3
    show_status
}

# Stop services
stop_services() {
    print_status "Stopping TravelNest services..."
    docker-compose -f "$COMPOSE_FILE" down
    print_status "Services stopped"
}

# Build images
build_images() {
    print_status "Building Docker images..."
    docker-compose -f "$COMPOSE_FILE" build
    print_status "Build completed"
}

# Rebuild images
rebuild_images() {
    print_status "Rebuilding Docker images (no cache)..."
    docker-compose -f "$COMPOSE_FILE" build --no-cache
    print_status "Rebuild completed"
}

# Show logs
show_logs() {
    docker-compose -f "$COMPOSE_FILE" logs -f
}

# Show API logs
show_api_logs() {
    docker-compose -f "$COMPOSE_FILE" logs -f api
}

# Show database logs
show_db_logs() {
    docker-compose -f "$COMPOSE_FILE" logs -f postgres
}

# Restart services
restart_services() {
    print_status "Restarting all services..."
    docker-compose -f "$COMPOSE_FILE" restart
    print_status "Services restarted"
}

# Restart API
restart_api() {
    print_status "Restarting API..."
    docker-compose -f "$COMPOSE_FILE" restart api
    print_status "API restarted"
}

# Restart database
restart_db() {
    print_status "Restarting database..."
    docker-compose -f "$COMPOSE_FILE" restart postgres
    print_status "Database restarted"
}

# Show service status
show_status() {
    docker-compose -f "$COMPOSE_FILE" ps
}

# Check health
check_health() {
    print_status "Checking service health..."
    
    # API health
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} API is healthy"
        curl -s http://localhost:5000/health | jq .
    else
        echo -e "${RED}✗${NC} API is not responding"
    fi
    
    # Database health
    if docker exec travelnest-db pg_isready > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} Database is healthy"
    else
        echo -e "${RED}✗${NC} Database is not responding"
    fi
}

# Clean up
clean_all() {
    print_warning "This will stop and remove all services and volumes!"
    read -p "Are you sure? (yes/no): " confirm
    
    if [ "$confirm" = "yes" ]; then
        print_status "Removing all services and volumes..."
        docker-compose -f "$COMPOSE_FILE" down -v
        print_status "Cleanup completed"
    else
        print_status "Cancelled"
    fi
}

# Database backup
backup_db() {
    BACKUP_FILE="travelnest_backup_$(date +%Y%m%d_%H%M%S).sql"
    print_status "Creating database backup: $BACKUP_FILE"
    docker exec travelnest-db pg_dump -U travelnest_user -d travelnest > "$BACKUP_FILE"
    print_status "Backup created: $BACKUP_FILE"
}

# Database restore
restore_db() {
    if [ -z "$1" ]; then
        print_error "Please provide backup file path"
        echo "Usage: ./docker-manage.sh db-restore <backup-file>"
        exit 1
    fi
    
    if [ ! -f "$1" ]; then
        print_error "Backup file not found: $1"
        exit 1
    fi
    
    print_warning "This will restore the database from backup!"
    read -p "Are you sure? (yes/no): " confirm
    
    if [ "$confirm" = "yes" ]; then
        print_status "Restoring database from: $1"
        docker exec -i travelnest-db psql -U travelnest_user -d travelnest < "$1"
        print_status "Database restored successfully"
    else
        print_status "Cancelled"
    fi
}

# Database shell
db_shell() {
    print_status "Connecting to database..."
    docker exec -it travelnest-db psql -U travelnest_user -d travelnest
}

# Run migrations
run_migrations() {
    print_status "Running database migrations..."
    docker exec travelnest-api dotnet ef database update --project TravelNest.Infrastructure
    print_status "Migrations completed"
}

# Main script logic
check_docker

case "${1:-help}" in
    up)
        start_services
        ;;
    down)
        stop_services
        ;;
    build)
        build_images
        ;;
    rebuild)
        rebuild_images
        ;;
    logs)
        show_logs
        ;;
    logs-api)
        show_api_logs
        ;;
    logs-db)
        show_db_logs
        ;;
    restart)
        restart_services
        ;;
    restart-api)
        restart_api
        ;;
    restart-db)
        restart_db
        ;;
    ps)
        show_status
        ;;
    health)
        check_health
        ;;
    clean)
        clean_all
        ;;
    db-backup)
        backup_db
        ;;
    db-restore)
        restore_db "$2"
        ;;
    db-shell)
        db_shell
        ;;
    migrate)
        run_migrations
        ;;
    help)
        print_help
        ;;
    *)
        print_error "Unknown command: $1"
        print_help
        exit 1
        ;;
esac
