import api from './axios';

export interface DashboardData {
  todayCheckIns: number;
  currentOccupancy: number;
  monthlyRevenue: number;
  currencyCode: string;
  activeReservations: number;
  occupancyChart: Array<{ label: string; value: number }>;
  revenueChart: Array<{ label: string; value: number }>;
  recentReservations: Array<{
    id: string;
    reservationNumber: string;
    guestName: string;
    propertyName: string;
    unitName: string;
    checkIn: string;
    checkOut: string;
    totalAmount: number;
    currencyCode: string;
    status: string;
  }>;
}

export const dashboardApi = {
  getDashboard: async (start: Date, end: Date): Promise<DashboardData> => {
    const response = await api.get('/dashboard', {
      params: { start: start.toISOString(), end: end.toISOString() },
    });
    return response.data;
  },
};
