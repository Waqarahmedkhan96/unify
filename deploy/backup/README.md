# Backup And Restore

Production PostgreSQL backups must be automated, retained according to policy, encrypted where supported, and restore-tested before release.

Development database backup example:

```powershell
docker exec unify-postgres pg_dump -U unify_app unify_erp > unify_erp_backup.sql
```

Development restore example:

```powershell
Get-Content .\unify_erp_backup.sql | docker exec -i unify-postgres psql -U unify_app unify_erp
```
