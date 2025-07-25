FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BlogPlatform.API/BlogPlatform.API.csproj", "BlogPlatform.API/"]
COPY ["BlogPlatform.Core/BlogPlatform.Core.csproj", "BlogPlatform.Core/"]
COPY ["BlogPlatform.Infrastructure/BlogPlatform.Infrastructure.csproj", "BlogPlatform.Infrastructure/"]
RUN dotnet restore "BlogPlatform.API/BlogPlatform.API.csproj"
COPY . .
WORKDIR "/src/BlogPlatform.API"
RUN dotnet build "BlogPlatform.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogPlatform.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogPlatform.API.dll"]