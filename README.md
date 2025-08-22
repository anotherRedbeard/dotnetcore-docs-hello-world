# .NET 8 Web Application with Background WebJob in a Custom Linux Container

This solution demonstrates a .NET 8 ASP.NET Core web application running alongside a scheduled .NET WebJob, all packaged within a custom Linux Docker container. It's designed for deployment to Azure App Service, showcasing how to integrate background processing (via WebJobs) with a web frontend in a containerized Azure environment. The project includes Docker configurations, shell scripts for container initialization and WebJob execution, and a GitHub Actions workflow for automated deployment to Azure.

# .NET 8 Hello World

Forked from [dotnetcore-docs-hello-world](https://github.com/Azure-Samples/dotnetcore-docs-hello-world)

This sample demonstrates a tiny Hello World .NET Core app for [App Service Web App](https://docs.microsoft.com/azure/app-service-web). This sample can be used in a .NET Azure App Service app as well as in a Custom Container Azure App Service app.

Another feature of this sample is it uses [WebJobs in Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/overview-webjobs). I'm using a triggered, scheduled WebJob to run a command line app. The command line app is a simple console app that runs every 5 minutes and writes to the console.
The WebJob `TriggeredDemo` is configured in the `Dockerfile.linux` by copying the `webjobs/triggered/TriggeredDemo/webJobSample.sh` execution script and `webjobs/triggered/TriggeredDemo/settings.job` schedule into the image under `/webjobs/triggered/TriggeredDemo`. The `webJobSample.sh` script executes `dotnetcoresample.dll run-command-line`.

## Features

### Web Application

The application provides a simple web interface built with ASP.NET Core Razor Pages.

### RESTful APIs

The application includes RESTful API endpoints:

* **Weather API**: Provides weather forecasts for US zipcodes
  * Endpoint: `GET /api/weather/{zipCode}`
  * Example: `http://localhost:8080/api/weather/90210`

### API Documentation Page

The application includes a built-in API documentation page that allows users to explore and test the available APIs:

* Access it via the "APIs" link in the navigation menu
* Each API is documented with endpoints, parameters, and return values
* Interactive testing interface allows you to try APIs directly from the documentation page

### Background Processing

The application demonstrates how to implement background processing in Azure App Service using WebJobs.

## Key Components

* **`Program.cs`**: Main entry point for the ASP.NET Core web application.
* **`CommandLineApp.cs`**: Implements the logic for the .NET WebJob, which is executed on a schedule.
* **`Dockerfile.linux`**: Defines the custom Linux Docker image, including setup for the web app, SSH, and the WebJob.
* **`webjobs/triggered/TriggeredDemo/webJobSample.sh`**: Shell script that runs the .NET WebJob (`CommandLineApp.cs`) inside the container.
* **`webjobs/triggered/TriggeredDemo/settings.job`**: Configuration file for the WebJob, defining its schedule (e.g., CRON expression).
* **`startup.sh`**: Script executed by `init_container.sh` to start the SSH service and the ASP.NET Core application.
* **`init_container.sh`**: The main entry point for the Docker container, responsible for initializing services.
* **`Controllers/WeatherController.cs`**: API controller implementing the Weather API endpoint.
* **`Models/WeatherForecast.cs`**: Data model for weather forecast information.
* **`Pages/APIs.cshtml`**: Documentation page for APIs with interactive testing capabilities.
* **`Pages/Embed/Bootstrap.cshtml`**: Minimal embed bootstrap page used for Teams tab handshake testing.
* **`.github/workflows/main_red-privateapp.yml`**: GitHub Actions workflow for CI/CD, building the Docker image, pushing it to Azure Container Registry (ACR), and deploying to Azure App Service.

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
Note: The `run-command-line` argument is handled in `Program.cs` to specifically trigger the `CommandLineApp.Run()` method, which contains the WebJob's logic.

In some cases, you may need to log into the container to run the command line app. To do this, use the following command:

```docker
docker exec -it hello-world /bin/bash
```

### Publish the image to your Registry

To build the Linux image locally and publish to ACR, run the following command:

```docker
docker build -f Dockerfile.linux -t dotnetcore-docs-hello-world-linux .
docker tag dotnetcore-docs-hello-world-linux <your_registry_name>.azurecr.io/dotnetcore-docs-hello-world-linux:latest
docker push <your_registry_name>.azurecr.io/dotnetcore-docs-hello-world-linux:latest
```

## Dockerfile Environment Variables

The `Dockerfile.linux` sets the following environment variables:

* `PORT=80`: Standard port for the web application.
* `SSH_PORT=2222`: Port exposed for [SSH access](https://learn.microsoft.com/en-us/azure/app-service/configure-custom-container?pivots=container-linux&tabs=debian#enable-ssh) into the container (primarily for debugging).
* `ASPNETCORE_URLS=http://+:80`: Configures the ASP.NET Core application to listen on port 80 for incoming HTTP requests.

## GitHub Actions Workflow

### main_red-privateapp.yml

This workflow automates the deployment of your application to Azure App Service using a custom Docker image. It logs in to Azure, builds and pushes the Docker image to Azure Container Registry (ACR), and then deploys the image to Azure App Service.

#### Workflow Steps

1. **Login to Azure:** Uses the Azure login action to authenticate with Azure using a service principal.
  a. **User Assigned Managed Identity:** A User Assigned Managed Identity is used to log into Azure and perform the actions of both the build and deploy steps with a federated credential. By default, GitHub will use two different subjects in the build and deploy stages, so you will need to ensure you have two federated credentials that match these two subjects.
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
