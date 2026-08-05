# Reverse Proxy

Production deployments should run the API behind an HTTPS-terminating reverse proxy.

Required behavior:

- HTTPS
- HSTS where appropriate
- Forwarded headers
- Request size limits
- Access logs without secrets

The API trusts `X-Forwarded-For` and `X-Forwarded-Proto` in Production so an HTTPS-terminating proxy can communicate with the container over internal HTTP. Use `Caddyfile.example` as the starting point and replace `api.example.com` with the real domain.
