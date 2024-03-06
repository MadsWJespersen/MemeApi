FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /MemeApi/
COPY . .
CMD ["dotnet", "run", "--project", "./MemeApi/MemeApi.csproj"]
EXPOSE 80