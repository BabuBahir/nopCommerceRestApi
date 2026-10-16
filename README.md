# nopCommerce + REST API

A [nopCommerce](https://www.nopcommerce.com/) store with a **built-in REST API and Swagger UI**, so you
can drive the catalog, orders, customers and shipments from a mobile app, a headless storefront or a
background job without writing a plugin first.

This is a fork of [nopSolutions/nopCommerce](https://github.com/nopSolutions/nopCommerce). The platform is
unchanged; the addition is the [`Nop.Plugin.Api.Rest`](src/Plugins/Nop.Plugin.Api.Rest) plugin, which is
compiled into the published Docker image and ships enabled.

- **Base platform:** nopCommerce **5.00**
- **Runtime:** .NET **10** (SDK 10.0.401)

## Contents

- [Quick start with Docker](#quick-start-with-docker)
- [Docker image reference](#docker-image-reference)
- [Building and running from source](#building-and-running-from-source)
- [Swagger](#swagger)
- [Authentication](#authentication)
- [API surface](#api-surface)
- [Configuration](#configuration)
- [Continuous integration](#continuous-integration)
- [Further documentation](#further-documentation)
- [About upstream nopCommerce](#about-upstream-nopcommerce)

## Quick start with Docker

The image is published to GitHub Container Registry and is **public** — no login is required to pull it.

**1. Pull the image.**

```bash
docker pull ghcr.io/babubahir/nopcommercerestapi:latest
```

**2. Confirm it arrived** (about 362 MB compressed).

```bash
docker images ghcr.io/babubahir/nopcommercerestapi
```

**3. Run it.** If port 80 is already taken on your machine, publish a different host port instead — the
container always listens on 80 internally.

```bash
docker run -d --name nopcommerce -p 80:80 ghcr.io/babubahir/nopcommercerestapi:latest
```

**4. Watch it start.**

```bash
docker logs -f nopcommerce
```

Then open <http://localhost> and complete the nopCommerce install wizard on first visit.

The image contains the web application only — **there is no database inside it**. During the wizard, point
nopCommerce at an MSSQL, PostgreSQL or MySQL server you already run. See
[Run the published image with a database](#run-the-published-image-with-a-database) for a compose file that
starts one alongside the image.

Once the store is installed and the REST API plugin is configured, the API is live:

```
http://localhost/swagger/api-rest/index.html
```

### If you need to log in

The package is public, so a signed-out `docker pull` works. If it is ever made private, authenticate first:

```bash
docker login ghcr.io -u BabuBahir
```

The password is a GitHub **personal access token** with the `read:packages` scope — your account password
will not work against a registry.

### On Apple Silicon and other arm64 machines

The published image is **`linux/amd64` only**, because the publish workflow builds without a platform
matrix. On an arm64 host Docker will fail the pull with *no matching manifest for linux/arm64*. Ask for the
amd64 build explicitly, and run it the same way:

```bash
docker pull --platform linux/amd64 ghcr.io/babubahir/nopcommercerestapi:latest
docker run --platform linux/amd64 -d --name nopcommerce -p 80:80 \
  ghcr.io/babubahir/nopcommercerestapi:latest
```

Docker Desktop emulates amd64 here, so it works but runs more slowly than native.

### Everyday commands

```bash
docker logs -f nopcommerce            # follow the log
docker stop nopcommerce               # stop, keep the container
docker start nopcommerce              # start it again
docker rm -f nopcommerce              # remove it
```

### Keep your data before you remove the container

nopCommerce writes uploads, logs, database backups and data-protection keys **inside the container**. A
`docker rm` discards all of them, and losing the data-protection keys invalidates the cookies of every
signed-in user. Mount the paths you care about to keep them:

```bash
docker run -d --name nopcommerce -p 80:80 \
  -v nopcommerce-keys:/app/App_Data/DataProtectionKeys \
  -v nopcommerce-uploads:/app/wwwroot/images/uploaded \
  -v nopcommerce-logs:/app/logs \
  ghcr.io/babubahir/nopcommercerestapi:latest
```

### Run the published image with a database

The compose files in this repository (`docker-compose.yml`, `mysql-docker-compose.yml`,
`postgresql-docker-compose.yml`) use `build: .`, so they **build the image from source** rather than
pulling the published one. To run the published image with a database, use this instead:

```yaml
services:
  nopcommerce_web:
    image: ghcr.io/babubahir/nopcommercerestapi:latest
    container_name: nopcommerce
    ports:
      - "80:80"
    volumes:
      - nopcommerce-keys:/app/App_Data/DataProtectionKeys
      - nopcommerce-uploads:/app/wwwroot/images/uploaded
    depends_on:
      - nopcommerce_database

  nopcommerce_database:
    image: "mcr.microsoft.com/mssql/server:2019-latest"
    container_name: nopcommerce_mssql_server
    environment:
      SA_PASSWORD: "nopCommerce_db_password"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Express"

volumes:
  nopcommerce-keys:
  nopcommerce-uploads:
```

The two named volumes are what [Keep your data](#keep-your-data-before-you-remove-the-container) asks for —
without them, replacing the container signs every user out and discards uploaded product images.

```bash
docker compose up
```

In the install wizard, use `nopcommerce_database` as the server name — that is the service name on the
compose network, not `localhost`. The database is not initialised for you, so create an empty database
first and let the wizard install the schema into it.

To build from source with a database instead, the repository's own files are unchanged:

```bash
docker compose up            # builds the image, app + MSSQL 2019, http://localhost
docker compose -f mysql-docker-compose.yml up
docker compose -f postgresql-docker-compose.yml up
```

## Docker image reference

| | |
|---|---|
| Image | `ghcr.io/babubahir/nopcommercerestapi` |
| Visibility | public, pulls anonymously |
| Base image | `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` |
| Platform | `linux/amd64` only — see [Apple Silicon](#on-apple-silicon-and-other-arm64-machines) |
| Size | ~362 MB compressed, 12 layers |
| Port | `80` (`ASPNETCORE_URLS=http://+:80`) |
| Includes | the whole `NopCommerce.sln`, so the REST API plugin is already built in |

Tags currently published: `latest` and `develop`. Browse them in the
[GitHub Packages UI](https://github.com/BabuBahir/nopCommerceRestApi/pkgs/container/nopcommercerestapi).

### Tags

| Tag | Example | Pushed when |
|---|---|---|
| `latest` | `ghcr.io/babubahir/nopcommercerestapi:latest` | push to `main` or `develop` |
| branch name | `:develop` | push to any branch that the workflow watches |
| semantic version | `:1.2.3` | a `v1.2.3` git tag is pushed (the `v` is dropped) |

The workflow builds on pull requests but **does not push** an image from them, so PRs get a compile check
without publishing anything.

Pull a specific version rather than `latest` in production, since `latest` moves with every push to
`develop`:

```bash
docker run -d -p 80:80 ghcr.io/babubahir/nopcommercerestapi:1.2.3
```

## Building and running from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download). `global.json` pins 10.0.401.

```bash
git clone https://github.com/BabuBahir/nopCommerceRestApi.git
cd nopCommerceRestApi
dotnet run --project src/Presentation/Nop.Web
```

The development profile listens on <http://localhost:54720>. Run the install wizard, then the API is at:

```
http://localhost:54720/swagger/api-rest/index.html
```

Build the whole solution with:

```bash
dotnet build src/NopCommerce.sln -c Release
```

### Plugin availability

The REST API plugin needs no setup step to be part of the build — `Nop.Plugin.Api.Rest` is already a project
in `src/NopCommerce.sln`, and it is listed in
[`src/Presentation/Nop.Web/App_Data/plugins.json`](src/Presentation/Nop.Web/App_Data/plugins.json), so
nopCommerce installs and enables it on first run.

To reach its configuration page, go to **Administration → Plugins → REST API → Configure**, generate an API
key, and save. The Swagger links are shown on that page, and each endpoint in the document carries its own
credential requirement.

## Swagger

The Swagger UI is served by the plugin itself. No host configuration is needed and no separate
`AddSwaggerGen` call was added to `Nop.Web`.

| | URL |
|---|---|
| **Swagger UI** | `/swagger/api-rest/index.html` |
| Back office JSON | `/swagger/api-backend/swagger.json` |
| Public store JSON | `/swagger/api-frontend/swagger.json` |

The API is published as **two OpenAPI documents**, split the way the official nopCommerce Web API splits
itself: back office operations in the backend document, public store operations in the public store
document. The Swagger UI page lists both in a **"Select a definition"** dropdown at the top, so there is a
single URL to bookmark.

Click **Authorize** to attach a credential:

- the **backend** document offers two schemes, `ApiKey` and `Bearer`
- the **public store** document offers `Bearer` only, because its operations refuse the API key with `403`
  and advertising it would promise something the runtime rejects

Each document's description block is that side's credential guide, written for the audience that reads it.

Only this plugin's controllers appear in the documents. A `DocInclusionPredicate` restricts the generator
to the `Nop.Plugin.Api.Rest` assembly, so attribute-routed endpoints belonging to other loaded plugins are
not published by accident.

Which document an operation lands in is decided by a single predicate, `ApiRestDefaults.IsFrontendPath`.
That same predicate drives the middleware, the document split and the published security requirement, so
the documented contract cannot promise a credential the runtime then refuses.

## Authentication

The API accepts **two kinds of credential**, and each is valid in exactly one header. Neither is ever
accepted in the other's place, so a caller holding both cannot present the wrong one by mistake.

**A bearer token** is a JWT with an expiry:

```
Authorization: Bearer <token>
```

**The shared API key** is a static secret you generate on the plugin configuration page, and it never
expires:

```
X-Api-Key: <API_KEY>
```

A missing or wrong credential is refused with `401`. There is **no session or cookie authentication** —
being signed in to the admin area does not authenticate an API request.

### Getting a token

```bash
# administrator token
curl -X POST http://localhost/api/rest/token \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@example.com","password":"..."}'

# customer token
curl -X POST http://localhost/api/rest/customer/token \
  -H 'Content-Type: application/json' \
  -d '{"email":"customer@example.com","password":"..."}'
```

Both endpoints report how long the token stays valid and which HTTP methods are guarded. Tokens are signed
with the shared API key, so **regenerating the key invalidates every token already issued**. There is no
refresh flow — post the credentials again for a new token.

Accounts with multi-factor authentication enabled are refused at both token endpoints: there is no way to
complete an interactive challenge through a REST call.

> Rate limits apply to the token endpoints too. That is what protects them from brute forcing.

## API surface

Every route lives under a single `/api/rest` prefix. This is deliberately **not** the official Web API's
`/api-backend/{Controller}/{Action}` shape — the routes here are resource-oriented, so
`GET /api/rest/orders/search?orderNumber=1003` rather than a controller/action pair.

| Scope | Routes | Accepted | Refused |
|---|---|---|---|
| **Back office** | `/api/rest/products`, `/orders`, `/shipments`, `/customers`, `/categories`, `/manufacturers`, `/logs`, `/sales`, `/metafields` and subroutes | API key **or** admin token | customer token → `403` |
| **Public store, personal** | `/api/rest/customer/me/*` | customer token, always | API key and admin token → `403` |
| **Public store, catalog** | `/api/rest/store/*` | anonymous while "Require a credential for reads" is off; customer token once it is on | API key and admin token → `403` |
| **Token endpoints** | `/api/rest/token`, `/api/rest/customer/token` | none — they are how you obtain a credential | — |

The two scopes are enforced against each other, not merely labelled. Both tokens are signed with the same
key, so without that check the scope claim would be decorative.

Every `customer/me` route takes the customer **from the credential itself**. There is no customer
identifier anywhere in those paths, query strings or bodies, so a caller cannot reach another shopper's
data by changing a value.

List endpoints return a paging envelope rather than a bare array, so a client can see the total without
fetching every page:

```json
{
  "items": [],
  "pageIndex": 0,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7
}
```

`pageIndex` is zero based, `pageSize` defaults to 20 and is capped at 200.

Controllers never return domain entities — the public contract is defined by DTOs.

**The full endpoint-by-endpoint list, with filters and request bodies, is in the
[plugin README](src/Plugins/Nop.Plugin.Api.Rest/README.md).**

## Configuration

Settings live on the plugin configuration page and are stored per store, so each tenant keeps its own key.

| Setting | Default | Notes |
|---|---|---|
| API key | generated | At least 32 characters, because it also signs bearer tokens. Use the generate button. |
| Require a credential for reads | off | When off, anonymous callers get `401` on nothing — but see the warning below. |
| Rate limit per minute | 60 | Per client per minute. |
| Admin token lifetime | 24 hours | Applies to tokens issued after the change. |
| Customer token lifetime | 7 days | Applies to tokens issued after the change. |

> ⚠️ **Leaving "Require a credential for reads" disabled also exposes the back office customer and order
> reads**, which contain personal data. `/api/rest/customer/me/*` is never public either way, and
> `/api/rest/store/*` moves from anonymous to requiring a customer token. Enable the setting for any store
> reachable from outside a trusted network.

Rate-limited clients are bucketed by the scope they proved: each customer token holder gets their own
bucket, admin-level callers share one whether they presented the API key or an admin token, and requests
with no valid credential are counted per IP.

## Continuous integration

| Workflow | Trigger | Does |
|---|---|---|
| [`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml) | push / PR to `develop` | Restores, builds and tests the solution on Windows with .NET 10 |
| [`.github/workflows/docker-publish.yml`](.github/workflows/docker-publish.yml) | push to `develop`/`main`, `v*.*.*` tags; PRs to `develop`/`main` | Builds the image with Buildx and pushes to GHCR (build-only on PRs) |
| [`.github/dependabot.yml`](.github/dependabot.yml) | weekly | Keeps GitHub Actions versions current |

Actions are pinned to commit SHAs.

## Further documentation

| Document | Contents |
|---|---|
| [Plugin README](src/Plugins/Nop.Plugin.Api.Rest/README.md) | The full API reference: every endpoint, authorization rules, token lifetimes, and the design decisions behind them |
| [API coverage tracker](src/Plugins/Nop.Plugin.Api.Rest/API-COVERAGE.md) | This surface measured against the two official nopCommerce Web API specifications, with the remaining work listed |
| [Postman collection](src/Plugins/Nop.Plugin.Api.Rest/postman/Nop.Plugin.Api.Rest.postman_collection.json) | Sample requests; set the `base_url` and `token` variables first |
| [Implementation plan](src/Plugins/Nop.Plugin.Api.Rest/PLAN.md) | The original design document for the plugin |

The plugin's own source lives in [`src/Plugins/Nop.Plugin.Api.Rest`](src/Plugins/Nop.Plugin.Api.Rest):
`Controllers/` holds the 15 controllers, `Models/` the DTOs and request bodies, `Security/` the
authentication handler and token factory, and `Infrastructure/` the Swagger, route and rate-limit wiring.

## About upstream nopCommerce

nopCommerce is a free and open-source eCommerce platform, developed and supported by a professional team
since 2008 and the most popular ASP.NET Core shopping cart. Everything in this repository below the REST
API plugin is upstream nopCommerce and is documented by the nopCommerce team.

- Official site: [nopcommerce.com](https://www.nopcommerce.com)
- Developer documentation: [docs.nopcommerce.com](https://docs.nopcommerce.com)
- Demo store: [demo.nopcommerce.com](https://www.nopcommerce.com/demo)
- Community forums: [nopcommerce.com/boards](https://www.nopcommerce.com/boards)
- Official Web API plugin: [nopcommerce.com/web-api](https://www.nopcommerce.com/web-api)
- Upstream repository: [nopSolutions/nopCommerce](https://github.com/nopSolutions/nopCommerce)

### Platform features

This fork inherits the full nopCommerce feature set — multi-vendor stores, multi-store, RFM and loyalty,
recurring products, forums, RFQ, punchout, and integrations with the major payment, shipping and marketing
providers.

- Cross-platform: Windows, Linux or Mac.
- Runs on .NET 10 with an MS SQL 2012 (or higher) backend database; PostgreSQL and MySQL are also supported.
- Docker support out of the box, including the prebuilt image published from this repository.
- Full web farm support — see the
  [web farms tutorial](https://docs.nopcommerce.com/en/developer/tutorials/web-farms.html).
- All methods are async, and multi-factor authentication is supported out of the box.
- Thousands of plugins and themes on the
  [nopCommerce Marketplace](https://www.nopcommerce.com/marketplace).

## License

nopCommerce is released under the [nopCommerce Public License](LICENSE.md). See
[LICENSE.md](LICENSE.md) for details.

## Contributing

Bug reports and pull requests are welcome. Please read the upstream
[CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) first.
