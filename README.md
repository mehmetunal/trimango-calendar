# 🏨 TrimangoCalendar

**HotelRunner Benzeri SaaS Kiralama Yönetim Platformu**

TrimangoCalendar, otel, apart, bungalov, villa, ev ve oda gibi konaklama birimlerinin yönetimi için geliştirilmiş, çok kiracılı (multi-tenant) bir SaaS platformudur. Mülk sahipleri, acenteler ve yöneticiler için kapsamlı bir rezervasyon ve takvim yönetim sistemi sunar.

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18-61DAFB)
![TypeScript](https://img.shields.io/badge/TypeScript-5.3-3178C6)

---

## 📋 İçindekiler

- [Özellikler](#-özellikler)
- [Teknoloji Yığını](#-teknoloji-yığını)
- [Proje Yapısı](#-proje-yapısı)
- [Kurulum](#-kurulum)
- [Çalıştırma](#-çalıştırma)
- [API Dokümantasyonu](#-api-dokümantasyonu)
- [Ortam Değişkenleri](#-ortam-değişkenleri)
- [Veritabanı](#-veritabanı)
- [Test](#-test)
- [Deployment](#-deployment)
- [Katkıda Bulunma](#-katkıda-bulunma)
- [Lisans](#-lisans)

---

## ✨ Özellikler

### 🏢 Çoklu Kiracı (Multi-Tenant) Mimarisi
- Her mülk sahibi kendi izole hesabına sahiptir
- Subdomain bazlı yönlendirme
- Özelleştirilebilir abonelik planları (Free, Pro, Enterprise)

### 🏠 Mülk Yönetimi
- **Desteklenen Mülk Tipleri:** Otel, Apart Otel, Bungalov, Villa, Ev, Oda, Pansiyon, Resort, Butik Otel, Dağ Evi
- Birim/oda yönetimi (kapasite, özellikler, fiyatlandırma)
- Fotoğraf ve medya yükleme
- Özellik ve amenity yönetimi (WiFi, Havuz, SPA, Otopark vb.)
- Check-in/Check-out saatleri yapılandırması

### 📅 Takvim & Rezervasyon
- **FullCalendar** entegrasyonu ile görsel takvim
- Günlük, haftalık, aylık görünüm
- Check-in / Check-out yönetimi
- İptal ve iade politikaları
- Müsaitlik kontrolü (race-condition korumalı)
- Otomatik rezervasyon numarası üretimi
- Misafir yönetimi ve geçmiş takibi

### 💰 Çoklu Para Birimi & Fiyatlandırma
- **Desteklenen Para Birimleri:** TRY (₺), USD ($), EUR (€), GBP (£)
- **TCMB** döviz kuru entegrasyonu (günlük otomatik güncelleme)
- Manuel kur tanımlama
- Sezon bazlı fiyatlandırma (yaz, kış, bayram, özel günler)
- Hafta içi / hafta sonu farklı fiyatlandırma
- Erken rezervasyon indirimleri
- Promosyon kodu sistemi
- Minimum / maksimum konaklama süresi kuralları

### 🏢 Acente & Kanal Yönetimi
- Acente kaydı ve yönetimi
- Mülk bazlı yetkilendirme seviyeleri:
  - Sadece Görüntüleme
  - Fiyat ve Müsaitlik
  - Rezervasyon Yapabilir
  - Tam Yetki
- Kontenjan (allotment) yönetimi
- Komisyon oranı belirleme (%)
- Markup fiyatlandırma (acentenin üstüne fiyat koyabilmesi)
- Fiyat görüntüleme tipleri: Net, Komisyon Dahil, Markup
- Tarih aralığı kısıtlaması
- Domain bazlı widget entegrasyonu

### 📊 Raporlama & Dashboard
- **Dashboard:**
  - Günlük check-in/out sayıları
  - Doluluk oranı (anlık)
  - Aylık gelir takibi
  - Aktif rezervasyon sayısı
  - Grafikler (doluluk, gelir trendi)
- **Rapor Tipleri:**
  - Doluluk raporu (günlük, haftalık, aylık)
  - Gelir raporu (mülk, acente, para birimi bazlı)
  - Mülk performans karşılaştırması
  - Acente performans raporu
  - Misafir analizi (milliyet, tekrar gelme oranı)
- **Export:** Excel (.xlsx) ve PDF

### 🔔 Bildirim Sistemi
- **Kanallar:** Email, SMS, Uygulama İçi (InApp), Push
- **Şablon Tabanlı:** Özelleştirilebilir email/SMS şablonları
- **Olay Bazlı:**
  - Yeni rezervasyon
  - Check-in hatırlatma (1 gün önce)
  - Rezervasyon iptali
  - Yetkilendirme değişiklikleri
  - Kontenjan uyarısı
  - Değerlendirme isteği (check-out sonrası)

### 🎨 Booking Widget (Online Rezervasyon Motoru)
- Web sitelerine gömülebilir (embed) rezervasyon formu
- Özelleştirilebilir tema ve renkler
- Canlı önizleme
- Mobil uyumlu (responsive) tasarım
- Çoklu dil desteği (TR, EN, DE, RU, AR)
- Domain kısıtlaması (güvenlik)
- SEO optimizasyonu (meta title, description)
- Pozisyon seçenekleri (sağ alt, sol alt, modal, tam sayfa)

### 👥 Kullanıcı Rolleri
1. **Admin (Sistem Yöneticisi)**
   - Tüm tenant'ları yönetme
   - Abonelik planlarını yönetme
   - Sistem ayarları
   - Global raporlar

2. **Tenant Owner (Mülk Sahibi)**
   - Mülk ve birim yönetimi
   - Rezervasyon yönetimi
   - Fiyatlandırma ve sezon ayarları
   - Acente yetkilendirme
   - Rapor görüntüleme
   - Widget yapılandırması

3. **Agency User (Acente Kullanıcısı)**
   - Yetkili olduğu mülkleri görüntüleme
   - Takvim ve müsaitlik kontrolü
   - Rezervasyon oluşturma (yetki dahilinde)
   - Fiyat görüntüleme (yetki dahilinde)
   - Kendi raporlarını görüntüleme

---

## 🛠 Teknoloji Yığını

### Backend
| Teknoloji | Açıklama |
|-----------|-----------|
| **.NET 8** | Web API framework |
| **Entity Framework Core 8** | ORM |
| **MSSQL** | Veritabanı |
| **Redis** | Cache ve oturum yönetimi |
| **JWT** | Kimlik doğrulama |
| **Serilog** | Loglama |
| **Swagger** | API dokümantasyonu |
| **FluentValidation** | Validasyon |
| **AutoMapper** | Nesne mapping |
| **MediatR** | CQRS pattern |
| **Hangfire** | Background jobs |

### Frontend
| Teknoloji | Açıklama |
|-----------|-----------|
| **React 18** | UI framework |
| **TypeScript** | Tip güvenliği |
| **Tailwind CSS** | Stil framework |
| **React Query (TanStack)** | Server state yönetimi |
| **Zustand** | Client state yönetimi |
| **React Hook Form + Zod** | Form yönetimi ve validasyon |
| **React Router v6** | Routing |
| **Recharts** | Grafik kütüphanesi |
| **FullCalendar** | Takvim bileşeni |
| **Lucide React** | İkon kütüphanesi |
| **Axios** | HTTP client |
| **date-fns** | Tarih işlemleri |
| **Vite** | Build tool |

---

## 📁 Proje Yapısı












# Frontend Mock Login Bilgileri

`VITE_USE_MOCK=true` aktifken kullanılacak hesaplar:

1. Admin
- Email: `admin@trimango.local`
- Şifre: `Admin123!`

2. TenantOwner
- Email: `owner@trimango.local`
- Şifre: `Owner123!`

3. AgencyUser
- Email: `agency@trimango.local`
- Şifre: `Agency123!`

Not: Değişikliklerin etkili olması için dev server yeniden başlatılmalıdır.
