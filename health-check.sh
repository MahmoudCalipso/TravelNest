#!/bin/bash

# TravelNest Solution Health Check Script
# This script verifies all components of the solution

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
PASSED=0
FAILED=0
WARNINGS=0

# Check result function
check_result() {
    local name=$1
    local result=$2
    local details=$3
    
    if [ $result -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $name"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} $name"
        if [ -n "$details" ]; then
            echo "  Details: $details"
        fi
        ((FAILED++))
    fi
}

warn_result() {
    local name=$1
    local details=$2
    
    echo -e "${YELLOW}⚠${NC} $name"
    if [ -n "$details" ]; then
        echo "  Details: $details"
    fi
    ((WARNINGS++))
}

# Print section header
print_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}\n"
}

# ============================================
# Start checks
# ============================================

print_header "TravelNest Solution Health Check"

# ============================================
# System Checks
# ============================================
print_header "1. System Requirements Check"

# Check Docker
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    echo -e "${GREEN}✓${NC} Docker installed: $DOCKER_VERSION"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} Docker is not installed"
    ((FAILED++))
fi

# Check Docker Compose
if command -v docker-compose &> /dev/null; then
    COMPOSE_VERSION=$(docker-compose --version)
    echo -e "${GREEN}✓${NC} Docker Compose installed: $COMPOSE_VERSION"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} Docker Compose is not installed"
    ((FAILED++))
fi

# Check .NET SDK
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✓${NC} .NET SDK installed: $DOTNET_VERSION"
    ((PASSED++))
else
    warn_result ".NET SDK not installed (required for local development)"
fi

# Check Git
if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version)
    echo -e "${GREEN}✓${NC} Git installed: $GIT_VERSION"
    ((PASSED++))
else
    warn_result "Git is not installed"
fi

# ============================================
# Project Structure Checks
# ============================================
print_header "2. Project Structure Check"

# Check solution file
if [ -f "TravelNest.sln" ]; then
    echo -e "${GREEN}✓${NC} Solution file found: TravelNest.sln"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} Solution file not found: TravelNest.sln"
    ((FAILED++))
fi

# Check project files
declare -a projects=(
    "TravelNest.API/TravelNest.API.csproj"
    "TravelNest.Application/TravelNest.Application.csproj"
    "TravelNest.Domain/TravelNest.Domain.csproj"
    "TravelNest.Infrastructure/TravelNest.Infrastructure.csproj"
)

for project in "${projects[@]}"; do
    if [ -f "$project" ]; then
        echo -e "${GREEN}✓${NC} Project found: $project"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} Project not found: $project"
        ((FAILED++))
    fi
done

# ============================================
# Docker Configuration Checks
# ============================================
print_header "3. Docker Configuration Check"

# Check Dockerfile
if [ -f "Dockerfile" ]; then
    echo -e "${GREEN}✓${NC} Dockerfile found"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} Dockerfile not found"
    ((FAILED++))
fi

# Check docker-compose.yml
if [ -f "docker-compose.yml" ]; then
    echo -e "${GREEN}✓${NC} docker-compose.yml found"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} docker-compose.yml not found"
    ((FAILED++))
fi

# Check .env file
if [ -f ".env" ]; then
    echo -e "${GREEN}✓${NC} .env file found"
    ((PASSED++))
else
    echo -e "${RED}✗${NC} .env file not found"
    echo "  Run: cp .env.example .env"
    ((FAILED++))
fi

# Check .dockerignore
if [ -f ".dockerignore" ]; then
    echo -e "${GREEN}✓${NC} .dockerignore found"
    ((PASSED++))
else
    warn_result ".dockerignore not found (recommended)"
fi

# ============================================
# Configuration Checks
# ============================================
print_header "4. Configuration Check"

# Check if .env has required values
if [ -f ".env" ]; then
    source .env
    
    # Check database config
    if [ -z "$DB_NAME" ] || [ -z "$DB_USER" ] || [ -z "$DB_PASSWORD" ]; then
        warn_result "Incomplete database configuration in .env"
    else
        echo -e "${GREEN}✓${NC} Database configuration present"
        ((PASSED++))
    fi
    
    # Check JWT config
    if [ -z "$JWT_SECRET" ] || [ -z "$JWT_ISSUER" ]; then
        warn_result "Incomplete JWT configuration in .env"
    else
        echo -e "${GREEN}✓${NC} JWT configuration present"
        ((PASSED++))
    fi
else
    echo -e "${RED}✗${NC} .env file not found, cannot check configuration"
    ((FAILED++))
fi

# ============================================
# Docker Service Checks
# ============================================
print_header "5. Docker Service Status Check"

