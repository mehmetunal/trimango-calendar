// src/api/report.api.ts
import api from './axios';

export interface ReportRequest {
  propertyId?: string;
  startDate: string;
  endDate: string;
  currencyCode?: string;
  groupBy?: 'day' | 'week' | 'month' | 'year';
  compareWithPrevious?: boolean;
  type?: string;
  format?: 'excel' | 'pdf';
}

export interface OccupancyReport {
  propertyId?: string;
  startDate: string;
  endDate: string;
  totalUnits: number;
  averageOccupancyRate: number;
  peakOccupancyDate?: string;
  lowestOccupancyDate?: string;
  totalRoomNights: number;
  dailyOccupancy: {
    date: string;
    totalUnits: number;
    reservedUnits: number;
    availableUnits: number;
    blockedUnits: number;
    occupancyRate: number;
  }[];
}

export interface RevenueReport {
  startDate: string;
  endDate: string;
  totalRevenue: number;
  currencyCode: string;
  totalReservations: number;
  averageRevenuePerDay: number;
  totalTax: number;
  totalServiceFee: number;
  totalDiscounts: number;
  netRevenue: number;
  highestRevenue: number;
  lowestRevenue: number;
  revenueByCurrency: {
    currencyCode: string;
    totalRevenue: number;
    count: number;
  }[];
  revenueByProperty: {
    propertyName: string;
    propertyId: string;
    totalRevenue: number;
    reservationCount: number;
    averagePerReservation: number;
    currencyCode: string;
  }[];
  monthlyRevenue: {
    period: string;
    year: number;
    month: number;
    monthName: string;
    totalRevenue: number;
    reservationCount: number;
    averagePerReservation: number;
    currencyCode: string;
    taxAmount: number;
    serviceFee: number;
    discountAmount: number;
    netRevenue: number;
  }[];
}

export interface PropertyPerformance {
  propertyId: string;
  propertyName: string;
  propertyType: string;
  totalRevenue: number;
  reservationCount: number;
  occupancyRate: number;
  averageRating: number;
  cancellationRate: number;
  revenuePerUnit: number;
}

export interface AgencyPerformance {
  agencyId: string;
  agencyName: string;
  reservationCount: number;
  totalRevenue: number;
  commissionAmount: number;
  cancellationRate: number;
  averageStayDuration: number;
}

export interface GuestStats {
  totalGuests: number;
  newGuests: number;
  returningGuests: number;
  averageStayDuration: number;
  averageRating: number;
  totalReviews: number;
  topNationalities: {
    nationality: string;
    count: number;
  }[];
}

export interface DashboardData {
  todayCheckIns: number;
  todayCheckOuts: number;
  currentOccupancy: number;
  totalUnits: number;
  occupiedUnits: number;
  totalProperties: number;
  totalReservations: number;
  activeReservations: number;
  pendingReservations: number;
  monthlyRevenue: number;
  currencyCode: string;
  averageRevenuePerReservation: number;
  recentReservations: any[];
  occupancyChart: { label: string; value: number }[];
  revenueChart: { label: string; value: number }[];
  topAgencies: {
    agencyId: string;
    agencyName: string;
    reservationCount: number;
    totalRevenue: number;
    currencyCode: string;
  }[];
  reservationChange?: number;
  revenueChange?: number;
}

export const reportApi = {
  // Dashboard
  getDashboard: async (startDate?: Date, endDate?: Date): Promise<DashboardData> => {
    const response = await api.get('/dashboard', {
      params: {
        startDate: startDate?.toISOString(),
        endDate: endDate?.toISOString(),
      },
    });
    return response.data;
  },

  getAgencyDashboard: async (
    agencyId: string,
    startDate?: Date,
    endDate?: Date
  ): Promise<DashboardData> => {
    const response = await api.get(`/dashboard/agency/${agencyId}`, {
      params: {
        startDate: startDate?.toISOString(),
        endDate: endDate?.toISOString(),
      },
    });
    return response.data;
  },

  // Raporlar
  getOccupancyReport: async (request: ReportRequest): Promise<OccupancyReport> => {
    const response = await api.post('/reports/occupancy', request);
    return response.data;
  },

  getRevenueReport: async (request: ReportRequest): Promise<RevenueReport> => {
    const response = await api.post('/reports/revenue', request);
    return response.data;
  },

  getPropertyPerformance: async (request: ReportRequest): Promise<PropertyPerformance[]> => {
    const response = await api.post('/reports/performance', request);
    return response.data;
  },

  getAgencyPerformance: async (request: ReportRequest): Promise<AgencyPerformance[]> => {
    const response = await api.post('/reports/agency-performance', request);
    return response.data;
  },

  getGuestStats: async (request: ReportRequest): Promise<GuestStats> => {
    const response = await api.post('/reports/guests', request);
    return response.data;
  },

  // Export
  exportReport: async (request: ReportRequest): Promise<Blob> => {
    const response = await api.post('/reports/export', request, {
      responseType: 'blob',
    });
    return response.data;
  },

  downloadReport: async (reportId: string): Promise<Blob> => {
    const response = await api.get(`/reports/download/${reportId}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  getReports: async (): Promise<any[]> => {
    const response = await api.get('/reports');
    return response.data;
  },
};