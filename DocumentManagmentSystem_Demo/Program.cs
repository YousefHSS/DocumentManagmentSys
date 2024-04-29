using DocumentManagmentSystem_Demo.Data;
using DocumentManagmentSystem_Demo.Helpers;
using DocumentManagmentSystem_Demo.Models;
using DocumentManagmentSystem_Demo.Repo;
using DocumentManagmentSystem_Demo.RoleManagment;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<PrimacyUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
// Register a factory delegate to resolve UserManager<IdentityUser> requests to UserManager<PrimacyUser>

builder.Services.AddTransient<UserManager<PrimacyUser>>();
builder.Services.AddTransient<RoleManager<IdentityRole>>();

builder.Services.AddTransient<SignInManager<PrimacyUser>>();
//builder.Services.AddTransient(typeof(EmailSender));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient(typeof(MainRepo<>));
builder.Services.AddTransient(typeof(DocumentRepository));


builder.Services.AddTransient(typeof(IRoleManagment), typeof(RoleManagment));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(6);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.Name = "DocumentManagmentSys";
    //options.Events = new CookieAuthenticationEvents
    //{
    //    OnRedirectToLogin = ctx =>
    //    {
    //        if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
    //        {
    //            ctx.Response.StatusCode = 401;
    //        }
    //        else
    //        {
    //            ctx.Response.Redirect(ctx.RedirectUri);
    //        }
    //        return Task.CompletedTask;
    //    }
    //};
});

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

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PrimacyUser>>();
    CreateRoles(app.Services, userManager, roleManager).Wait();    // do you things here
}






app.Run();


async Task CreateRoles(IServiceProvider serviceProvider, UserManager<PrimacyUser> userManager, RoleManager<IdentityRole> roleManager)
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

    var adminUser = new PrimacyUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true,
        Name = "Admin",
        Surname = "Admin"
    };

    var uploader = new PrimacyUser
    {
        UserName = "Uploader@Email.com",
        Email = "Uploader@Email.com",
        EmailConfirmed = true,
        Name = "Uploader",
        Surname = "Uploader"
    };

    var revisor = new PrimacyUser
    {
        UserName = "Revisor@Email.com",
        Email = "Revisor@Email.com",
        EmailConfirmed = true,
        Name = "Revisor",
        Surname = "Revisor"
    };
    var finalizer = new PrimacyUser
    {
        UserName = "Finalizer@Email.com",
        Email = "Finalizer@Email.com",
        EmailConfirmed = true,
        Name = "Finalizer",
        Surname = "Finalizer"
    };

    // Check if the admin exists, create it if not
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
        var createUploader = await userManager.CreateAsync(uploader, adminPassword);
        var createRevisor = await userManager.CreateAsync(revisor, adminPassword);
        var createFinalizer = await userManager.CreateAsync(finalizer, adminPassword);
        if (createAdmin.Succeeded)
        {
            // Here we assign the new user the "Admin" role 
            await userManager.AddToRoleAsync(adminUser, "Admin");
            await userManager.AddToRoleAsync(uploader, "Uploader");
            await userManager.AddToRoleAsync(revisor, "Revisor");
            await userManager.AddToRoleAsync(finalizer, "Finalizer");
        }
    }
}



