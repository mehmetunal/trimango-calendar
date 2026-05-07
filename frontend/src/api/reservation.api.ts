// src/api/reservation.api.ts
import api from './axios';
import type { Reservation } from '../types/reservation';
import type { PaginatedResult } from '../types/common';

export interface ReservationQueryParams {
  checkInFrom?: string;
  checkInTo?: string;
  checkOutFrom?: string;
  checkOutTo?: string;
  status?: string;
  propertyId?: string;
  unitId?: string;
  guestId?: string;
  searchTerm?: string;
  source?: string;
  currencyCode?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CreateReservationDto {
  unitId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  checkIn: string;
  checkOut: string;
  adults: number;
  children?: number;
  infants?: number;
  currencyCode?: string;
  promoCode?: string;
  specialRequests?: string;
  source?: string;
  tcKimlikNo?: string;
  passportNumber?: string;
  nationality?: string;
}

export interface CreateAgencyReservationDto {
  authorizationId: string;
  unitId: string;
  checkIn: string;
  checkOut: string;
  adults: number;
  children?: number;
  firstName: string;
  lastName: string;
}

export const reservationApi = {
  getAll: async (params?: ReservationQueryParams): Promise<PaginatedResult<Reservation>> => {
    const response = await api.get('/reservations', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Reservation> => {
    const response = await api.get(`/reservations/${id}`);
    return response.data;
  },

  getByNumber: async (number: string): Promise<Reservation> => {
    const response = await api.get(`/reservations/number/${number}`);
    return response.data;
  },

  create: async (data: CreateReservationDto): Promise<Reservation> => {
    const response = await api.post('/reservations', data);
    return response.data;
  },

  getAgencyReservations: async (agencyId: string, params?: ReservationQueryParams): Promise<PaginatedResult<Reservation>> => {
    const response = await api.get('/reservations', { params: { ...params, agencyId } });
    return response.data;
  },

  createAgencyReservation: async (_agencyId: string, data: CreateAgencyReservationDto): Promise<Reservation> => {
    const response = await api.post('/Agency/my-reservations', data);
    return response.data;
  },

  cancelAgencyReservation: async (agencyId: string, id: string, reason: string): Promise<void> => {
    await api.post(`/reservations/${id}/cancel`, { reason, agencyId });
  },

  checkIn: async (id: string): Promise<Reservation> => {
    const response = await api.post(`/reservations/${id}/check-in`);
    return response.data;
  },

  checkOut: async (id: string, isLate: boolean = false): Promise<Reservation> => {
    const response = await api.post(`/reservations/${id}/check-out`, null, {
      params: { isLate },
    });
    return response.data;
  },

  cancel: async (id: string, reason: string): Promise<void> => {
    await api.post(`/reservations/${id}/cancel`, reason);
  },

  getCalendar: async (start: Date, end: Date, propertyId?: string): Promise<any[]> => {
    const response = await api.get('/reservations/calendar', {
      params: { start: start.toISOString(), end: end.toISOString(), propertyId },
    });
    return response.data;
  },

  getAvailability: async (
    propertyId: string,
    startDate: Date,
    endDate: Date
  ): Promise<any> => {
    const response = await api.get(`/reservations/availability/${propertyId}`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
    return response.data;
  },

  checkUnitAvailability: async (
    unitId: string,
    checkIn: Date,
    checkOut: Date
  ): Promise<boolean> => {
    const response = await api.get(`/reservations/check-availability/${unitId}`, {
      params: {
        checkIn: checkIn.toISOString(),
        checkOut: checkOut.toISOString(),
      },
    });
    return response.data.isAvailable;
  },
};
