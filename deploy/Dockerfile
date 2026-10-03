# Merkwerk – multi-arch image (amd64 + arm64). Build context: repository root.
#   docker buildx build -f deploy/Dockerfile --platform linux/amd64,linux/arm64 -t ghcr.io/<owner>/merkwerk:<tag> .
#   docker buildx build -f deploy/Dockerfile --target migrations --platform linux/amd64,linux/arm64 -t ghcr.io/<owner>/merkwerk-migrations:<tag> .
# The web client (sources/Web.Client) is not part of this image yet – delivery via Caddy follows with LP-116/LP-160.

# ---------- build (runs on the build machine's native platform) ----------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY global.json ./
COPY .config/ ./.config/
COPY sources/ ./sources/
RUN dotnet restore sources/Web.Core/Web.Core.csproj -a $TARGETARCH
RUN dotnet publish sources/Web.Core/Web.Core.csproj -c Release -a $TARGETARCH --no-restore -o /app

# ---------- EF Core migration bundle ----------
FROM build AS migrations-build
ARG TARGETARCH
# dotnet-ef in the version pinned in .config/dotnet-tools.json (same as EF Core).
RUN dotnet tool restore
# The bundle builds the Web.Core host; it only needs a connection string to start (passed again via --connection).
ENV ConnectionStrings__Default="Server=localhost;Database=build;User=build;Password=build"
RUN dotnet ef migrations bundle \
      -p sources/Data.Database -s sources/Web.Core \
      -r linux-$TARGETARCH --self-contained -o /bundle/efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrations
WORKDIR /bundle
COPY --from=migrations-build /bundle/efbundle .
ENTRYPOINT ["sh", "-c", "./efbundle --connection \"$ConnectionStrings__Default\""]

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Mount points for the volumes in docker-compose.yml. Docker copies their owner into a new named volume; without this
# they would belong to root and the non-root app user could not write media or data protection keys.
RUN mkdir -p /data/media /data/keys && chown -R $APP_UID /data
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Web.Core.dll"]
