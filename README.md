# AI Development Platform — REST API

ASP.NET Core 8 (C#) üzərində **N-Layer Architecture** prinsipi ilə qurulmuş, JWT autentifikasiyalı,
rol-əsaslı (Admin / Manager / User) icazə idarəetməli, tam funksional Admin Panel API-li, SMTP
inteqrasiyalı (qeydiyyat/şifrə bərpası emailləri) və Microsoft SQL Server ilə işləyən REST API.

## 🏗️ Arxitektura (N-Layer)

```
AIDevAPI.sln
└── src/
    ├── AIDevAPI.Domain          → Entity-lər, Enum-lar, sabitlər (heç bir asılılığı yoxdur)
    ├── AIDevAPI.Application     → DTO-lar, interfeyslər, biznes-məntiq (servislər)
    ├── AIDevAPI.Infrastructure  → EF Core DbContext, repository-lər, JWT/SMTP servisləri
    └── AIDevAPI.API             → Controller-lər, Program.cs, middleware, appsettings
```

**Asılılıq istiqaməti:** `API → Infrastructure → Application → Domain` (Domain heç nədən asılı deyil).

## 🚀 Texnologiyalar

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core 8** + **SQL Server**
- **ASP.NET Core Identity** (istifadəçi/rol idarəetməsi, şifrə hash-ləmə, lockout)
- **JWT Bearer** autentifikasiya (access + refresh token, token rotation)
- **SMTP** ilə email göndərmə (qeydiyyat, email təsdiqi, şifrə bərpası)
- **Swagger / OpenAPI** (Bearer token dəstəyi ilə)
- **Repository + Unit of Work** pattern
- Mərkəzləşdirilmiş **Exception Handling Middleware**

## 📦 Layihə strukturu və N-Layer izahı

| Qat | Məsuliyyət |
|---|---|
| **Domain** | `ApplicationUser`, `ApplicationRole`, `RefreshToken`, `AIProject` entity-ləri, `ProjectStatus` enum-u, rol sabitləri |
| **Application** | DTO-lar (Auth/User/Role/Project/Dashboard), interfeyslər (`IAuthService`, `IUserService`, `IRoleService`, `IProjectService`, `IEmailService`, `ITokenService`, `ICurrentUserService`, repository interfeysləri), servis implementasiyaları (biznes qaydalar, validasiya) |
| **Infrastructure** | `ApplicationDbContext` (IdentityDbContext), `GenericRepository`, `ProjectRepository`, `RefreshTokenRepository`, `UnitOfWork`, `TokenService` (JWT generasiya/validasiya), `EmailService` (SMTP), `CurrentUserService`, `DbSeeder` (ilkin admin + rollar) |
| **API** | Bütün controller-lər, `Program.cs` (DI, JWT, Swagger, CORS konfiqurasiyası), `ExceptionHandlingMiddleware`, `appsettings.json` |

## 🔐 Endpoint-lər

### `AuthController` — `/api/auth`
| Metod | Endpoint | Açıqlama |
|---|---|---|
| POST | `/register` | Qeydiyyat (avtomatik "User" rolu, email göndərilir, access+refresh token qaytarılır) |
| POST | `/login` | Giriş (email və ya username ilə) |
| POST | `/refresh-token` | Access tokeni refresh token ilə yeniləmək (rotation) |
| POST | `/revoke-token` 🔒 | Refresh tokeni ləğv etmək (çıxış) |
| POST | `/forgot-password` | Şifrə bərpa linkini email-ə göndərmək |
| POST | `/reset-password` | Token ilə yeni şifrə təyin etmək |
| POST | `/confirm-email` | Email təsdiqi |
| POST | `/change-password` 🔒 | Daxil olmuş istifadəçinin şifrəni dəyişməsi |

### `AccountController` — `/api/account` 🔒
| Metod | Endpoint | Açıqlama |
|---|---|---|
| GET | `/me` | Öz profilini görmək |
| PUT | `/me` | Öz profilini yeniləmək |

### `ProjectsController` — `/api/projects` 🔒 (AI layihələri — nümunə biznes modulu)
| Metod | Endpoint | Açıqlama |
|---|---|---|
| GET | `/` | Layihələr (Admin → hamısı, User → yalnız özünün) |
| GET | `/{id}` | Tək layihə |
| POST | `/` | Yeni layihə yaratmaq |
| PUT | `/{id}` | Layihəni yeniləmək |
| DELETE | `/{id}` | Layihəni silmək (soft-delete) |

### Admin Panel — yalnız `Admin` rolu 🔐

**`AdminUsersController` — `/api/admin/users`**
GET (siyahı, səhifələmə+axtarış) · GET `/{id}` · PUT `/{id}` · DELETE `/{id}` ·
POST `/{id}/lock` · POST `/{id}/unlock` · POST `/{id}/roles` · DELETE `/{id}/roles/{roleName}`

**`AdminRolesController` — `/api/admin/roles`**
GET · GET `/{id}` · POST · DELETE `/{id}`

**`AdminDashboardController` — `/api/admin/dashboard`**
GET `/stats` → istifadəçi/rol/layihə statistikası

## ⚙️ Quraşdırma

### 1. Tələblər
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (lokal, Docker və ya Azure SQL)

### 2. Connection string (`src/AIDevAPI.API/appsettings.json`)
```json
"DefaultConnection": "Server=localhost,1433;Database=AIDevPlatformDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true"
```
Docker ilə SQL Server qaldırmaq istəsəniz:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. JWT ayarları
`appsettings.json` → `JwtSettings.SecretKey` sahəsini **mütləq** dəyişin (minimum 32 simvol, production-da `dotnet user-secrets` və ya environment variable ilə saxlayın).

### 4. SMTP ayarları (email göndərmə)
`appsettings.json` → `SmtpSettings` bölməsini öz SMTP məlumatlarınızla doldurun.
Gmail istifadə edirsinizsə: 2FA aktivləşdirin və **App Password** yaradıb `Password` sahəsinə yazın
(adi Gmail şifrəniz işləməyəcək).

### 5. Migrasiya yaratmaq və database-i qurmaq
```bash
cd AIDevAPI
dotnet tool install --global dotnet-ef   # əgər quraşdırılmayıbsa

dotnet ef migrations add InitialCreate -p src/AIDevAPI.Infrastructure -s src/AIDevAPI.API
dotnet ef database update -p src/AIDevAPI.Infrastructure -s src/AIDevAPI.API
```
> Qeyd: Tətbiq işə düşəndə `Program.cs` daxilində `context.Database.MigrateAsync()` avtomatik
> çağırılır — yəni manual `database update` etməsəniz belə, ilk `dotnet run` zamanı database
> avtomatik yaranıb seed olunacaq (admin istifadəçi + rollar).

### 6. İşə salmaq
```bash
dotnet restore
dotnet run --project src/AIDevAPI.API
```
Swagger UI: `https://localhost:7080/swagger` (və ya `http://localhost:5080/swagger`)

## 👤 Default Admin istifadəçi

İlk işə salınanda avtomatik yaradılır:

| Email | Şifrə |
|---|---|
| `admin@aidev.local` | `Admin@12345` |

> **Production-a çıxmazdan əvvəl bu şifrəni mütləq dəyişin!**

## 🔑 Swagger-də token ilə test etmək
1. `/api/auth/login` (və ya `/register`) ilə `accessToken` alın.
2. Swagger UI-də sağ yuxarıdakı **Authorize** düyməsinə basın.
3. `Bearer {accessToken}` formatında daxil edin.
4. Artıq `[Authorize]` ilə işarələnmiş bütün endpoint-lər əlçatandır.

## 📌 Qeydlər
- Şifrələr ASP.NET Core Identity tərəfindən **PBKDF2** ilə hash-lənərək saxlanılır (heç vaxt plain-text yox).
- 5 səhv giriş cəhdindən sonra hesab 15 dəqiqəliyə avtomatik bloklanır (lockout).
- Refresh token-lər verilənlər bazasında saxlanılır, hər istifadədə **rotation** olunur (köhnəsi ləğv edilir).
- Şifrə dəyişəndə/bərpa olunanda istifadəçinin bütün aktiv sessiyaları (refresh tokenləri) avtomatik ləğv olunur.
- `ProjectsController` AI development layihələrini idarə etmək üçün tam CRUD nümunəsidir — öz biznes modullarınızı eyni Repository/UnitOfWork pattern-i ilə əlavə edə bilərsiniz.
