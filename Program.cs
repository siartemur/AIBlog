using AIBlog.Data;
using AIBlog.Interfaces;
using AIBlog.Services;
using AIBlog.Services.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 🔌 Razor + MVC
builder.Services.AddControllersWithViews();

// 🔐 Cookie Authentication + Yetkilendirme
builder.Services.AddAuthentication("UserAuth")
    .AddCookie("UserAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(6);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// 🔗 Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var provider = builder.Configuration["DatabaseProvider"];

    if (provider == "Postgres")
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
    }
    else // default SQL Server
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});


// 💡 Generic Repository + Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 🧠 Servis Katmanları
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 🧬 Seed işlemi – DB migrate + ilk veri
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    SeedData.Initialize(context);
}

// 🌐 Route Tanımları

// 📌 Varsayılan route (önce gelmeli!)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 📌 SEO uyumlu blog detay slug route (en son!)
app.MapControllerRoute(
    name: "blog-details",
    pattern: "blog/{url}",
    defaults: new { controller = "Blog", action = "Details" });

app.Run();
