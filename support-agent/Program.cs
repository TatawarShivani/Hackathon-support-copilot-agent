using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using SupportAgent.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure AWS Bedrock
var awsProfile = Environment.GetEnvironmentVariable("AWS_PROFILE") ?? "as24-bedrock-readonly";
var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1";

builder.Services.AddSingleton<IAmazonBedrockRuntime>(sp =>
{
    var chain = new CredentialProfileStoreChain();
    AWSCredentials credentials;

    if (chain.TryGetAWSCredentials(awsProfile, out credentials))
    {
        return new AmazonBedrockRuntimeClient(credentials, Amazon.RegionEndpoint.GetBySystemName(awsRegion));
    }

    // Fallback to default credentials
    return new AmazonBedrockRuntimeClient(Amazon.RegionEndpoint.GetBySystemName(awsRegion));
});

// Register custom services
builder.Services.AddScoped<IJiraService, JiraService>();
builder.Services.AddScoped<IClaudeService, ClaudeService>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
