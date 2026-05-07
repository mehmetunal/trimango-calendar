import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Building2,
  Calendar,
  TrendingUp,
  DollarSign,
  Users,
  BedDouble,
  Star,
  Clock,
  AlertCircle,
  CheckCircle,
  XCircle,
  ArrowUp,
  ArrowDown,
  Search,
  Filter,
  Eye,
  Plus,
  ChevronRight,
} from 'lucide-react';
import { agencyApi } from '../../api/agency.api';
import { reservationApi } from '../../api/reservation.api';
import { reportApi } from '../../api/report.api';
import { Button, Card, Badge } from '../../components/ui';
import { LineChart, BarChart } from '../../components/charts';
import { formatCurrency, formatDate, formatDateTime } from '../../utils/format';
import { RESERVATION_STATUSES } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';

export default function AgencyDashboard() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  const [dateRange, setDateRange] = useState({
    start: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
    end: new Date(),
  });

  // Queries
  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['agency', 'dashboard', agencyId, dateRange],
    queryFn: () => reportApi.getAgencyDashboard(agencyId!, dateRange.start, dateRange.end),
    enabled: !!agencyId,
  });

  const { data: recentReservations } = useQuery({
    queryKey: ['agency', 'recent-reservations', agencyId],
    queryFn: () => reservationApi.getAgencyReservations(agencyId!, { page: 1, pageSize: 5 }),
    enabled: !!agencyId,
  });

  const { data: myProperties } = useQuery({
    queryKey: ['agency', 'properties', agencyId],
    queryFn: () => agencyApi.getMyProperties(agencyId!),
    enabled: !!agencyId,
  });

  // Stats
  const stats = useMemo(() => {
    if (!dashboard) return [];
    return [
      {
        title: 'Toplam Rezervasyon',
        value: dashboard.totalReservations || 0,
        icon: Calendar,
        color: 'blue',
        change: dashboard.reservationChange || 0,
        trend: (dashboard.reservationChange || 0) >= 0 ? 'up' : 'down',
      },
      {
        title: 'Toplam Gelir',
        value: formatCurrency(dashboard.totalRevenue || 0, dashboard.currencyCode || 'TRY'),
        icon: DollarSign,
        color: 'green',
        change: dashboard.revenueChange || 0,
        trend: (dashboard.revenueChange || 0) >= 0 ? 'up' : 'down',
      },
      {
        title: 'Aktif Rezervasyon',
        value: dashboard.activeReservations || 0,
        icon: CheckCircle,
        color: 'purple',
      },
      {
        title: 'Yetkili Mülk',
        value: myProperties?.length || 0,
        icon: Building2,
        color: 'orange',
      },
    ];
  }, [dashboard, myProperties]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Acente Paneli</h1>
          <p className="text-sm text-gray-500 mt-1">
            Hoş geldiniz, {user?.firstName} {user?.lastName}
          </p>
        </div>
        <Button
          onClick={() => navigate('/agency/reservations/new')}
          leftIcon={<Plus className="w-4 h-4" />}
        >
          Yeni Rezervasyon
        </Button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <Card key={index} className="p-5">
              <div className="flex items-center justify-between mb-3">
                <div className={`p-2 rounded-lg ${
                  stat.color === 'blue' ? 'bg-blue-50' :
                  stat.color === 'green' ? 'bg-green-50' :
                  stat.color === 'purple' ? 'bg-purple-50' :
                  'bg-orange-50'
                }`}>
                  <Icon className={`w-5 h-5 ${
                    stat.color === 'blue' ? 'text-blue-600' :
                    stat.color === 'green' ? 'text-green-600' :
                    stat.color === 'purple' ? 'text-purple-600' :
                    'text-orange-600'
                  }`} />
                </div>
                {stat.change !== undefined && (
                  <div className={`flex items-center gap-1 text-xs font-medium ${
                    stat.trend === 'up' ? 'text-green-600' : 'text-red-600'
                  }`}>
                    {stat.trend === 'up' ? <ArrowUp className="w-3 h-3" /> : <ArrowDown className="w-3 h-3" />}
                    %{Math.abs(stat.change).toFixed(1)}
                  </div>
                )}
              </div>
              <div className="text-2xl font-bold text-gray-900">{stat.value}</div>
              <div className="text-sm text-gray-500 mt-1">{stat.title}</div>
            </Card>
          );
        })}
      </div>

      {/* My Properties */}
      <Card>
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <h3 className="text-lg font-semibold">Yetkili Olduğum Mülkler</h3>
          <Button variant="ghost" size="sm" onClick={() => navigate('/agency/properties')}>
            Tümünü Gör <ChevronRight className="w-4 h-4 ml-1" />
          </Button>
        </div>
        <div className="p-4">
          {myProperties && myProperties.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {myProperties.slice(0, 6).map((property: any) => (
                <div
                  key={property.propertyId}
                  onClick={() => navigate(`/agency/properties/${property.propertyId}`)}
                  className="p-4 rounded-xl border hover:border-blue-300 hover:shadow-md cursor-pointer transition-all"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div>
                      <h4 className="font-semibold text-gray-900">{property.propertyName}</h4>
                      <p className="text-xs text-gray-500">{property.propertyType}</p>
                    </div>
                    <Badge color={property.isActive ? 'green' : 'red'}>
                      {property.isActive ? 'Aktif' : 'Pasif'}
                    </Badge>
                  </div>
                  
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    <div className="flex items-center gap-1 text-gray-500">
                      <BedDouble className="w-3.5 h-3.5" />
                      <span>{property.totalUnits} birim</span>
                    </div>
                    <div className="flex items-center gap-1 text-gray-500">
                      <Calendar className="w-3.5 h-3.5" />
                      <span>{property.activeReservations} rez.</span>
                    </div>
                  </div>
                  
                  {property.remainingAllotment !== null && (
                    <div className="mt-3 pt-3 border-t">
                      <div className="flex items-center justify-between text-xs">
                        <span className="text-gray-500">Kontenjan</span>
                        <span className={`font-medium ${
                          property.remainingAllotment > 0 ? 'text-green-600' : 'text-red-600'
                        }`}>
                          {property.remainingAllotment} kaldı
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-1.5 mt-1">
                        <div
                          className="bg-blue-500 h-1.5 rounded-full"
                          style={{
                            width: `${property.totalAllotment ? ((property.totalAllotment - property.remainingAllotment) / property.totalAllotment * 100) : 0}%`
                          }}
                        />
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-gray-500">
              <Building2 className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p>Henüz yetkili olduğunuz mülk bulunmamaktadır</p>
            </div>
          )}
        </div>
      </Card>

      {/* Charts & Recent Reservations */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Revenue Chart */}
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Gelir Grafiği</h3>
          {dashboard?.revenueChart ? (
            <LineChart data={dashboard.revenueChart} dataKey="value" height={250} />
          ) : (
            <div className="flex items-center justify-center h-64 text-gray-500">
              Veri bulunamadı
            </div>
          )}
        </Card>

        {/* Recent Reservations */}
        <Card>
          <div className="px-6 py-4 border-b flex items-center justify-between">
            <h3 className="text-lg font-semibold">Son Rezervasyonlar</h3>
            <Button variant="ghost" size="sm" onClick={() => navigate('/agency/reservations')}>
              Tümünü Gör <ChevronRight className="w-4 h-4 ml-1" />
            </Button>
          </div>
          <div className="divide-y">
            {recentReservations?.items?.map((reservation: any) => {
              const statusConfig = RESERVATION_STATUSES[reservation.status as keyof typeof RESERVATION_STATUSES];
              return (
                <div
                  key={reservation.id}
                  onClick={() => navigate(`/agency/reservations/${reservation.id}`)}
                  className="px-6 py-3 hover:bg-gray-50 cursor-pointer transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="text-sm font-medium text-blue-600">
                          {reservation.reservationNumber}
                        </span>
                        <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                          statusConfig?.bgColor
                        } ${statusConfig?.textColor}`}>
                          {statusConfig?.label}
                        </span>
                      </div>
                      <p className="text-sm text-gray-900 mt-0.5">{reservation.guestName}</p>
                      <p className="text-xs text-gray-500">
                        {reservation.propertyName} - {reservation.unitName}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium">
                        {formatCurrency(reservation.totalAmount, reservation.currencyCode)}
                      </p>
                      <p className="text-xs text-gray-500">
                        {formatDate(reservation.checkIn)} - {formatDate(reservation.checkOut)}
                      </p>
                    </div>
                  </div>
                </div>
              );
            })}
            {!recentReservations?.items?.length && (
              <div className="px-6 py-8 text-center text-gray-500">
                Henüz rezervasyon bulunmamaktadır
              </div>
            )}
          </div>
        </Card>
      </div>
    </div>
  );
}
AgencyMyProperties.tsx
