// src/api/property.api.ts
import api from './axios';
import type { Property, Unit } from '../types/property';
import type { PaginatedResult, ApiResponse } from '../types/common';

export const propertyApi = {
  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    type?: string;
    city?: string;
    isActive?: boolean;
  }): Promise<PaginatedResult<Property>> => {
    const response = await api.get('/properties', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Property> => {
    const response = await api.get(`/properties/${id}`);
    return response.data;
  },

  getBySlug: async (slug: string): Promise<Property> => {
    const response = await api.get(`/properties/slug/${slug}`);
    return response.data;
  },

  create: async (data: FormData | any): Promise<Property> => {
    const response = await api.post('/properties', data);
    return response.data;
  },

  update: async (id: string, data: Partial<Property>): Promise<Property> => {
    const response = await api.put(`/properties/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/properties/${id}`);
  },

  toggleActive: async (id: string): Promise<boolean> => {
    const response = await api.patch(`/properties/${id}/toggle`);
    return response.data;
  },

  uploadImages: async (propertyId: string, files: File[]): Promise<any> => {
    const formData = new FormData();
    files.forEach((file) => formData.append('files', file));
    const response = await api.post(`/properties/${propertyId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  deleteImage: async (propertyId: string, imageId: string): Promise<void> => {
    await api.delete(`/properties/${propertyId}/images/${imageId}`);
  },

  // Units
  getUnits: async (propertyId: string): Promise<Unit[]> => {
    const response = await api.get(`/properties/${propertyId}/units`);
    return response.data;
  },

  createUnit: async (propertyId: string, data: any): Promise<Unit> => {
    const response = await api.post(`/properties/${propertyId}/units`, data);
    return response.data;
  },

  updateUnit: async (unitId: string, data: Partial<Unit>): Promise<Unit> => {
    const response = await api.put(`/units/${unitId}`, data);
    return response.data;
  },

  deleteUnit: async (unitId: string): Promise<void> => {
    await api.delete(`/units/${unitId}`);
  },
};