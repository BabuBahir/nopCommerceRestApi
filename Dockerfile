# Multi-stage build: first stage builds the .NET app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /src                                                                    
COPY ./src ./

# build solution   
RUN dotnet build NopCommerce.sln --no-incremental -c Release

# publish project with memory optimization
WORKDIR /src/Presentation/Nop.Web
RUN dotnet publish Nop.Web.csproj -c Release -o /app/published \
  /p:ServerGarbageCollection=true \
  /p:ConcurrentGarbageCollection=true \
  /p:GarbageCollectionAdaptation=true

WORKDIR /app/published

RUN mkdir logs bin

RUN chmod 775 App_Data \
              App_Data/DataProtectionKeys \
              bin \
              logs \
              Plugins \
              wwwroot/bundles \
              wwwroot/db_backups \
              wwwroot/files/exportimport \
              wwwroot/icons \
              wwwroot/images \
              wwwroot/images/thumbs \
              wwwroot/images/uploaded \
              wwwroot/sitemaps

# Final stage: use .NET runtime with MariaDB installed via Alpine packages
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime 

# add globalization support
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# installs required packages
RUN apk add tiff --no-cache --repository https://dl-cdn.alpinelinux.org/alpine/edge/main/ --allow-untrusted
RUN apk add libgdiplus --no-cache --repository https://dl-cdn.alpinelinux.org/alpine/edge/community/ --allow-untrusted
RUN apk add libc-dev tzdata gcompat --no-cache

# Install MariaDB
RUN apk add --no-cache mariadb mariadb-client

# Create MariaDB data directory and set permissions
RUN mkdir -p /var/lib/mysql /run/mysqld && \
    chown -R mysql:mysql /var/lib/mysql /run/mysqld

# Initialize MariaDB data directory during build
RUN mariadb-install-db --user=mysql --datadir=/var/lib/mysql --auth-root-authentication-method=normal

# Copy database initialization script
COPY init-db.sql /docker-entrypoint-initdb.d/
RUN chmod -R +x /docker-entrypoint-initdb.d/* || true

WORKDIR /app

COPY --from=build /app/published .

# Copy appsettings.json
COPY appsettings.json /app/App_Data/appsettings.json

# Set GC environment variables for memory optimization
ENV DOTNET_GCHeapHardLimit=80
ENV DOTNET_GCGen0HeapSize=1000000
ENV DOTNET_GCGen1HeapSize=2000000
ENV DOTNET_GCGen2HeapSize=4000000
ENV DOTNET_ThreadPoolMin=4
ENV DOTNET_ThreadPoolMax=16

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80 3306

# Create startup script
COPY start.sh /start.sh
RUN chmod +x /start.sh

ENTRYPOINT ["/start.sh"]