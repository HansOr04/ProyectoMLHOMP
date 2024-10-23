using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProyectoMLHOMP.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // <-- Asegúrate de importar este espacio de nombres

var builder = WebApplication.CreateBuilder(args);

// Configurar la base de datos
builder.Services.AddDbContext<DataProyecto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataProyecto") ?? throw new InvalidOperationException("Connection string 'DataProyecto' not found.")));

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Users/Login";  // Ruta donde está tu página de login
        options.LogoutPath = "/Users/Logout"; // Ruta donde está la página de logout
        options.Cookie.HttpOnly = true;      // Para mayor seguridad
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Tiempo de expiración de la cookie
    });

// Agregar servicios de HttpClient
builder.Services.AddHttpClient();

// Agregar servicios al contenedor (controladores y vistas)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar autenticación y autorización
app.UseAuthentication();  // <-- Middleware para la autenticación
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
