# SwipeForCause — Azure Infrastructure

Bicep templates that provision the full MVP stack: App Service, Static Web App,
PostgreSQL, Blob Storage, CDN, and a Function App for media processing.

## Prerequisites

- Azure CLI (`az`) version 2.61 or later
- An active Azure subscription
- Contributor role on the target subscription

## Deploying

### 1. Log in

```bash
az login
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

### 2. Create the resource group

```bash
az group create --name sfc-rg-dev --location eastus
```

### 3a. Deploy with the parameter file (edit secrets first)

Edit `main.bicepparam` and replace the placeholder values, then run:

```bash
az deployment group create \
  --resource-group sfc-rg-dev \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

### 3b. Or pass secrets inline (recommended for CI)

```bash
az deployment group create \
  --resource-group sfc-rg-dev \
  --template-file infra/main.bicep \
  --parameters environment=dev \
               location=eastus \
               postgresAdminPassword='<SECURE_PASSWORD>' \
               clerkSecretKey='<CLERK_KEY>' \
               sendGridApiKey='<SENDGRID_KEY>'
```

### 4. View outputs

```bash
az deployment group show \
  --resource-group sfc-rg-dev \
  --name main \
  --query properties.outputs
```

## Staging / Production

Change the `environment` parameter to `staging` or `prod`. Use a separate
resource group per environment:

```bash
az group create --name sfc-rg-prod --location eastus

az deployment group create \
  --resource-group sfc-rg-prod \
  --template-file infra/main.bicep \
  --parameters environment=prod \
               location=eastus \
               postgresAdminPassword='...' \
               clerkSecretKey='...' \
               sendGridApiKey='...'
```

## What gets provisioned

| Resource | SKU / Tier | Naming pattern |
|---|---|---|
| App Service Plan | B1 (Linux) | `sfc-plan-{env}` |
| App Service (.NET API) | — | `sfc-api-{env}` |
| Static Web App (React) | Free | `sfc-web-{env}` |
| PostgreSQL Flexible Server | B1ms, v16, 32 GB | `sfc-pg-{env}` |
| Storage Account | Standard LRS | `sfcstorage{env}` |
| CDN (Standard Microsoft) | — | `sfc-cdn-{env}` |
| Function App (Consumption) | Y1 Dynamic | `sfc-func-{env}` |

## Security notes

- All traffic is HTTPS / TLS 1.2+ only.
- Blob containers are private (no anonymous access). The CDN blocks the
  `uploads` container so only processed media is served.
- App Service and Function App use system-assigned managed identities with
  Storage Blob Data Contributor role on the media storage account.
- Secrets are passed as secure Bicep parameters and are never written to
  template files.
- PostgreSQL enforces SSL and allows only Azure-internal traffic by default.
