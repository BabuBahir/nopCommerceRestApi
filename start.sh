#!/bin/sh
set -e

# Initialize MariaDB data directory if not already initialized
if [ ! -f /app/.db_initialized ]; then
    echo "Initializing MariaDB..."
    
    # Start MariaDB temporarily for initialization
    /usr/bin/mariadbd --user=mysql --datadir=/var/lib/mysql --socket=/run/mysqld/mysqld.sock &
    MARIA_PID=$!
    
    # Wait for MariaDB to be ready
    for i in $(seq 1 30); do
        if /usr/bin/mariadb --socket=/run/mysqld/mysqld.sock -u root -e "SELECT 1" >/dev/null 2>&1; then
            echo "MariaDB is ready"
            break
        fi
        echo "Waiting for MariaDB... attempt $i"
        sleep 2
    done
    
    # Set root password and create database
    /usr/bin/mariadb --socket=/run/mysqld/mysqld.sock -u root << EOF
ALTER USER 'root'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD:-nopCommerce_db_password}';
CREATE DATABASE IF NOT EXISTS nopcommerce;
CREATE USER IF NOT EXISTS 'nopcommerce'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD:-nopCommerce_db_password}';
GRANT ALL PRIVILEGES ON nopcommerce.* TO 'nopcommerce'@'localhost';
FLUSH PRIVILEGES;
EOF
    
    # Stop MariaDB
    /usr/bin/mariadb-admin --socket=/run/mysqld/mysqld.sock -u root -p"${MYSQL_PASSWORD:-nopCommerce_db_password}" shutdown
    wait $MARIA_PID
    
    touch /app/.db_initialized
    echo "Database initialized"
fi

# Modify MariaDB configuration to enable TCP networking
sed -i 's/skip-networking/#skip-networking/' /etc/my.cnf.d/mariadb-server.cnf
sed -i 's/#bind-address=0.0.0.0/bind-address=0.0.0.0/' /etc/my.cnf.d/mariadb-server.cnf

# Start MariaDB in the background with TCP enabled
echo "Starting MariaDB..."
/usr/bin/mariadbd --user=mysql --datadir=/var/lib/mysql --socket=/run/mysqld/mysqld.sock &

# Wait for MariaDB to be ready
for i in $(seq 1 30); do
    if /usr/bin/mariadb --socket=/run/mysqld/mysqld.sock -u root -p"${MYSQL_PASSWORD:-nopCommerce_db_password}" -e "SELECT 1" >/dev/null 2>&1; then
        echo "MariaDB is ready"
        break
    fi
    echo "Waiting for MariaDB... attempt $i"
    sleep 2
done

# Update appsettings.json with the correct connection string
echo "Updating connection string..."
cat > /app/App_Data/appsettings.json << EOF
{
  "ConnectionStrings": {
    "ConnectionString": "Server=localhost;User ID=root;Password=${MYSQL_PASSWORD:-nopCommerce_db_password};Database=nopcommerce;Allow User Variables=True;Use XA Transactions=False;Connect Timeout=30;",
    "DataProvider": "mysql",
    "SQLCommandTimeout": 3000,
    "WithNoLock": false,
    "Collation": null,
    "CharacterSet": null,
    "CloseDataContextAfterUse": true,
    "BulkCopyWithCheckConstraints": true
  },
  "CacheConfig": {
    "DefaultCacheTime": 60,
    "LinqDisableQueryCache": false
  },
  "CommonConfig": {
    "DisplayFullErrorStack": false,
    "UserAgentStringsPath": "~/App_Data/browscap.xml",
    "CrawlerOnlyUserAgentStringsPath": "~/App_Data/browscap.crawlersonly.xml",
    "CrawlerOnlyAdditionalUserAgentStringsPath": "~/App_Data/additional.crawlers.xml",
    "UseSessionStateTempDataProvider": false,
    "ScheduleTaskRunTimeout": null,
    "StaticFilesCacheControl": "public,max-age=31536000",
    "ServeUnknownFileTypes": false,
    "UseAutofac": true,
    "PermitLimit": 0,
    "QueueCount": 0,
    "RejectionStatusCode": 503
  },
  "DistributedCacheConfig": {
    "DistributedCacheType": "redissynchronizedmemory",
    "Enabled": false,
    "ConnectionString": "127.0.0.1:6379,ssl=False",
    "SchemaName": "dbo",
    "TableName": "DistributedCache",
    "InstanceName": "nopCommerce",
    "PublishIntervalMs": 500
  },
  "HostingConfig": {
    "UseProxy": false,
    "ForwardedProtoHeaderName": "",
    "ForwardedForHeaderName": "",
    "KnownProxies": "",
    "KnownNetworks": ""
  },
  "InstallationConfig": {
    "DisableSampleData": false,
    "DisabledPlugins": "Misc.AzureBlob,Misc.CloudflareImages",
    "InstallRegionalResources": true
  },
  "PluginConfig": {
    "UseUnsafeLoadAssembly": true
  },
  "WebOptimizer": {
    "EnableJavaScriptBundling": true,
    "EnableCssBundling": true,
    "JavaScriptBundleSuffix": ".scripts",
    "CssBundleSuffix": ".styles",
    "EnableCaching": true,
    "EnableMemoryCache": true,
    "EnableDiskCache": true,
    "CacheDirectory": "/app/wwwroot/bundles",
    "EnableTagHelperBundling": false,
    "CdnUrl": "",
    "AllowEmptyBundle": true,
    "HttpsCompression": 2,
    "MemoryCacheTimeToLive": "01:00:00"
  }
}
EOF

echo "Starting nopCommerce application..."

# Start the ASP.NET application in the foreground
exec dotnet Nop.Web.dll