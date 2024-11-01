FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./WebApplicationSWEN3/. /app/WebApplicationSWEN3
COPY ./DAL/. /app/DAL

COPY ./SharedResources/. /app/SharedResources

# Build the application
RUN dotnet build ./WebApplicationSWEN3/WebApplicationSWEN3.csproj
# Expose the application port
EXPOSE 8081

# Run the application
ENTRYPOINT ["dotnet", "run", "--project", "./WebApplicationSWEN3/WebApplicationSWEN3.csproj", "--urls", "http://0.0.0.0:8081"]
