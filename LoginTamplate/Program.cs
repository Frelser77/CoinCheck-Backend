using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using LoginTamplate.Data;
using LoginTamplate.Model;
using LoginTamplate.ModelBinder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi servizi al container.
builder.Services.AddControllers(options =>
{
    // This will insert your custom model binder at the start of the collection
    options.ModelBinderProviders.Insert(0, new CustomModelBinderProvider());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = null;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Aggiungi i servizi Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});


//// Configurazione del contesto del database
builder.Services.AddDbContext<CoinCheckContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Configurazione JWT Bearer
builder
    .Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

builder.Services.AddTransient<IEmailService, EmailService>();


var credentialPath = builder.Configuration["GoogleCloud:CredentialsPath"];
var credential = GoogleCredential.FromFile(credentialPath);
builder.Services.AddSingleton(sp => StorageClient.Create(credential));
builder.Services.AddSingleton(sp => UrlSigner.FromServiceAccountPath(credentialPath));

var supportedCultures = new[] { "it-IT" };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("it-IT");
    options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
});


var app = builder.Build();

// Configurazione middleware per la localizzazione
app.UseRequestLocalization();

// Abilita Swagger solo in ambiente di sviluppo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

app.UseCors("CorsPolicy");

// Configura la pipeline di richiesta HTTP.
app.UseAuthentication();
app.UseAuthorization();

// Mappe controller API.
app.MapControllers();

app.Run();
