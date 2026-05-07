// src/pages/tenant/Dashboard.tsx
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../../api/dashboard.api';
import { clsx } from 'clsx';
import {
  TrendingUp,
  Users,
  BedDouble,
  DollarSign,
  Calendar,
  ArrowUp,
  ArrowDown,
} from 'lucide-react';
import { LineChart, BarChart } from '../../components/charts';
import { formatCurrency, formatDate } from '../../utils/format';

function DashboardSkeleton() {
  return <div className="p-6 text-sm text-gray-500">Yukleniyor...</div>;
}

export default function TenantDashboard() {
  const [dateRange, setDateRange] = useState({
    start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
    end: new Date(),
  });

  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['dashboard', dateRange],
    queryFn: () => dashboardApi.getDashboard(dateRange.start, dateRange.end),
  });

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  const stats = [
    {
      title: 'Bugün Check-in',
      value: dashboard?.todayCheckIns || 0,
      icon: Users,
      color: 'blue',
      change: '+12%',
      trend: 'up',
    },
    {
      title: 'Doluluk Oranı',
      value: `%${dashboard?.currentOccupancy || 0}`,
      icon: BedDouble,
      color: 'green',
      change: '+5%',
      trend: 'up',
    },
    {
      title: 'Aylık Gelir',
      value: formatCurrency(dashboard?.monthlyRevenue || 0, dashboard?.currencyCode || 'TRY'),
      icon: DollarSign,
      color: 'purple',
      change: '+18%',
      trend: 'up',
    },
    {
      title: 'Aktif Rezervasyon',
      value: dashboard?.activeReservations || 0,
      icon: Calendar,
      color: 'orange',
      change: '-3%',
      trend: 'down',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-sm text-gray-500 mt-1">
            İşletmenizin genel durumu
          </p>
        </div>
        <div className="flex items-center gap-3">
          <select
            className="border rounded-lg px-3 py-2 text-sm"
            onChange={(e) => {
              const days = parseInt(e.target.value);
              setDateRange({
                start: new Date(Date.now() - days * 24 * 60 * 60 * 1000),
                end: new Date(),
              });
            }}
          >
            <option value="7">Son 7 gün</option>
            <option value="30">Son 30 gün</option>
            <option value="90">Son 3 ay</option>
            <option value="365">Son 1 yıl</option>
          </select>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, index) => (
          <div key={index} className="bg-white rounded-xl border p-5 hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between mb-3">
              <div className={clsx(
                'p-2 rounded-lg',
                {
                  'bg-blue-50': stat.color === 'blue',
                  'bg-green-50': stat.color === 'green',
                  'bg-purple-50': stat.color === 'purple',
                  'bg-orange-50': stat.color === 'orange',
                }
              )}>
                <stat.icon className={clsx(
                  'w-5 h-5',
                  {
                    'text-blue-600': stat.color === 'blue',
                    'text-green-600': stat.color === 'green',
                    'text-purple-600': stat.color === 'purple',
                    'text-orange-600': stat.color === 'orange',
                  }
                )} />
              </div>
              <div className={clsx(
                'flex items-center gap-1 text-xs font-medium',
                stat.trend === 'up' ? 'text-green-600' : 'text-red-600'
              )}>
                {stat.trend === 'up' ? <ArrowUp className="w-3 h-3" /> : <ArrowDown className="w-3 h-3" />}
                {stat.change}
              </div>
            </div>
            <div className="text-2xl font-bold text-gray-900">{stat.value}</div>
            <div className="text-sm text-gray-500 mt-1">{stat.title}</div>
          </div>
        ))}
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl border p-6">
          <h3 className="text-lg font-semibold mb-4">Doluluk Grafiği</h3>
          <LineChart data={dashboard?.occupancyChart || []} dataKey="value" />
        </div>
        
        <div className="bg-white rounded-xl border p-6">
          <h3 className="text-lg font-semibold mb-4">Gelir Grafiği</h3>
          <BarChart data={dashboard?.revenueChart || []} dataKey="value" />
        </div>
      </div>

      {/* Recent Reservations */}
      <div className="bg-white rounded-xl border">
        <div className="px-6 py-4 border-b">
          <h3 className="text-lg font-semibold">Son Rezervasyonlar</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Rez. No</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Misafir</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mülk/Birim</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tarih</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tutar</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Durum</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {dashboard?.recentReservations?.map((reservation) => (
                <tr key={reservation.id} className="hover:bg-gray-50 cursor-pointer">
                  <td className="px-6 py-4 text-sm font-medium text-blue-600">
                    {reservation.reservationNumber}
                  </td>
                  <td className="px-6 py-4 text-sm">{reservation.guestName}</td>
                  <td className="px-6 py-4 text-sm">
                    <div>{reservation.propertyName}</div>
                    <div className="text-xs text-gray-500">{reservation.unitName}</div>
                  </td>
                  <td className="px-6 py-4 text-sm">
                    <div>{formatDate(reservation.checkIn)}</div>
                    <div className="text-xs text-gray-500">{formatDate(reservation.checkOut)}</div>
                  </td>
                  <td className="px-6 py-4 text-sm font-medium">
                    {formatCurrency(reservation.totalAmount, reservation.currencyCode)}
                  </td>
                  <td className="px-6 py-4">
                    <StatusBadge status={reservation.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  const statusConfig: Record<string, { color: string; label: string }> = {
    Pending: { color: 'bg-yellow-100 text-yellow-800', label: 'Beklemede' },
    Confirmed: { color: 'bg-green-100 text-green-800', label: 'Onaylandı' },
    CheckedIn: { color: 'bg-blue-100 text-blue-800', label: 'Giriş Yapıldı' },
    CheckedOut: { color: 'bg-gray-100 text-gray-800', label: 'Çıkış Yapıldı' },
    Cancelled: { color: 'bg-red-100 text-red-800', label: 'İptal' },
  };

  const config = statusConfig[status] || statusConfig.Pending;

  return (
    <span className={clsx('inline-flex px-2 py-1 text-xs font-medium rounded-full', config.color)}>
      {config.label}
    </span>
  );
}
