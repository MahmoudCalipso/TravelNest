# TravelNest Docker Setup Guide

## Overview
This project is containerized with Docker and Docker Compose for easy deployment. The setup includes:
- **API**: ASP.NET Core 10 backend
- **Database**: PostgreSQL 16
- **Database Management**: PgAdmin (optional)

## Prerequisites
- Docker Desktop installed ([Download](https://www.docker.com/products/docker-desktop))
- Docker Compose (included with Docker Desktop)
- Git

## Quick Start

### 1. Clone the Repository
```bash
git clone <repository-url>
cd TravelNest
```

### 2. Configure Environment Variables
The `.env` file contains all configuration. Update it as needed:

```bash
# Database
DB_NAME=travelnest
DB_USER=travelnest_user
DB_PASSWORD=SecurePassword123!
DB_PORT=5432

# API
API_PORT=5000
ENVIRONMENT=Production

# JWT
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong1234567890
JWT_ISSUER=https://travelnest.com
JWT_AUDIENCE=TravelNestAPI
JWT_EXPIRATION=60

# PgAdmin
PGADMIN_PORT=5050
```

### 3. Start the Application
```bash
docker-compose up -d
```

The services will start:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger/index.html
- **Health Check**: http://localhost:5000/health
- **PgAdmin**: http://localhost:5050

## Common Docker Commands

### Start Services
```bash
# Start in background
docker-compose up -d

# Start with logs
docker-compose up

# Start specific service
docker-compose up -d postgres
```

### Stop Services
```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
```

### Check Service Status
```bash
# List running containers
docker-compose ps

# View service logs
docker-compose logs api

# Check health status
curl http://localhost:5000/health
```

### Rebuild Images
```bash
# Rebuild API image
docker-compose build --no-cache api

# Rebuild everything
docker-compose build --no-cache
```

## Database Management

### Using PgAdmin
1. Open http://localhost:5050
2. Login with credentials from `.env` (default: admin@travelnest.com / Admin123!)
3. Add server:
   - Host: postgres
   - Port: 5432
   - Database: travelnest
   - Username: travelnest_user
   - Password: SecurePassword123!

### Direct Database Access
```bash
# Connect to PostgreSQL
docker exec -it travelnest-db psql -U travelnest_user -d travelnest

# List tables
\dt

# Exit
\q
```

### Database Backup
```bash
# Create backup
docker exec travelnest-db pg_dump -U travelnest_user -d travelnest > backup.sql

# Restore backup
docker exec -i travelnest-db psql -U travelnest_user -d travelnest < backup.sql
```

## Health Checks

### Check API Health
```bash
curl http://localhost:5000/health
```

Response format:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "checks": {
    "EntityFrameworkCore": {
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    }
  }
}
```

### Monitor with Docker
```bash
# View container health status
docker-compose ps

# Get detailed health info
docker inspect travelnest-api | grep -A 20 "Health"
```

## Development Workflow

### Development Mode
For development, use a local environment file:

```bash
# Create .env.development
cp .env .env.development
# Edit .env.development with development values
ENVIRONMENT=Development
```

### Rebuild After Code Changes
```bash
# Rebuild API image
docker-compose build --no-cache api

# Restart API service
docker-compose up -d api
```

### Debug API
```bash
# View real-time logs
docker-compose logs -f api

# Get container ID
docker-compose ps

# Access container shell
docker exec -it travelnest-api /bin/bash
```

## Troubleshooting

### Services Won't Start
```bash
# Check Docker daemon
docker ps

# Check compose syntax
docker-compose config

# View startup logs
docker-compose logs
```

### Database Connection Failed
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check logs
docker-compose logs postgres

# Verify connection
docker exec travelnest-db pg_isready
```

### API Can't Connect to Database
1. Ensure PostgreSQL is healthy: `docker-compose logs postgres`
2. Check connection string in `.env`
3. Verify database name, user, and password match
4. Restart API: `docker-compose restart api`

### Port Already in Use
```bash
# Change ports in .env
API_PORT=5001
DB_PORT=5433
PGADMIN_PORT=5051

# Restart services
docker-compose down
docker-compose up -d
```

## Performance Tuning

### Database Performance
```bash
# Increase PostgreSQL memory
# Edit docker-compose.yml
postgres:
  environment:
    - shared_buffers=256MB
    - effective_cache_size=1GB
```

### API Performance
```bash
# Increase API memory limit
# Edit docker-compose.yml
api:
  deploy:
    resources:
      limits:
        memory: 2G
```

## Security Best Practices

1. **Change Default Passwords**
   - Update JWT_SECRET with a secure key
   - Change DB_PASSWORD
   - Change PGADMIN_PASSWORD

2. **Environment Variables**
   - Never commit `.env` to version control
   - Use `.env.example` for defaults
   - Add `.env` to `.gitignore`

3. **Network Security**
   - Use environment-specific networks
   - Limit port exposure
   - Use SSL/TLS for production

4. **Database Security**
   - Use strong passwords
   - Regular backups
   - Restrict direct access

## Production Deployment

### Using Docker Stack (Swarm)
```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.yml travelnest

# View services
docker stack services travelnest
```

### Using Kubernetes
```bash
# Generate k8s manifests
docker-compose convert > k8s-manifest.yaml

# Deploy to cluster
kubectl apply -f k8s-manifest.yaml
```

### Environment Configuration for Production
```env
ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443
JWT_SECRET=<use-secure-key-generator>
DB_PASSWORD=<use-strong-password>
PGADMIN_PASSWORD=<use-strong-password>
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/dotnet/architecture/containerized-lifecycle/)

## Support

For issues or questions:
1. Check the logs: `docker-compose logs`
2. Review the troubleshooting section above
3. Check Docker documentation
4. Open an issue on the repository

---

**Version**: 1.0  
**Last Updated**: 2024-01-15
