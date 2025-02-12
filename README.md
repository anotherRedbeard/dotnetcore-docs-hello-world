# .NET 8 Hello World

Forked from [dotnetcore-docs-hello-world](https://github.com/Azure-Samples/dotnetcore-docs-hello-world)

This sample demonstrates a tiny Hello World .NET Core app for [App Service Web App](https://docs.microsoft.com/azure/app-service-web). This sample can be used in a .NET Azure App Service app as well as in a Custom Container Azure App Service app.

## Log in to Azure Container Registry

Using the Azure CLI, log in to the Azure Container Registry (ACR):

```azurecli
az acr login -n <your_registry_name>
```

## Running in a Docker Container

### Locally

To run the app locally, use the following command:

```docker
docker build -f Dockerfile.linux -t dotnetcore-docs-hello-world .
docker run -d --name hello-world -p 8080:80 dotnetcore-docs-hello-world
```

To test the web app, navigate to `http://localhost:8080` in your browser. To test the command line app, run the following command:

```docker
docker exec hello-world dotnet /app/dotnetcoresample.dll run-command-line
```

### Publish the image to your Registry

To build the Linux image locally and publish to ACR, run the following command:

```docker
docker build -f Dockerfile.linux -t dotnetcore-docs-hello-world-linux . 
docker tag dotnetcore-docs-hello-world-windows <your_registry_name>.azurecr.io/dotnetcore-docs-hello-world-linux:latest
docker push <your_registry_name>.azurecr.io/dotnetcore-docs-hello-world-linux:latest
```

### GitHub Actions Worflow

#### main_red-privateapp.yml

This workflow automates the deployment of your application to Azure App Service using a custom Docker image. It logs in to Azure, builds and pushes the Docker image to Azure Container Registry (ACR), and then deploys the image to Azure App Service.

#### Workflow Steps

1. **Login to Azure:** Uses the Azure login action to authenticate with Azure using a service principal.
  a. **User Assigned Managed Identity:** A User Assigned Managed Identity is used to log into azure and perform the actions of both the build and deploy steps with a federated credential. By default, GitHub will use two different subjects in the build and deploy stages, so you will need to ensure you have two federated credentials that match these two subjects.
  b. **Update secrets and variables:** Please make sure you update the secrets and variables needed in GitHub settings:

    | Secret/Variable Name       | Description                                      |
    |----------------------------|--------------------------------------------------|
    | `secrets.AZURE_MI_CLIENT_ID`          | The client ID of the service principal.          |
    | `secrets.ENTRA_TENANT_ID`          | The tenant ID of the Azure Active Directory.     |
    | `secrets.AZURE_SUBSCRIPTION_ID`    | The subscription ID where the resources reside.  |
    | `vars.AZURE_APP_NAME`             | The name of the Azure Web App.                   |
    | `vars.AZURE_RG_NAME`           | The resource group of the Azure Web App.         |
    | `vars.AZURE_ACR_NAME`                 | The name of the Azure Container Registry.        |

2. **Deploy to Azure Web App:** Uses the azure/webapps-deploy action to deploy the Docker image to Azure App Service.
3. **Ensure app is using managed identity to connect to ACR:** Configures the web app to use managed identity for pulling images from ACR.
4. **Sign out of Azure:** Logs out of Azure to clean up the session.

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
