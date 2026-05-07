# 🏨 TrimangoCalendar

## HotelRunner Benzeri SaaS Kiralama Yönetim Platformu

---

## 📋 İçindekiler

1. [Proje Hakkında](#-proje-hakkında)
2. [Mimari Genel Bakış](#-mimari-genel-bakış)
3. [İş Modeli](#-iş-modeli)
4. [Backend Mimarisi](#-backend-mimarisi)
5. [Frontend Mimarisi](#-frontend-mimarisi)
6. [Veritabanı Tasarımı](#-veritabanı-tasarımı)
7. [Modül Detayları](#-modül-detayları)
8. [API Endpoint'leri](#-api-endpointleri)
9. [Güvenlik](#-güvenlik)
10. [Deployment](#-deployment)

---

## 🎯 Proje Hakkında

### Amaç

**TrimangoCalendar**, otel, apart, bungalov, villa, ev ve oda gibi konaklama birimlerinin yönetimi için geliştirilmiş, **çok kiracılı (multi-tenant) bir SaaS platformudur.**

HotelRunner benzeri bir iş modeliyle, mülk sahiplerinin konaklama birimlerini dijital ortamda yönetmelerini, acentelere yetki vererek satış kanallarını genişletmelerini ve online rezervasyon almalarını sağlar.

### Temel Özellikler

- 🏢 **Multi-Tenant Mimarisi:** Her mülk sahibi kendi izole hesabına sahiptir
- 🏠 **Çoklu Mülk Tipi Desteği:** Otel, Apart, Bungalov, Villa, Ev, Oda, Pansiyon, Resort, Butik Otel, Dağ Evi
- 📅 **Takvim & Rezervasyon:** FullCalendar entegrasyonlu görsel takvim
- 💰 **Çoklu Para Birimi:** TRY, USD, EUR, GBP desteği, TCMB kur entegrasyonu
- 🏢 **Acente Yönetimi:** Yetkilendirme seviyeleri, kontenjan, komisyon
- 🔔 **Bildirim Sistemi:** Email, SMS, InApp bildirimler
- 📊 **Raporlama:** Doluluk, gelir, performans raporları
- 🎨 **Booking Widget:** Web sitelerine gömülebilir rezervasyon motoru

### Hedef Kitle

| Kullanıcı Rolü | Açıklama |
|----------------|----------|
| **Admin** | Sistemi yöneten süper kullanıcı |
| **Tenant (Mülk Sahibi)** | Otel, apart, villa vb. işletme sahipleri |
| **Agency (Acente)** | Seyahat acenteleri, tur operatörleri, OTA'lar |

---

## 🏗 Mimari Genel Bakış
┌─────────────────────────────────────────────────────────────────┐
│ CLIENT LAYER │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Admin │ │ Tenant │ │ Agency │ │ Booking │ │
│ │ Panel │ │ Panel │ │ Panel │ │ Widget │ │
│ └────┬─────┘ └────┬─────┘ └────┬─────┘ └──────┬───────┘ │
│ │ │ │ │ │
│ └──────────────┴──────────────┴───────────────┘ │
│ │ │
│ React 18 + TypeScript │
│ Tailwind CSS + Vite │
└──────────────────────────┬──────────────────────────────────────┘
│
HTTPS / REST API
│
┌──────────────────────────┴──────────────────────────────────────┐
│ API GATEWAY │
│ .NET 8 Web API │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Auth │ │ Tenant │ │ Rate │ │ Request │ │
│ │ JWT │ │ Middleware│ │ Limiting │ │ Logging │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
└──────────────────────────┬──────────────────────────────────────┘
│
┌──────────────────────────┴──────────────────────────────────────┐
│ BUSINESS LAYER │
│ (.NET Core) │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Tenant │ │ Property │ │Reservation│ │ Pricing │ │
│ │ Service │ │ Service │ │ Service │ │ Service │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Agency │ │ Calendar │ │ Report │ │ Notification │ │
│ │ Service │ │ Service │ │ Service │ │ Service │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
└──────────────────────────┬──────────────────────────────────────┘
│
┌──────────────────────────┴──────────────────────────────────────┐
│ DATA LAYER │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Entity │ │Repository│ │ Unit of │ │ Dapper │ │
│ │ Framework│ │ Pattern │ │ Work │ │ (Raw SQL) │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
└──────────────────────────┬──────────────────────────────────────┘
│
┌──────────────────────────┴──────────────────────────────────────┐
│ INFRASTRUCTURE │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ MSSQL │ │ Redis │ │ Hangfire │ │ Serilog │ │
│ │ Database │ │ Cache │ │ Jobs │ │ Logging │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
└─────────────────────────────────────────────────────────────────┘


---

## 💼 İş Modeli

### Abonelik Planları

| Özellik | Free | Pro (₺49/ay) | Enterprise (₺199/ay) |
|---------|------|-------------|---------------------|
| Mülk Sayısı | 5 | 25 | Sınırsız |
| Rezervasyon/Ay | 50 | 500 | Sınırsız |
| Kullanıcı | 1 | 5 | Sınırsız |
| Acente Yetkilendirme | 3 | 10 | Sınırsız |
| Para Birimi | TRY | TRY, USD, EUR | Tümü |
| Booking Widget | ❌ | ✅ | ✅ |
| Raporlama | Temel | Gelişmiş | Özel |
| API Erişimi | ❌ | ❌ | ✅ |
| Destek | Email | Öncelikli | 7/24 |

---

## 🔧 Backend Mimarisi

### Proje Katmanları
TrimangoCalendar/
├── TrimangoCalendar.API/ # Web API Katmanı
├── TrimangoCalendar.Core/ # İş Mantığı Katmanı
├── TrimangoCalendar.Data/ # Veri Erişim Katmanı
└── TrimangoCalendar.Shared/ # Paylaşılan Yardımcılar


### Katman Sorumlulukları

#### TrimangoCalendar.API (Presentation Layer)
- **Controllers:** HTTP isteklerini karşılar
- **Middleware:** Tenant, Exception, Logging
- **Filters:** Validation, Authorization
- **BackgroundJobs:** Hangfire job'ları
- **Program.cs:** DI konfigürasyonu, pipeline

#### TrimangoCalendar.Core (Business Layer)
- **Entities:** Veritabanı entity'leri (30+ entity)
- **Interfaces:** Servis kontratları
- **Services:** İş mantığı implementasyonları
- **DTOs:** Data Transfer Objects
- **Validators:** FluentValidation kuralları
- **ValueObjects:** Money, Address vb.
- **Enums:** Sabit değerler

#### TrimangoCalendar.Data (Data Access Layer)
- **Context:** AppDbContext (40+ DbSet)
- **Configurations:** Entity Framework Fluent API
- **Repositories:** Generic + Specific repository'ler
- **UnitOfWork:** Transaction yönetimi
- **Migrations:** EF Core migration'ları
- **SeedData:** Başlangıç verileri

### Servis Mimarisi
Controllers → Services → Repositories → DbContext → MSSQL
↓ ↓
DTOs UnitOfWork
↓ ↓
Validators Transactions

### Backend Servisler

| Servis | Arayüz | Sorumluluk |
|--------|--------|------------|
| **TenantService** | `ITenantService` | Tenant CRUD, subdomain yönetimi |
| **PropertyService** | `IPropertyService` | Mülk CRUD, slug yönetimi |
| **UnitService** | `IUnitService` | Birim CRUD, kapasite yönetimi |
| **ReservationService** | `IReservationService` | Rezervasyon CRUD, check-in/out |
| **PricingService** | `IPricingService` | Fiyat hesaplama, döviz dönüşümü |
| **CurrencyService** | `ICurrencyService` | Para birimi, kur yönetimi |
| **SeasonRateService** | `ISeasonRateService` | Sezon fiyatlandırması |
| **AgencyService** | `IAgencyService` | Acente yönetimi, yetkilendirme |
| **CalendarService** | `ICalendarService` | Takvim blokajı, müsaitlik |
| **NotificationService** | `INotificationService` | Email, SMS bildirimleri |
| **ReportService** | `IReportService` | Raporlama, dashboard |
| **BookingEngineService** | `IBookingEngineService` | Widget rezervasyon motoru |
| **ImageService** | `IImageService` | Fotoğraf yükleme, optimizasyon |
| **EmailService** | `IEmailService` | SMTP email gönderimi |
| **SmsService** | `ISmsService` | SMS gönderimi |
| **FileStorageService** | `IFileStorageService` | Dosya depolama |

### Repository Mimarisi
IUnitOfWork
├── ITenantRepository (TenantRepository)
├── IPropertyRepository (PropertyRepository)
├── IUnitRepository (UnitRepository)
├── IReservationRepository (ReservationRepository)
├── IGuestRepository (GuestRepository)
├── IAgencyRepository (AgencyRepository)
├── IPricingRepository (PricingRepository)
├── ICalendarRepository (CalendarRepository)
└── INotificationRepository (NotificationRepository)

text

---

## 🎨 Frontend Mimarisi

### Klasör Yapısı
frontend/src/
├── api/ # API servisleri (Axios)
│ ├── axios.ts # Axios yapılandırması
│ ├── auth.api.ts # Kimlik doğrulama
│ ├── tenant.api.ts # Tenant işlemleri
│ ├── property.api.ts # Mülk işlemleri
│ ├── reservation.api.ts # Rezervasyon işlemleri
│ ├── pricing.api.ts # Fiyatlandırma
│ ├── agency.api.ts # Acente yönetimi
│ ├── calendar.api.ts # Takvim işlemleri
│ ├── report.api.ts # Raporlama
│ └── widget.api.ts # Widget yönetimi
│
├── components/ # Bileşenler
│ ├── ui/ # Temel UI bileşenleri
│ │ ├── Button.tsx
│ │ ├── Input.tsx
│ │ ├── Select.tsx
│ │ ├── Modal.tsx
│ │ ├── Card.tsx
│ │ ├── Badge.tsx
│ │ ├── Tabs.tsx
│ │ ├── Pagination.tsx
│ │ ├── ConfirmDialog.tsx
│ │ └── Loading.tsx
│ ├── layout/ # Sayfa düzenleri
│ │ ├── AdminLayout.tsx
│ │ ├── TenantLayout.tsx
│ │ └── AgencyLayout.tsx
│ └── charts/ # Grafikler
│ ├── LineChart.tsx
│ ├── BarChart.tsx
│ └── PieChart.tsx
│
├── pages/ # Sayfalar
│ ├── auth/ # Kimlik doğrulama
│ │ ├── Login.tsx
│ │ └── Register.tsx
│ ├── admin/ # Admin paneli
│ │ └── Dashboard.tsx
│ ├── tenant/ # Mülk sahibi paneli
│ │ ├── Dashboard.tsx
│ │ ├── Properties/
│ │ ├── Reservations/
│ │ ├── Calendar/
│ │ ├── Pricing/
│ │ ├── Agencies/
│ │ ├── Reports/
│ │ └── Widgets/
│ └── agency/ # Acente paneli
│ ├── Dashboard.tsx
│ ├── MyProperties.tsx
│ ├── Calendar.tsx
│ └── Reservations/
│
├── stores/ # State yönetimi (Zustand)
│ ├── authStore.ts
│ └── appStore.ts
│
├── hooks/ # Custom hooks
│ ├── useAuth.ts
│ ├── useDebounce.ts
│ └── usePagination.ts
│
├── types/ # TypeScript tipleri
│ ├── common.ts
│ ├── property.ts
│ ├── reservation.ts
│ └── agency.ts
│
└── utils/ # Yardımcılar
├── format.ts
├── constants.ts
└── validators.ts

text

### Sayfa Yapısı

#### Admin Paneli (`/admin`)
- **Dashboard:** Sistem genel istatistikleri
- **Tenants:** Bayi listesi ve yönetimi
- **Agencies:** Acente listesi
- **Subscriptions:** Abonelik planları
- **Reports:** Global raporlar

#### Tenant Paneli (`/dashboard`)
- **Dashboard:** İşletme özeti, grafikler
- **Properties:** Mülk listesi, ekleme/düzenleme
- **Units:** Birim yönetimi
- **Reservations:** Rezervasyon listesi, takvim
- **Calendar:** Blokaj ve müsaitlik yönetimi
- **Pricing:** Sezon fiyatları, para birimleri
- **Agencies:** Acente yetkilendirme
- **Reports:** Doluluk, gelir raporları
- **Widgets:** Booking widget ayarları
- **Settings:** Profil, firma, bildirim ayarları

#### Acente Paneli (`/agency`)
- **Dashboard:** Acente özeti
- **My Properties:** Yetkili mülkler
- **Calendar:** Mülk takvimi
- **Reservations:** Rezervasyon listesi ve oluşturma
- **Reports:** Acente performans raporları
- **Settings:** Acente ayarları

### State Yönetimi
Zustand (Client State)
├── authStore: Kullanıcı, token, login/logout
└── appStore: Sidebar, seçili mülk, para birimi

React Query (Server State)
├── useQuery: Veri okuma (GET)
├── useMutation: Veri yazma (POST/PUT/DELETE)
└── queryClient.invalidateQueries: Cache temizleme

text

---

## 🗄️ Veritabanı Tasarımı

### Veritabanı Diyagramı
┌─────────────────────────────────────────────────────────────────┐
│ CORE TABLES │
├─────────────────────────────────────────────────────────────────┤
│ │
│ ┌──────────┐ ┌──────────────┐ ┌──────────────┐ │
│ │ Tenants │────→│ Properties │────→│ Units │ │
│ └──────────┘ └──────────────┘ └──────────────┘ │
│ │ │ │ │
│ │ │ │ │
│ ▼ ▼ ▼ │
│ ┌──────────┐ ┌──────────────┐ ┌──────────────┐ │
│ │Tenant │ │ Property │ │ SeasonRates │ │
│ │Settings │ │ Images │ │ SpecialDay │ │
│ └──────────┘ └──────────────┘ │ Rates │ │
│ └──────────────┘ │
│ │
├─────────────────────────────────────────────────────────────────┤
│ RESERVATION FLOW │
├─────────────────────────────────────────────────────────────────┤
│ │
│ ┌──────────┐ ┌──────────────┐ ┌──────────────┐ │
│ │ Guests │────→│ Reservations │←────│ Units │ │
│ └──────────┘ └──────────────┘ └──────────────┘ │
│ │ │ │
│ │ │ │
│ ▼ ▼ │
│ ┌──────────┐ ┌──────────────┐ │
│ │ Guest │ │ Reservation │ │
│ │Documents │ │ History │ │
│ └──────────┘ └──────────────┘ │
│ │
├─────────────────────────────────────────────────────────────────┤
│ AGENCY SYSTEM │
├─────────────────────────────────────────────────────────────────┤
│ │
│ ┌──────────┐ ┌──────────────┐ ┌──────────────┐ │
│ │ Agencies │────→│ Agency │←────│ Properties │ │
│ │ │ │Authorizations│ │ │ │
│ └──────────┘ └──────────────┘ └──────────────┘ │
│ │ │ │
│ ▼ ▼ │
│ ┌──────────┐ ┌──────────────┐ │
│ │ Agency │ │ Agency │ │
│ │ Users │ │ Commissions │ │
│ └──────────┘ └──────────────┘ │
│ │
├─────────────────────────────────────────────────────────────────┤
│ SUPPORTING TABLES │
├─────────────────────────────────────────────────────────────────┤
│ │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │Currencies│ │Exchange │ │Promotions│ │ Calendar │ │
│ │ │ │ Rates │ │ │ │ Blocks │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
│ │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │Notifica- │ │Notif. │ │ Booking │ │ Reports │ │
│ │ tions │ │Templates │ │ Widgets │ │ │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
│ │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐ │
│ │ Audit │ │ System │ │ Email │ │ SMS │ │
│ │ Logs │ │ Settings │ │Templates │ │ Templates │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────────┘ │
│ │
└─────────────────────────────────────────────────────────────────┘

text

### Tüm Tablolar (40+)

#### Çekirdek Tablolar (Core)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Tenants** | Kiracı/abone bilgileri | Id, Name, Subdomain, Email, Plan, MaxProperties |
| **TenantSettings** | Kiracı ayarları | Timezone, DateFormat, DefaultCurrency, DefaultLanguage |
| **SubscriptionPlans** | Abonelik planları | Name, Code, Price, MaxProperties, MaxUsers |
| **TenantSubscriptions** | Kiracı abonelikleri | TenantId, PlanId, Status, PaymentMethod |

#### Mülk Tabloları (Property)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Properties** | Mülkler | Id, TenantId, Type, Name, Slug, City, CheckInTime |
| **PropertyImages** | Mülk fotoğrafları | PropertyId, FilePath, ThumbnailPath, IsMain, SortOrder |
| **PropertyReviews** | Değerlendirmeler | PropertyId, GuestId, Rating, Comment, IsApproved |
| **Units** | Birimler/odalar | PropertyId, Name, UnitNumber, MaxAdults, BasePrice |
| **UnitImages** | Birim fotoğrafları | UnitId, FilePath, ThumbnailPath |
| **UnitAmenities** | Birim özellikleri | UnitId, AmenityName |

#### Rezervasyon Tabloları (Reservation)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Reservations** | Rezervasyonlar | UnitId, GuestId, CheckIn, CheckOut, Status, TotalAmount |
| **ReservationHistories** | Durum geçmişi | ReservationId, OldStatus, NewStatus, Note |
| **ReservationPayments** | Ödemeler | ReservationId, Amount, PaymentMethod, TransactionId |
| **Guests** | Misafirler | FirstName, LastName, Email, Phone, Nationality |
| **GuestDocuments** | Misafir belgeleri | GuestId, DocumentType, DocumentNumber |
| **GuestNotes** | Misafir notları | GuestId, Note, CreatedBy |

#### Acente Tabloları (Agency)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Agencies** | Acenteler | CompanyName, Email, DefaultCommissionRate |
| **AgencyUsers** | Acente kullanıcıları | AgencyId, FirstName, Email, Role |
| **AgencyAuthorizations** | Yetkilendirmeler | AgencyId, PropertyId, Level, Allotment |
| **AgencyCommissions** | Komisyonlar | AgencyId, ReservationId, Amount |

#### Fiyatlandırma Tabloları (Pricing)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Currencies** | Para birimleri | Code, Symbol, Name, IsBaseCurrency |
| **ExchangeRates** | Döviz kurları | BaseCurrency, TargetCurrency, Rate, Date |
| **CurrencyFormats** | Para formatları | CurrencyCode, Format |
| **SeasonRates** | Sezon fiyatları | UnitId, StartDate, EndDate, WeekdayPrice, WeekendPrice |
| **SpecialDayRates** | Özel gün fiyatları | UnitId, Date, Price |
| **Promotions** | İndirimler | Code, Name, DiscountValue, Type |
| **PromotionUsages** | İndirim kullanımları | PromotionId, ReservationId |

#### Takvim Tabloları (Calendar)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **CalendarBlocks** | Takvim blokajları | UnitId, StartDate, EndDate, Type, Reason |
| **CalendarEvents** | Takvim olayları | UnitId, Date, EventType |
| **CalendarNotes** | Takvim notları | UnitId, Date, Note |

#### Bildirim Tabloları (Notification)

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **Notifications** | Bildirimler | TenantId, Type, Channel, Status |
| **NotificationTemplates** | Bildirim şablonları | Code, Subject, BodyTemplate |
| **NotificationPreferences** | Bildirim tercihleri | TenantId, Type, EmailEnabled, SMSEnabled |
| **NotificationLogs** | Bildirim logları | NotificationId, Status, ErrorMessage |

#### Widget & Rapor Tabloları

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **BookingWidgets** | Booking widget'ları | PropertyId, WidgetKey, Theme, PrimaryColor |
| **WidgetIntegrations** | Widget domainleri | WidgetId, Domain |
| **WidgetThemes** | Widget temaları | Name, CSS |
| **Reports** | Raporlar | TenantId, Type, StartDate, EndDate, FilePath |
| **ReportSchedules** | Rapor zamanlamaları | ReportId, CronExpression |
| **DashboardWidgets** | Dashboard bileşenleri | TenantId, WidgetType |

#### Sistem Tabloları

| Tablo | Açıklama | Önemli Sütunlar |
|-------|----------|-----------------|
| **AuditLogs** | Denetim kayıtları | EntityName, EntityId, Action, UserId |
| **SystemSettings** | Sistem ayarları | Key, Value |
| **EmailTemplates** | Email şablonları | Code, Subject, Body |
| **SmsTemplates** | SMS şablonları | Code, Message |

---

## 📦 Modül Detayları

### MODÜL 1: Tenant (Abone) Yönetimi

**Amaç:** Çoklu kiracı yapısını yönetmek

**Backend:**
- `TenantService` ile CRUD işlemleri
- `TenantMiddleware` ile subdomain bazlı yönlendirme
- `TenantSettings` ile özelleştirme

**Frontend:**
- Admin panelinde tenant listesi
- Tenant detay sayfası
- Abonelik planı yönetimi

**Veritabanı:**
- `Tenants` tablosu
- `TenantSettings` tablosu
- `SubscriptionPlans` tablosu
- `TenantSubscriptions` tablosu

---

### MODÜL 2: Mülk (Property) Yönetimi

**Amaç:** Farklı tipteki konaklama birimlerini yönetmek

**Backend:**
- `PropertyService` ile mülk CRUD
- `UnitService` ile birim/oda yönetimi
- `ImageService` ile fotoğraf yükleme
- Slug otomatik oluşturma

**Frontend:**
- Grid/List görünümlü mülk listesi
- Mülk ekleme formu (tip, adres, özellikler)
- Birim yönetimi sayfası
- Fotoğraf yükleme ve sıralama

**Veritabanı:**
- `Properties`, `Units`, `PropertyImages`, `UnitImages`
- `PropertyReviews` (değerlendirmeler)

---

### MODÜL 3: Fiyatlandırma & Para Birimi

**Amaç:** Çoklu para birimi ve dinamik fiyatlandırma

**Backend:**
- `PricingService` ile fiyat hesaplama
- `CurrencyService` ile döviz kuru yönetimi
- `SeasonRateService` ile sezon fiyatları
- `Money` value object ile tip güvenli para yönetimi
- TCMB API entegrasyonu (Hangfire ile günlük)

**Frontend:**
- Sezon fiyatı ekleme/düzenleme
- Takvim üzerinde fiyat görüntüleme
- Para birimi seçimi
- Promosyon kodu yönetimi

**Veritabanı:**
- `Currencies`, `ExchangeRates`
- `SeasonRates`, `SpecialDayRates`
- `Promotions`, `PromotionUsages`

---

### MODÜL 4: Rezervasyon Sistemi

**Amaç:** Tam kapsamlı rezervasyon yönetimi

**Backend:**
- `ReservationService` ile rezervasyon CRUD
- Pessimistic lock ile race-condition önleme
- Otomatik rezervasyon numarası
- Check-in/Check-out durum yönetimi
- İptal ve iade politikası

**Frontend:**
- FullCalendar entegrasyonlu takvim
- Rezervasyon listesi (filtreleme, arama)
- Check-in/out butonları
- Rezervasyon detay modal'ı

**Veritabanı:**
- `Reservations`, `ReservationHistories`
- `ReservationPayments`, `Guests`
- `GuestDocuments`, `GuestNotes`

---

### MODÜL 5: Acente Yetkilendirme

**Amaç:** Mülk sahiplerinin acentelere yetki vermesi

**Backend:**
- `AgencyService` ile acente CRUD
- Yetkilendirme seviyeleri (ViewOnly, PriceAndAvailability, CanReserve, FullAccess)
- Kontenjan (allotment) yönetimi
- Komisyon hesaplama

**Frontend:**
- Acente listesi
- Yetkilendirme formu (seviye, izinler, kontenjan)
- Yetkilendirme kartları

**Veritabanı:**
- `Agencies`, `AgencyUsers`
- `AgencyAuthorizations`, `AgencyCommissions`

---

### MODÜL 6: Takvim & Blokaj

**Amaç:** Müsaitlik takvimi ve blokaj yönetimi

**Backend:**
- `CalendarService` ile blokaj CRUD
- Müsaitlik kontrolü
- Bakım, kapalı sezon, özel kullanım blokaj tipleri

**Frontend:**
- FullCalendar görünümü
- Blokaj ekleme modal'ı
- Mülk sahibi ve acente takvim görünümleri

**Veritabanı:**
- `CalendarBlocks`, `CalendarEvents`, `CalendarNotes`

---

### MODÜL 7: Bildirim Sistemi

**Amaç:** Otomatik bildirim gönderimi

**Backend:**
- `NotificationService` ile çok kanallı bildirim
- Email (SMTP), SMS, InApp, Push
- Şablon tabanlı bildirimler
- Bildirim tercihleri

**Frontend:**
- Bildirim listesi
- Okundu/okunmadı durumu
- Bildirim tercihleri sayfası

**Veritabanı:**
- `Notifications`, `NotificationTemplates`
- `NotificationPreferences`, `NotificationLogs`

---

### MODÜL 8: Raporlama

**Amaç:** İşletme performans raporları

**Backend:**
- `ReportService` ile rapor oluşturma
- Dashboard verileri
- Doluluk, gelir, performans raporları
- Excel/PDF export

**Frontend:**
- Dashboard (istatistik kartları, grafikler)
- Rapor sayfaları (doluluk, gelir, acente)
- Filtreleme ve export butonları

**Veritabanı:**
- `Reports`, `ReportSchedules`
- `DashboardWidgets`

---

### MODÜL 9: Booking Widget

**Amaç:** Web sitelerine gömülebilir rezervasyon motoru

**Backend:**
- `BookingEngineService` ile widget API
- Widget konfigürasyonu
- Embed kod üretimi
- Domain kısıtlaması

**Frontend:**
- Widget ayarları sayfası
- Canlı önizleme
- Tema ve renk özelleştirme
- Embed kod kopyalama

**Veritabanı:**
- `BookingWidgets`, `WidgetIntegrations`, `WidgetThemes`

---

## 📡 API Endpoint'leri

### Auth Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| POST | `/api/auth/login` | Kullanıcı girişi | Public |
| POST | `/api/auth/register` | Kullanıcı kaydı | Public |
| POST | `/api/auth/refresh` | Token yenileme | Public |
| POST | `/api/auth/logout` | Çıkış | Auth |

### Tenant Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/tenants` | Tenant listesi | Admin |
| GET | `/api/tenants/{id}` | Tenant detay | Admin |
| POST | `/api/tenants` | Tenant oluştur | Admin |
| PUT | `/api/tenants/{id}` | Tenant güncelle | Admin |

### Property Endpoints| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/properties` | Mülk listesi | Tenant |
| GET | `/api/properties/{id}` | Mülk detay | Tenant |
| POST | `/api/properties` | Mülk ekle | Tenant |
| PUT | `/api/properties/{id}` | Mülk güncelle | Tenant |
| DELETE | `/api/properties/{id}` | Mülk sil | Tenant |
| POST | `/api/properties/{id}/images` | Fotoğraf yükle | Tenant |
| GET | `/api/properties/{id}/units` | Birim listesi | Tenant |

### Reservation Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/reservations` | Rezervasyon listesi | Tenant, Agency |
| POST | `/api/reservations` | Rezervasyon oluştur | Tenant, Agency |
| GET | `/api/reservations/{id}` | Rezervasyon detay | Tenant, Agency |
| POST | `/api/reservations/{id}/check-in` | Check-in | Tenant |
| POST | `/api/reservations/{id}/check-out` | Check-out | Tenant |
| POST | `/api/reservations/{id}/cancel` | İptal | Tenant, Agency |
| GET | `/api/reservations/availability/{unitId}` | Müsaitlik kontrolü | Public |

### Pricing Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| POST | `/api/pricing/calculate` | Fiyat hesapla | Public |
| GET | `/api/pricing/seasons/{unitId}` | Sezon listesi | Tenant |
| POST | `/api/pricing/seasons` | Sezon ekle | Tenant |
| GET | `/api/pricing/currencies` | Para birimleri | Public |
| GET | `/api/pricing/exchange-rate` | Döviz kuru | Public |

### Agency Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/agencies` | Acente listesi | Admin |
| POST | `/api/agencies/grant` | Yetkilendir | Tenant |
| GET | `/api/agencies/authorizations/{propertyId}` | Yetki listesi | Tenant |
| DELETE | `/api/agencies/authorizations/{id}` | Yetki sil | Tenant |
| GET | `/api/agencies/my-properties` | Yetkili mülklerim | Agency |

### Report Endpoints
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/api/dashboard` | Dashboard verisi | Tenant, Agency |
| POST | `/api/reports/occupancy` | Doluluk raporu | Tenant |
| POST | `/api/reports/revenue` | Gelir raporu | Tenant |
| POST | `/api/reports/export` | Rapor export | Tenant |

### Widget Endpoints (Public)
| Method | Endpoint | Açıklama | Yetki |
|--------|----------|----------|-------|
| GET | `/widget/api/config/{key}` | Widget config | Public |
| POST | `/widget/api/search/{key}` | Müsaitlik ara | Public |
| POST | `/widget/api/book/{key}` | Rezervasyon yap | Public |

---

## 🔒 Güvenlik

### Kimlik Doğrulama
- **JWT (JSON Web Token)** tabanlı authentication
- Token süresi: 24 saat (yapılandırılabilir)
- Refresh token desteği
- Role-based authorization (Admin, TenantOwner, AgencyUser)

### Veri Güvenliği
- **Multi-Tenant Veri İzolasyonu:** Her sorguda TenantId filtresi
- **Row-Level Security:** Veritabanı seviyesinde izolasyon
- **HTTPS:** Tüm iletişim şifreli
- **CORS:** Sadece izinli domain'lerden erişim
- **Rate Limiting:** Dakikada 100 istek limiti

### Input Validasyonu
- FluentValidation ile server-side validasyon
- Zod + React Hook Form ile client-side validasyon
- SQL Injection koruması (parametreli sorgular)
- XSS koruması

---

## 🚀 Deployment

### Gereksinimler
- .NET 8 SDK
- Node.js 18+
- SQL Server 2019+
- Redis (opsiyonel)
- IIS / Nginx / Azure App Service

### Production Build
```bash
# Backend
cd backend/TrimangoCalendar.API
dotnet publish -c Release -o ./publish

# Frontend
cd frontend
npm run build
# Çıktı: frontend/dist/
Docker
bash
docker-compose up -d
📊 Performans Optimizasyonları
Redis Cache: Sık sorgulanan veriler için

Response Compression: Brotli + Gzip

Lazy Loading: EF Core lazy loading proxies

Pagination: Tüm liste endpoint'lerinde

Image Optimization: ImageSharp ile otomatik optimizasyon

Database Indexing: Sorgu desenlerine göre indeksler

Background Jobs: Ağır işlemler Hangfire ile arka planda

🧪 Test
bash
# Backend Unit Tests
cd backend
dotnet test

# Frontend Tests
cd frontend
npm run test
📝 Lisans
MIT License - Detaylar için LICENSE dosyasına bakın.

Made with ❤️ by TrimangoCalendar Team

text

---

Bu kapsamlı dökümantasyon dosyasını projenizin ana dizinine `PROJECT_DOCUMENTATION.md` olarak kaydedebilirsiniz. İçeriği kendi bilgilerinize göre özelleştirebilirsiniz.

Herhangi bir bölümü değiştirmek veya ekleme yapmak ister misiniz?
