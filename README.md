# dotnet.Identity

Dotnet Economy Identity microservice

## Create and publish package

```powershell
$version="1.0.2"
$owner="Dotnet-Economy"
$gh_pat="[PAT HERE]"

dotnet pack src/dotnet.Identity.Contracts/ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/dotnet.Identity -o ../packages

dotnet nuget push ../packages/dotnet.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```
