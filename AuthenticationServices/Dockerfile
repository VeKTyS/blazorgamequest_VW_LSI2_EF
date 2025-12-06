# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BlazorGameQuest.sln ./
COPY AuthenticationServices/AuthenticationServices.csproj AuthenticationServices/
COPY SharedModels/SharedModels.csproj SharedModels/
RUN dotnet restore AuthenticationServices/AuthenticationServices.csproj

COPY . .
WORKDIR /src/AuthenticationServices
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthenticationServices.dll"]