FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY ./WebApplicationSWEN3/. /app/WebApplicationSWEN3
COPY ./DAL/. /app/DAL

RUN dotnet build ./WebApplicationSWEN3/WebApplicationSWEN3.csproj

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app ./  

# Expose the application port
EXPOSE 8081

# Run the application
ENTRYPOINT ["dotnet", "WebApplicationSWEN3.dll", "--urls", "http://0.0.0.0:8081"]
