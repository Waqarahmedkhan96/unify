# Unify ERP — Deployment

> Foundation deployment guide.

## Environments
- Development
- Testing
- Staging
- Production

## Backend
- Docker multi-stage image.
- ASP.NET Core.
- HTTPS behind reverse proxy.
- Environment variables.
- Health checks.
- Non-root runtime user.

## Database
- PostgreSQL.
- Automated backups.
- Restore verification.
- Migrations before deployment.

## Flutter
Android:
- Release App Bundle.
- Google Play.
Windows:
- Signed installer.
- Versioned releases.

## CI/CD
GitHub Actions:
- Build
- Test
- Analyze
- Docker image
- Release artifacts

## Monitoring
- Structured logs
- Health endpoints
- Metrics
- Backup monitoring
- Audit trail review through protected Platform API

## Pre-Deployment Checks
- Apply EF Core migrations before starting the new application version.
- Confirm `/health/live` and `/health/ready` return healthy responses.
- Confirm protected Platform endpoints reject anonymous requests.
- Confirm audit entries are created for representative create and update workflows.

## Rollback
- Restore DB backup if required.
- Roll back application version.
- Verify data consistency.
