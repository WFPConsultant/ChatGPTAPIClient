using ChatGPTAPIClient.Models;

var builder = WebApplication.CreateBuilder(args);

//Bind Secrets to get secrets from appsetting.json
var Secrets = builder.Configuration.GetSection("Secrets").Get<Secrets>();
if (Secrets?.ApiKey?.StartsWith("sk") == true)
{
    var envKey = Secrets.ApiKey.Substring(4);
    var envValue = Environment.GetEnvironmentVariable(envKey);
    if (!string.IsNullOrWhiteSpace(envValue))
        Secrets.ApiKey = envValue;
}
builder.Services.AddSingleton(Secrets);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
