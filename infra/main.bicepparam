using './main.bicep'

// =============================================================================
// SwipeForCause — Production environment parameters
// =============================================================================

param environment = 'prod'
param location = 'eastus'

// Secure values — pass via CLI or replace placeholders before deploying.
// NEVER commit real secrets to source control.
param postgresAdminPassword = '<REPLACE_WITH_SECURE_PASSWORD>'
param clerkSecretKey = '<REPLACE_WITH_CLERK_SECRET_KEY>'
param sendGridApiKey = '<REPLACE_WITH_SENDGRID_API_KEY>'
