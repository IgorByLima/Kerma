using Kerma.API.Hubs;
using Kerma.Application.Services;
using Kerma.Domain.Entities;
using Kerma.Domain.Interfaces;
using Kerma.Infrastructure.Data;
using Kerma.Infrastructure.Printing;
using Kerma.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ─── DbContext + MariaDB ──────────────────────────────────────────────────────
builder.Services.AddDbContext<KermaDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// ─── Injeção de dependência — Repositórios ────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IFornecedorRepositorio, FornecedorRepositorio>();
builder.Services.AddScoped<IFornecedorItemRepositorio, FornecedorItemRepositorio>();
// ─── Injeção de dependência — Repositórios ────────────────────────────────────
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(RepositorioGenerico<>));
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IFornecedorRepositorio, FornecedorRepositorio>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IFornecedorItemRepositorio, FornecedorItemRepositorio>();
builder.Services.AddScoped<IEventoRepositorio, EventoRepositorio>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IItemRepositorio, ItemRepositorio>();
builder.Services.AddScoped<IDevolucaoRepositorio, DevolucaoRepositorio>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICaixaRepositorio, CaixaRepositorio>();
builder.Services.AddScoped<ICaixaService, CaixaService>();
builder.Services.AddScoped<IVendaRepositorio, VendaRepositorio>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<ITicketRepositorio, TicketRepositorio>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IRepositorio<Devolucao>, DevolucaoRepositorio>();
builder.Services.AddScoped<IDevolucaoService, DevolucaoService>();
builder.Services.AddScoped<IImpressoraService, ImpressoraService>();
builder.Services.AddScoped<ICozinhaEventoRepositorio, CozinhaEventoRepositorio>();
builder.Services.AddScoped<ICozinhaEventoService, CozinhaEventoService>();
// ─── Injeção de dependência — Services ───────────────────────────────────────
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();

// ─── JWT ──────────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Kerma API", Version = "v1" });

    // Permite enviar o token JWT pelo Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("DesenvolvimentoLocal", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ─── Aplicar migrations automaticamente ──────────────────────────────────────
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<KermaDbContext>();
//    await db.Database.MigrateAsync();
//}

// ─── Middlewares ──────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DesenvolvimentoLocal");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<FilaCozinhaHub>("/hubs/cozinha");


app.Run();