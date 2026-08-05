# Reverse Proxy

Production deployments should run the API behind an HTTPS-terminating reverse proxy.

Required behavior:

- HTTPS
- HSTS where appropriate
- Forwarded headers
- Request size limits
- Access logs without secrets
