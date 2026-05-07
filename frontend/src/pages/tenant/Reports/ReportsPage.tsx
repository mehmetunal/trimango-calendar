// src/pages/tenant/Reports/ReportsPage.tsx
import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Download,
  Calendar,
  TrendingUp,
  TrendingDown,
  DollarSign,
  Users,
  BedDouble,
  Building2,
  Star,
  Filter,
  RefreshCw,
  FileText,
  Printer,
  Mail,
  ChevronDown,
  ArrowUp,
  ArrowDown,
  BarChart3,
  PieChart,
  Activity,
  Target,
  Award,
  Clock,
  AlertCircle,
} from 'lucide-react';
import { reportApi } from '../../../api/report.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Card, Tabs } from '../../../components/ui';
import {
  LineChart,
  BarChart,
  AreaChart,
  PieChart as RePieChart,
  ResponsiveContainer,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  Line,
  Bar,
  Area,
  Pie,
  Cell,
} from 'recharts';
import { formatCurrency, formatDate, formatDateTime } from '../../../utils/format';
import { CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

// Types
interface ReportFilter {
  propertyId: string;
  startDate: string;
  endDate: string;
  currencyCode: string;
  groupBy: 'day' | 'week' | 'month' | 'year';
  compareWithPrevious: boolean;
}

interface OccupancyData {
  date: string;
  totalUnits: number;
  reservedUnits: number;
  occupancyRate: number;
  availableUnits: number;
  blockedUnits: number;
}

interface RevenueData {
  period: string;
  totalRevenue: number;
  reservationCount: number;
  averagePerReservation: number;
  currencyCode: string;
  taxAmount: number;
  serviceFee: number;
  discountAmount: number;
  netRevenue: number;
}

interface PropertyPerformance {
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

interface AgencyPerformance {
  agencyId: string;
  agencyName: string;
  reservationCount: number;
  totalRevenue: number;
  commissionAmount: number;
  cancellationRate: number;
  averageStayDuration: number;
}

interface GuestStats {
  totalGuests: number;
  newGuests: number;
  returningGuests: number;
  averageStayDuration: number;
  topNationalities: { nationality: string; count: number }[];
  averageRating: number;
  totalReviews: number;
}

const COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#06B6D4', '#84CC16'];

export default function ReportsPage() {
  // States
  const [activeTab, setActiveTab] = useState<'occupancy' | 'revenue' | 'performance' | 'agency' | 'guests'>('occupancy');
  const [filter, setFilter] = useState<ReportFilter>({
    propertyId: '',
    startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
    currencyCode: 'TRY',
    groupBy: 'day',
    compareWithPrevious: false,
  });
  const [selectedProperty, setSelectedProperty] = useState<string>('');

  // Queries
  const { data: properties } = useQuery({
    queryKey: ['properties', 'all'],
    queryFn: () => propertyApi.getAll({ pageSize: 1000, isActive: true }),
  });

  const { data: occupancyData, isLoading: occupancyLoading } = useQuery({
    queryKey: ['report', 'occupancy', filter],
    queryFn: () => reportApi.getOccupancyReport(filter),
    enabled: activeTab === 'occupancy',
  });

  const { data: revenueData, isLoading: revenueLoading } = useQuery({
    queryKey: ['report', 'revenue', filter],
    queryFn: () => reportApi.getRevenueReport(filter),
    enabled: activeTab === 'revenue',
  });

  const { data: performanceData, isLoading: performanceLoading } = useQuery({
    queryKey: ['report', 'performance', filter],
    queryFn: () => reportApi.getPropertyPerformance(filter),
    enabled: activeTab === 'performance',
  });

  const { data: agencyData, isLoading: agencyLoading } = useQuery({
    queryKey: ['report', 'agency', filter],
    queryFn: () => reportApi.getAgencyPerformance(filter),
    enabled: activeTab === 'agency',
  });

  const { data: guestData, isLoading: guestLoading } = useQuery({
    queryKey: ['report', 'guests', filter],
    queryFn: () => reportApi.getGuestStats(filter),
    enabled: activeTab === 'guests',
  });

  // Export handler
  const handleExport = async (format: 'excel' | 'pdf') => {
    try {
      const blob = await reportApi.exportReport({
        ...filter,
        type: activeTab,
        format,
      });
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${activeTab}_report_${filter.startDate}_${filter.endDate}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      a.click();
      window.URL.revokeObjectURL(url);
      
      toast.success('Rapor indirildi');
    } catch (error: any) {
      toast.error('Rapor indirilemedi');
    }
  };

  // Calculate summary stats
  const occupancySummary = useMemo(() => {
    if (!occupancyData?.dailyOccupancy) return null;
    const data = occupancyData.dailyOccupancy;
    return {
      average: (data.reduce((sum, d) => sum + d.occupancyRate, 0) / data.length).toFixed(1),
      peak: Math.max(...data.map(d => d.occupancyRate)).toFixed(1),
      lowest: Math.min(...data.map(d => d.occupancyRate)).toFixed(1),
      peakDate: data.find(d => d.occupancyRate === Math.max(...data.map(d => d.occupancyRate)))?.date,
      lowestDate: data.find(d => d.occupancyRate === Math.min(...data.map(d => d.occupancyRate)))?.date,
      totalRoomNights: data.reduce((sum, d) => sum + d.reservedUnits, 0),
    };
  }, [occupancyData]);

  const revenueSummary = useMemo(() => {
    if (!revenueData?.monthlyRevenue) return null;
    const data = revenueData.monthlyRevenue;
    return {
      total: revenueData.totalRevenue,
      average: revenueData.averageRevenuePerDay,
      highest: Math.max(...data.map(d => d.totalRevenue)),
      lowest: Math.min(...data.map(d => d.totalRevenue)),
      taxTotal: revenueData.totalTax,
      serviceFeeTotal: revenueData.totalServiceFee,
      netTotal: revenueData.totalRevenue - revenueData.totalTax - revenueData.totalServiceFee,
    };
  }, [revenueData]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Raporlar</h1>
          <p className="text-sm text-gray-500 mt-1">
            Detaylı raporlar ve analizler
          </p>
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
          <Button
            variant="outline"
            size="sm"
            leftIcon={<Printer className="w-4 h-4" />}
          >
            Yazdır
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
          <Select
            label="Mülk"
            value={filter.propertyId}
            onChange={(e) => setFilter({ ...filter, propertyId: e.target.value })}
            options={[
              { value: '', label: 'Tüm Mülkler' },
              ...(properties?.items?.map((p: any) => ({
                value: p.id,
                label: p.name,
              })) || []),
            ]}
          />
          
          <Input
            label="Başlangıç"
            type="date"
            value={filter.startDate}
            onChange={(e) => setFilter({ ...filter, startDate: e.target.value })}
          />
          
          <Input
            label="Bitiş"
            type="date"
            value={filter.endDate}
            onChange={(e) => setFilter({ ...filter, endDate: e.target.value })}
          />
          
          <Select
            label="Grupla"
            value={filter.groupBy}
            onChange={(e) => setFilter({ ...filter, groupBy: e.target.value as any })}
            options={[
              { value: 'day', label: 'Günlük' },
              { value: 'week', label: 'Haftalık' },
              { value: 'month', label: 'Aylık' },
              { value: 'year', label: 'Yıllık' },
            ]}
          />
          
          <Select
            label="Para Birimi"
            value={filter.currencyCode}
            onChange={(e) => setFilter({ ...filter, currencyCode: e.target.value })}
            options={CURRENCIES.map(c => ({ value: c.code, label: `${c.symbol} ${c.name}` }))}
          />
        </div>
        
        <div className="flex items-center justify-between mt-3 pt-3 border-t">
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={filter.compareWithPrevious}
              onChange={(e) => setFilter({ ...filter, compareWithPrevious: e.target.checked })}
              className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <span className="text-sm text-gray-600">Önceki dönemle karşılaştır</span>
          </label>
          
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setFilter({
              ...filter,
              propertyId: '',
              startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0],
              endDate: new Date().toISOString().split('T')[0],
              groupBy: 'day',
              compareWithPrevious: false,
            })}
            leftIcon={<RefreshCw className="w-4 h-4" />}
          >
            Sıfırla
          </Button>
        </div>
      </Card>

      {/* Tabs */}
      <div className="border-b">
        <div className="flex gap-1 -mb-px overflow-x-auto">
          {[
            { key: 'occupancy', label: 'Doluluk', icon: BedDouble },
            { key: 'revenue', label: 'Gelir', icon: DollarSign },
            { key: 'performance', label: 'Mülk Performansı', icon: Building2 },
            { key: 'agency', label: 'Acente Performansı', icon: Users },
            { key: 'guests', label: 'Misafir Analizi', icon: Star },
          ].map((tab) => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key as any)}
                className={`flex items-center gap-2 px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap ${
                  activeTab === tab.key
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <Icon className="w-4 h-4" />
                {tab.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* Tab Content */}
      <div className="space-y-6">
        {/* OCCUPANCY REPORT */}
        {activeTab === 'occupancy' && (
          <>
            {/* Summary Cards */}
            {occupancySummary && (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <Card className="p-4">
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <Target className="w-4 h-4" />
                    Ortalama Doluluk
                  </div>
                  <div className="text-2xl font-bold text-blue-600">%{occupancySummary.average}</div>
                </Card>
                <Card className="p-4">
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <TrendingUp className="w-4 h-4 text-green-500" />
                    En Yüksek Doluluk
                  </div>
                  <div className="text-2xl font-bold text-green-600">%{occupancySummary.peak}</div>
                  <div className="text-xs text-gray-500 mt-1">
                    {occupancySummary.peakDate && formatDate(occupancySummary.peakDate)}
                  </div>
                </Card>
                <Card className="p-4">
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <TrendingDown className="w-4 h-4 text-red-500" />
                    En Düşük Doluluk
                  </div>
                  <div className="text-2xl font-bold text-red-600">%{occupancySummary.lowest}</div>
                  <div className="text-xs text-gray-500 mt-1">
                    {occupancySummary.lowestDate && formatDate(occupancySummary.lowestDate)}
                  </div>
                </Card>
                <Card className="p-4">
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <BedDouble className="w-4 h-4" />
                    Toplam Oda-Gece
                  </div>
                  <div className="text-2xl font-bold text-purple-600">{occupancySummary.totalRoomNights}</div>
                </Card>
              </div>
            )}

            {/* Occupancy Chart */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Doluluk Grafiği</h3>
              {occupancyLoading ? (
                <div className="h-80 flex items-center justify-center">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
                </div>
              ) : occupancyData?.dailyOccupancy ? (
                <ResponsiveContainer width="100%" height={400}>
                  <AreaChart data={occupancyData.dailyOccupancy}>
                    <defs>
                      <linearGradient id="occupancyGradient" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#3B82F6" stopOpacity={0.3} />
                        <stop offset="95%" stopColor="#3B82F6" stopOpacity={0} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                    <XAxis
                      dataKey="date"
                      tickFormatter={(date) => new Date(date).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })}
                      stroke="#9CA3AF"
                      fontSize={12}
                    />
                    <YAxis
                      tickFormatter={(value) => `%${value}`}
                      stroke="#9CA3AF"
                      fontSize={12}
                    />
                    <Tooltip
                      formatter={(value: number) => [`%${value}`, 'Doluluk']}
                      labelFormatter={(date) => formatDate(date)}
                    />
                    <Area
                      type="monotone"
                      dataKey="occupancyRate"
                      stroke="#3B82F6"
                      strokeWidth={2}
                      fill="url(#occupancyGradient)"
                      name="Doluluk Oranı"
                    />
                  </AreaChart>
                </ResponsiveContainer>
              ) : (
                <div className="text-center py-10 text-gray-500">Veri bulunamadı</div>
              )}
            </Card>

            {/* Daily Occupancy Table */}
            <Card>
              <div className="px-6 py-4 border-b">
                <h3 className="text-lg font-semibold">Günlük Doluluk Detayı</h3>
              </div>
              <div className="overflow-x-auto max-h-96 overflow-y-auto">
                <table className="w-full">
                  <thead className="bg-gray-50 sticky top-0">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tarih</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Toplam Birim</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Dolu</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Müsait</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Bloke</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Doluluk %</th>
                      <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Durum</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {occupancyData?.dailyOccupancy?.map((day: OccupancyData, index: number) => (
                      <tr key={index} className="hover:bg-gray-50">
                        <td className="px-4 py-3 text-sm font-medium">{formatDate(day.date)}</td>
                        <td className="px-4 py-3 text-sm text-center">{day.totalUnits}</td>
                        <td className="px-4 py-3 text-sm text-center">{day.reservedUnits}</td>
                        <td className="px-4 py-3 text-sm text-center">{day.availableUnits || day.totalUnits - day.reservedUnits - (day.blockedUnits || 0)}</td>
                        <td className="px-4 py-3 text-sm text-center">{day.blockedUnits || 0}</td>
                        <td className="px-4 py-3 text-sm text-center font-medium">
                          <span className={`inline-flex items-center gap-1 ${
                            day.occupancyRate >= 80 ? 'text-green-600' :
                            day.occupancyRate >= 50 ? 'text-blue-600' :
                            day.occupancyRate >= 30 ? 'text-yellow-600' :
                            'text-red-600'
                          }`}>
                            %{day.occupancyRate.toFixed(1)}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-center">
                          <div className="w-full bg-gray-200 rounded-full h-2 max-w-[100px] mx-auto">
                            <div
                              className={`h-2 rounded-full ${
                                day.occupancyRate >= 80 ? 'bg-green-500' :
                                day.occupancyRate >= 50 ? 'bg-blue-500' :
                                day.occupancyRate >= 30 ? 'bg-yellow-500' :
                                'bg-red-500'
                              }`}
                              style={{ width: `${day.occupancyRate}%` }}
                            />
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          </>
        )}

        {/* REVENUE REPORT */}
        {activeTab === 'revenue' && (
          <>
            {/* Revenue Summary */}
            {revenueSummary && (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <Card className="p-4 bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
                  <div className="text-sm text-blue-600 mb-1">Toplam Gelir</div>
                  <div className="text-2xl font-bold text-blue-900">
                    {formatCurrency(revenueSummary.total, filter.currencyCode)}
                  </div>
                </Card>
                <Card className="p-4 bg-gradient-to-br from-green-50 to-green-100 border-green-200">
                  <div className="text-sm text-green-600 mb-1">Net Gelir</div>
                  <div className="text-2xl font-bold text-green-900">
                    {formatCurrency(revenueSummary.netTotal, filter.currencyCode)}
                  </div>
                </Card>
                <Card className="p-4 bg-gradient-to-br from-purple-50 to-purple-100 border-purple-200">
                  <div className="text-sm text-purple-600 mb-1">Günlük Ortalama</div>
                  <div className="text-2xl font-bold text-purple-900">
                    {formatCurrency(revenueSummary.average, filter.currencyCode)}
                  </div>
                </Card>
                <Card className="p-4 bg-gradient-to-br from-orange-50 to-orange-100 border-orange-200">
                  <div className="text-sm text-orange-600 mb-1">Vergi + Servis</div>
                  <div className="text-2xl font-bold text-orange-900">
                    {formatCurrency(revenueSummary.taxTotal + revenueSummary.serviceFeeTotal, filter.currencyCode)}
                  </div>
                </Card>
              </div>
            )}

            {/* Revenue Chart */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Gelir Grafiği</h3>
              {revenueLoading ? (
                <div className="h-80 flex items-center justify-center">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
                </div>
              ) : revenueData?.monthlyRevenue ? (
                <ResponsiveContainer width="100%" height={400}>
                  <BarChart data={revenueData.monthlyRevenue}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                    <XAxis
                      dataKey="monthName"
                      stroke="#9CA3AF"
                      fontSize={12}
                    />
                    <YAxis
                      tickFormatter={(value) => formatCurrency(value, filter.currencyCode)}
                      stroke="#9CA3AF"
                      fontSize={12}
                    />
                    <Tooltip
                      formatter={(value: number) => [formatCurrency(value, filter.currencyCode), 'Gelir']}
                    />
                    <Legend />
                    <Bar dataKey="totalRevenue" name="Toplam Gelir" fill="#3B82F6" radius={[4, 4, 0, 0]} />
                    <Bar dataKey="reservationCount" name="Rezervasyon Sayısı" fill="#10B981" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <div className="text-center py-10 text-gray-500">Veri bulunamadı</div>
              )}
            </Card>

            {/* Revenue by Property */}
            {revenueData?.revenueByProperty && revenueData.revenueByProperty.length > 0 && (
              <Card className="p-6">
                <h3 className="text-lg font-semibold mb-4">Mülk Bazında Gelir Dağılımı</h3>
                <ResponsiveContainer width="100%" height={300}>
                  <RePieChart>
                    <Pie
                      data={revenueData.revenueByProperty}
                      dataKey="totalRevenue"
                      nameKey="propertyName"
                      cx="50%"
                      cy="50%"
                      outerRadius={100}
                      label={({ propertyName, totalRevenue }) => 
                        `${propertyName}: ${formatCurrency(totalRevenue, filter.currencyCode)}`
                      }
                    >
                      {revenueData.revenueByProperty.map((entry: any, index: number) => (
                        <Cell key={index} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      formatter={(value: number) => [formatCurrency(value, filter.currencyCode), 'Gelir']}
                    />
                  </RePieChart>
                </ResponsiveContainer>
              </Card>
            )}

            {/* Revenue Table */}
            <Card>
              <div className="px-6 py-4 border-b">
                <h3 className="text-lg font-semibold">Gelir Detayı</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Dönem</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Rezervasyon</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Brüt Gelir</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Vergi</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Servis</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">İndirim</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Net Gelir</th>
                      <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ortalama</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {revenueData?.monthlyRevenue?.map((item: RevenueData, index: number) => (
                      <tr key={index} className="hover:bg-gray-50">
                        <td className="px-4 py-3 text-sm font-medium">{item.period}</td>
                        <td className="px-4 py-3 text-sm text-right">{item.reservationCount}</td>
                        <td className="px-4 py-3 text-sm text-right font-medium">
                          {formatCurrency(item.totalRevenue, item.currencyCode)}
                        </td>
                        <td className="px-4 py-3 text-sm text-right text-red-600">
                          -{formatCurrency(item.taxAmount, item.currencyCode)}
                        </td>
                        <td className="px-4 py-3 text-sm text-right text-red-600">
                          -{formatCurrency(item.serviceFee, item.currencyCode)}
                        </td>
                        <td className="px-4 py-3 text-sm text-right text-red-600">
                          -{formatCurrency(item.discountAmount, item.currencyCode)}
                        </td>
                        <td className="px-4 py-3 text-sm text-right font-bold text-green-600">
                          {formatCurrency(item.netRevenue, item.currencyCode)}
                        </td>
                        <td className="px-4 py-3 text-sm text-right">
                          {formatCurrency(item.averagePerReservation, item.currencyCode)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Card>
          </>
        )}

        {/* PROPERTY PERFORMANCE */}
        {activeTab === 'performance' && (
          <Card>
            <div className="px-6 py-4 border-b">
              <h3 className="text-lg font-semibold">Mülk Performans Karşılaştırması</h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mülk</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Tip</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Rezervasyon</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Gelir</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Doluluk</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Puan</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">İptal %</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Birim Başı Gelir</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {performanceData?.map((property: PropertyPerformance, index: number) => (
                    <tr key={index} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm font-medium">{property.propertyName}</td>
                      <td className="px-4 py-3 text-sm text-center">{property.propertyType}</td>
                      <td className="px-4 py-3 text-sm text-right">{property.reservationCount}</td>
                      <td className="px-4 py-3 text-sm text-right font-medium">
                        {formatCurrency(property.totalRevenue, filter.currencyCode)}
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex items-center justify-center gap-2">
                          <div className="w-20 bg-gray-200 rounded-full h-2">
                            <div
                              className="bg-blue-500 h-2 rounded-full"
                              style={{ width: `${property.occupancyRate}%` }}
                            />
                          </div>
                          <span className="text-sm">%{property.occupancyRate.toFixed(1)}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex items-center justify-center gap-1">
                          <Star className="w-4 h-4 text-yellow-400 fill-current" />
                          <span className="text-sm">{property.averageRating.toFixed(1)}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <span className={`text-sm font-medium ${
                          property.cancellationRate > 20 ? 'text-red-600' :
                          property.cancellationRate > 10 ? 'text-yellow-600' :
                          'text-green-600'
                        }`}>
                          %{property.cancellationRate.toFixed(1)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-sm text-right">
                        {formatCurrency(property.revenuePerUnit, filter.currencyCode)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        )}

        {/* AGENCY PERFORMANCE */}
        {activeTab === 'agency' && (
          <Card>
            <div className="px-6 py-4 border-b">
              <h3 className="text-lg font-semibold">Acente Performansı</h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Acente</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Rezervasyon</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Toplam Gelir</th>
                    <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Komisyon</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Ort. Kalış</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">İptal %</th>
                    <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Performans</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {agencyData?.map((agency: AgencyPerformance, index: number) => (
                    <tr key={index} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-sm font-medium">{agency.agencyName}</td>
                      <td className="px-4 py-3 text-sm text-right">{agency.reservationCount}</td>
                      <td className="px-4 py-3 text-sm text-right font-medium">
                        {formatCurrency(agency.totalRevenue, filter.currencyCode)}
                      </td>
                      <td className="px-4 py-3 text-sm text-right">
                        {formatCurrency(agency.commissionAmount, filter.currencyCode)}
                      </td>
                      <td className="px-4 py-3 text-sm text-center">{agency.averageStayDuration.toFixed(1)} gün</td>
                      <td className="px-4 py-3 text-center">
                        <span className={`text-sm ${
                          agency.cancellationRate > 20 ? 'text-red-600' : 'text-green-600'
                        }`}>
                          %{agency.cancellationRate.toFixed(1)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex items-center justify-center gap-1">
                          {[...Array(5)].map((_, i) => (
                            <Star
                              key={i}
                              className={`w-4 h-4 ${
                                i < (agency.reservationCount > 10 ? 4 : agency.reservationCount > 5 ? 3 : 2)
                                  ? 'text-yellow-400 fill-current'
                                  : 'text-gray-300'
                              }`}
                            />
                          ))}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        )}

        {/* GUEST ANALYTICS */}
        {activeTab === 'guests' && guestData && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Guest Stats */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Misafir İstatistikleri</h3>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Toplam Misafir</span>
                  <span className="text-lg font-bold">{guestData.totalGuests}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Yeni Misafir</span>
                  <span className="text-lg font-bold text-blue-600">{guestData.newGuests}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Tekrar Gelen</span>
                  <span className="text-lg font-bold text-green-600">{guestData.returningGuests}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Ortalama Kalış</span>
                  <span className="text-lg font-bold">{guestData.averageStayDuration.toFixed(1)} gün</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Ortalama Puan</span>
                  <div className="flex items-center gap-1">
                    <Star className="w-5 h-5 text-yellow-400 fill-current" />
                    <span className="text-lg font-bold">{guestData.averageRating.toFixed(1)}</span>
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">Toplam Değerlendirme</span>
                  <span className="text-lg font-bold">{guestData.totalReviews}</span>
                </div>
              </div>
            </Card>

            {/* Nationality Distribution */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Milliyet Dağılımı</h3>
              {guestData.topNationalities && guestData.topNationalities.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <RePieChart>
                    <Pie
                      data={guestData.topNationalities}
                      dataKey="count"
                      nameKey="nationality"
                      cx="50%"
                      cy="50%"
                      outerRadius={100}
                      label={({ nationality, count }) => `${nationality}: ${count}`}
                    >
                      {guestData.topNationalities.map((entry: any, index: number) => (
                        <Cell key={index} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </RePieChart>
                </ResponsiveContainer>
              ) : (
                <div className="text-center py-10 text-gray-500">Veri bulunamadı</div>
              )}
            </Card>
          </div>
        )}
      </div>
    </div>
  );
}