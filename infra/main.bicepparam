using './main.bicep'

// =============================================================================
// SwipeForCause — Dev environment parameters
// =============================================================================

param environment = 'dev'
param location = 'eastus'

// Secure values — replace placeholders before deploying or pass via CLI
param postgresAdminPassword = '<REPLACE_WITH_SECURE_PASSWORD>'
param clerkSecretKey = '<REPLACE_WITH_CLERK_SECRET_KEY>'
param sendGridApiKey = '<REPLACE_WITH_SENDGRID_API_KEY>'
