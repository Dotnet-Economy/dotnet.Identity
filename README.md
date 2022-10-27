# dotnet.Identity

Dotnet Economy Identity microservice

## Create and publish package

```powershell
$version="1.0.4"
$owner="Dotnet-Economy"
$gh_pat="[PAT HERE]"

dotnet pack src/dotnet.Identity.Contracts/ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/dotnet.Identity -o ../packages

dotnet nuget push ../packages/dotnet.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image

```powershell
$env:GH_OWNER="Dotnet-Economy"
$env:GH_PAT="[PAT HERE]"
$appname="dotneteconomy"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/dotnet.identity:$version" .
```

## Run the docker image

```powershell
$adminPass="[PASSWORD HERE]"
$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"
docker run -it --rm -p 5004:5004 --name identity -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" -e IdentitySettings__AdminUserPassword=$adminPass dotnet.identity:$version
```

## Publishing the docker image

```powershell
az acr login --name $appname
docker tag dotnet.identity:$version "$appname.azurecr.io/dotnet.identity:$version"
docker push "$appname.azurecr.io/dotnet.identity:$version"
```
