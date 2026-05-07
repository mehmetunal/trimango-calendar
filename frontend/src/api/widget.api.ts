// src/api/widget.api.ts
import api from './axios';

export interface BookingWidget {
  id: string;
  propertyId: string;
  propertyName: string;
  widgetKey: string;
  theme: string;
  primaryColor: string;
  secondaryColor: string;
  fontFamily: string;
  showPropertyImages: boolean;
  showAmenities: boolean;
  showReviews: boolean;
  showPriceBreakdown: boolean;
  position: string;
  customCSS: string;
  metaTitle: string;
  metaDescription: string;
  sharingImage: string;
  requirePayment: boolean;
  minAdvanceDays: number;
  maxAdvanceDays: number;
  defaultLanguage: string;
  availableLanguages: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  integrations: WidgetIntegration[];
}

export interface WidgetIntegration {
  id: string;
  domain: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateWidgetDto {
  propertyId: string;
  theme?: string;
  primaryColor?: string;
  secondaryColor?: string;
  fontFamily?: string;
  showPropertyImages?: boolean;
  showAmenities?: boolean;
  showReviews?: boolean;
  showPriceBreakdown?: boolean;
  position?: string;
  requirePayment?: boolean;
  minAdvanceDays?: number;
  maxAdvanceDays?: number;
  defaultLanguage?: string;
  availableLanguages?: string[];
  metaTitle?: string;
  metaDescription?: string;
}

export const widgetApi = {
  // Widget CRUD
  getAll: async (): Promise<BookingWidget[]> => {
    const response = await api.get('/widgets');
    return response.data;
  },

  getById: async (id: string): Promise<BookingWidget> => {
    const response = await api.get(`/widgets/${id}`);
    return response.data;
  },

  getByProperty: async (propertyId: string): Promise<BookingWidget[]> => {
    const response = await api.get(`/widgets/property/${propertyId}`);
    return response.data;
  },

  create: async (data: CreateWidgetDto): Promise<BookingWidget> => {
    const response = await api.post('/widgets', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateWidgetDto>): Promise<BookingWidget> => {
    const response = await api.put(`/widgets/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/widgets/${id}`);
  },

  toggleActive: async (id: string): Promise<boolean> => {
    const response = await api.patch(`/widgets/${id}/toggle`);
    return response.data;
  },

  // Widget config
  getConfig: async (widgetKey: string): Promise<BookingWidget> => {
    const response = await api.get(`/widget/config/${widgetKey}`);
    return response.data;
  },

  getEmbedCode: async (widgetKey: string): Promise<string> => {
    const response = await api.get(`/widget/embed/${widgetKey}`);
    return response.data;
  },

  // Domain yönetimi
  getDomains: async (widgetId: string): Promise<WidgetIntegration[]> => {
    const response = await api.get(`/widgets/${widgetId}/domains`);
    return response.data;
  },

  addDomain: async (widgetId: string, domain: string): Promise<WidgetIntegration> => {
    const response = await api.post(`/widgets/${widgetId}/domains`, { domain });
    return response.data;
  },

  removeDomain: async (widgetId: string, integrationId: string): Promise<void> => {
    await api.delete(`/widgets/${widgetId}/domains/${integrationId}`);
  },

  // Önizleme
  getPreview: async (widgetId: string): Promise<any> => {
    const response = await api.get(`/widgets/${widgetId}/preview`);
    return response.data;
  },
};