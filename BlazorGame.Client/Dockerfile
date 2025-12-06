# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BlazorGameQuest.sln ./
COPY BlazorGame.Client/BlazorGame.Client.csproj BlazorGame.Client/
COPY SharedModels/SharedModels.csproj SharedModels/
RUN dotnet restore BlazorGame.Client/BlazorGame.Client.csproj

COPY . .
WORKDIR /src/BlazorGame.Client
RUN dotnet publish -c Release -o /app/publish

# Serve Blazor WASM as static files with Nginx
FROM nginx:alpine AS final
COPY BlazorGame.Client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]