FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /MemeApi

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /MemeApi
COPY --from=build-env /MemeApi/out .
ENTRYPOINT ["dotnet", "MemeApi.dll"]
EXPOSE 5000