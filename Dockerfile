#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./IngBackendApi.Application/IngBackendApi.Application.csproj", "./IngBackendApi.Application/"]
COPY . .
WORKDIR "/src/."
RUN dotnet build "./IngBackendApi.Application/IngBackendApi.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS migration
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN apt update -y && apt install uuid-runtime
RUN dotnet ef migrations add $(uuidgen) --project "./IngBackendApi.Application" -o /app/build -v  -- --environment Production
RUN dotnet ef migrations bundle --project "./IngBackendApi.Application" --self-contained -r linux-x64 --force -- --environment Production

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IngBackendApi.Application/IngBackendApi.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=migration /src/efbundle .
USER root
RUN chmod 777 -R /app/wwwroot
RUN apt update -y && apt install -y libgssapi-krb5-2
ENTRYPOINT ["dotnet", "IngBackendApi.Application.dll"]
