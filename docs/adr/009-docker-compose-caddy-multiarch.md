# 009 – Operation via Docker Compose with Caddy and multi-arch images

- Status: Accepted
- Date: 2026-10-02
- Ticket: LP-002

## Context

The family instance runs on a Raspberry Pi (ARM64); development happens on Windows (x64). Schools may self-host later.

## Decision

Three containers via Docker Compose: `proxy` (Caddy, HTTPS with `tls internal` on the home network), `app` (.NET 10) and `db` (MySQL 8.4). CI builds images for amd64 and arm64 on tags `v*` and publishes them to GitHub Container Registry. Migrations run via an EF migration bundle before the app starts.

## Alternatives considered

- Bare-metal installation – hard to update and reproduce.
- Kubernetes – far too heavy for a Pi and a solo developer.

## Consequences

- Same compose file locally and on the Pi.
- Pi boots from SSD; nightly `mysqldump` backup.
