using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Helpers;

using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

// Register a factory delegate to resolve UserManager<IdentityUser> requests to UserManager<PrimacyUser>
builder.Services.AddTransient<UserManager<PrimacyUser>>();
builder.Services.AddTransient<RoleManager<IdentityRole>>();
builder.Services.AddTransient<SignInManager<PrimacyUser>>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient(typeof(MainRepo<>));
builder.Services.AddTransient(typeof(DocumentRepository));
builder.Services.AddTransient(typeof(IRoleManagment), typeof(RoleManagment));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

builder.Services.AddIdentityCore<PrimacyUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();



    app.ApplyMigrations();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Add the authorization middleware
app.UseAuthorization();

app.MapIdentityApi<PrimacyUser>();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PrimacyUser>>();
    CreateRoles(app.Services, userManager, roleManager).Wait();
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



