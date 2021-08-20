FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ["devOpsProject.csproj", "backend/"]
RUN dotnet restore "./backend/devOpsProject.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "devOpsProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "devOpsProject.csproj" -c Release -o /app/publish

#FROM base AS final
FROM mcr.microsoft.com/dotnet/core/sdk:3.1
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "devOpsProject.dll"]
