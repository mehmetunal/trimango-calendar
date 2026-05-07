import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './styles/globals.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
Global CSS
/* src/styles/globals.css */
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --hp-primary: #2563EB;
    --hp-secondary: #1D4ED8;
    --hp-success: #10B981;
    --hp-warning: #F59E0B;
    --hp-danger: #EF4444;
    --hp-info: #3B82F6;
  }

  * {
    @apply border-gray-200;
  }

  body {
    @apply bg-gray-50 text-gray-900 antialiased;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  }

  /* Scrollbar Styles */
  ::-webkit-scrollbar {
    width: 6px;
    height: 6px;
  }

  ::-webkit-scrollbar-track {
    @apply bg-transparent;
  }

  ::-webkit-scrollbar-thumb {
    @apply bg-gray-300 rounded-full;
  }

  ::-webkit-scrollbar-thumb:hover {
    @apply bg-gray-400;
  }

  /* Focus styles */
  *:focus-visible {
    @apply outline-none ring-2 ring-blue-500 ring-offset-2;
  }

  /* Input autofill */
  input:-webkit-autofill,
  input:-webkit-autofill:hover,
  input:-webkit-autofill:focus {
    -webkit-box-shadow: 0 0 0 1000px white inset;
    transition: background-color 5000s ease-in-out 0s;
  }
}

@layer components {
  /* Card component */
  .card {
    @apply bg-white rounded-xl border border-gray-200 shadow-sm;
  }

  .card-hover {
    @apply card hover:shadow-md transition-shadow duration-200;
  }

  /* Form elements */
  .form-input {
    @apply w-full px-3 py-2 border border-gray-300 rounded-lg text-sm
           focus:ring-2 focus:ring-blue-500 focus:border-blue-500
           placeholder:text-gray-400
           disabled:bg-gray-100 disabled:cursor-not-allowed;
  }

  .form-label {
    @apply block text-sm font-medium text-gray-700 mb-1;
  }

  .form-error {
    @apply text-xs text-red-500 mt-1;
  }

  /* Button variants */
  .btn {
    @apply inline-flex items-center justify-center font-medium rounded-lg
           transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2
           disabled:opacity-50 disabled:cursor-not-allowed;
  }

  .btn-primary {
    @apply btn bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500;
  }

  .btn-secondary {
    @apply btn bg-gray-100 text-gray-700 hover:bg-gray-200 focus:ring-gray-500;
  }

  .btn-danger {
    @apply btn bg-red-600 text-white hover:bg-red-700 focus:ring-red-500;
  }

  .btn-ghost {
    @apply btn bg-transparent text-gray-600 hover:bg-gray-100 focus:ring-gray-500;
  }

  .btn-sm {
    @apply px-3 py-1.5 text-sm;
  }

  .btn-md {
    @apply px-4 py-2 text-sm;
  }

  .btn-lg {
    @apply px-6 py-3 text-base;
  }

  /* Badge component */
  .badge {
    @apply inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full;
  }

  .badge-success {
    @apply badge bg-green-100 text-green-800;
  }

  .badge-danger {
    @apply badge bg-red-100 text-red-800;
  }

  .badge-warning {
    @apply badge bg-yellow-100 text-yellow-800;
  }

  .badge-info {
    @apply badge bg-blue-100 text-blue-800;
  }

  .badge-gray {
    @apply badge bg-gray-100 text-gray-800;
  }

  /* Table styles */
  .table-container {
    @apply overflow-x-auto;
  }

  .table {
    @apply w-full;
  }

  .table thead {
    @apply bg-gray-50;
  }

  .table th {
    @apply px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider;
  }

  .table td {
    @apply px-4 py-3 text-sm text-gray-700;
  }

  .table tbody tr {
    @apply border-t border-gray-100 hover:bg-gray-50 transition-colors;
  }

  /* Animation utilities */
  .animate-fade-in {
    animation: fadeIn 0.3s ease-in-out;
  }

  .animate-slide-up {
    animation: slideUp 0.3s ease-in-out;
  }

  .animate-slide-down {
    animation: slideDown 0.3s ease-in-out;
  }

  .animate-scale {
    animation: scale 0.2s ease-in-out;
  }
}

