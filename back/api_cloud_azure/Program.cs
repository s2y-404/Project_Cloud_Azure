using api_cloud_azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();


app.MapGet("/", () =>
{
    return Results.NotFound("Default endpoint don't existe");
})
.WithName("Default")
.WithOpenApi();


/// <summary>
/// method to create a new resource group and a new virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /vm/create/demo-cloud-1/windows
/// </remarks>
/// <param name="name">The name of the resource group and the virtual machine</param>
/// <param name="os">The operating system of the virtual machine</param>
/// <returns>Description text of the creation avancement</return>
app.MapGet("/vm/create/{name}/{os}", (string name, string os) =>
{
    AzureVM vm = new AzureVM(name);

    switch (os)
    {
        case "windows":
            _ = vm.CreateAndDeleteAzureResourcesWithDelay("windows");
            break;

        case "ubuntu":
            _ = vm.CreateAndDeleteAzureResourcesWithDelay("ubuntu");
            break;

        case "debian":
            _ = vm.CreateAndDeleteAzureResourcesWithDelay("debian");
            break;

        default:
            return Results.NotFound("Invalid OS");
    }

    if (vm.GetIP() == null)
    {
        return Results.BadRequest("Resource Group Not Created");
    }
    else
    {
        return Results.Ok($"{vm.GetLogMessages()}");
    }

}).WithTags("Virtual Machine");

/// <summary>
/// method to get the public IP of a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
///     
///     GET /vm/ip/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
/// <returns>The public IP of the virtual machine</returns>
app.MapGet("/vm/ip/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);

    return Results.Ok($"{vm.GetIP()}");

}).WithTags("Virtual Machine");

/// <summary>
/// method to get the status of a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /vm/status/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
/// <returns>The status of the virtual machine</returns>
app.MapGet("/vm/status/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);

    return Results.Ok($"{vm.CheckVMStatus()}");

}).WithTags("Virtual Machine");

/// <summary>
/// method to start a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /vm/stop/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
app.MapGet("/vm/stop/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);
    vm.ShutDownVM();

    return "VM Stop";

}).WithTags("Virtual Machine");

/// <summary>
/// method to start a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /vm/start/demo-cloud-1
/// </remarks>
app.MapGet("/vm/restart/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);
    vm.RestartVM();

    return "VM Restart";

}).WithTags("Virtual Machine");

/// <summary>
/// method to delete a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /vm/delete/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
app.MapGet("/vm/delete/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);
    vm.DeleteAzureVM();

    return "VM Deleted";

}).WithTags("Virtual Machine");

/// <summary>
/// method to delete a resource group
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /rg/delete/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
app.MapGet("/rg/delete/{name}", (string name) =>
{
    AzureVM vm = new AzureVM(name);
    vm.DeleteAzureRG();

    return "RG Deleted";

}).WithTags("Ressource Group");

/// <summary>
/// method to get the log messages of a virtual machine
/// </summary>
/// 
/// <remarks>
/// Sample request:
/// 
///     GET /log/demo-cloud-1
/// </remarks>
/// <param name="name">The name of the virtual machine</param>
/// <returns>The log messages of the virtual machine</returns>
app.MapGet("/log/{name}", (string name) =>
{
    return new AzureVM(name).GetLogMessages();

}).WithName("Log");

app.Run();

