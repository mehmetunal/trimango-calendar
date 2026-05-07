[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingWidgetController : ControllerBase
{
    private readonly IBookingEngineService _bookingEngine;
    
    public BookingWidgetController(IBookingEngineService bookingEngine)
    {
        _bookingEngine = bookingEngine;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateWidget([FromBody] CreateWidgetDto dto)
    {
        var tenantId = GetTenantId();
        var widget = await _bookingEngine.CreateWidgetAsync(dto.PropertyId, dto);
        return Ok(new { success = true, data = widget });
    }
    
    [HttpGet("{propertyId}")]
    public async Task<IActionResult> GetWidgets(Guid propertyId)
    {
        var widgets = await _bookingEngine.GetPropertyWidgetsAsync(propertyId);
        return Ok(new { success = true, data = widgets });
    }
    
    [HttpGet("embed/{widgetId}")]
    public async Task<IActionResult> GetEmbedCode(Guid widgetId)
    {
        var widget = await _bookingEngine.GetWidgetByIdAsync(widgetId);
        var embedCode = await _bookingEngine.GetWidgetEmbedCode(widget.WidgetKey);
        
        return Ok(new { success = true, data = new { widget, embedCode } });
    }
    
    [HttpPut("{widgetId}")]
    public async Task<IActionResult> UpdateWidget(Guid widgetId, [FromBody] UpdateWidgetDto dto)
    {
        var widget = await _bookingEngine.UpdateWidgetAsync(widgetId, dto);
        return Ok(new { success = true, data = widget });
    }
    
    [HttpDelete("{widgetId}")]
    public async Task<IActionResult> DeleteWidget(Guid widgetId)
    {
        await _bookingEngine.DeleteWidgetAsync(widgetId);
        return Ok(new { success = true, message = "Widget silindi" });
    }
    
    [HttpGet("preview/{widgetKey}")]
    public IActionResult PreviewWidget(string widgetKey)
    {
        return Redirect($"/widget/{widgetKey}");
    }
    
    private Guid GetTenantId()
    {
        return (Guid)HttpContext.Items["TenantId"];
    }
}
🎉 TÜM MODÜLLER TAMAMLANDI!
İşte size eksiksiz bir HotelRunner benzeri SaaS projesinin tüm modülleri:

Proje Özeti:
✅ MODÜL 1: Tenant Yönetimi - Çoklu abone, subdomain, plan yönetimi

✅ MODÜL 2: Mülk Yönetimi - Otel, apart, villa, bungalov, oda yönetimi

✅ MODÜL 3: Fiyatlandırma & Para Birimi - TRY, USD, EUR, GBP desteği, TCMB entegrasyonu

✅ MODÜL 4: Rezervasyon Sistemi - Müsaitlik, check-in/out, iptal politikası

✅ MODÜL 5: Acente Yetkilendirme - Yetki seviyeleri, kontenjan, komisyon

✅ MODÜL 6: Bildirim & Raporlama - Email, SMS, dashboard, Excel raporlar

✅ MODÜL 7: Booking Engine - Online rezervasyon widget'ı, embed kodu

Başlangıç için önerim:
Önce Modül 1 (Tenant) ve Modül 2 (Mülk) ile başlayın

Ardından Modül 3 (Fiyatlandırma) ve Modül 4 (Rezervasyon)

Sonra Modül 5 (Acente), Modül 6 (Bildirim/Rapor) ve Modül 7 (Widget)

Her modül için size verdiğim kodları direkt kullanabilir, kendi ihtiyaçlarınıza göre özelleştirebilirsiniz. Tek başınıza bu projeyi 3-4 ayda MVP olarak çıkarabilirsiniz.

Herhangi bir modülle ilgili ek detay veya sorunuz varsa sorabilirsiniz! Başarılar! 🚀

Frontendini de yapman gerekiyor
Kesinlikle haklısınız! Backend kadar frontend de çok önemli. Size React ile modern bir admin panel, tenant paneli ve acente paneli hazırlayalım.

🎨 FRONTEND MİMARİSİ
Proje Yapısı
hotel-platform-frontend/
├── public/
│   ├── index.html
│   └── assets/
│       ├── images/
│       └── icons/
├── src/
│   ├── api/                    # API istekleri
│   │   ├── axios.ts
│   │   ├── auth.api.ts
│   │   ├── tenant.api.ts
│   │   ├── property.api.ts
│   │   ├── reservation.api.ts
│   │   ├── pricing.api.ts
│   │   ├── agency.api.ts
│   │   ├── calendar.api.ts
│   │   ├── report.api.ts
│   │   └── widget.api.ts
│   │
│   ├── components/             # Ortak bileşenler
│   │   ├── ui/                 # Temel UI bileşenleri
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Select.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── Table.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Badge.tsx
│   │   │   ├── Tabs.tsx
│   │   │   ├── DatePicker.tsx
│   │   │   ├── Loading.tsx
│   │   │   ├── Pagination.tsx
│   │   │   ├── Toast.tsx
│   │   │   └── ConfirmDialog.tsx
│   │   │
│   │   ├── layout/             # Layout bileşenleri
│   │   │   ├── AdminLayout.tsx
│   │   │   ├── TenantLayout.tsx
│   │   │   ├── AgencyLayout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── Header.tsx
│   │   │   └── Footer.tsx
│   │   │
│   │   ├── charts/             # Grafik bileşenleri
│   │   │   ├── LineChart.tsx
│   │   │   ├── BarChart.tsx
│   │   │   ├── PieChart.tsx
│   │   │   └── OccupancyChart.tsx
│   │   │
│   │   └── calendar/           # Takvim bileşenleri
│   │       ├── BookingCalendar.tsx
│   │       ├── AvailabilityGrid.tsx
│   │       └── DateRangePicker.tsx
│   │
│   ├── pages/                  # Sayfalar
│   │   ├── admin/              # Admin paneli
│   │   │   ├── Dashboard.tsx
│   │   │   ├── Tenants/
│   │   │   │   ├── TenantList.tsx
│   │   │   │   ├── TenantDetail.tsx
│   │   │   │   └── TenantForm.tsx
│   │   │   ├── Agencies/
│   │   │   │   ├── AgencyList.tsx
│   │   │   │   └── AgencyDetail.tsx
│   │   │   └── Reports/
│   │   │       └── GlobalReports.tsx
│   │   │
│   │   ├── tenant/             # Mülk sahibi paneli
│   │   │   ├── Dashboard.tsx
│   │   │   ├── Properties/
│   │   │   │   ├── PropertyList.tsx
│   │   │   │   ├── PropertyForm.tsx
│   │   │   │   └── PropertyDetail.tsx
│   │   │   ├── Units/
│   │   │   │   ├── UnitList.tsx
│   │   │   │   └── UnitForm.tsx
│   │   │   ├── Reservations/
│   │   │   │   ├── ReservationList.tsx
│   │   │   │   ├── ReservationDetail.tsx
│   │   │   │   └── ReservationCalendar.tsx
│   │   │   ├── Pricing/
│   │   │   │   ├── SeasonRates.tsx
│   │   │   │   └── BulkPricing.tsx
│   │   │   ├── Agencies/
│   │   │   │   ├── Authorizations.tsx
│   │   │   │   └── GrantAuthorization.tsx
│   │   │   ├── Calendar/
│   │   │   │   └── BlockManagement.tsx
│   │   │   ├── Reports/
│   │   │   │   ├── OccupancyReport.tsx
│   │   │   │   └── RevenueReport.tsx
│   │   │   ├── Widgets/
│   │   │   │   └── WidgetSettings.tsx
│   │   │   └── Settings/
│   │   │       ├── Profile.tsx
│   │   │       └── Notifications.tsx
│   │   │
│   │   ├── agency/             # Acente paneli
│   │   │   ├── Dashboard.tsx
│   │   │   ├── MyProperties.tsx
│   │   │   ├── Calendar.tsx
│   │   │   ├── Reservations/
│   │   │   │   ├── ReservationList.tsx
│   │   │   │   └── CreateReservation.tsx
│   │   │   └── Reports.tsx
│   │   │
│   │   └── auth/               # Kimlik doğrulama
│   │       ├── Login.tsx
│   │       ├── Register.tsx
│   │       ├── ForgotPassword.tsx
│   │       └── ResetPassword.tsx
│   │
│   ├── hooks/                  # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useTenant.ts
│   │   ├── usePagination.ts
│   │   ├── useToast.ts
│   │   └── useDebounce.ts
│   │
│   ├── contexts/               # Context API
│   │   ├── AuthContext.tsx
│   │   ├── TenantContext.tsx
│   │   └── ThemeContext.tsx
│   │
│   ├── types/                  # TypeScript tipleri
│   │   ├── tenant.ts
│   │   ├── property.ts
│   │   ├── reservation.ts
│   │   ├── pricing.ts
│   │   ├── agency.ts
│   │   └── common.ts
│   │
│   ├── utils/                  # Yardımcı fonksiyonlar
│   │   ├── format.ts
│   │   ├── validators.ts
│   │   └── constants.ts
│   │
│   ├── styles/                 # Global stiller
│   │   ├── globals.css
│   │   └── variables.css
│   │
│   ├── App.tsx
│   └── main.tsx
│
├── package.json
├── tsconfig.json
├── tailwind.config.js
└── vite.config.ts
1. TEMEL KURULUM VE YAPILANDIRMA
package.json
{
  "name": "hotel-platform-frontend",
  "version": "1.0.0",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "lint": "eslint . --ext ts,tsx"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "react-hook-form": "^7.48.0",
    "@hookform/resolvers": "^3.3.2",
    "zod": "^3.22.4",
    "axios": "^1.6.2",
    "@tanstack/react-query": "^5.12.0",
    "@tanstack/react-table": "^8.11.0",
    "recharts": "^2.10.3",
    "date-fns": "^2.30.0",
    "lucide-react": "^0.294.0",
    "clsx": "^2.0.0",
    "tailwind-merge": "^2.1.0",
    "react-hot-toast": "^2.4.1",
    "react-day-picker": "^8.10.0",
    "fullcalendar": "^6.1.10",
    "@fullcalendar/react": "^6.1.10",
    "@fullcalendar/daygrid": "^6.1.10",
    "zustand": "^4.4.7"
  },
  "devDependencies": {
    "@types/react": "^18.2.43",
    "@types/react-dom": "^18.2.17",
    "@vitejs/plugin-react": "^4.2.1",
    "autoprefixer": "^10.4.16",
    "postcss": "^8.4.32",
    "tailwindcss": "^3.3.6",
    "typescript": "^5.3.3",
    "vite": "^5.0.8"
  }
}
API Yapılandırması
