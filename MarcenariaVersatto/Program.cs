using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MarcenariaVersatto.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração local (senha do banco) fica em appsettings.Local.json, que NÃO vai para o GitHub.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllersWithViews(options =>
{
    // Mensagens padrão de validação em português
    var msg = options.ModelBindingMessageProvider;
    msg.SetValueIsInvalidAccessor(v => $"O valor '{v}' não é válido.");
    msg.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
    msg.SetAttemptedValueIsInvalidAccessor((v, campo) => $"O valor '{v}' não é válido para {campo}.");
    msg.SetValueMustNotBeNullAccessor(campo => $"Informe o campo {campo}.");
    msg.SetMissingBindRequiredValueAccessor(campo => $"Informe o campo {campo}.");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
