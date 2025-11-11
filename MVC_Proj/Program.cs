using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC_Proj.Data; // ajuste se o namespace do seu projeto for diferente
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EF Core + Npgsql (Postgres)
// Usa a connection string "DefaultConnection" do appsettings.json ou variável de ambiente
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity com suporte a Roles e persistência via EntityFramework (ApplicationDbContext)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        // aqui você pode ajustar requisitos de senha, lockout, etc.
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Aplica migrations e executa seed inicial (roles, admin, dados de exemplo)
// Faz dentro de um scope para obter RoleManager/UserManager/DbContext
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // aplica quaisquer migrations pendentes (útil em desenvolvimento/Docker)
        context.Database.Migrate();

        // SeedData.InitializeAsync deve criar roles (Admin/User) e um admin seed
        // (implementação do SeedData apresentada anteriormente)
        SeedData.InitializeAsync(services).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao migrar/seedar a base de dados.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Autenticação deve vir antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Necessário para as páginas de Identity (se estiver usando a UI pronta)
app.MapRazorPages();

app.Run();