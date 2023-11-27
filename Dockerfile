FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0@sha256:bb65e39b662be0265f780afae9cdbfcaa315ef63edb245ad9fb2aa1aabca0b6b AS build
ARG TARGETARCH

RUN dpkg --add-architecture arm64
RUN apt update && apt install --yes clang gcc-aarch64-linux-gnu llvm zlib1g-dev zlib1g-dev:arm64

WORKDIR /source

COPY . .

RUN DOTNET_INSTALL_DIR="/usr/share/dotnet" && curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --jsonfile global.json --install-dir $DOTNET_INSTALL_DIR

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./src/LondonTravel.Skill --runtime linux-arm64 --self-contained true /p:AssemblyName=bootstrap /p:PublishAot=true /p:PublishReadyToRun=true

FROM scratch AS export
ARG TARGETARCH
COPY --from=build /source/artifacts/publish/LondonTravel.Skill/release_*/ ./artifacts/publish/LondonTravel.Skill/release_linux-arm64
