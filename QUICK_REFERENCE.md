# TravelNest - Quick Reference Guide

## 🚀 Quick Start

### Start Everything
```powershell
# Windows
docker-compose up -d

# Linux/Mac
docker-compose up -d
```

### Stop Everything
```powershell
docker-compose down
```

### View Logs
```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
```

---

## 📊 Management Commands

### Using Management Script (Recommended)

**Windows PowerShell:**
```powershell
.\docker-manage.ps1 up              # Start services
.\docker-manage.ps1 down            # Stop services
.\docker-manage.ps1 health          # Check health
.\docker-manage.ps1 logs            # View logs
.\docker-manage.ps1 restart-api     # Restart API
.\docker-manage.ps1 db-backup       # Backup database
.\docker-manage.ps1 db-restore -BackupFile backup.sql   # Restore
```

**Linux/Mac:**
```bash
chmod +x docker-manage.sh
./docker-manage.sh up              # Start services
./docker-manage.sh down            # Stop services
./docker-manage.sh health          # Check health
./docker-manage.sh logs            # View logs
./docker-manage.sh db-backup       # Backup database
./docker-manage.sh db-restore backup.sql  # Restore
```

### Direct Docker Commands

```powershell
# Build images
docker-compose build

# Rebuild without cache
docker-compose build --no-cache

# See running containers
docker-compose ps

# Execute command in container
docker exec travelnest-api dotnet --version

# Connect to database
docker exec -it travelnest-db psql -U travelnest_user -d travelnest
```

---

## 🏥 Health Check

### Run Health Check Script

**Windows:**
```powershell
.\health-check.ps1
```

**Linux/Mac:**
```bash
chmod +x health-check.sh
./health-check.sh
```

### Manual Health Checks

```powershell
# API Health
curl http://localhost:5000/health

# Database
docker exec travelnest-db pg_isready -U travelnest_user

# List services
docker-compose ps
```

---

## 🗄️ Database Operations

### Backup Database
```powershell
# Using script
.\docker-manage.ps1 db-backup

# Manual
docker exec travelnest-db pg_dump -U travelnest_user -d travelnest > backup.sql
```

### Restore Database
```powershell
# Using script
.\docker-manage.ps1 db-restore -BackupFile backup.sql

# Manual
Get-Content backup.sql | docker exec -i travelnest-db psql -U travelnest_user -d travelnest
```

### Database Shell
```powershell
# Using script
.\docker-manage.ps1 db-shell

# Manual
docker exec -it travelnest-db psql -U travelnest_user -d travelnest
```

### Useful SQL Commands
```sql
-- List tables
\dt

-- Describe table
\d table_name

-- List databases
\l

-- Count rows
SELECT COUNT(*) FROM table_name;

-- Find soft-deleted records
SELECT * FROM table_name WHERE "IsDeleted" = true;

-- Exit
\q
```

---

## 🌐 Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| API | http://localhost:5000 | - |
| Swagger UI | http://localhost:5000/swagger/index.html | - |
| Health Check | http://localhost:5000/health | - |
| PgAdmin | http://localhost:5050 | admin@travelnest.com / Admin123! |
| Database | localhost:5432 | travelnest_user / SecurePassword123! |

---

## ⚙️ Environment Configuration

Edit `.env` to change settings:

```env
# API
API_PORT=5000              # Change API port
ENVIRONMENT=Production    # Development/Production

# Database
DB_NAME=travelnest
DB_USER=travelnest_user
DB_PASSWORD=SecurePassword123!  # Change password!
DB_PORT=5432

# JWT
JWT_SECRET=<YOUR_SECRET_HERE>   # Generate new secret
JWT_ISSUER=https://travelnest.com
JWT_AUDIENCE=TravelNestAPI
JWT_EXPIRATION=60

# PgAdmin
PGADMIN_EMAIL=admin@travelnest.com
PGADMIN_PASSWORD=Admin123!      # Change password!
PGADMIN_PORT=5050
```

---

## 🔧 Common Tasks

### Rebuild API After Code Changes
```powershell
# Option 1: Using script
.\docker-manage.ps1 rebuild

# Option 2: Manual
docker-compose build --no-cache api
docker-compose up -d api
```

### View Recent Logs (Last 50 Lines)
```powershell
docker-compose logs --tail=50 api
```

### Restart All Services
```powershell
.\docker-manage.ps1 restart
```

### Remove Everything (Including Data)
```powershell
.\docker-manage.ps1 clean
# or manually
docker-compose down -v
```

### Check Container Resource Usage
```powershell
docker stats
```

---

## 🐛 Troubleshooting

### API Won't Start
```powershell
# Check logs
docker-compose logs api

# Common issues:
# 1. Port 5000 in use - change API_PORT in .env
# 2. Database not running - check docker-compose logs postgres
# 3. Code error - check for compilation issues
```

### Database Connection Failed
```powershell
# Check PostgreSQL
docker-compose logs postgres

# Verify it's ready
docker exec travelnest-db pg_isready

# Check connection string in .env
```

### Out of Disk Space
```powershell
# Clean up Docker
docker system prune -a

# Remove volumes
docker volume prune
```

### Port Already in Use
```powershell
# Find what's using port 5000
netstat -ano | findstr :5000

# Kill process (replace PID)
taskkill /PID <PID> /F

# Or change port in .env
```

---

## 📝 Soft Delete Usage

### Delete an Entity
```csharp
var repository = _unitOfWork.Repository<User>();
var user = await repository.GetByIdAsync(userId);

// Soft delete - sets IsDeleted = true
repository.Remove(user);
await _unitOfWork.SaveChangesAsync();
```

### Query (Auto-Excludes Deleted)
```csharp
// Automatically excludes soft-deleted records
var users = await repository.GetAllAsync();

// To include deleted records (with IgnoreQueryFilters)
var allUsers = await repository.Query()
    .IgnoreQueryFilters()
    .ToListAsync();
```

### Find Soft-Deleted Records
```sql
SELECT * FROM "User" WHERE "IsDeleted" = true;
```

---

## 📚 Documentation Files

- **DOCKER_SETUP.md** - Complete Docker guide
- **IMPLEMENTATION_SUMMARY.md** - Full implementation details
- **README.md** - Project overview

---

## 🚨 Important Notes

1. **Sensitive Data**
   - Never commit .env to version control
   - Keep .env.example for templates

2. **Database Backups**
   - Regular backups recommended
   - Use automated scripts for production

3. **Production Deployment**
   - Change all default passwords
   - Use strong JWT secret
   - Enable SSL/TLS
   - Configure proper resource limits

4. **Soft Delete**
   - All deletions are soft (non-destructive)
   - Data preserved for audit trail
   - Query filters automatically exclude deleted records

---

## 📞 Support

For issues:
1. Run `health-check.ps1` script
2. Check Docker logs: `docker-compose logs`
3. Review DOCKER_SETUP.md troubleshooting section
4. Check for error messages in application logs

---

**Version**: 1.0  
**Last Updated**: January 2024  
**Status**: ✅ Ready for Production
