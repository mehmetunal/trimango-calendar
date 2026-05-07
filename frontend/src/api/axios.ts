// src/api/axios.ts
import axios from 'axios';
import { useAuthStore } from '../stores/authStore';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

const useMock = import.meta.env.VITE_USE_MOCK === 'true';
const mockNow = new Date().toISOString();
const mockReservations: Array<Record<string, unknown>> = [
  { id: 'r1', reservationNumber: 'RSV-1001', guestName: 'Ali Veli', guestEmail: 'ali@mail.com', guestPhone: '5551112233', propertyId: 'p1', propertyName: 'Sahil Otel', unitId: 'u1', unitName: '101', checkIn: mockNow, checkOut: mockNow, totalNights: 2, totalAmount: 8200, currencyCode: 'TRY', status: 'Confirmed', adults: 2, children: 0, source: 'Agency' }
];

function formatTRY(value: number) {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
}

function getPayload(config: { data?: unknown }) {
  if (!config.data) return {};
  if (typeof config.data === 'string') {
    try {
      return JSON.parse(config.data);
    } catch {
      return {};
    }
  }
  return (config.data as Record<string, unknown>) || {};
}

function mockResponse(
  url: string,
  method: string,
  payload: Record<string, unknown> = {},
  params: Record<string, unknown> = {}
) {
  const isGet = method.toLowerCase() === 'get';
  if (url.includes('/dashboard')) {
    return {
      todayCheckIns: 4,
      currentOccupancy: 72,
      monthlyRevenue: 184500,
      currencyCode: 'TRY',
      activeReservations: 18,
      totalReservations: 56,
      reservationChange: 12.4,
      revenueChange: 9.8,
      occupancyChart: [{ label: 'Pzt', value: 62 }, { label: 'Sal', value: 70 }, { label: 'Car', value: 74 }],
      revenueChart: [{ label: 'Hafta 1', value: 42000 }, { label: 'Hafta 2', value: 51000 }, { label: 'Hafta 3', value: 46000 }],
      recentReservations: [
        { id: 'r1', reservationNumber: 'RSV-1001', guestName: 'Ali Veli', propertyName: 'Sahil Otel', unitName: '101', checkIn: mockNow, checkOut: mockNow, totalAmount: 8200, currencyCode: 'TRY', status: 'Confirmed' }
      ],
    };
  }
  if (url.includes('/agencies/my-properties')) {
    const allProperties = [
      { propertyId: 'p1', propertyName: 'Sahil Otel', propertyType: 'Hotel', city: 'Izmir', totalUnits: 24, activeReservations: mockReservations.filter((r) => r.propertyId === 'p1').length, canCreateReservation: true, canSetPrices: true, priceDisplay: 'Net', commissionRate: 10, defaultMarkupRate: 5, remainingAllotment: 22, totalAllotment: 30, isActive: true },
      { propertyId: 'p2', propertyName: 'Dag Evi Sapanca', propertyType: 'Villa', city: 'Sakarya', totalUnits: 8, activeReservations: 3, canCreateReservation: true, canSetPrices: true, priceDisplay: 'Net', commissionRate: 12, defaultMarkupRate: 4, remainingAllotment: 15, totalAllotment: 20, isActive: true },
      { propertyId: 'p3', propertyName: 'Liman Apart', propertyType: 'ApartHotel', city: 'Antalya', totalUnits: 14, activeReservations: 5, canCreateReservation: true, canSetPrices: false, priceDisplay: 'Net', commissionRate: 9, defaultMarkupRate: 3, remainingAllotment: 10, totalAllotment: 15, isActive: true },
    ];

    if (isGet && /my-properties\/[^/]+$/.test(url)) {
      const propertyId = url.split('/').pop() || 'p1';
      const selected = allProperties.find((p) => p.propertyId === propertyId) || allProperties[0];
      return {
        propertyId: selected.propertyId,
        propertyName: selected.propertyName,
        propertyDescription: 'Mock property',
        propertyType: selected.propertyType,
        address: selected.city,
        city: selected.city,
        checkInTime: '14:00',
        checkOutTime: '12:00',
        authorizationLevel: 'CanReserve',
        canViewPrices: true,
        canSetPrices: true,
        canCreateReservation: true,
        canModifyReservation: true,
        canCancelReservation: true,
        priceDisplay: 'Net',
        commissionRate: 10,
        customCommissionRate: 10,
        defaultMarkupRate: 5,
        maxMarkupRate: 20,
        hasAllotment: true,
        totalAllotment: 30,
        usedAllotment: 8,
        remainingAllotment: 22,
        units: [
          { unitId: `${selected.propertyId}-u1`, unitName: '101', unitNumber: '101', maxAdults: 2, maxChildren: 1, basePrice: 3500, currencyCode: 'TRY', isActive: true },
          { unitId: `${selected.propertyId}-u2`, unitName: '102', unitNumber: '102', maxAdults: 3, maxChildren: 1, basePrice: 4200, currencyCode: 'TRY', isActive: true },
        ],
        validFrom: null,
        validTo: null,
      };
    }
    const page = Number(params.page || 1);
    const pageSize = Number(params.pageSize || 20);
    const search = String(params.search || '').trim().toLowerCase();
    const type = String(params.type || '').trim();
    const filtered = allProperties.filter((p) => {
      const searchOk = !search || p.propertyName.toLowerCase().includes(search) || p.city.toLowerCase().includes(search);
      const typeOk = !type || p.propertyType === type;
      return searchOk && typeOk;
    });
    const start = (page - 1) * pageSize;
    const items = filtered.slice(start, start + pageSize);
    return {
      items,
      page,
      pageSize,
      totalPages: Math.max(1, Math.ceil(filtered.length / pageSize)),
      totalCount: filtered.length,
    };
  }
  if (url.includes('/calendar/agency/')) {
    const propertyId = url.split('/').pop() || 'p1';
    const units = [
      { unitId: `${propertyId}-u1`, unitName: '101', unitNumber: '101', basePrice: 3500 },
      { unitId: `${propertyId}-u2`, unitName: '102', unitNumber: '102', basePrice: 4200 },
    ];
    const start = new Date(String(params.startDate || new Date().toISOString()));
    const end = new Date(String(params.endDate || new Date(Date.now() + 30 * 86400000).toISOString()));
    const days: string[] = [];
    for (let d = new Date(start); d < end; d.setDate(d.getDate() + 1)) {
      days.push(new Date(d).toISOString().split('T')[0]);
    }

    const reservationItems = mockReservations.filter((r) => String(r.propertyId) === propertyId);
    return {
      propertyId,
      propertyName: propertyId === 'p1' ? 'Sahil Otel' : propertyId === 'p2' ? 'Dag Evi Sapanca' : 'Liman Apart',
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      canViewPrices: true,
      canSetPrices: true,
      canCreateReservation: true,
      priceDisplay: 'Net',
      commissionRate: 10,
      defaultMarkupRate: 5,
      units: units.map((unit) => ({
        unitId: unit.unitId,
        unitName: unit.unitName,
        unitNumber: unit.unitNumber,
        dailyData: days.map((date) => {
          const reservation = reservationItems.find((r) => {
            const rUnit = String(r.unitId || '');
            if (rUnit && rUnit !== unit.unitId) return false;
            const checkIn = new Date(String(r.checkIn)).toISOString().split('T')[0];
            const checkOut = new Date(String(r.checkOut)).toISOString().split('T')[0];
            return date >= checkIn && date < checkOut;
          });
          if (reservation) {
            return {
              date,
              status: 'Reserved',
              statusDescription: 'Rezerve',
              reservation: { id: reservation.id },
              reservationNumber: reservation.reservationNumber,
              guestName: reservation.guestName,
              currencyCode: 'TRY',
              agencyPrice: unit.basePrice,
            };
          }
          return {
            date,
            status: 'Available',
            statusDescription: 'Musait',
            currencyCode: 'TRY',
            agencyPrice: unit.basePrice,
          };
        }),
      })),
    };
  }
  if (url.includes('/agencies/authorization/')) {
    return {
      id: 'auth-1',
      agencyId: 'agency-001',
      agencyName: 'Blue Agency',
      propertyId: 'p1',
      propertyName: 'Sahil Otel',
      propertyType: 'Hotel',
      level: 'CanReserve',
      canViewPrices: true,
      canSetPrices: true,
      canCreateReservation: true,
      canModifyReservation: true,
      canCancelReservation: true,
      priceDisplay: 'Net',
      customCommissionRate: 10,
      defaultMarkupRate: 5,
      maxMarkupRate: 20,
      hasAllotment: true,
      totalAllotment: 30,
      usedAllotment: 8,
      remainingAllotment: 22,
      isActive: true,
      validFrom: null,
      validTo: null,
      grantedAt: new Date().toISOString(),
      notes: '',
    };
  }
  if (url.includes('/reservations/calendar')) return [];
  if (url.includes('/reservations') && method.toLowerCase() === 'post') {
    const createdId = `r${mockReservations.length + 1}`;
    const checkIn = String(payload.checkIn || mockNow);
    const checkOut = String(payload.checkOut || mockNow);
    const created = {
      id: createdId,
      reservationNumber: `RSV-${1000 + mockReservations.length + 1}`,
      guestName: String(payload.guestName || 'Misafir'),
      guestEmail: String(payload.guestEmail || ''),
      guestPhone: String(payload.guestPhone || ''),
      propertyId: String(payload.propertyId || 'p1'),
      propertyName: 'Sahil Otel',
      unitId: String(payload.unitId || 'u1'),
      unitName: '101',
      checkIn,
      checkOut,
      totalNights: Number(payload.nights || 2),
      totalAmount: Number(payload.totalAmount || 8200),
      currencyCode: 'TRY',
      status: 'Confirmed',
      adults: Number(payload.adults || 2),
      children: Number(payload.children || 0),
      source: 'Agency',
      createdAt: new Date().toISOString(),
    };
    mockReservations.unshift(created);
    return created;
  }
  if (url.includes('/reservations') && method.toLowerCase() === 'get') {
    return {
      items: mockReservations,
      page: 1,
      pageSize: 20,
      totalPages: 1,
      totalCount: mockReservations.length,
    };
  }
  if (url.includes('/properties')) {
    return {
      items: [{ id: 'p1', tenantId: 't1', type: 'Hotel', name: 'Sahil Otel', slug: 'sahil-otel', description: '', shortDescription: '', email: '', phone: '', address: 'Izmir', district: '', city: 'Izmir', country: 'TR', postalCode: '', latitude: null, longitude: null, checkInTime: '14:00', checkOutTime: '12:00', coverImageUrl: '', amenities: [], averageRating: 4.5, reviewCount: 18, totalUnitCount: 24, startingPrice: 2500, currencyCode: 'TRY', isActive: true, createdAt: mockNow }],
      page: 1, pageSize: 20, totalPages: 1, totalCount: 1,
    };
  }
  if (url.includes('/currencies')) return [{ code: 'TRY', name: 'Turk Lirasi', symbol: '₺', isBase: true, isActive: true }];
  if (url.includes('/exchange')) return { baseCurrency: 'TRY', targetCurrency: 'USD', rate: 0.031 };
  if (url.includes('/pricing/calculate') && method.toLowerCase() === 'post') {
    const nights = Number(payload.nights || 2);
    const nightly = 3500;
    const baseAmount = nightly * nights;
    const taxAmount = Math.round(baseAmount * 0.1);
    const serviceFee = 300;
    const grandTotal = baseAmount + taxAmount + serviceFee;
    const dailyPrices = Array.from({ length: nights }).map((_, index) => ({
      date: new Date(Date.now() + index * 24 * 60 * 60 * 1000).toISOString(),
      isWeekend: index % 6 === 0 || index % 7 === 0,
      basePrice: nightly,
      finalPrice: nightly,
      actualPrice: nightly,
      markupRate: 0,
      markupAmount: 0,
      currencyCode: 'TRY',
      formattedPrice: formatTRY(nightly),
    }));

    return {
      canReserve: true,
      pricingType: 'Net',
      isCommissionIncluded: false,
      appliedMarkupRate: 0,
      appliedMarkupAmount: 0,
      breakdown: {
        dailyPrices,
        basePrice: { amount: baseAmount, currencyCode: 'TRY', formattedPrice: formatTRY(baseAmount) },
      },
      baseAmount: { amount: baseAmount, currencyCode: 'TRY', formattedPrice: formatTRY(baseAmount) },
      taxAmount: { amount: taxAmount, currencyCode: 'TRY', formattedPrice: formatTRY(taxAmount) },
      serviceFee: { amount: serviceFee, currencyCode: 'TRY', formattedPrice: formatTRY(serviceFee) },
      discountAmount: { amount: 0, currencyCode: 'TRY', formattedPrice: formatTRY(0) },
      grandTotal: { amount: grandTotal, currencyCode: 'TRY', formattedPrice: formatTRY(grandTotal) },
      averageNightlyPrice: { amount: nightly, currencyCode: 'TRY', formattedPrice: formatTRY(nightly) },
      minimumStayRequirement: 1,
      cancellationPolicy: 'Giris tarihinden 24 saat once ucretsiz iptal.',
      reservationDeadline: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
    };
  }
  if (url.includes('/reports')) return { revenueChart: [], occupancyChart: [], performanceChart: [] };
  if (url.includes('/auth/profile')) return { id: 'u1' };
  return { success: true };
}

