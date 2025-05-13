FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Nuget.config", "."]
COPY ["src/CleanPatternWithCloudNative.Api/CleanPatternWithCloudNative.Api.csproj", "src/CleanPatternWithCloudNative.Api/"]
COPY ["src/CleanPatternWithCloudNative.Application/CleanPatternWithCloudNative.Application.csproj", "src/CleanPatternWithCloudNative.Application/"]
COPY ["src/CleanPatternWithCloudNative.Domain/CleanPatternWithCloudNative.Domain.csproj", "src/CleanPatternWithCloudNative.Domain/"]
COPY ["src/CleanPatternWithCloudNative.Infrastructure/CleanPatternWithCloudNative.Infrastructure.csproj", "src/CleanPatternWithCloudNative.Infrastructure/"]
COPY ["src/CleanPatternWithCloudNative.ServiceDefaults/CleanPatternWithCloudNative.ServiceDefaults.csproj", "src/CleanPatternWithCloudNative.ServiceDefaults/"]
RUN dotnet restore "./src/CleanPatternWithCloudNative.Api/CleanPatternWithCloudNative.Api.csproj"
COPY . .
WORKDIR "/src/src/CleanPatternWithCloudNative.Api"
RUN dotnet build "./CleanPatternWithCloudNative.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CleanPatternWithCloudNative.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanPatternWithCloudNative.Api.dll"]