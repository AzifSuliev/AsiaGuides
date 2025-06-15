using AsiaGuides.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using AsiaGuides.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using AsiaGuides.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();


// Add services to the container.
builder.Services.AddControllersWithViews();
// For localhost
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// For railway
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddRazorPages();
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseDeveloperExceptionPage(); // для проверки
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection(); 
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
// Для проверки
//app.MapGet("/", () => "Hello from AsiaGuides!");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=UserHome}/{action=Index}/{id?}");

app.MapGet("/", context =>
{
    context.Response.Redirect("/User/UserHome/Index");
    return Task.CompletedTask;
});

app.Run();
