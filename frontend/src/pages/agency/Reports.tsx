// src/pages/agency/Reports.tsx
import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Download,
  Calendar,
  TrendingUp,
  TrendingDown,
  DollarSign,
  Building2,
  Star,
  Filter,
  FileText,
  BarChart3,
} from 'lucide-react';
import { reportApi } from '../../api/report.api';
import { agencyApi } from '../../api/agency.api';
import { Button, Input, Select, Card, Tabs } from '../../components/ui';
import { LineChart, BarChart, PieChart } from '../../components/charts';
import { formatCurrency, formatDate } from '../../utils/format';
import { useAuthStore } from '../../stores/authStore';
import toast from 'react-hot-toast';

export default function AgencyReports() {
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  const [activeTab, setActiveTab] = useState<'overview' | 'revenue' | 'properties'>('overview');
  const [dateRange, setDateRange] = useState({
    startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
  });

  const { data: dashboard } = useQuery({
    queryKey: ['agency', 'dashboard', agencyId, dateRange],
    queryFn: () => reportApi.getAgencyDashboard(agencyId!, new Date(dateRange.startDate), new Date(dateRange.endDate)),
    enabled: !!agencyId,
  });

  const { data: revenueData } = useQuery({
    queryKey: ['agency', 'revenue', agencyId, dateRange],
    queryFn: () => reportApi.getRevenueReport({
      startDate: dateRange.startDate,
      endDate: dateRange.endDate,
    }),
    enabled: !!agencyId && activeTab === 'revenue',
  });

  const handleExport = async (format: 'excel' | 'pdf') => {
    try {
      const blob = await reportApi.exportReport({
        startDate: dateRange.startDate,
        endDate: dateRange.endDate,
        format,
      });
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `acente_raporu_${dateRange.startDate}_${dateRange.endDate}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      a.click();
      window.URL.revokeObjectURL(url);
      
      toast.success('Rapor indirildi');
    } catch (error: any) {
      toast.error('Rapor indirilemedi');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Raporlar</h1>
          <p className="text-sm text-gray-500 mt-1">Acente performans raporları</p>
        </div>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleExport('excel')}
            leftIcon={<Download className="w-4 h-4" />}
          >
            Excel
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleExport('pdf')}
            leftIcon={<FileText className="w-4 h-4" />}
          >
            PDF
          </Button>
        </div>
      </div>

      {/* Date Filter */}
      <Card className="p-4">
        <div className="grid grid-cols-2 gap-4">
          <Input
            label="Başlangıç"
            type="date"
            value={dateRange.startDate}
            onChange={(e) => setDateRange({ ...dateRange, startDate: e.target.value })}
          />
          <Input
            label="Bitiş"
            type="date"
            value={dateRange.endDate}
            onChange={(e) => setDateRange({ ...dateRange, endDate: e.target.value })}
          />
        </div>
      </Card>

      {/* Tabs */}
      <Tabs
        tabs={[
          { key: 'overview', label: 'Genel Bakış', icon: BarChart3 },
          { key: 'revenue', label: 'Gelir', icon: DollarSign },
          { key: 'properties', label: 'Mülk Bazlı', icon: Building2 },
        ]}
        activeTab={activeTab}
        onChange={(tab) => setActiveTab(tab as any)}
      />

      {/* Overview Tab */}
      {activeTab === 'overview' && dashboard && (
        <div className="space-y-6">
          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <Card className="p-5">
              <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                <TrendingUp className="w-4 h-4" />
                Toplam Rezervasyon
              </div>
              <div className="text-2xl font-bold text-blue-600">
                {dashboard.totalReservations || 0}
              </div>
            </Card>
            <Card className="p-5">
              <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                <DollarSign className="w-4 h-4" />
                Toplam Gelir
              </div>
              <div className="text-2xl font-bold text-green-600">
                {formatCurrency(dashboard.monthlyRevenue || 0, dashboard.currencyCode || 'TRY')}
              </div>
            </Card>
            <Card className="p-5">
              <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                <Building2 className="w-4 h-4" />
                Yetkili Mülk
              </div>
              <div className="text-2xl font-bold text-purple-600">
                {dashboard.totalProperties || 0}
              </div>
            </Card>
            <Card className="p-5">
              <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                <Star className="w-4 h-4" />
                Aktif Rezervasyon
              </div>
              <div className="text-2xl font-bold text-orange-600">
                {dashboard.activeReservations || 0}
              </div>
            </Card>
          </div>

          {/* Charts */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Gelir Grafiği</h3>
              {dashboard.revenueChart ? (
                <LineChart data={dashboard.revenueChart} dataKey="value" height={250} />
              ) : (
                <div className="flex items-center justify-center h-64 text-gray-500">
                  Veri bulunamadı
                </div>
              )}
            </Card>

            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Rezervasyon Dağılımı</h3>
              {dashboard.topAgencies ? (
                <PieChart data={dashboard.topAgencies} dataKey="reservationCount" nameKey="propertyName" height={250} />
              ) : (
                <div className="flex items-center justify-center h-64 text-gray-500">
                  Veri bulunamadı
                </div>
              )}
            </Card>
          </div>
        </div>
      )}

      {/* Revenue Tab */}
      {activeTab === 'revenue' && revenueData && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Gelir Detayı</h3>
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Dönem</th>
                  <th>Rezervasyon</th>
                  <th>Brüt Gelir</th>
                  <th>Komisyon</th>
                  <th>Net Gelir</th>
                </tr>
              </thead>
              <tbody>
                {revenueData.monthlyRevenue?.map((item: any, index: number) => (
                  <tr key={index}>
                    <td className="font-medium">{item.monthName} {item.year}</td>
                    <td>{item.reservationCount}</td>
                    <td className="font-medium">
                      {formatCurrency(item.totalRevenue, item.currencyCode)}
                    </td>
                    <td className="text-green-600">
                      {formatCurrency((item.totalRevenue * 10) / 100, item.currencyCode)}
                    </td>
                    <td className="font-bold text-blue-600">
                      {formatCurrency(item.netRevenue, item.currencyCode)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {/* Properties Tab */}
      {activeTab === 'properties' && (
        <Card>
          <div className="card-header">
            <h3 className="text-lg font-semibold">Mülk Bazlı Performans</h3>
          </div>
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Mülk</th>
                  <th>Rezervasyon</th>
                  <th>Gelir</th>
                  <th>Komisyon</th>
                  <th>İptal %</th>
                </tr>
              </thead>
              <tbody>
                {revenueData?.revenueByProperty?.map((property: any, index: number) => (
                  <tr key={index}>
                    <td className="font-medium">{property.propertyName}</td>
                    <td>{property.reservationCount}</td>
                    <td className="font-medium">
                      {formatCurrency(property.totalRevenue, property.currencyCode)}
                    </td>
                    <td className="text-green-600">
                      {formatCurrency((property.totalRevenue * 10) / 100, property.currencyCode)}
                    </td>
                    <td>
                      <span className="text-sm text-gray-500">%5</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  );
}