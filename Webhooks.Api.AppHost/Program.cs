var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Webhooks_Api>("webhooks-api");

builder.Build().Run();
