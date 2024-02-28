using ChocolateDelivery.DAL;
using ChocolateDelivery.UI.CustomFilters;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// connect to mysql with connection string from app settings
// Add services to the container.

ConfigurationManager configuration = builder.Configuration;
var rabbitMQSection = configuration.GetSection("ConnectionStrings");
var connection_string = rabbitMQSection["DefaultConnection"];
builder.Services.AddDbContext<ChocolateDeliveryEntities>(options => options.UseMySql(connection_string, ServerVersion.AutoDetect(connection_string)));

//Below code is used to check session globally
builder.Services.AddControllers(config =>
{
    config.Filters.Add(new CheckSession());
});

//Below code is used to check session controller wise
//builder.Services.AddScoped<CheckSession>();

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

int session_expire_time = configuration.GetValue<int>("Session_Expires_Time");
//Below two lines is used for using session across the app
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(session_expire_time);
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

//app.MapAreaControllerRoute(
//                 name: "Admin",
//                 areaName: "Admin",
//                 pattern: "Admin/{controller=Home}/{action=Index}");

app.MapAreaControllerRoute(
            name: "MyAreaListRoute",
            areaName: "Admin",
             pattern: "List/{id:int}",
            defaults: new { controller = "List", action = "Index" });
app.MapAreaControllerRoute(
            name: "MerchantListRoute",
            areaName: "Merchant",
             pattern: "Merchant/List/{id:int}",
            defaults: new { controller = "List", action = "Index" });
app.MapAreaControllerRoute(
    name: "ReportRoute",
       areaName: "Admin",
    pattern: "Report/{id:int}",
    defaults: new { controller = "Report", action = "Index" });
app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
//giving specific routing for list page where we are not passing action in URL because it will always be Index()
app.MapControllerRoute(
    name: "ListRoute",
    pattern: "List/{id:int}",
    defaults: new { controller = "List", action = "Index" });
app.MapControllerRoute(
    name: "PaymentDueRoute",
    pattern: "PaymentDue/Update/{id?}/{secondid?}",
    defaults: new { controller = "PaymentDue", action = "Update" });
app.MapControllerRoute(
    name: "ProductListRoute",
    pattern: "Product/{id:int}",
    defaults: new { controller = "Product", action = "Index" });

app.MapAreaControllerRoute(
    name: "default",
     areaName: "Admin",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
