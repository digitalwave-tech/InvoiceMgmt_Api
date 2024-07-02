   

    using BusinessLayer.Repositories;
    using DAL.Interfaces;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;

    var builder = WebApplication.CreateBuilder(args);

    // Jwt configuration starts here
    var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
    var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
    // Jwt configuration ends here

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddScoped<ICustomer, CustomerRepo>();
    builder.Services.AddScoped<IInvoice, InvoiceRepo>();
    builder.Services.AddScoped<IErrorLogging,ErrorLogRepo>();

    builder.Services.AddAuthorization();
    builder.Services.AddCors(options =>
    {
        //options.AddPolicy("AllowOrigin",
        //    builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        options.AddPolicy("AllowLocalhost",
        builder => builder.WithOrigins("http://localhost:3000") // Adjust this URL to match your React app's URL
                          .AllowAnyMethod()
                          .AllowAnyHeader());

    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseRouting(); // Ensure routing is configured before authentication and authorization
    app.UseCors("AllowLocalhost");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

