using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using user_panel.Context;
using user_panel.Services.Base;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinReservationServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.CityServices;
using user_panel.Services.Entity.DistrictServices;
using user_panel.Services.Entity.LogServices;
using user_panel.Services.Entity.ReviewServices;
using user_panel.Services.Entity.WorkoutService;
using user_panel.Services.Firebase;
using user_panel.Settings;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Serilog Konfig�rasyonu ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .CreateLogger();

builder.Host.UseSerilog();


// --- 2. Veritaban� ve Identity Servisleri ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// --- 3. Firebase Admin SDK'n�n Ba�lat�lmas� ---
try
{
    var firebaseConfig = builder.Configuration.GetSection("Firebase");
    var projectId = firebaseConfig["ProjectId"];
    var credentialsPath = firebaseConfig["CredentialsPath"];

    if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(credentialsPath))
    {
        throw new InvalidOperationException("Firebase 'ProjectId' or 'CredentialsPath' is not configured in appsettings.json.");
    }

    var absoluteCredentialsPath = Path.Combine(AppContext.BaseDirectory, credentialsPath);

    if (!File.Exists(absoluteCredentialsPath))
    {
        // Dosya bulunamazsa, bu kritik bir hatad�r.
        Log.Fatal("Firebase credentials file NOT FOUND at: {Path}. Ensure the file name is correct and 'Copy to Output Directory' is set to 'Copy if newer'.", absoluteCredentialsPath);
        throw new FileNotFoundException("Firebase credentials file could not be located.", absoluteCredentialsPath);
    }

    // --- YEN� YAKLA�IM BURADA ---
    // �nce kimlik bilgisini olu�tur.
    var credential = GoogleCredential.FromFile(absoluteCredentialsPath);

    // Bu kimlik bilgisini kullanarak bir FirestoreDb instance'� olu�tur.
    var firestoreDb = new FirestoreDbBuilder
    {
        ProjectId = projectId,
        Credential = credential
    }.Build();

    // Bu olu�turdu�umuz instance'� DI container'a singleton olarak ekle.
    builder.Services.AddSingleton(firestoreDb);

    // FirebaseApp'i sadece Firestore d���nda ba�ka Firebase servisleri (�rn: Auth, Messaging)
    // kullanacaksak ba�latmam�z gerekir. �imdilik bu ad�m� da ekleyelim, gelecekte laz�m olabilir.
    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = projectId
        });
    }

    Log.Information("Firebase services successfully configured for project {ProjectId}", projectId);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Firebase Admin SDK initialization failed.");
    throw;
}


// --- 4. Kendi Servislerinizin Kaydedilmesi ---
builder.Services.AddScoped(typeof(IEntityService<,>), typeof(EntityService<,>));
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<ICabinService, CabinService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICabinReservationService, CabinReservationService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// --- 5. Ayar S�n�flar�n�n Kaydedilmesi (GoogleMapsSettings) ---
builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection(GoogleMapsSettings.SectionName));
//--- 6.Form Ayarlari ---
builder.Services.Configure<FormOptions>(o => {
    o.MultipartBodyLengthLimit = 256 * 1024 * 1024; // 256 MB örnek
});

// --- Uygulama Yap�land�rmas� ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();