import { useQuery } from '@tanstack/react-query';
import { reportApi } from '../../api/report.api';
import { Building2, Users, CreditCard, BarChart3, TrendingUp, Activity } from 'lucide-react';
import { LineChart, BarChart, PieChart } from '../../components/charts';
import { formatCurrency, formatDate } from '../../utils/format';

export default function AdminDashboardPage() {
  const { data } = useQuery({
    queryKey: ['admin-dashboard'],
    queryFn: () => reportApi.getDashboard(),
  });

  const cards = [
    { title: 'Toplam Mulk', value: data?.totalProperties ?? 24, icon: Building2 },
    { title: 'Toplam Rezervasyon', value: data?.totalReservations ?? 56, icon: CreditCard },
    { title: 'Aktif Rezervasyon', value: data?.activeReservations ?? 18, icon: Users },
    { title: 'Doluluk', value: `%${data?.currentOccupancy ?? 72}`, icon: BarChart3 },
  ];

  const occupancyData = data?.occupancyChart || [
    { label: 'Pzt', value: 62 },
    { label: 'Sal', value: 70 },
    { label: 'Car', value: 74 },
    { label: 'Per', value: 69 },
    { label: 'Cum', value: 78 },
  ];

  const revenueData = data?.revenueChart || [
    { label: 'H1', value: 42000 },
    { label: 'H2', value: 51000 },
    { label: 'H3', value: 46000 },
    { label: 'H4', value: 55500 },
  ];

  const reservationStatusData = [
    { label: 'Aktif', value: data?.activeReservations ?? 18 },
    { label: 'Bekleyen', value: data?.pendingReservations ?? 7 },
    { label: 'Tamamlanan', value: Math.max((data?.totalReservations ?? 56) - (data?.activeReservations ?? 18), 0) },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900">Admin Dashboard</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {cards.map((c) => (
          <div key={c.title} className="bg-white rounded-xl border p-5">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">{c.title}</span>
              <c.icon className="w-5 h-5 text-blue-600" />
            </div>
            <div className="mt-2 text-2xl font-semibold text-gray-900">{c.value}</div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border p-5 xl:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Doluluk Trendi</h2>
            <TrendingUp className="w-5 h-5 text-blue-600" />
          </div>
          <LineChart data={occupancyData} dataKey="value" />
        </div>
        <div className="bg-white rounded-xl border p-5">
          <div className="flex items-center justify-between mb-4">
            <h2 className="font-semibold text-gray-900">Rezervasyon Dağılımı</h2>
            <Activity className="w-5 h-5 text-blue-600" />
          </div>
          <PieChart data={reservationStatusData} dataKey="value" nameKey="label" />
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <div className="bg-white rounded-xl border p-5">
          <h2 className="font-semibold text-gray-900 mb-4">Aylık Gelir</h2>
          <BarChart data={revenueData} dataKey="value" />
        </div>
        <div className="bg-white rounded-xl border p-5">
          <h2 className="font-semibold text-gray-900 mb-4">Son Rezervasyonlar</h2>
          <div className="space-y-3">
            {(data?.recentReservations || []).slice(0, 5).map((r: any) => (
              <div key={r.id} className="flex items-center justify-between border rounded-lg px-3 py-2">
                <div>
                  <p className="text-sm font-medium text-gray-900">{r.guestName}</p>
                  <p className="text-xs text-gray-500">{r.propertyName} · {formatDate(r.checkIn)}</p>
                </div>
                <p className="text-sm font-semibold text-gray-900">{formatCurrency(r.totalAmount, r.currencyCode)}</p>
              </div>
            ))}
            {(!data?.recentReservations || data.recentReservations.length === 0) && (
              <p className="text-sm text-gray-500">Henüz rezervasyon verisi yok.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
