using HospitalManagementSystem.Data;
using HospitalManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// ===================================
// Servicios
//====================================

// Configuración de JSON (Ignorar ciclos de referencia)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 1. Configuración de la Base de Datos (Dapper)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
}

builder.Services.AddSingleton<DapperDbConnectionFactory>();

// 2. Servicios Personalizados
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();



// 3. Configuración de JWT (Authentication)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key no configurada. Por favor, genere una y añádala en appsettings.json");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Eventos para logging detallado
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {Exception}", context.Exception);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });


// 4. Configuración de Autorización (Policies)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOrAdmin", policy => policy.RequireRole("Doctor", "Admin"));
    options.AddPolicy("PatientOrAdmin", policy => policy.RequireRole("Patient", "Admin"));
    options.AddPolicy("EmployeeOrAdmin", policy => policy.RequireRole("Employee", "Admin"));
});


// 5. Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Añade el dominio de tu frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


// 6. Configuración de Swagger/OpenAPI con JWT (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Hospital Management System API", 
        Version = "v1",
        Description = "API para gestión de pacientes, doctores y citas médicas",
        Contact = new OpenApiContact
        {
            Name = "Soporte Hospital",
            Email = "victor19vargas2018@gmail.com"
        }
    });

    // Configuración para el botón "Authorize"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                { 
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// 7. Configuración de Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// =========================================================================================
//                                   SECTION: APP PIPELINE (Middlewares)
// =========================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management System API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Usa CORS
app.UseCors("AllowAngular");

// MIDDLEWARES DE SEGURIDAD (¡Orden crítico!)
app.UseAuthentication(); // 1. Lee y valida el token
app.UseAuthorization();  // 2. Aplica las reglas [Authorize]

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));


app.Run();
