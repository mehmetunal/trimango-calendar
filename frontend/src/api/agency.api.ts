// src/api/agency.api.ts
import api from './axios';
import type { ApiResponse, PaginatedResult } from '../types/common';

export interface Agency {
  id: string;
  companyName: string;
  taxNumber: string;
  email: string;
  phone: string;
  contactPerson: string;
  type: string;
  typeDescription: string;
  defaultCommissionRate: number;
  authorizedPropertyCount: number;
  isVerified: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface Authorization {
  id: string;
  agencyId: string;
  agencyName: string;
  propertyId: string;
  propertyName: string;
  propertyType: string;
  level: string;
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  canModifyReservation: boolean;
  canCancelReservation: boolean;
  priceDisplay: string;
  customCommissionRate: number | null;
  defaultMarkupRate: number | null;
  maxMarkupRate: number | null;
  hasAllotment: boolean;
  totalAllotment: number | null;
  usedAllotment: number;
  remainingAllotment: number;
  isActive: boolean;
  validFrom: string | null;
  validTo: string | null;
  grantedAt: string;
  notes: string;
}

export interface AuthorizedProperty {
  authorizationId: string;
  propertyId: string;
  propertyName: string;
  propertyType: string;
  city: string;
  totalUnits: number;
  activeReservations: number;
  canCreateReservation: boolean;
  canSetPrices: boolean;
  priceDisplay: string;
  commissionRate: number;
  defaultMarkupRate: number | null;
  remainingAllotment: number | null;
  totalAllotment: number | null;
  isActive: boolean;
}

export interface AuthorizedPropertyDetail {
  authorizationId: string;
  propertyId: string;
  propertyName: string;
  propertyDescription: string;
  propertyType: string;
  address: string;
  city: string;
  checkInTime: string;
  checkOutTime: string;
  authorizationLevel: string;
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  canModifyReservation: boolean;
  canCancelReservation: boolean;
  priceDisplay: string;
  commissionRate: number;
  defaultMarkupRate: number | null;
  maxMarkupRate: number | null;
  hasAllotment: boolean;
  totalAllotment: number | null;
  usedAllotment: number;
  remainingAllotment: number;
  units: AuthorizedUnit[];
  validFrom: string | null;
  validTo: string | null;
}

export interface AuthorizedUnit {
  unitId: string;
  unitName: string;
  unitNumber: string;
  maxAdults: number;
  maxChildren: number;
  basePrice: number | null;
  currencyCode: string;
  isActive: boolean;
}

export interface GrantAuthorizationDto {
  agencyId: string;
  propertyId: string;
  level: string;
  canViewPrices?: boolean;
  canSetPrices?: boolean;
  canCreateReservation?: boolean;
  canModifyReservation?: boolean;
  canCancelReservation?: boolean;
  priceDisplay?: string;
  customCommissionRate?: number;
  defaultMarkupRate?: number;
  maxMarkupRate?: number;
  hasAllotment?: boolean;
  totalAllotment?: number;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

export const agencyApi = {
  // Acente listesi
  getAll: async (params?: any): Promise<PaginatedResult<Agency>> => {
    const response = await api.get('/agencies', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Agency> => {
    const response = await api.get(`/agencies/${id}`);
    return response.data;
  },

  search: async (searchTerm: string): Promise<Agency[]> => {
    const response = await api.get('/agencies/search', { params: { searchTerm } });
    return response.data;
  },

  create: async (data: any): Promise<Agency> => {
    const response = await api.post('/agencies', data);
    return response.data;
  },

  update: async (id: string, data: any): Promise<Agency> => {
    const response = await api.put(`/agencies/${id}`, data);
    return response.data;
  },

  // Yetkilendirme
  getAllAuthorizations: async (): Promise<Authorization[]> => {
    const response = await api.get('/agencies/authorizations');
    return response.data;
  },

  getPropertyAuthorizations: async (propertyId: string): Promise<Authorization[]> => {
    const response = await api.get(`/agencies/authorizations/property/${propertyId}`);
    return response.data;
  },

  getAgencyAuthorizations: async (agencyId: string): Promise<Authorization[]> => {
    const response = await api.get(`/agencies/authorizations/agency/${agencyId}`);
    return response.data;
  },

  grantAuthorization: async (data: GrantAuthorizationDto): Promise<Authorization> => {
    const response = await api.post('/agencies/grant', data);
    return response.data;
  },

  updateAuthorization: async (authId: string, data: Partial<GrantAuthorizationDto>): Promise<Authorization> => {
    const response = await api.put(`/agencies/authorizations/${authId}`, data);
    return response.data;
  },

  revokeAuthorization: async (authId: string): Promise<void> => {
    await api.delete(`/agencies/authorizations/${authId}`);
  },

  // Acente paneli
  getMyProperties: async (agencyId: string, params?: any): Promise<PaginatedResult<AuthorizedProperty>> => {
    const response = await api.get(`/agencies/my-properties`, { params });
    return response.data;
  },

  getPropertyDetail: async (agencyId: string, propertyId: string): Promise<AuthorizedPropertyDetail> => {
    const response = await api.get(`/agencies/my-properties/${propertyId}`);
    return response.data;
  },

  getAuthorizationDetail: async (agencyId: string, propertyId: string): Promise<Authorization> => {
    const response = await api.get(`/agencies/authorization/${propertyId}`);
    return response.data;
  },

  canSetPrice: async (agencyId: string, unitId: string): Promise<boolean> => {
    const response = await api.get(`/agencies/can-set-price/${unitId}`);
    return response.data;
  },

  checkAllotment: async (authId: string): Promise<boolean> => {
    const response = await api.get(`/agencies/check-allotment/${authId}`);
    return response.data;
  },
};