function mapApiPath(url: string): string {
  if (!url) return url;
  const mappings: Array<[RegExp, string]> = [
    [/^\/agencies\b/i, '/Agency'],
    [/^\/reservations\b/i, '/Reservation'],
    [/^\/properties\b/i, '/Property'],
    [/^\/pricing\b/i, '/Pricing'],
    [/^\/dashboard\b/i, '/Dashboard'],
    [/^\/tenants\b/i, '/Tenant'],
    [/^\/reports\b/i, '/Report'],
    [/^\/widgets\b/i, '/BookingWidget'],
    [/^\/auth\b/i, '/Auth'],
  ];

  for (const [pattern, replacement] of mappings) {
    if (pattern.test(url)) {
      return url.replace(pattern, replacement);
    }
  }

  return url;
}

// Request interceptor - Token ekle
api.interceptors.request.use(
  (config) => {
    if (config.url) {
      config.url = mapApiPath(config.url);
    }

    if (useMock) {
      config.adapter = async () => {
        const payload = getPayload(config);
        const data = mockResponse(
          config.url || '',
          config.method || 'get',
          payload,
          (config.params as Record<string, unknown>) || {}
        );
        return {
          data,
          status: 200,
          statusText: 'OK',
          headers: {},
          config,
        };
      };
    }

    const token = useAuthStore.getState().token;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Tenant ID ekle (subdomain'den geliyorsa)
    const tenantId = localStorage.getItem('tenantId');
    if (tenantId) {
      config.headers['X-Tenant-Id'] = tenantId;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - Hata yönetimi
api.interceptors.response.use(
  (response) => {
    const payload = response.data;
    if (payload && typeof payload === 'object' && 'success' in payload && 'data' in payload) {
      response.data = (payload as { data: unknown }).data;
    }
    return response;
  },
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error.response?.data || error);
  }
);

export default api;
