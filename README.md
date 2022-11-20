[![Identity CICD](https://github.com/Dotnet-Economy/dotnet.Identity/actions/workflows/cicd.yml/badge.svg)](https://github.com/Dotnet-Economy/dotnet.Identity/actions/workflows/cicd.yml)

# dotnet.Identity

Dotnet Economy Identity microservice

## Create and publish package

```powershell
$version="1.0.11"
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
# docker tag dotnet.identity:$version "$appname.azurecr.io/dotnet.identity:$version"
docker push "$appname.azurecr.io/dotnet.identity:$version"
```

## Creating the Kubernetes namespace

```powershell
$namespace="identity"
kubectl create namespace $namespace
```

## Creating Kubernetes secrets (obsolete)

```powershell
kubectl create secret generic identity-secrets --from-literal=cosmosdb-connectionstring=$cosmosDbConnString --from-literal=servicebus-connectionstring=$serviceBusConnString --from-literal=admin-password=$adminPass -n $namespace
```

## Creating the Kubernetes pod

```powershell
kubectl apply -f ./Kubernetes/identity.yaml -n $namespace
```

## Creating the pod managed identity

```powershell
az identity create -g $appname -n $namespace
$IDENTITY_RESOURCE_ID=az identity show -g $appname -n $namespace --query id -otsv

az aks pod-identity add -g $appname --cluster-name $appname --namespace $namespace -n $namespace --identity-resource-id $IDENTITY_RESOURCE_ID
```

## Granting acess to Key Vault secrets

```powershell
$IDENTITY_CLIENT_ID=az identity show -g $appname -n $namespace --query clientId -otsv
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```

## Creating the signing certificate

```powershell
kubectl apply -f ./Kubernetes/signing-cer.yaml -n $namespace
```

## Installing the Helm chart

```powershell
$helmUser=[guid]::Empty.Guid
$helmPassword=az acr login --name $appname --expose-token --output tsv --query accessToken

$env:HELM_EXPERIMENTAL_OCI=1

helm registry login "$appname.azurecr.io" --username  $helmUser --password $helmPassword

$chartVersion="0.1.0"
helm upgrade identity-service oci://$appname.azurecr.io/helm/microservice --version $chartVersion -f ./helm/values.yaml -n $namespace --install #--debug
```

## Required organisation secrets for Github workflow

GH_PAT: Created in GitHub user profile --> Settings --> Developer settings --> Personal access token
AZURE_TENANT_ID: From AAD properties page
AZURE_SUBSCRIPTION_ID: From Azure Portal Subscription
AZURE_CLIENT_ID: From AAD App registration
