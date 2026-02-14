# SwipeForCause — Azure Infrastructure

Bicep templates that provision the full MVP stack using a modular architecture.

## File Structure

```
infra/
├── main.bicep              # Orchestration — wires all modules together
├── main.bicepparam         # Production parameter file
├── modules/
│   ├── app-service.bicep   # App Service Plan + .NET 8 API + staging slot
│   ├── static-web-app.bicep# Azure Static Web App (React frontend)
│   ├── postgresql.bicep    # PostgreSQL Flexible Server (v16, B1ms)
│   ├── storage.bicep       # Blob Storage + 5 containers
│   ├── cdn.bicep           # CDN profile + endpoint (Standard Microsoft)
│   └── functions.bicep     # Azure Functions (Consumption, .NET 8 isolated)
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

| Resource | SKU / Tier | Naming Pattern |
|----------|-----------|----------------|
| App Service Plan | B1 (Linux) | `sfc-plan-{env}` |
| App Service (.NET 8 API) | — | `sfc-api-{env}` |
| App Service Staging Slot | — | `sfc-api-{env}/staging` |
| Static Web App (React) | Free | `sfc-web-{env}` |
| PostgreSQL Flexible Server | Burstable B1ms, v16, 32 GB | `sfc-pg-{env}` |
| Storage Account | Standard_LRS, Hot | `sfcstorage{env}` |
| Blob Containers | — | uploads, videos, images, avatars, logos |
| CDN Profile | Standard Microsoft | `sfc-cdn-{env}` |
| CDN Endpoint | HTTPS only | `sfc-cdn-ep-{env}` |
| Function App | Y1 Consumption | `sfc-func-{env}` |
| Function Storage | Standard_LRS | `sfcfuncstor{env}` |

## Deployment

### 1. Authenticate

```bash
az login
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

### 2. Create the resource group

```bash
az group create --name sfc-rg-prod --location eastus
```

### 3a. Deploy using the parameter file

Edit `main.bicepparam` to set your secret values, then:

```bash
az deployment group create \
  --resource-group sfc-rg-prod \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

### 3b. Deploy passing secrets inline (recommended for CI)

```bash
az deployment group create \
  --resource-group sfc-rg-prod \
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
  --resource-group sfc-rg-prod \
  --name main \
  --query properties.outputs
```

### 5. Validate without deploying (what-if)

```bash
az deployment group what-if \
  --resource-group sfc-rg-prod \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

## Multi-Environment Setup

Use separate resource groups per environment:

```bash
# Dev
az group create --name sfc-rg-dev --location eastus
az deployment group create \
  --resource-group sfc-rg-dev \
  --template-file infra/main.bicep \
  --parameters environment=dev postgresAdminPassword='...'

# Staging
az group create --name sfc-rg-staging --location eastus
az deployment group create \
  --resource-group sfc-rg-staging \
  --template-file infra/main.bicep \
  --parameters environment=staging postgresAdminPassword='...'
```

## Estimated Monthly Cost (MVP)

| Resource | SKU | Est. Monthly Cost |
|----------|-----|-------------------|
| App Service Plan (B1) | 1 core, 1.75 GB RAM | ~$13 |
| PostgreSQL Flexible (B1ms) | 1 vCore, 2 GB RAM, 32 GB storage | ~$15 |
| Storage Account (Standard LRS) | Pay per use | ~$1–5 |
| CDN (Standard Microsoft) | Pay per GB transferred | ~$1–5 |
| Azure Functions (Consumption) | First 1M executions free | ~$0–2 |
| Static Web App (Free tier) | — | $0 |
| **Total estimated** | | **~$30–40/month** |

> Costs vary by region and usage. See [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/) for precise estimates.

## Security Notes

- All traffic is HTTPS / TLS 1.2+ only
- Blob containers are private — no anonymous access
- CDN blocks the `uploads` container (raw uploads are never served publicly)
- App Service and Function App use system-assigned managed identities with Storage Blob Data Contributor role
- PostgreSQL enforces SSL and allows only Azure-internal traffic by default
- Secrets are passed as `@secure()` Bicep parameters — never committed to source control
