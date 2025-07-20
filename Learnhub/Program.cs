using Learnhub.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LearnhubDB")));

// Add MVC services
builder.Services.AddControllersWithViews();

// Enable session management & caching
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

// Enable static files (ensuring CSS & JS load correctly)
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add core middleware
app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure static resources load correctly
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Configure the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
