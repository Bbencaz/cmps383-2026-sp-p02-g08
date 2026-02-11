
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

builder.Services.AddIdentity<User, Role>
().AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/authentication/login"; // where unauthenticated users get redirected
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401; // Prevents redirect; sends HTTP 401 instead
        return Task.CompletedTask;
    };
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();

    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        db.SaveChanges();
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new Role { Name = "Admin" });

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new Role { Name = "User" });

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    async Task<User> CreateUserIfNotExists(string username, string password, string role)
    {
        var user = await userManager.FindByNameAsync(username);  // check if user exists
        if (user == null)
        {
            user = new User { UserName = username };             // create a new User object
            await userManager.CreateAsync(user, password);      // Identity handles hashing
            await userManager.AddToRoleAsync(user, role);       // assigns user to role
        }
        return user;
    }

    var galkadi = await CreateUserIfNotExists("galkadi", "Password123!", "Admin");
    var bob = await CreateUserIfNotExists("bob", "Password123!", "User");
    var sue = await CreateUserIfNotExists("sue", "Password123!", "User");

}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(x =>
    {
        x.MapControllers();
    });

if (app.Environment.IsDevelopment())
{
    app.UseSpa(x =>
    x.UseProxyToSpaDevelopmentServer("http://localhost:5173"));
}
else
{
    app.MapFallbackToFile("index.html");
}


app.Run();



//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program
{


}

