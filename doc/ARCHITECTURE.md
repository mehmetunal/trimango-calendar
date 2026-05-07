# TrimangoCalendar - Mimari ve Akış Diyagramları

## İçindekiler

1. [Sistem Mimarisi Genel Bakış](#1-sistem-mimarisi-genel-bakış)
2. [Kullanıcı Rolleri ve Yetkiler](#2-kullanıcı-rolleri-ve-yetkiler)
3. [Temel İş Akışları](#3-temel-iş-akışları)
4. [Backend Katman Mimarisi](#4-backend-katman-mimarisi)
5. [Frontend Bileşen Mimarisi](#5-frontend-bileşen-mimarisi)
6. [Veritabanı İlişkileri](#6-veritabanı-ilişkileri)
7. [API İletişim Akışı](#7-api-iletişim-akışı)
8. [Deployment Mimarisi](#8-deployment-mimarisi)

---

## 1. Sistem Mimarisi Genel Bakış

```mermaid
flowchart TB
  subgraph ClientLayer[Client Layer]
    Admin[Admin Panel\nReact + TS]
    Tenant[Tenant Panel\nReact + TS]
    Agency[Agency Panel\nReact + TS]
    Widget[Booking Widget\nEmbed JS + CSS]
  end

  subgraph ApiLayer[API Layer - .NET 8 Web API]
    CORS[CORS Policy]
    JWT[JWT Auth]
    TenantMw[Tenant Middleware]
    Rate[Rate Limiting]
    Log[Request Logging]
  end

  subgraph BizLayer[Business Logic Layer]
    S1[Tenant Service]
    S2[Property Service]
    S3[Reservation Service]
    S4[Pricing Service]
    S5[Agency Service]
    S6[Calendar Service]
    S7[Report Service]
    S8[Notification Service]
  end

  subgraph DataLayer[Data Access Layer]
    UoW[Unit of Work]
    Repos[Repositories]
    EF[EF Core + Dapper]
  end

  subgraph Infra[Infrastructure]
    SQL[(SQL Server)]
    Redis[(Redis)]
    Hangfire[Hangfire Jobs]
    Serilog[Serilog]
    TCMB[TCMB API]
  end

  Admin --> ApiLayer
  Tenant --> ApiLayer
  Agency --> ApiLayer
  Widget --> ApiLayer

  ApiLayer --> BizLayer --> DataLayer --> Infra
```

---

## 2. Kullanıcı Rolleri ve Yetkiler

```mermaid
flowchart TD
  SA[Super Admin]
  A[Admin]
  TO[Tenant Owner]
  AU[Agency User]

  SA --> A
  SA --> TO
  SA --> AU
```

### Yetki Matrisi

| İşlem | Admin | Tenant | Agency |
|---|---|---|---|
| Tenant Yönetimi | ✅ | ❌ | ❌ |
| Mülk Ekleme | ✅ | ✅ | ❌ |
| Mülk Görüntüleme | ✅ | ✅ | ✅* |
| Birim Yönetimi | ✅ | ✅ | ❌ |
| Fiyat Belirleme | ✅ | ✅ | ✅* |
| Rezervasyon Yapma | ✅ | ✅ | ✅* |
| Check-in/out | ✅ | ✅ | ❌ |
| Acente Yetkilendirme | ✅ | ✅ | ❌ |
| Rapor Görüntüleme | ✅ | ✅ | ✅* |
| Sistem Ayarları | ✅ | ❌ | ❌ |
| Abonelik Yönetimi | ✅ | ❌ | ❌ |
| Widget Yönetimi | ✅ | ✅ | ❌ |
| Profil Düzenleme | ✅ | ✅ | ✅ |

* `✅*` işaretli yetkiler, tenant tarafından verilen acente yetkilendirme seviyesine bağlıdır.

---

## 3. Temel İş Akışları

### 3.1 Tenant Kayıt Akışı

```mermaid
flowchart LR
  A[Kayıt Formu] --> B[Email Doğrulama]
  B --> C[Subdomain Oluşturma]
  C --> D[Tenant Oluşturma\nFree Plan]
  D --> E[Demo Veri Yükleme]
  E --> F[Dashboard'a Yönlendirme]
```

### 3.2 Rezervasyon Oluşturma Akışı

```mermaid
flowchart LR
  A[Tarih Seçimi] --> B[Müsaitlik Kontrolü]
  B --> C[Fiyat Hesaplama]
  C --> D[Misafir Bilgileri]
  D --> E[Rezervasyon Oluşturma\nStatus: Pending]
```

### 3.3 Check-in / Check-out Akışı

```mermaid
flowchart TB
  subgraph CheckIn
    A1[Rezervasyon Bul]
    A2[Tarih Kontrolü]
    A3[Durum Güncelle\nCheckedIn]
    A4[Hoşgeldin Bildirimi]
    A1 --> A2 --> A3 --> A4
  end

  subgraph CheckOut
    B1[Rezervasyon Bul]
    B2[Ödeme Kontrolü]
    B3[Durum Güncelle\nCheckedOut]
    B4[Değerlendirme İsteği]
    B1 --> B2 --> B3 --> B4
  end
```

### 3.4 Acente Yetkilendirme Akışı

```mermaid
flowchart LR
  A[Acente Seçimi] --> B[Mülk Seçimi]
  B --> C[Yetki Seviyesi]
  C --> D[Opsiyonel Kontenjan]
  D --> E[Acente Takvimi Görür]
  E --> F[Yetkiye Göre Rezervasyon]
```

### 3.5 Fiyat Hesaplama Akışı

```mermaid
flowchart TD
  A[Girdi\nUnitId, CheckIn, CheckOut, Kişi, Para Birimi] --> B[Baz Fiyat]
  B --> C[Ek Ücretler\nHafta Sonu, Özel Gün, Ekstra Yatak]
  C --> D[Döviz Dönüşümü]
  D --> E[Vergi ve Servis Ücreti]
  E --> F[Promosyon]
  F --> G[Genel Toplam]
```

### 3.6 Booking Widget Akışı

```mermaid
flowchart LR
  A[Embed Script] --> B[Widget Config Çek]
  B --> C[Tarih ve Kişi Seçimi]
  C --> D[Müsait Birimler]
  D --> E[Misafir Bilgileri]
  E --> F[Rezervasyon Tamamla]
```

---

## 4. Backend Katman Mimarisi

```mermaid
flowchart TB
  Req[HTTP Request] --> Ctl[Controllers]
  Ctl --> Med[MediatR / Handlers]
  Med --> Val[FluentValidation]
  Med --> Svc[Application Services]
  Svc --> Repo[Repositories / UoW]
  Repo --> Db[(SQL Server)]
```

---

## 5. Frontend Bileşen Mimarisi

```mermaid
flowchart TB
  App[React App] --> Router[Route Layer]
  Router --> Pages[Pages]
  Pages --> Components[Shared Components]
  Components --> ApiClient[API Client]
  ApiClient --> Backend[Backend API]
```

---

## 6. Veritabanı İlişkileri

```mermaid
erDiagram
  TENANT ||--o{ PROPERTY : owns
  PROPERTY ||--o{ UNIT : contains
  UNIT ||--o{ RESERVATION : has
  RESERVATION }o--|| GUEST : belongs_to
  TENANT ||--o{ AGENCY_AUTHORIZATION : grants
  AGENCY_AUTHORIZATION }o--|| AGENCY : for
  PROPERTY ||--o{ BOOKING_WIDGET : has
  BOOKING_WIDGET ||--o{ WIDGET_INTEGRATION : includes
  TENANT ||--o{ NOTIFICATION : creates
```

---

## 7. API İletişim Akışı

```mermaid
sequenceDiagram
  participant UI as Frontend
  participant API as Backend API
  participant Auth as JWT/Auth
  participant DB as Database

  UI->>API: HTTP Request
  API->>Auth: Token Validate
  Auth-->>API: Claims
  API->>DB: Query/Command
  DB-->>API: Result
  API-->>UI: JSON Response
```

---

## 8. Deployment Mimarisi

```mermaid
flowchart LR
  User[Kullanıcı Tarayıcısı] --> CDN[Static Hosting / CDN]
  CDN --> FE[Frontend]
  FE --> API[Trimango API]
  API --> DB[(SQL Server)]
  API --> REDIS[(Redis)]
  API --> JOBS[Hangfire]
```
