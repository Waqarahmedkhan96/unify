# Reverse Proxy

Production deployments should run the API behind an HTTPS-terminating reverse proxy.

Required behavior:

- HTTPS
- HSTS where appropriate
- Forwarded headers
- Request size limits
- Access logs without secrets

The API trusts `X-Forwarded-For` and `X-Forwarded-Proto` in Production so an HTTPS-terminating proxy can communicate with the container over internal HTTP.

Recommended production path:

1. Point a real DNS record at the server.
2. Set `UNIFY_DOMAIN` to that hostname.
3. Set `UNIFY_TLS_EMAIL` to the certificate contact email.
4. Run:

```powershell
docker compose -f docker-compose.yml -f docker-compose.production.example.yml -f docker-compose.https.example.yml up -d --build
```

Caddy will request and renew public TLS certificates automatically. The API remains plain HTTP inside the private Docker network and is exposed publicly only through HTTPS.

For Flutter web, build with the HTTPS API base URL:

```powershell
cd apps\unify_app
..\..\flutter\bin\flutter.bat build web --dart-define=UNIFY_API_URL=https://api.example.com
```

Host `apps/unify_app/build/web` on the same domain under a separate web server, CDN, or another Caddy site block. If the UI uses a different domain, set `Cors__AllowedOrigins__0` to the exact HTTPS UI origin.
