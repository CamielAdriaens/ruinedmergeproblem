using Microsoft.AspNetCore.Authentication.JwtBearer;
using FileStorage.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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


// Register MongoDbContext as a singleton service
builder.Services.AddSingleton<MongoDbContext>();

// Add JWT Bearer authentication using Google's authentication scheme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.Audience = "328029009489-7j3ksnst6u05dbq0uhtnc2862t027se7.apps.googleusercontent.com"; // Your Google Client ID
    });
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); // Ensure this is added before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
