var builder = DistributedApplication.CreateBuilder(args);

var redisDb = builder.AddRedis("cacheDb");
var rabbit = builder.AddRabbitMQ("rabbitMQ");
var oraclePatientDataHistoryDb = builder.AddOracle("patientDataHistoryDb");
var patientDataDb = builder.AddOracle("patientDataDb");


var bff = builder.AddProject<Projects.WebClientBffGateway>("webclientbffgateway");

builder.AddNpmApp("angular", "../Clients/patient-monitoring-client")
    .WithReference(bff)
    .WaitFor(bff)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var patientData = builder.AddProject<Projects.PatientDataAPI>("patientdataapi")
    .WithReference(rabbit).WaitFor(rabbit)
    .WithReference(patientDataDb);

var alerting = builder.AddProject<Projects.AlertingService>("alertingservice")
    .WithReference(rabbit)
    .WaitFor(rabbit);

var history = builder.AddProject<Projects.PatientHistoryService>("patienthistoryservice")
    .WithReference(rabbit)
    .WithReference(oraclePatientDataHistoryDb)
    .WaitFor(oraclePatientDataHistoryDb)
    .WaitFor(rabbit);

var monitoring = builder.AddProject<Projects.PatientMonitoringService>("patientmonitoringservice")
    .WithReference(redisDb).WaitFor(redisDb)
    .WithReference(rabbit).WaitFor(rabbit);


bff
.WithReference(patientData)
.WithReference(monitoring)
.WithReference(history)
.WithReference(alerting);

builder.Build().Run();
