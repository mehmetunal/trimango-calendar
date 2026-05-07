import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Plus,
  Search,
  Filter,
  MoreVertical,
  Edit,
  Trash2,
  Eye,
  Power,
  Building2,
  MapPin,
  Star,
  BedDouble,
  DollarSign,
  Grid,
  List,
  ChevronDown,
} from 'lucide-react';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Card, Badge, Pagination, Modal, ConfirmDialog } from '../../../components/ui';
import { useDebounce } from '../../../hooks/useDebounce';
import { usePropertyFilter } from '../../../hooks/usePropertyFilter';
import { formatCurrency } from '../../../utils/format';
import { PROPERTY_TYPES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function PropertyList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  // View mode
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');
  
  // Filters
  const { filters, debouncedSearch, setFilter, toggleAmenity, clearFilters, hasActiveFilters } = usePropertyFilter();
  
  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(12);
  
  // Delete confirmation
  const [deleteProperty, setDeleteProperty] = useState<any>(null);
  
  // Sort
  const [sortBy, setSortBy] = useState('createdAt');
  const [sortDesc, setSortDesc] = useState(true);

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    type: filters.type || undefined,
    city: filters.city || undefined,
    search: debouncedSearch || undefined,
    sortBy,
    sortDescending: sortDesc,
  }), [page, pageSize, filters.type, filters.city, debouncedSearch, sortBy, sortDesc]);

  const { data, isLoading } = useQuery({
    queryKey: ['properties', queryParams],
    queryFn: () => propertyApi.getAll(queryParams),
    keepPreviousData: true,
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: string) => propertyApi.delete(id),
    onSuccess: () => {
      toast.success('Mülk başarıyla silindi');
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      setDeleteProperty(null);
    },
    onError: (error: any) => {
      toast.error(error.message || 'Silme işlemi başarısız');
    },
  });

  // Toggle active mutation
  const toggleMutation = useMutation({
    mutationFn: (id: string) => propertyApi.toggleActive(id),
    onSuccess: () => {
      toast.success('Mülk durumu güncellendi');
      queryClient.invalidateQueries({ queryKey: ['properties'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const PropertyCard = ({ property }: { property: any }) => (
    <div className="bg-white rounded-xl border hover:shadow-lg transition-all duration-300 group cursor-pointer"
         onClick={() => navigate(`/dashboard/properties/${property.id}`)}>
      {/* Property Image */}
      <div className="relative h-48 rounded-t-xl overflow-hidden bg-gray-200">
        {property.coverImageUrl ? (
          <img
            src={property.coverImageUrl}
            alt={property.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <div className="flex items-center justify-center h-full text-gray-400">
            <Building2 className="w-16 h-16" />
          </div>
        )}
        
        {/* Badges */}
        <div className="absolute top-3 left-3 flex gap-2">
          <span className="px-2.5 py-1 text-xs font-medium bg-white/90 backdrop-blur-sm rounded-full shadow-sm">
            {PROPERTY_TYPES.find(t => t.value === property.type)?.icon}
            {' '}
            {PROPERTY_TYPES.find(t => t.value === property.type)?.label}
          </span>
          {!property.isActive && (
            <span className="px-2.5 py-1 text-xs font-medium bg-red-100 text-red-700 rounded-full">
              Pasif
            </span>
          )}
        </div>

        {/* Quick Actions */}
        <div className="absolute top-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity">
          <div className="flex gap-1">
            <button
              onClick={(e) => {
                e.stopPropagation();
                navigate(`/dashboard/properties/${property.id}/edit`);
              }}
              className="p-2 bg-white rounded-lg shadow-sm hover:bg-blue-50 text-gray-600 hover:text-blue-600"
            >
              <Edit className="w-4 h-4" />
            </button>
            <button
              onClick={(e) => {
                e.stopPropagation();
                setDeleteProperty(property);
              }}
              className="p-2 bg-white rounded-lg shadow-sm hover:bg-red-50 text-gray-600 hover:text-red-600"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      {/* Property Info */}
      <div className="p-4">
        <div className="flex items-start justify-between mb-2">
          <h3 className="font-semibold text-gray-900 line-clamp-1">{property.name}</h3>
          {property.averageRating > 0 && (
            <div className="flex items-center gap-1 text-sm text-yellow-500">
              <Star className="w-4 h-4 fill-current" />
              <span>{property.averageRating.toFixed(1)}</span>
            </div>
          )}
        </div>

        <div className="flex items-center gap-1 text-sm text-gray-500 mb-3">
          <MapPin className="w-3.5 h-3.5" />
          <span className="line-clamp-1">{property.city}{property.district ? `, ${property.district}` : ''}</span>
        </div>

        <div className="flex items-center justify-between pt-3 border-t">
          <div>
            <span className="text-lg font-bold text-blue-600">
              {formatCurrency(property.startingPrice, property.currencyCode)}
            </span>
            <span className="text-xs text-gray-500"> / gece</span>
          </div>
          <div className="flex items-center gap-1 text-sm text-gray-500">
            <BedDouble className="w-4 h-4" />
            <span>{property.totalUnitCount} birim</span>
          </div>
        </div>
      </div>
    </div>
  );

  const PropertyRow = ({ property }: { property: any }) => (
    <tr className="hover:bg-gray-50 cursor-pointer transition-colors"
        onClick={() => navigate(`/dashboard/properties/${property.id}`)}>
      <td className="px-4 py-3">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-lg bg-gray-200 overflow-hidden flex-shrink-0">
            {property.coverImageUrl ? (
              <img src={property.coverImageUrl} alt="" className="w-full h-full object-cover" />
            ) : (
              <div className="flex items-center justify-center h-full">
                <Building2 className="w-6 h-6 text-gray-400" />
              </div>
            )}
          </div>
          <div>
            <div className="font-medium text-gray-900">{property.name}</div>
            <div className="text-sm text-gray-500">
              {PROPERTY_TYPES.find(t => t.value === property.type)?.label}
            </div>
          </div>
        </div>
      </td>
      <td className="px-4 py-3 text-sm">
        <div className="flex items-center gap-1">
          <MapPin className="w-3.5 h-3.5 text-gray-400" />
          {property.city}
        </div>
      </td>
      <td className="px-4 py-3 text-sm text-center">
        <span className="inline-flex items-center gap-1">
          <BedDouble className="w-4 h-4 text-gray-400" />
          {property.totalUnitCount}
        </span>
      </td>
      <td className="px-4 py-3 text-sm font-medium">
        {formatCurrency(property.startingPrice, property.currencyCode)}
      </td>
      <td className="px-4 py-3">
        <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
          property.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
        }`}>
          {property.isActive ? 'Aktif' : 'Pasif'}
        </span>
      </td>
      <td className="px-4 py-3">
        <div className="flex items-center gap-1">
          <button
            onClick={(e) => {
              e.stopPropagation();
              navigate(`/dashboard/properties/${property.id}/edit`);
            }}
            className="p-1.5 rounded-lg hover:bg-blue-50 text-gray-400 hover:text-blue-600"
          >
            <Edit className="w-4 h-4" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              toggleMutation.mutate(property.id);
            }}
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-gray-600"
          >
            <Power className="w-4 h-4" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              setDeleteProperty(property);
            }}
            className="p-1.5 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>
      </td>
    </tr>
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mülklerim</h1>
          <p className="text-sm text-gray-500 mt-1">
            Toplam {data?.totalCount || 0} mülk
          </p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 rounded-md transition-colors ${
                viewMode === 'grid' ? 'bg-white shadow-sm' : 'text-gray-500 hover:text-gray-700'
              }`}
            >
              <Grid className="w-4 h-4" />
            </button>
            <button
              onClick={() => setViewMode('list')}
              className={`p-2 rounded-md transition-colors ${
                viewMode === 'list' ? 'bg-white shadow-sm' : 'text-gray-500 hover:text-gray-700'
              }`}
            >
              <List className="w-4 h-4" />
            </button>
          </div>
          <Button
            onClick={() => navigate('/dashboard/properties/new')}
            leftIcon={<Plus className="w-4 h-4" />}
          >
            Yeni Mülk Ekle
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl border p-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Mülk adı ara..."
              value={filters.search}
              onChange={(e) => setFilter('search', e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <Select
            value={filters.type}
            onChange={(e) => setFilter('type', e.target.value)}
            options={[
              { value: '', label: 'Tüm Tipler' },
              ...PROPERTY_TYPES.map(t => ({ value: t.value, label: `${t.icon} ${t.label}` })),
            ]}
          />

          <Select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
            options={[
              { value: 'createdAt', label: 'Eklenme Tarihi' },
              { value: 'name', label: 'İsim' },
              { value: 'price', label: 'Fiyat' },
              { value: 'rating', label: 'Değerlendirme' },
            ]}
          />

          <Button
            variant="outline"
            onClick={() => setSortDesc(!sortDesc)}
            rightIcon={<ChevronDown className={`w-4 h-4 transition-transform ${sortDesc ? '' : 'rotate-180'}`} />}
          >
            {sortDesc ? 'Azalan' : 'Artan'}
          </Button>
        </div>

        {hasActiveFilters && (
          <div className="mt-3 pt-3 border-t flex items-center justify-between">
            <span className="text-sm text-gray-500">Filtreler aktif</span>
            <Button variant="ghost" size="sm" onClick={clearFilters}>
              Filtreleri Temizle
            </Button>
          </div>
        )}
      </div>

      {/* Property List */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="bg-white rounded-xl border animate-pulse">
              <div className="h-48 bg-gray-200 rounded-t-xl" />
              <div className="p-4 space-y-3">
                <div className="h-5 bg-gray-200 rounded w-3/4" />
                <div className="h-4 bg-gray-200 rounded w-1/2" />
                <div className="h-4 bg-gray-200 rounded w-1/4" />
              </div>
            </div>
          ))}
        </div>
      ) : viewMode === 'grid' ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {data?.items.map((property) => (
            <PropertyCard key={property.id} property={property} />
          ))}
        </div>
      ) : (
        <div className="bg-white rounded-xl border overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Mülk</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Konum</th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Birim</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fiyat</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Durum</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data?.items.map((property) => (
                <PropertyRow key={property.id} property={property} />
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Empty State */}
      {!isLoading && data?.items.length === 0 && (
        <div className="text-center py-16">
          <Building2 className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz mülk eklenmemiş</h3>
          <p className="text-sm text-gray-500 mb-6">İlk mülkünüzü ekleyerek başlayın</p>
          <Button onClick={() => navigate('/dashboard/properties/new')} leftIcon={<Plus className="w-4 h-4" />}>
            İlk Mülkü Ekle
          </Button>
        </div>
      )}

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <Pagination
          currentPage={data.page}
          totalPages={data.totalPages}
          onPageChange={setPage}
          pageSize={pageSize}
          onPageSizeChange={setPageSize}
          totalCount={data.totalCount}
        />
      )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteProperty}
        onClose={() => setDeleteProperty(null)}
        onConfirm={() => deleteProperty && deleteMutation.mutate(deleteProperty.id)}
        title="Mülkü Sil"
        message={`"${deleteProperty?.name}" mülkünü silmek istediğinize emin misiniz? Bu işlem geri alınamaz ve tüm birimler, rezervasyonlar ve diğer veriler de silinecektir.`}
        confirmLabel="Sil"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
UnitManagement.tsx
