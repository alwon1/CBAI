var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddProject<Projects.CBAI_Web>("web");

builder.Build().Run();
