using ESS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.OpenApi.Models;
using ESS.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DBConnection");

builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "JWT_OR_COOKIE";
    options.DefaultChallengeScheme = "JWT_OR_COOKIE";
})
    .AddCookie(options =>
    {
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.HttpOnly = false;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Path = "/";
        options.LoginPath = "/identity/account/loginidentity/account/login";
        options.LogoutPath = "/identity/account/loginidentity/account/logout";
        options.AccessDeniedPath = "/Identity/Account/accessdenied";

    })

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtToken:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtToken:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtToken:SigningKey"]))
        };
    }).AddPolicyScheme("JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            return context.Request.Path.StartsWithSegments("/api/") ? "Bearer" : "Cookie";
        };
    });

//builder.Services.AddAuthentication().AddIdentityServerJwt();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MemberPolicy", pb =>
    {
        pb.RequireRole("Member");

    });

    options.AddPolicy("EmployerPolicy", pb =>
    {
        pb.RequireRole("Employer");

    });

    options.AddPolicy("AccountHandlerPolicy", pb =>
    {
        pb.RequireRole("AccountHandler");

    });

    options.AddPolicy("CanSetPasswordPolicy", pb =>
    {
        pb.RequireClaim("CanChangePassword", "Yes");

    });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        }
    ).AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

//Default Email & SMS Service
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<ISmsSender, SmsSender>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");

app.MapRazorPages();

app.MapDefaultControllerRoute();

string? environment = builder.Configuration["Environment"];
if (environment != null && environment == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