if command -v docker &> /dev/null; then
    # Check if Docker daemon is running
    if docker info > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} Docker daemon is running"
        ((PASSED++))
        
        # Check running containers
        RUNNING=$(docker-compose ps -q 2>/dev/null | wc -l)
        if [ $RUNNING -gt 0 ]; then
            echo -e "${GREEN}✓${NC} Services are running ($RUNNING containers)"
            ((PASSED++))
            
            # Check each service
            if docker-compose ps | grep -q "travelnest-api"; then
                echo -e "${GREEN}✓${NC} API service is running"
                ((PASSED++))
            else
                echo -e "${RED}✗${NC} API service is not running"
                ((FAILED++))
            fi
            
            if docker-compose ps | grep -q "travelnest-db"; then
                echo -e "${GREEN}✓${NC} Database service is running"
                ((PASSED++))
            else
                echo -e "${RED}✗${NC} Database service is not running"
                ((FAILED++))
            fi
        else
            warn_result "No services running currently"
            echo "  Run: docker-compose up -d"
        fi
    else
        echo -e "${RED}✗${NC} Docker daemon is not running"
        ((FAILED++))
    fi
fi

# ============================================
# API Health Checks
# ============================================
print_header "6. API Health Check"

if command -v curl &> /dev/null; then
    # Check API health endpoint
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} API health endpoint responding"
        ((PASSED++))
        
        # Get detailed health info
        HEALTH_STATUS=$(curl -s http://localhost:5000/health | grep -o '"status":"[^"]*"' | cut -d'"' -f4)
        if [ "$HEALTH_STATUS" == "Healthy" ]; then
            echo -e "${GREEN}✓${NC} API health status: $HEALTH_STATUS"
            ((PASSED++))
        else
            warn_result "API health status: $HEALTH_STATUS"
        fi
    else
        warn_result "API health endpoint not responding"
        echo "  API may not be running: docker-compose up -d api"
    fi
    
    # Check Swagger UI
    if curl -s http://localhost:5000/swagger/index.html > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} Swagger UI is accessible"
        ((PASSED++))
    else
        warn_result "Swagger UI not accessible"
    fi
else
    warn_result "curl not installed, skipping HTTP checks"
fi

# ============================================
# Database Checks
# ============================================
print_header "7. Database Checks"

if docker ps | grep -q "travelnest-db"; then
    if docker exec travelnest-db pg_isready -U travelnest_user > /dev/null 2>&1; then
        echo -e "${GREEN}✓${NC} Database is responding"
        ((PASSED++))
        
        # Check if database exists
        DB_EXISTS=$(docker exec travelnest-db psql -U travelnest_user -lqt | cut -d '|' -f 1 | grep -w travelnest | wc -l)
        if [ $DB_EXISTS -gt 0 ]; then
            echo -e "${GREEN}✓${NC} Database 'travelnest' exists"
            ((PASSED++))
            
            # Count tables
            TABLE_COUNT=$(docker exec travelnest-db psql -U travelnest_user -d travelnest -c "\dt" | grep "public" | wc -l)
            echo -e "${GREEN}✓${NC} Database contains $TABLE_COUNT tables"
            ((PASSED++))
        else
            warn_result "Database 'travelnest' not found"
        fi
    else
        echo -e "${RED}✗${NC} Database is not responding"
        ((FAILED++))
    fi
else
    warn_result "Database container not running"
fi

# ============================================
# File Integrity Checks
# ============================================
print_header "8. File Integrity Check"

# Check critical config files
declare -a critical_files=(
    "TravelNest.Infrastructure/DependencyInjection.cs"
    "TravelNest.API/Program.cs"
    "TravelNest.Infrastructure/Data/ApplicationDbContext.cs"
)

for file in "${critical_files[@]}"; do
    if [ -f "$file" ]; then
        echo -e "${GREEN}✓${NC} Critical file present: $file"
        ((PASSED++))
    else
        echo -e "${RED}✗${NC} Critical file missing: $file"
        ((FAILED++))
    fi
done

# ============================================
# Summary
# ============================================
print_header "Summary"

TOTAL=$((PASSED + FAILED + WARNINGS))

echo "Total Checks: $TOTAL"
echo -e "${GREEN}Passed: $PASSED${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo -e "${YELLOW}Warnings: $WARNINGS${NC}"

if [ $FAILED -eq 0 ]; then
    echo -e "\n${GREEN}✓ Solution health check passed!${NC}\n"
    
    # Show next steps
    echo "Next Steps:"
    echo "1. Start services: docker-compose up -d"
    echo "2. View logs: docker-compose logs -f"
    echo "3. Check health: curl http://localhost:5000/health"
    echo "4. View Swagger: http://localhost:5000/swagger/index.html"
    echo ""
    exit 0
else
    echo -e "\n${RED}✗ Solution health check found issues!${NC}\n"
    echo "Please address the failures above and run this check again."
    echo ""
    exit 1
fi
