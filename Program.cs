
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TestToken.AutoMapper;
using TestToken.Data;
using TestToken.Models;
using TestToken.Repositories.Interfaces;
using TestToken.Repositories.Services;
using TestToken.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using TestToken.DTO.PaymentDto;
using Stripe;
using TokenService = TestToken.Repositories.Services.TokenService;
using CloudinaryDotNet;


namespace TestToken
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            //add connection string 
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // add email config 
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            builder.Services.AddSingleton<EmailTemplateService>(provider =>
          new EmailTemplateService(Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "WelcomeEmailTemplate.html")));

            // add stripe settings 
            var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
            StripeConfiguration.ApiKey = stripeSettings!.SecretKey;

            //add identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //add automapper
            // builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            //add authentication 
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.SaveToken = false;
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience= builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            // add authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
            });
            //add cloudinary service 
            // First, bind the configuration
            var cloudinarySettings = new CloudinarySettings();
            builder.Configuration.GetSection("CloudinarySettings").Bind(cloudinarySettings);

            // Validate the settings
            if (cloudinarySettings == null ||
                string.IsNullOrEmpty(cloudinarySettings.CloudName) ||
                string.IsNullOrEmpty(cloudinarySettings.ApiKey) ||
                string.IsNullOrEmpty(cloudinarySettings.ApiSecret))
            {
                throw new InvalidOperationException("Cloudinary settings are not properly configured.");
            }

            // Create the Cloudinary account
            var account = new CloudinaryDotNet.Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            );
            builder.Services.AddLogging();

            // Create the Cloudinary instance
            var cloudinary = new Cloudinary(account);
            // Register the Cloudinary instance in the dependency injection container
            builder.Services.AddSingleton(cloudinary);
            //inject Services
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailService,EmailService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //add swagger settings 
            builder.Services.AddSwaggerGen(options=>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "E-Commerce"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter You JWT Key"

                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",

                        }, Name = "Bearer",
                        In = ParameterLocation.Header

                    },
                    new List<string>()
                }
            });
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                    policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin()
                );
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
           // app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseCors("MyPolicy");
            app.UseAuthorization();
          

            app.MapControllers();

            //seed roles
            using (var scope = app.Services.CreateScope())
            {
                var roleManager =  scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedData.seedRolesAsync(roleManager);
            }
            app.Run();
        }
    }
}
