// src/api/pricing.api.ts
import api from './axios';
import type { ApiResponse } from '../types/common';

export interface PriceCalculationRequest {
  unitId: string;
  checkIn: string;
  checkOut: string;
  adults: number;
  children: number;
  currencyCode: string;
  promoCode?: string;
  agencyId?: string;
}

export interface DailyPrice {
  date: string;
  dayName: string;
  isWeekend: boolean;
  basePrice: number;
  actualPrice: number;
  currencyCode: string;
  seasonName: string;
}

export interface PriceBreakdown {
  dailyPrices: DailyPrice[];
  basePrice: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  weekendSurcharge: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  } | null;
  seasonSurcharge: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  } | null;
  extraBedCharge: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  } | null;
  promotionDiscount: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  } | null;
}

export interface PriceCalculationResult {
  unitId: string;
  unitName: string;
  checkIn: string;
  checkOut: string;
  totalNights: number;
  adults: number;
  children: number;
  breakdown: PriceBreakdown;
  totalPrice: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  averageNightlyPrice: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  taxAmount: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  serviceFee: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  grandTotal: {
    amount: number;
    currencyCode: string;
    formattedPrice: string;
  };
  cancellationPolicy?: {
    policyType: string;
    freeCancellationDays: number;
    cancellationFee: number | null;
    description: string;
  };
}

export interface SeasonRate {
  id: string;
  unitId: string;
  name: string;
  startDate: string;
  endDate: string;
  weekdayPrice: number;
  weekendPrice: number | null;
  specialDayPrice: number | null;
  currencyCode: string;
  minStayDays: number;
  maxStayDays: number;
  cancellationPolicy: string;
  freeCancellationDays: number;
  cancellationFee: number | null;
  isActive: boolean;
  createdAt: string;
}

export interface CreateSeasonRateDto {
  unitId: string;
  name: string;
  startDate: string;
  endDate: string;
  weekdayPrice: number;
  weekendPrice?: number;
  specialDayPrice?: number;
  currencyCode?: string;
  minStayDays?: number;
  maxStayDays?: number;
}

export interface Currency {
  code: string;
  symbol: string;
  name: string;
  isBaseCurrency: boolean;
  isActive: boolean;
}

export interface ExchangeRate {
  baseCurrency: string;
  targetCurrency: string;
  rate: number;
  date: string;
}

export const pricingApi = {
  // Fiyat hesaplama
  calculatePrice: async (data: PriceCalculationRequest): Promise<PriceCalculationResult> => {
    const response = await api.post('/pricing/calculate', data);
    return response.data;
  },

  getDailyPrices: async (unitId: string, checkIn: string, checkOut: string, currency: string = 'TRY'): Promise<DailyPrice[]> => {
    const response = await api.get(`/pricing/daily-prices/${unitId}`, {
      params: { checkIn, checkOut, currency },
    });
    return response.data;
  },

  // Sezon fiyatları
  getSeasonRates: async (unitId: string): Promise<SeasonRate[]> => {
    const response = await api.get(`/pricing/seasons/${unitId}`);
    return response.data;
  },

  createSeasonRate: async (data: CreateSeasonRateDto): Promise<SeasonRate> => {
    const response = await api.post('/pricing/seasons', data);
    return response.data;
  },

  updateSeasonRate: async (id: string, data: Partial<CreateSeasonRateDto>): Promise<SeasonRate> => {
    const response = await api.put(`/pricing/seasons/${id}`, data);
    return response.data;
  },

  deleteSeasonRate: async (id: string): Promise<void> => {
    await api.delete(`/pricing/seasons/${id}`);
  },

  // Para birimleri
  getCurrencies: async (): Promise<Currency[]> => {
    const response = await api.get('/pricing/currencies');
    return response.data;
  },

  getExchangeRate: async (from: string = 'TRY', to: string = 'USD'): Promise<ExchangeRate> => {
    const response = await api.get('/pricing/exchange-rate', { params: { from, to } });
    return response.data;
  },

  // Promosyon
  validatePromoCode: async (code: string, unitId: string, checkIn: string, checkOut: string): Promise<any> => {
    const response = await api.post('/pricing/validate-promo', { code, unitId, checkIn, checkOut });
    return response.data;
  },
};