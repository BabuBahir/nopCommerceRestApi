# Render Free Tier Deployment Plan for nopCommerce

## Overview
Deploy nopCommerce (ASP.NET Core 10) on Render's free tier with MariaDB bundled inside the web service container.

## Constraints
- Render free tier: 0.1 CPU, 512 MB RAM
- No persistent disks (data lost on redeploy)
- No private services (`type: pserv`)
- No managed databases

## Architecture
Single web service with:
- ASP.NET Core 10 application
- MariaDB running inside the same container
- No persistent storage (acceptable per user requirements)

## Files to Modify

### 1. Dockerfile
Add MariaDB installation and configuration to the existing Dockerfile.

Changes:
- Install MariaDB in the runtime stage
- Create database initialization script
- Start MariaDB before the ASP.NET app
- Update entrypoint to manage both services

### 2. render.yml
Create a new render.yml at project root with:
- Single web service using Docker runtime
- Free tier plan
- Environment variables for database configuration
- Health check endpoint

### 3. appsettings.json
Update connection string to use localhost (since MariaDB runs locally in the container)

## Implementation Steps

### Step 1: Update Dockerfile
Add MariaDB to the runtime image and create an entrypoint script that:
1. Starts MariaDB service
2. Creates the nopcommerce database if it doesn't exist
3. Updates appsettings.json connection string
4. Starts the ASP.NET application

### Step 2: Create render.yml
```yaml
services:
  - type: web
    name: nopcommerce-web
    runtime: docker
    plan: free
    dockerfilePath: ./Dockerfile
    envVars:
      - key: MYSQL_ROOT_PASSWORD
        value: your_secure_password
      - key: MYSQL_DATABASE
        value: nopcommerce
      - key: ConnectionStrings__ConnectionString
        value: Server=localhost;User ID=root;Password=your_secure_password;Database=nopcommerce;Allow User Variables=True;Use XA Transactions=False;Connect Timeout=30;
      - key: ConnectionStrings__DataProvider
        value: mysql
    healthCheckPath: /health
```

### Step 3: Update appsettings.json
Modify the connection string to point to localhost since MariaDB runs in the same container.

## Local Docker Testing

### Test 1: Verify MariaDB Installation in Docker
```bash
# Pull the MariaDB Alpine image to test
docker pull mariadb:10.11-alpine

# Run MariaDB container to verify it works
docker run --name test-mariadb -e MYSQL_ROOT_PASSWORD=test123 -d mariadb:10.11-alpine

# Check if MariaDB is running
docker exec test-mariadb mariadb -u root -ptest123 -e "SHOW DATABASES;"

# Clean up
docker stop test-mariadb
docker rm test-mariadb
```

### Test 2: Build and Test the nopCommerce Docker Image
```bash
# Build the Docker image with the updated Dockerfile
docker build -t nopcommerce:latest .

# Run the container locally to test
docker run --name nopcommerce-test \
  -e MYSQL_ROOT_PASSWORD=test123 \
  -e MYSQL_DATABASE=nopcommerce \
  -p 8080:80 \
  -d nopcommerce:latest

# Check container logs
docker logs nopcommerce-test

# Test if the web app is responding
curl -I http://localhost:8080/

# Test if MariaDB is accessible from the container
docker exec nopcommerce-test mariadb -u root -ptest123 -e "SHOW DATABASES;"

# Clean up
docker stop nopcommerce-test
docker rm nopcommerce-test
```

### Test 3: Verify nopCommerce Database Initialization
```bash
# Run the container with a fresh volume
docker run --name nopcommerce-fresh \
  -e MYSQL_ROOT_PASSWORD=test123 \
  -e MYSQL_DATABASE=nopcommerce \
  -p 8080:80 \
  -d nopcommerce:latest

# Wait for initialization (check logs)
sleep 30
docker logs nopcommerce-fresh

# Verify database was created
docker exec nopcommerce-fresh mariadb -u root -ptest123 -e "SHOW DATABASES;" | grep nopcommerce

# Clean up
docker stop nopcommerce-fresh
docker rm nopcommerce-fresh
```

### Test 4: Memory Usage Check
```bash
# Run container and check memory usage
docker run --name nopcommerce-memory \
  -e MYSQL_ROOT_PASSWORD=test123 \
  -p 8080:80 \
  -d nopcommerce:latest

# Monitor memory usage
docker stats nopcommerce-memory

# Clean up
docker stop nopcommerce-memory
docker rm nopcommerce-memory
```

## Validation
1. Run all local Docker tests above to verify MariaDB and nopCommerce work together
2. Confirm the application starts without errors
3. Verify database connectivity from the application
4. Check that memory usage stays within 512 MB limit
5. Deploy to Render using the render.yml

## Risks
- Free tier may be slow for nopCommerce with 512 MB RAM
- No persistent storage means data loss on every redeploy
- MariaDB inside container adds memory overhead
- May need to adjust GC settings in Nop.Web.csproj for memory constraints