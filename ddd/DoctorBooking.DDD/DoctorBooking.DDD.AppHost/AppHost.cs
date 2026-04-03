var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.DoctorBooking_DDD_Api>("api")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(api)
    .WaitFor(api);

api.PublishWithContainerFiles(webfrontend, "wwwroot");

await builder.Build().RunAsync();
