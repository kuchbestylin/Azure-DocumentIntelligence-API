
var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.IdDocumentEndpoint>("iddocumentendpoint");

builder.AddProject<Projects.AWS_Textract_OCR_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
