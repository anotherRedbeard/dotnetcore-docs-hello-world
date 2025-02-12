using dotnetcoresample;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<CommandLineApp>();

var app = builder.Build();

// Check for command-line arguments
if (args.Contains("run-command-line"))
{
    using (var scope = app.Services.CreateScope())
    {
        var commandLineApp = scope.ServiceProvider.GetRequiredService<CommandLineApp>();
        commandLineApp.Run();
    }
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Configure the application to listen on port 80
app.Urls.Add("http://*:80");

// Configure and run WebJobs
var host = new HostBuilder()
    .ConfigureWebJobs(webJobsBuilder =>
    {
        webJobsBuilder.AddTimers();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();

app.Run();
