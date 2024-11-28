using Polly;
using Polly.Extensions.Http;
using WebClientBffGateway.Models;
using WebClientBffGateway.Models.ComblexModels;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(x => x.AddDefaultPolicy(o => o.AllowAnyHeader().WithOrigins("http://localhost:4300").AllowCredentials().AllowAnyMethod()));

builder.Services.AddHttpClient();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();


//builder.Services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("fullData", async (int patientId, HttpClient client, IConfiguration configuration) =>
{
    var monitorUrl = "https://patientmonitoringservice";// configuration["RemoteServices:MonitorService"];
    //var patientDataUrl = "http://Patientdataapi";// configuration["RemoteServices:PatientDataService"];

    var monitorDataTask = client.GetFromJsonAsync<string>($"{monitorUrl}/vitals/{patientId}");
    //var userDataTask = client.GetFromJsonAsync<PatientBasicInfoModel>($"{patientDataUrl}/api/patients/{patientId}");

    //await Task.WhenAll(monitorDataTask, userDataTask);
    
    var monitorData = await monitorDataTask;
    //var userData  = await userDataTask;
    //var result = new FullPatientCurrentStatusData(userData, monitorData);
    
    return Results.Ok(monitorData);
});

app.UseCors();
app.MapOpenApi();

app.UseHttpsRedirection();

app.MapReverseProxy();

app.Run();



// client httpClient pool