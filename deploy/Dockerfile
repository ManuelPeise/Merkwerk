# Merkwerk – multi-arch image (amd64 + arm64). Build context: repository root.
#   docker buildx build -f deploy/Dockerfile --platform linux/amd64,linux/arm64 -t ghcr.io/<owner>/merkwerk:<tag> .
#   docker buildx build -f deploy/Dockerfile --target migrations --platform linux/amd64,linux/arm64 -t ghcr.io/<owner>/merkwerk-migrations:<tag> .
# Requires the projects from LP-001.

# ---------- build (runs on the build machine's native platform) ----------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY global.json ./
COPY sources/ ./sources/
RUN dotnet restore sources/Web/Web.csproj -a $TARGETARCH
RUN dotnet publish sources/Web/Web.csproj -c Release -a $TARGETARCH --no-restore -o /app

# ---------- EF Core migration bundle ----------
FROM build AS migrations-build
ARG TARGETARCH
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
      -p sources/Data.Database -s sources/Web \
      -r linux-$TARGETARCH --self-contained -o /bundle/efbundle

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrations
WORKDIR /bundle
COPY --from=migrations-build /bundle/efbundle .
ENTRYPOINT ["sh", "-c", "./efbundle --connection \"$ConnectionStrings__Default\""]

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Web.dll"]
