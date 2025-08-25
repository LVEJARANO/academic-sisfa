using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using AcademicoSFA.Repositories;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Application.Services;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Domain.Monitoring; // ← NUEVA LÍNEA
using Prometheus; // ← NUEVA LÍNEA
using Serilog; // ← NUEVA LÍNEA
using Serilog.Sinks.Elasticsearch; // ← NUEVA LÍNEA

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURACIÓN DE SERILOG PARA ELASTICSEARCH =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "sisfa-logs-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7
    })
    .CreateLogger();

builder.Host.UseSerilog(); // ← NUEVA LÍNEA

// En esta parte del código se usa la memoria 
// caché de la sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IPasswordHasher<Participante>, PasswordHasher<Participante>>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<SfaDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DBSFA"),
        new MySqlServerVersion(new Version(8, 0, 32))
    ));

//*** INYECCION DE DEPENDENCIAS DE REPOSITORIES (tu código existente)
builder.Services.AddScoped<MateriaRepository>();
builder.Services.AddScoped<MatriculaRepository>();
builder.Services.AddScoped<GradosGruposRepository>();
builder.Services.AddScoped<PagosRepository>();
builder.Services.AddScoped<PeriodoAcademicoRepository>();
builder.Services.AddScoped<NotasRepository>();
builder.Services.AddScoped<GrupoAcademicoRepository>();
builder.Services.AddTransient<IPasswordHasher<Participante>, PasswordHasher<Participante>>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<IGradosGruposRepository, GradosGruposRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IGrupoAcademicoRepository, GrupoAcademicoRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IMatriculaRepository, MatriculaRepository>();
builder.Services.AddScoped<INotasRepository, NotasRepository>();
builder.Services.AddScoped<IPagosRepository, PagosRepository>();
builder.Services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
builder.Services.AddScoped<IPeriodoRepository, PeriodoAcademicoRepository>();

// ===== REGISTRAR SERVICIO DE MÉTRICAS =====
builder.Services.AddSingleton<ISisfaMetrics, SisfaMetrics>(); // ← NUEVA LÍNEA

// Configuración del servicio de hash de contraseñas
builder.Services.AddScoped<IPasswordHasher<Participante>, PasswordHasher<Participante>>();

// Configuración de la autenticación (tu código existente)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(options =>
{
    options.LoginPath = "/InicioSesion/Login";
    options.AccessDeniedPath = "/InicioSesion/AccesoDenegado";
})
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.SaveTokens = true;
});

// Agrega Razor Pages (tu código existente)
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Presentation");

builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.PageViewLocationFormats.Add("/Pages/Partials/{0}" + RazorViewEngine.ViewExtension);
});

// ===== AGREGAR MÉTRICAS HTTP AUTOMÁTICAS =====
//builder.Services.AddHttpMetrics(); // ← NUEVA LÍNEA

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== MIDDLEWARE DE MÉTRICAS =====
app.UseHttpMetrics(); // ← NUEVA LÍNEA

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Configuración de rutas de Razor Pages
app.MapRazorPages();

// ===== ENDPOINT DE MÉTRICAS PARA PROMETHEUS =====
app.MapMetrics(); // ← NUEVA LÍNEA (disponible en /metrics)

app.Run();