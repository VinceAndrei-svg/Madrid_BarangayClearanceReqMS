using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj1.Filters;
using Proj1.Interfaces;
using Proj1.MappingProfiles;
using Proj1.Models.Configuration;
using Proj1.Repositories;
using Proj1.Seed;
using Proj1.Services;
using ApplicationDbContext = Proj1.Data.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// DATABASE CONFIGURATION
// ========================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // Best Practice: Auto-retry on transient failures
    ));

// ========================================
// IDENTITY & AUTHENTICATION
// ========================================

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production with email confirmation
    
    // Password policy - follows NIST guidelines
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings - prevents brute force attacks
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>() // Enable role-based authorization
.AddEntityFrameworkStores<ApplicationDbContext>();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Session expires after 8 hours
    options.SlidingExpiration = true; // Extends session if user is active
});

// ========================================
// DEPENDENCY INJECTION - REPOSITORIES
// ========================================
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<BarangayInfo>(
    builder.Configuration.GetSection("BarangayInfo"));
builder.Services.Configure<ClearanceSettings>(
    builder.Configuration.GetSection("ClearanceSettings"));


// Best Practice: Register from most specific to least specific
builder.Services.AddScoped<IResidentRepository, ResidentRepository>();
builder.Services.AddScoped<IClearanceRequestRepository, ClearanceRequestRepository>();
builder.Services.AddScoped<IClearanceTypeRepository, ClearanceTypeRepository>();

// ✅ Audit log repository
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// ========================================
// DEPENDENCY INJECTION - SERVICES
// ========================================

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IClearanceRequestService, ClearanceRequestService>();
builder.Services.AddScoped<IClearanceTypeService, ClearanceTypeService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IPdfClearanceService, PdfClearanceService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// ✅ Audit log service
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ✅ Audit action filter (MUST be registered before AddControllersWithViews)
builder.Services.AddScoped<AuditActionFilter>();

// ========================================
// AUTOMAPPER CONFIGURATION
// ========================================

// ✅ FIXED: Register AutoMapper ONLY ONCE
// Best Practice: Register all profiles from assembly
builder.Services.AddAutoMapper(typeof(ResidentProfile).Assembly);

// ========================================
// MVC & RAZOR PAGES
// ========================================

// ✅ CRITICAL FIX: Register AddControllersWithViews ONLY ONCE
builder.Services.AddControllersWithViews(options =>
{
    // Best Practice: Add anti-forgery token to all POST requests automatically
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    
    // ✅ CRITICAL: Add audit action filter
    options.Filters.AddService<AuditActionFilter>();
});

builder.Services.AddRazorPages();

// ========================================
// SESSION (Optional - for multi-step forms)
// ========================================

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Security: Prevent JavaScript access
    options.Cookie.IsEssential = true;
});

// ========================================
// BUILD APPLICATION
// ========================================

var app = builder.Build();

// ========================================
// SEED DATABASE (Roles, Admin, Clearance Types)
// ========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // Seed roles and admin user
        await IdentitySeeder.SeedRolesAndAdminAsync(services);
        
        // Seed clearance types
        await ClearanceTypeSeeder.SeedClearanceTypesAsync(services);
        
        // Log success
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        
        // In development, throw to see the error immediately
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// ========================================
// VALIDATE AUTOMAPPER (Development only)
// ========================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var mapper = scope.ServiceProvider.GetRequiredService<AutoMapper.IMapper>();
    
    try
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("AutoMapper configuration is valid.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "AutoMapper configuration is invalid!");
        throw; // Fail fast on invalid mapping
    }
}

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

if (!app.Environment.IsDevelopment())
{
    // Production error handling
    app.UseExceptionHandler("/Home/Error");
    
    // HSTS - Security header for HTTPS
    app.UseHsts();
}
else
{
    // Development error page with stack trace
    app.UseDeveloperExceptionPage();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Serve static files (CSS, JS, images)
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Session middleware (if using sessions)
app.UseSession();

// Authentication middleware - MUST come before Authorization
app.UseAuthentication();

// Authorization middleware
app.UseAuthorization();

// ========================================
// ROUTE CONFIGURATION
// ========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Razor Pages (for Identity UI)
app.MapRazorPages();

// ========================================
// RUN APPLICATION
// ========================================

app.Run();