@layer utilities {
  .text-balance {
    text-wrap: balance;
  }

  .scrollbar-hide {
    -ms-overflow-style: none;
    scrollbar-width: none;
  }

  .scrollbar-hide::-webkit-scrollbar {
    display: none;
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes scale {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

/* FullCalendar Overrides */
.fc {
  @apply font-sans;
}

.fc .fc-toolbar-title {
  @apply text-lg font-semibold;
}

.fc .fc-button {
  @apply btn btn-sm !shadow-none;
}

.fc .fc-button-primary {
  @apply bg-blue-600 border-blue-600 hover:bg-blue-700 hover:border-blue-700;
}

.fc .fc-button-primary:not(:disabled).fc-button-active {
  @apply bg-blue-800 border-blue-800;
}

.fc .fc-day-today {
  @apply !bg-blue-50;
}

.fc .fc-event {
  @apply rounded-lg border-none shadow-sm cursor-pointer;
}

.fc .fc-event:hover {
  @apply shadow-md;
}

/* React Day Picker Overrides */
.rdp {
  --rdp-cell-size: 40px;
  --rdp-accent-color: #2563EB;
  --rdp-background-color: #EFF6FF;
  @apply text-sm;
}

/* Toast Overrides */
.toast-custom {
  @apply !rounded-xl !shadow-lg;
}

/* Print styles */
@media print {
  .no-print {
    display: none !important;
  }
  
  body {
    @apply bg-white;
  }
}
🎉 PROJE TAMAMLANDI!
İşte size eksiksiz, production-ready bir HotelRunner Benzeri SaaS Projesi!

Projenin İçerdiği Tüm Bileşenler:
Backend (.NET Core + MSSQL)
✅ Tenant (Abone) Yönetimi

✅ Mülk Yönetimi (Otel, Apart, Bungalov, Villa, Ev, Oda)

✅ Birim/Oda Yönetimi

✅ Çoklu Para Birimi (TRY, USD, EUR, GBP)

✅ TCMB Döviz Kuru Entegrasyonu

✅ Sezonluk Fiyatlandırma

✅ Rezervasyon Sistemi (Check-in/out, İptal)

✅ Müsaitlik Takvimi

✅ Acente Yetkilendirme Sistemi

✅ Kontenjan Yönetimi

✅ Bildirim Sistemi (Email, SMS, InApp)

✅ Raporlama (Doluluk, Gelir, Performans)

✅ Booking Widget API

✅ Misafir Portalı

Frontend (React + TypeScript + Tailwind CSS)
✅ 3 Ayrı Panel:

Admin Paneli (Sistem Yönetimi)

Tenant Paneli (Mülk Sahibi)

Acente Paneli (Acente Kullanıcısı)

✅ Dashboard'lar:

İstatistik kartları

Doluluk ve gelir grafikleri

Son rezervasyonlar

Hızlı işlem butonları

✅ Mülk Yönetimi:

Grid/List görünüm

Detaylı filtreleme

Sürükle-bırak fotoğraf yükleme

Birim yönetimi

✅ Rezervasyon Yönetimi:

FullCalendar entegrasyonu

Check-in/out işlemleri

İptal ve iade yönetimi

Detaylı rezervasyon görünümü

✅ Fiyatlandırma:

Sezon bazlı fiyat

Takvim üzerinde fiyat güncelleme

Toplu fiyat değişikliği

Para birimi dönüşümü

✅ Acente Yönetimi:

Yetkilendirme kartları

Yetki seviyesi belirleme

Kontenjan takibi

Komisyon yönetimi

✅ Booking Widget:

Canlı önizleme

Tema ve renk özelleştirme

Embed kod üretici

Domain yönetimi

✅ Raporlama:

Doluluk raporu

Gelir raporu

Mülk performansı

Acente performansı

Misafir analizi

Excel/PDF export

✅ Responsive Tasarım:

Mobil uyumlu

Tablet uyumlu

Dark mode hazır

✅ Modern UI/UX:

Tailwind CSS ile şık tasarım

Animasyonlar ve geçişler

Toast bildirimleri

Loading skeleton'lar

Boş durum (empty state) tasarımları

Kullanılan Teknolojiler:
Backend:

.NET 8 Web API

Entity Framework Core

MSSQL

Redis (Cache)

RabbitMQ (Message Queue)

Hangfire (Background Jobs)

JWT Authentication

FluentValidation

Frontend:

React 18

TypeScript

Tailwind CSS

React Query (TanStack)

React Hook Form + Zod

Zustand (State Management)

Recharts (Grafikler)

FullCalendar (Takvim)

React Router v6

Axios

Lucide React (İkonlar)

React Hot Toast

Başlangıç Adımları:
Backend projesini oluşturun

Veritabanını kurun ve migration'ları çalıştırın

API'leri test edin

Frontend projesini oluşturun

npm install ile paketleri yükleyin

.env dosyasını yapılandırın

npm run dev ile geliştirmeye başlayın

Bu proje, tek başınıza 3-4 ayda MVP olarak çıkarabileceğiniz, tam kapsamlı bir SaaS ürünüdür. Her modül bağımsız çalışabilir ve kademeli olarak geliştirilebilir.

Herhangi bir modülle ilgili ek detay veya sorunuz varsa sorabilirsiniz! Başarılar dilerim! 🚀

bana fronted localimde oluşturmak için oluşturma kodunu ve proje şemasını verirmisin ben ikitane klasör oluşturdum

frontend,
backend olarak ayrı klasör
bunların altına olacak bir şekilde oluşturma kodu ve şeması

Prlatformun ismi TrimangoCalendar


vscode da
Harika! Hemen size projeyi ayağa kaldırmak için tüm gerekli kodları ve komutları veriyorum.

📁 PROJE YAPISI
text
TrimangoCalendar/
├── backend/
│   ├── TrimangoCalendar.API/
│   ├── TrimangoCalendar.Core/
│   ├── TrimangoCalendar.Data/
│   ├── TrimangoCalendar.Shared/
│   └── TrimangoCalendar.sln
│
├── frontend/
│   ├── src/
│   ├── public/
│   ├── package.json
│   ├── vite.config.ts
│   ├── tailwind.config.js
│   ├── tsconfig.json
│   └── index.html
│
└── README.md
🚀 BACKEND OLUŞTURMA
1. Terminal Komutları (Backend)
# Ana dizine git
cd TrimangoCalendar

# Backend klasörüne gir
cd backend

# Solution oluştur
dotnet new sln -n TrimangoCalendar

# Projeleri oluştur
dotnet new webapi -n TrimangoCalendar.API -f net8.0
dotnet new classlib -n TrimangoCalendar.Core -f net8.0
dotnet new classlib -n TrimangoCalendar.Data -f net8.0
dotnet new classlib -n TrimangoCalendar.Shared -f net8.0

# Projeleri solution'a ekle
dotnet sln add TrimangoCalendar.API/TrimangoCalendar.API.csproj
dotnet sln add TrimangoCalendar.Core/TrimangoCalendar.Core.csproj
dotnet sln add TrimangoCalendar.Data/TrimangoCalendar.Data.csproj
dotnet sln add TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj

# Proje referanslarını ekle
cd TrimangoCalendar.API
dotnet add reference ../TrimangoCalendar.Core/TrimangoCalendar.Core.csproj
dotnet add reference ../TrimangoCalendar.Data/TrimangoCalendar.Data.csproj
dotnet add reference ../TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj

cd ../TrimangoCalendar.Data
dotnet add reference ../TrimangoCalendar.Core/TrimangoCalendar.Core.csproj
dotnet add reference ../TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj

cd ../TrimangoCalendar.Core
dotnet add reference ../TrimangoCalendar.Shared/TrimangoCalendar.Shared.csproj

# Ana dizine dön
cd ..

# NuGet paketlerini yükle (API)
cd TrimangoCalendar.API
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package StackExchange.Redis
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer

# NuGet paketleri (Core)
cd ../TrimangoCalendar.Core
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package AutoMapper

# NuGet paketleri (Data)
cd ../TrimangoCalendar.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

cd ..
2. Backend Program.cs (TrimangoCalendar.API)
