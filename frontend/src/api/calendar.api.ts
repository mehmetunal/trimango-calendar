// src/api/calendar.api.ts
import api from './axios';
import type { ApiResponse } from '../types/common';

export interface CalendarBlock {
  id: string;
  unitId: string;
  propertyId?: string;
  type: string;
  startDate: string;
  endDate: string;
  reason: string;
  notes: string;
  isActive: boolean;
  createdAt: string;
}

export interface BlockDatesDto {
  unitId: string;
  propertyId?: string;
  type: string;
  startDate: string;
  endDate: string;
  reason: string;
  notes?: string;
  applyToAllUnits?: boolean;
  createdByTenantId?: string;
  createdByAgencyId?: string;
}

export interface DailyPriceDto {
  unitId: string;
  date: string;
  price: number;
  currencyCode: string;
  setByTenantId?: string;
  setByAgencyId?: string;
}

export interface SetDailyPriceDto {
  unitId: string;
  startDate: string;
  endDate: string;
  price: number;
  currencyCode: string;
  applyToWeekends?: boolean;
  weekendPrice?: number;
  setByTenantId?: string;
  setByAgencyId?: string;
  source?: string;
}

export interface CalendarDayStatus {
  date: string;
  status: string;
  statusDescription: string;
  reservationNumber?: string;
  guestName?: string;
  basePrice?: number;
  currencyCode?: string;
  agencyPrice?: number;
  blockReason?: string;
}

export interface UnitCalendar {
  unitId: string;
  unitName: string;
  unitNumber: string;
  dailyData: CalendarDayStatus[];
}

export interface AgencyCalendar {
  propertyId: string;
  propertyName: string;
  startDate: string;
  endDate: string;
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  priceDisplay: string;
  commissionRate: number;
  defaultMarkupRate: number | null;
  units: UnitCalendar[];
}

export interface OwnerCalendar {
  propertyId: string;
  propertyName: string;
  startDate: string;
  endDate: string;
  units: UnitCalendar[];
}

export const calendarApi = {
  // Blokaj yönetimi
  getBlocks: async (params: {
    propertyId?: string;
    unitIds?: string[];
    startDate: Date;
    endDate: Date;
  }): Promise<CalendarBlock[]> => {
    const response = await api.get('/calendar/blocks', { params });
    return response.data;
  },

  createBlock: async (data: BlockDatesDto): Promise<CalendarBlock> => {
    const response = await api.post('/calendar/blocks', data);
    return response.data;
  },

  updateBlock: async (id: string, data: Partial<BlockDatesDto>): Promise<CalendarBlock> => {
    const response = await api.put(`/calendar/blocks/${id}`, data);
    return response.data;
  },

  deleteBlock: async (id: string): Promise<void> => {
    await api.delete(`/calendar/blocks/${id}`);
  },

  createBulkBlocks: async (data: {
    propertyId: string;
    type: string;
    startDate: string;
    endDate: string;
    reason: string;
    notes?: string;
  }): Promise<void> => {
    await api.post('/calendar/blocks/bulk', data);
  },

  // Fiyat yönetimi
  getPrices: async (params: {
    propertyId?: string;
    startDate: Date;
    endDate: Date;
  }): Promise<DailyPriceDto[]> => {
    const response = await api.get('/calendar/prices', { params });
    return response.data;
  },

  setDailyPrice: async (data: SetDailyPriceDto): Promise<void> => {
    await api.post('/calendar/prices', data);
  },

  setBulkPrices: async (data: {
    unitIds: string[];
    startDate: string;
    endDate: string;
    price: number;
    currencyCode: string;
  }): Promise<void> => {
    await api.post('/calendar/prices/bulk', data);
  },

  // Takvim görünümleri
  getAgencyCalendar: async (
    agencyId: string,
    propertyId: string,
    startDate: Date,
    endDate: Date
  ): Promise<AgencyCalendar> => {
    const response = await api.get(`/calendar/agency/${propertyId}`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
    return response.data;
  },

  getOwnerCalendar: async (
    propertyId: string,
    startDate: Date,
    endDate: Date
  ): Promise<OwnerCalendar> => {
    const response = await api.get(`/calendar/owner/${propertyId}`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
    return response.data;
  },

  canAgencyBook: async (
    agencyId: string,
    unitId: string,
    checkIn: string,
    checkOut: string
  ): Promise<boolean> => {
    const response = await api.get('/calendar/can-book', {
      params: { agencyId, unitId, checkIn, checkOut },
    });
    return response.data;
  },
};