using Microsoft.EntityFrameworkCore;
using XMLEdition.Core.Services;
using XMLEdition.DAL.EF;
using XMLEdition.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ProjectContext>(options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ));

builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CourseItemRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<CourseTypeRepository>();
builder.Services.AddScoped<LessonRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<CourseService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();   
}

app.UseStaticFiles();

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
