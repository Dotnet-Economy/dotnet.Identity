FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5004

ENV ASPNETCORE_URLS=http://+:5004

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY ["src/dotnet.Identity.Contracts/dotnet.Identity.Contracts.csproj", "src/dotnet.Identity.Contracts/"]
COPY ["src/dotnet.Identity.Service/dotnet.Identity.Service.csproj", "src/dotnet.Identity.Service/"]

RUN --mount=type=secret, id=GH_OWNER,dst=/GH_OWNER --mount=type=secret, id=GH_PAT,dst=/GH_PAT \
    dotnet nuget add source --username USERNAME --password `cat /GH_PAT` --store-password-in-clear-text --name github "https://nuget.pkg.github.com/`cat /GH_OWNER`/index.json"

RUN dotnet restore "src/dotnet.Identity.Service/dotnet.Identity.Service.csproj"
COPY ./src ./src
WORKDIR "/src/dotnet.Identity.Service"
RUN dotnet publish "dotnet.Identity.Service.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dotnet.Identity.Service.dll"]
