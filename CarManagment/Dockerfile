# Use the official .NET image as the base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8088

# Use the official build image for .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CarManagment.csproj", "./"]
RUN dotnet restore "CarManagment.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet publish "CarManagment.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CarManagment.dll"]
