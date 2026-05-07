import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Building2,
  MapPin,
  BedDouble,
  DollarSign,
  Calendar,
  Star,
  Eye,
  ChevronRight,
  Filter,
  Grid,
  List,
  AlertCircle,
  CheckCircle,
  TrendingUp,
  Users,
} from 'lucide-react';
import { agencyApi } from '../../api/agency.api';
import { Button, Input, Select, Card, Badge, Pagination } from '../../components/ui';
import { useDebounce } from '../../hooks/useDebounce';
import { formatCurrency } from '../../utils/format';
import { PROPERTY_TYPES } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';

export default function AgencyMyProperties() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  // States
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  const [page, setPage] = useState(1);
  const debouncedSearch = useDebounce(search);

  const { data: properties, isLoading } = useQuery({
    queryKey: ['agency', 'properties', agencyId, { page, search: debouncedSearch, type: typeFilter }],
    queryFn: () => agencyApi.getMyProperties(agencyId!, {
      page,
      pageSize: 12,
      search: debouncedSearch || undefined,
      type: typeFilter || undefined,
    }),
    enabled: !!agencyId,
  });

  const PropertyCard = ({ property }: { property: any }) => (
    <Card
      className="hover:shadow-lg transition-all cursor-pointer group"
      onClick={() => navigate(`/agency/properties/${property.propertyId}`)}
    >
      <div className="relative h-40 bg-gradient-to-br from-blue-50 to-blue-100 rounded-t-xl flex items-center justify-center">
        <Building2 className="w-16 h-16 text-blue-300 group-hover:scale-110 transition-transform" />
        
        {/* Badges */}
        <div className="absolute top-3 left-3 flex gap-2">
          <span className="px-2 py-1 text-xs font-medium bg-white/90 backdrop-blur-sm rounded-full shadow-sm">
            {PROPERTY_TYPES.find(t => t.value === property.propertyType)?.icon}
            {' '}
            {PROPERTY_TYPES.find(t => t.value === property.propertyType)?.label}
          </span>
        </div>

        {/* Commission Badge */}
        <div className="absolute top-3 right-3">
          <span className="px-2 py-1 text-xs font-medium bg-green-100 text-green-700 rounded-full">
            %{property.commissionRate} Komisyon
          </span>
        </div>
      </div>

      <div className="p-4">
        <h3 className="font-semibold text-gray-900 mb-1">{property.propertyName}</h3>
        
        <div className="flex items-center gap-1 text-sm text-gray-500 mb-3">
          <MapPin className="w-3.5 h-3.5" />
          <span>{property.city}</span>
        </div>

        <div className="grid grid-cols-2 gap-2 text-sm mb-3">
          <div className="flex items-center gap-1 text-gray-500">
            <BedDouble className="w-3.5 h-3.5" />
            <span>{property.totalUnits} birim</span>
          </div>
          <div className="flex items-center gap-1 text-gray-500">
            <Calendar className="w-3.5 h-3.5" />
            <span>{property.activeReservations} rez.</span>
          </div>
          {property.canSetPrices && (
            <div className="flex items-center gap-1 text-green-600">
              <DollarSign className="w-3.5 h-3.5" />
              <span>Fiyat yetkisi</span>
            </div>
          )}
          {property.canCreateReservation && (
            <div className="flex items-center gap-1 text-blue-600">
              <CheckCircle className="w-3.5 h-3.5" />
              <span>Rez. yetkisi</span>
            </div>
          )}
        </div>

        {property.remainingAllotment !== null && (
          <div className="pt-3 border-t">
            <div className="flex items-center justify-between text-xs mb-1">
              <span className="text-gray-500">Kontenjan</span>
              <span className={`font-medium ${
                property.remainingAllotment > 0 ? 'text-green-600' : 'text-red-600'
              }`}>
                {property.remainingAllotment} kaldı
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-1.5">
              <div
                className={`h-1.5 rounded-full ${
                  property.remainingAllotment > 0 ? 'bg-blue-500' : 'bg-red-500'
                }`}
                style={{
                  width: `${property.totalAllotment ? ((property.totalAllotment - property.remainingAllotment) / property.totalAllotment * 100) : 0}%`
                }}
              />
            </div>
          </div>
        )}

        <Button
          className="w-full mt-3"
          size="sm"
          onClick={(e) => {
            e.stopPropagation();
            navigate(`/agency/calendar/${property.propertyId}`);
          }}
        >
          Takvimi Görüntüle
        </Button>
      </div>
    </Card>
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mülklerim</h1>
          <p className="text-sm text-gray-500 mt-1">
            Yetkili olduğunuz tüm mülkler
          </p>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 rounded-md ${viewMode === 'grid' ? 'bg-white shadow-sm' : 'text-gray-500'}`}
            >
              <Grid className="w-4 h-4" />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={`p-2 rounded-md ${viewMode === 'list' ? 'bg-white shadow-sm' : 'text-gray-500'}`}
            >
              <List className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl border p-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Mülk ara..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg text-sm"
            />
          </div>
          <Select
            value={typeFilter}
            onChange={(e) => setTypeFilter(e.target.value)}
            options={[
              { value: '', label: 'Tüm Tipler' },
              ...PROPERTY_TYPES.map(t => ({ value: t.value, label: `${t.icon} ${t.label}` })),
            ]}
          />
        </div>
      </div>

      {/* Properties */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[...Array(6)].map((_, i) => (
            <Card key={i} className="animate-pulse">
              <div className="h-40 bg-gray-200 rounded-t-xl" />
              <div className="p-4 space-y-3">
                <div className="h-4 bg-gray-200 rounded w-3/4" />
                <div className="h-3 bg-gray-200 rounded w-1/2" />
              </div>
            </Card>
          ))}
        </div>
      ) : viewMode === 'grid' ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {properties?.items?.map((property: any) => (
            <PropertyCard key={property.propertyId} property={property} />
          ))}
        </div>
      ) : (
        <Card className="overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mülk</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tip</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Birim</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Rez.</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Komisyon</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {properties?.items?.map((property: any) => (
                <tr
                  key={property.propertyId}
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => navigate(`/agency/properties/${property.propertyId}`)}
                >
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <Building2 className="w-8 h-8 text-blue-500" />
                      <div>
                        <div className="font-medium">{property.propertyName}</div>
                        <div className="text-xs text-gray-500">{property.city}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm">
                    {PROPERTY_TYPES.find(t => t.value === property.propertyType)?.label}
                  </td>
                  <td className="px-4 py-3 text-center text-sm">{property.totalUnits}</td>
                  <td className="px-4 py-3 text-center text-sm">{property.activeReservations}</td>
                  <td className="px-4 py-3 text-center">
                    <span className="text-sm font-medium text-green-600">%{property.commissionRate}</span>
                  </td>
                  <td className="px-4 py-3 text-center">
                    <Button size="sm" variant="ghost">
                      <Eye className="w-4 h-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      {!isLoading && properties?.items?.length === 0 && (
        <div className="text-center py-16">
          <Building2 className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz yetkili mülk yok</h3>
          <p className="text-sm text-gray-500">
            Mülk sahipleri tarafından yetkilendirildiğiniz mülkler burada görünecektir
          </p>
        </div>
      )}

      {properties && properties.totalPages > 1 && (
        <Pagination
          currentPage={properties.page}
          totalPages={properties.totalPages}
          onPageChange={setPage}
          totalCount={properties.totalCount}
        />
      )}
    </div>
  );
}
AgencyCalendar.tsx
