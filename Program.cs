using FoodMart.Services.AdminAuthServices;
using FoodMart.Services.CategoryServices;
using FoodMart.Services.DashboardServices;
using FoodMart.Services.DiscountServices;
using FoodMart.Services.EmailServices;
using FoodMart.Services.FeatureServices;
using FoodMart.Services.ProductServices;
using FoodMart.Services.SaleServices;
using FoodMart.Services.SubscriberServices;
using FoodMart.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using FluentValidation;
using FoodMart.Filters;
using FoodMart.Validators;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSerilog((services, logger) => logger
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext().WriteTo.Console());
builder.Services.AddValidatorsFromAssemblyContaining<AdminLoginDtoValidator>();
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(_ => "Lütfen geçerli bir sayı giriniz.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, _) => "Girilen değer geçerli değil.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Bu alan zorunludur.");
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<FormValidationFilter>();
    options.Filters.Add<AdminOperationFilter>();
});

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = serviceProvider
        .GetRequiredService<IOptions<MongoDbSettings>>()
        .Value;

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var settings = serviceProvider
        .GetRequiredService<IOptions<MongoDbSettings>>()
        .Value;

    var client = serviceProvider.GetRequiredService<IMongoClient>();

    return client.GetDatabase(settings.DatabaseName);
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISubscriberService, SubscriberService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.AccessDeniedPath = "/Admin/Account/AccessDenied";

        options.Cookie.Name = "FoodMarkt.AdminAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<FoodMart.Services.ImageServices.ImageService>();
builder.Services.AddScoped<FoodMart.Services.MongoIndexService>();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.Name = "FoodMart.CartSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddScoped<FoodMart.Services.CartServices.CartService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    await next(context);
});
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<FoodMart.Services.MongoIndexService>().InitializeAsync();
    var adminAuthService =
        scope.ServiceProvider
            .GetRequiredService<IAdminAuthService>();

    var configuration =
        scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

    var adminExists =
        await adminAuthService.AdminExistsAsync();

    if (!adminExists)
    {
        var adminSeed =
            configuration
                .GetSection("AdminSeed")
                .Get<AdminSeedSettings>();

        if (adminSeed is not null &&
            !string.IsNullOrWhiteSpace(adminSeed.FullName) &&
            !string.IsNullOrWhiteSpace(adminSeed.Email) &&
            !string.IsNullOrWhiteSpace(adminSeed.Password))
        {
            await adminAuthService.CreateAdminAsync(
                adminSeed.FullName,
                adminSeed.Email,
                adminSeed.Password);
        }
    }
}
app.Run();
