using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RequestFlow.API.Filter;
using RequestFlow.API.Middleware;
using RequestFlow.Common.Reporter;
using RequestFlow.Common.Settings;
using RequestFlow.Data.Data;
using RequestFlow.Data.Identity;
using RequestFlow.Data.Provider;
using RequestFlow.Services.Contracts;
using RequestFlow.Services.Core;
using RequestFlow.Services.Dto.User;
using RequestFlow.Services.DtoValidation.User;
using RequestFlow.Services.Mapping;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Fluent Validations
builder.Services.AddTransient<IValidator<UserCreateDto>, UserCreateDtoValidator>();
builder.Services.AddTransient<IValidator<UserUpdateDto>, UserUpdateDtoValidator>();
builder.Services.AddTransient<IValidator<ForgetPasswordDto>, ForgetPasswordDtoValidator>();
builder.Services.AddTransient<IValidator<LoginDto>, LoginDtoValidator>();
builder.Services.AddTransient<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();
builder.Services.AddTransient<IValidator<ChangePasswordDto>, ChangePasswordDtoValidator>();

//identity config
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = Convert.ToInt32(builder.Configuration.GetSection("IdentitySettings:RequireLength").Value);
    options.Password.RequiredUniqueChars = Convert.ToInt32(builder.Configuration.GetSection("IdentitySettings:RequireUniqueChars").Value);

    //Email settings
    options.User.RequireUniqueEmail = true;
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//db config
//services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
}
, ServiceLifetime.Transient);

//configure services
builder.Services.Configure<SentrySettings>(builder.Configuration.GetSection("SentrySettings"));
builder.Services.Configure<SystemSettings>(builder.Configuration.GetSection("SystemSettings"));
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection("IdentitySettings"));
builder.Services.AddScoped<IErrorReporter, SentryErrorReporter>();

ConfigureLog(builder.Services);

//sentry setting

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()    // ? allow any domain
            .AllowAnyHeader()    // ? allow all headers
            .AllowAnyMethod();   // ? allow GET, POST, PUT, DELETE etc.
    });
});
var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSettings:SecretKey").Value);
var audience = builder.Configuration.GetSection("JWTSettings:Audience").Value;
var issuer = builder.Configuration.GetSection("JWTSettings:Issuer").Value;
var tokenLifespan = Convert.ToInt32(builder.Configuration.GetSection("JWTSettings:TokenLifespan").Value);

builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(tokenLifespan));

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidIssuer = issuer,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

//configure Auto-Mapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

//services
builder.Services.AddTransient(typeof(IUserService), typeof(UserService));
builder.Services.AddTransient(typeof(ILanguageService), typeof(LanguageService));
builder.Services.AddTransient(typeof(IMetaDataService), typeof(MetaDataService));
builder.Services.AddTransient(typeof(ISubscriptionService), typeof(SubscriptionService));
builder.Services.AddTransient(typeof(IApplicationRoleService), typeof(ApplicationRoleService));
builder.Services.AddTransient(typeof(IResourceService), typeof(ResourceService));
builder.Services.AddTransient(typeof(ITenantService), typeof(TenantService));
builder.Services.AddTransient<ITenantProvider, TenantProvider>();

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ValidateModelStateFilter));
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ILS.Portal.WebApi - ", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

static void ConfigureLog(IServiceCollection services)
{
    var serviceProvider = services.BuildServiceProvider();
    var logger = serviceProvider.GetService<ILogger<ExceptionHandlingMiddleware>>();
    services.AddSingleton(typeof(ILogger), logger);
}

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
