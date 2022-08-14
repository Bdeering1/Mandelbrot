using System.Text.Json.Serialization;
using Mandelbrot.Server.Core;
using Mandelbrot.Server.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddRazorPages();

builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});
//builder.Services.AddResponseCompression(opts =>
//{
//    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
//});

ConfigureServices(builder.Services);
var app = builder.Build();


//app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapHub<UpdateHub>("/hub");
app.MapFallbackToFile("index.html");
        
app.Run();


static void ConfigureServices(IServiceCollection collection)
{
    collection.AddSingleton<Camera>();
    collection.AddSingleton<SetGenerator>();
    collection.AddSingleton<EscapeTime>();
    collection.AddSingleton<UpdateHub>();
}

