var builder = DistributedApplication.CreateBuilder(args);


var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("webhooks");

var project =builder.AddProject<Projects.Webhooks_Api>("webhooks-api")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
