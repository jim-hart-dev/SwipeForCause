# SwipeForCause — Azure Infrastructure

Bicep templates that provision the dev/prototype stack using a modular architecture.

## Naming Convention

All resources follow the pattern `{type}-sfc-{component?}-{env}`:

| Resource Type | Pattern | Example |
|---|---|---|
| Resource Group | `rg-sfc-{env}` | `rg-sfc-prod` |
| App Service Plan | `plan-sfc-{env}` | `plan-sfc-prod` |
| App Service | `app-sfc-{component}-{env}` | `app-sfc-api-prod` |
| Static Web App | `stapp-sfc-{env}` | `stapp-sfc-prod` |
| PostgreSQL Server | `db-sfc-{env}` | `db-sfc-prod` |
| Storage Account | `stsfc{env}` | `stsfcprod` |
| Key Vault (future) | `kv-sfc-{env}` | `kv-sfc-prod` |

## File Structure

```
infra/
├── main.bicep              # Orchestration — wires all modules together
├── main.bicepparam         # Production parameter file
├── modules/
│   ├── app-service.bicep   # App Service Plan (Free) + .NET 8 API
│   ├── static-web-app.bicep# Azure Static Web App (React frontend)
│   ├── postgresql.bicep    # PostgreSQL Flexible Server (v16, B1ms)
│   └── storage.bicep       # Blob Storage + 5 containers
└── README.md
```

## Prerequisites

| Tool | Minimum Version | Install |
|------|----------------|---------|
| Azure CLI | 2.61+ | [Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) |
| Bicep CLI | 0.25+ | Bundled with Azure CLI, or `az bicep install` |

Verify installation:

```bash
az --version
az bicep version
```

## Resources Provisioned

| Resource | SKU / Tier | Name |
|----------|-----------|------|
| App Service Plan | F1 Free (Linux) | `plan-sfc-{env}` |
| App Service (.NET 8 API) | — | `app-sfc-api-{env}` |
| Static Web App (React) | Free | `stapp-sfc-{env}` |
| PostgreSQL Flexible Server | Burstable B1ms, v16, 32 GB | `db-sfc-{env}` |
| Storage Account | Standard_LRS, Hot | `stsfc{env}` |
| Blob Containers | — | uploads, videos, images, avatars, logos |

### Not yet provisioned (add when needed)

These resources were deferred to keep prototype costs low:

- **CDN** — add `modules/cdn.bicep` when media delivery needs caching
- **Azure Functions** — add `modules/functions.bicep` when media processing is built
- **Key Vault** — add `modules/keyvault.bicep` to move secrets out of App Service settings
- **Staging slot** — upgrade App Service to Standard (S1+) tier first

## Deployment

### 1. Authenticate

```bash
az login
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

### 2. Create the resource group

```bash
az group create --name rg-sfc-prod --location eastus
```

### 3a. Deploy using the parameter file

Edit `main.bicepparam` to set your secret values, then:

```bash
az deployment group create \
  --resource-group rg-sfc-prod \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

### 3b. Deploy passing secrets inline (recommended for CI)

```bash
az deployment group create \
  --resource-group rg-sfc-prod \
  --template-file infra/main.bicep \
  --parameters \
    environment=prod \
    location=eastus \
    postgresAdminPassword='<SECURE_PASSWORD>' \
    clerkSecretKey='<CLERK_KEY>' \
    sendGridApiKey='<SENDGRID_KEY>'
```

### 4. View deployment outputs

```bash
az deployment group show \
  --resource-group rg-sfc-prod \
  --name main \
  --query properties.outputs
```

### 5. Validate without deploying (what-if)

```bash
az deployment group what-if \
  --resource-group rg-sfc-prod \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

## Multi-Environment Setup

Use separate resource groups per environment:

```bash
# Dev
az group create --name rg-sfc-dev --location eastus
az deployment group create \
  --resource-group rg-sfc-dev \
  --template-file infra/main.bicep \
  --parameters environment=dev postgresAdminPassword='...'

# Staging
az group create --name rg-sfc-staging --location eastus
az deployment group create \
  --resource-group rg-sfc-staging \
  --template-file infra/main.bicep \
  --parameters environment=staging postgresAdminPassword='...'
```

## Estimated Monthly Cost (Prototype)

| Resource | SKU | Est. Monthly Cost |
|----------|-----|-------------------|
| App Service Plan (F1) | Free tier, 60 CPU-min/day | $0 |
| PostgreSQL Flexible (B1ms) | 1 vCore, 2 GB RAM, 32 GB storage | ~$15 |
| Storage Account (Standard LRS) | Pay per use | ~$0.01–1 |
| Static Web App (Free tier) | — | $0 |
| **Total estimated** | | **~$15–16/month** |

> Upgrade App Service to B1 (~$13/month) when you need always-on or custom domains.
> Costs vary by region and usage. See [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/) for precise estimates.

## Security Notes

- All traffic is HTTPS / TLS 1.2+ only
- Blob containers are private — no anonymous access
- App Service uses a system-assigned managed identity with Storage Blob Data Contributor role
- PostgreSQL enforces SSL and allows only Azure-internal traffic by default
- Secrets are passed as `@secure()` Bicep parameters — never committed to source control
