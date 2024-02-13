using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<RoleManager<IdentityRole>>();
builder.Services.AddTransient<UserManager<IdentityUser>>();
builder.Services.AddTransient<SignInManager<IdentityUser>>();
builder.Services.AddTransient(typeof(IRepository<>), typeof(MainRepo<>));
builder.Services.AddTransient(typeof(DocumentRepository));
builder.Services.AddTransient(typeof(IRoleManagment), typeof(RoleManagment));
// Add RoleManager to RoleManagment and UserManager



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    CreateRoles(app.Services, userManager, roleManager).Wait();    // do you things here
}


app.Run();


async Task CreateRoles(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{


    string[] roleNames = { "Admin", "Revisor", "User", "Uploader", "Finalizer" }; // You can add more roles as needed


    IdentityResult roleResult;


    foreach (var roleName in roleNames)
    {
        // Check if the role exists, create it if not
        var roleExist = await roleManager.RoleExistsAsync(roleName);

        if (!roleExist)
        {
            // Create the roles and seed them to the database
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    string adminEmail = "admoon@Email.com"; // Admin email
    string adminPassword = "Admin123!"; // Admin password

    var adminUser = new IdentityUser
    {
        UserName = adminEmail,
        Email = adminEmail
    };

    // Check if the admin exists, create it if not
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
        if (createAdmin.Succeeded)
        {
            // Here we assign the new user the "Admin" role 
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

