// src/pages/tenant/Properties/PropertyDetail.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Building2,
  MapPin,
  Mail,
  Phone,
  Globe,
  Clock,
  BedDouble,
  Users,
  Star,
  Edit,
  Trash2,
  Image,
  Plus,
  Eye,
  EyeOff,
  Calendar,
  DollarSign,
  Check,
  X,
  Upload,
  Copy,
} from 'lucide-react';
import { propertyApi } from '../../../api/property.api';
import { Button, Card, Badge, Modal, Tabs, ConfirmDialog } from '../../../components/ui';
import { formatCurrency, formatDate, formatTime } from '../../../utils/format';
import { PROPERTY_TYPES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function PropertyDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  const [activeTab, setActiveTab] = useState<'info' | 'units' | 'images' | 'reviews'>('info');
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const { data: property, isLoading } = useQuery({
    queryKey: ['property', id],
    queryFn: () => propertyApi.getById(id!),
    enabled: !!id,
  });

  const { data: units } = useQuery({
    queryKey: ['units', id],
    queryFn: () => propertyApi.getUnits(id!),
    enabled: !!id && activeTab === 'units',
  });

  const deleteMutation = useMutation({
    mutationFn: () => propertyApi.delete(id!),
    onSuccess: () => {
      toast.success('Mülk silindi');
      navigate('/dashboard/properties');
    },
    onError: (error: any) => toast.error(error.message),
  });

  const toggleMutation = useMutation({
    mutationFn: () => propertyApi.toggleActive(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['property', id] });
      toast.success('Mülk durumu güncellendi');
    },
  });

  if (isLoading) {
    return (
      <div className="space-y-6 animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/4" />
        <div className="h-64 bg-gray-200 rounded-xl" />
      </div>
    );
  }

  if (!property) {
    return (
      <div className="text-center py-20">
        <Building2 className="w-20 h-20 text-gray-300 mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-gray-900 mb-2">Mülk bulunamadı</h2>
        <Button onClick={() => navigate('/dashboard/properties')}>
          Mülklere Dön
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/dashboard/properties')}
            className="p-2 rounded-lg hover:bg-gray-100"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-gray-900">{property.name}</h1>
              <Badge color={property.isActive ? 'green' : 'red'}>
                {property.isActive ? 'Aktif' : 'Pasif'}
              </Badge>
              <Badge color="blue">
                {PROPERTY_TYPES.find(t => t.value === property.type)?.label}
              </Badge>
            </div>
            <div className="flex items-center gap-1 text-sm text-gray-500 mt-1">
              <MapPin className="w-3.5 h-3.5" />
              {property.city}, {property.country}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => toggleMutation.mutate()}
            leftIcon={property.isActive ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
          >
            {property.isActive ? 'Pasif Yap' : 'Aktif Yap'}
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate(`/dashboard/properties/${id}/edit`)}
            leftIcon={<Edit className="w-4 h-4" />}
          >
            Düzenle
          </Button>
          <Button
            variant="danger"
            size="sm"
            onClick={() => setShowDeleteConfirm(true)}
            leftIcon={<Trash2 className="w-4 h-4" />}
          >
            Sil
          </Button>
        </div>
      </div>

      {/* Tabs */}
      <Tabs
        tabs={[
          { key: 'info', label: 'Bilgiler', icon: Building2 },
          { key: 'units', label: `Birimler (${property.totalUnitCount || 0})`, icon: BedDouble },
          { key: 'images', label: 'Fotoğraflar', icon: Image },
          { key: 'reviews', label: `Değerlendirmeler (${property.reviewCount || 0})`, icon: Star },
        ]}
        activeTab={activeTab}
        onChange={(tab) => setActiveTab(tab as any)}
      />

      {/* Tab Content */}
      {activeTab === 'info' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="lg:col-span-2 space-y-6">
            {/* Description */}
            <Card>
              <div className="card-header">
                <h3 className="text-lg font-semibold">Açıklama</h3>
              </div>
              <div className="card-body">
                <p className="text-gray-700 whitespace-pre-wrap">
                  {property.description || 'Açıklama eklenmemiş.'}
                </p>
              </div>
            </Card>

            {/* Contact Info */}
            <Card>
              <div className="card-header">
                <h3 className="text-lg font-semibold">İletişim Bilgileri</h3>
              </div>
              <div className="card-body">
                <div className="grid grid-cols-2 gap-4">
                  {property.email && (
                    <div className="flex items-center gap-2 text-sm">
                      <Mail className="w-4 h-4 text-gray-400" />
                      <a href={`mailto:${property.email}`} className="text-blue-600">{property.email}</a>
                    </div>
                  )}
                  {property.phone && (
                    <div className="flex items-center gap-2 text-sm">
                      <Phone className="w-4 h-4 text-gray-400" />
                      <a href={`tel:${property.phone}`}>{property.phone}</a>
                    </div>
                  )}
                  {property.website && (
                    <div className="flex items-center gap-2 text-sm">
                      <Globe className="w-4 h-4 text-gray-400" />
                      <a href={property.website} target="_blank" className="text-blue-600">{property.website}</a>
                    </div>
                  )}
                </div>
              </div>
            </Card>

            {/* Amenities */}
            {property.amenities && property.amenities.length > 0 && (
              <Card>
                <div className="card-header">
                  <h3 className="text-lg font-semibold">Özellikler</h3>
                </div>
                <div className="card-body">
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                    {property.amenities.map((amenity: string) => (
                      <div key={amenity} className="flex items-center gap-2 text-sm">
                        <Check className="w-4 h-4 text-green-500" />
                        {amenity}
                      </div>
                    ))}
                  </div>
                </div>
              </Card>
            )}
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Quick Stats */}
            <Card>
              <div className="card-header">
                <h3 className="text-lg font-semibold">Özet</h3>
              </div>
              <div className="card-body space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-500">Birim Sayısı</span>
                  <span className="font-medium">{property.totalUnitCount}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-500">Başlangıç Fiyat</span>
                  <span className="font-medium text-blue-600">
                    {formatCurrency(property.startingPrice, property.currencyCode)}
                  </span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-500">Check-in</span>
                  <span className="font-medium">{property.checkInTime}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-500">Check-out</span>
                  <span className="font-medium">{property.checkOutTime}</span>
                </div>
                {property.averageRating > 0 && (
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-500">Puan</span>
                    <div className="flex items-center gap-1">
                      <Star className="w-4 h-4 text-yellow-400 fill-current" />
                      <span className="font-medium">{property.averageRating.toFixed(1)}</span>
                      <span className="text-xs text-gray-500">({property.reviewCount})</span>
                    </div>
                  </div>
                )}
              </div>
            </Card>

            {/* Quick Actions */}
            <Card>
              <div className="card-body space-y-2">
                <Button
                  className="w-full"
                  size="sm"
                  onClick={() => navigate(`/dashboard/calendar?propertyId=${id}`)}
                  leftIcon={<Calendar className="w-4 h-4" />}
                >
                  Takvimi Gör
                </Button>
                <Button
                  className="w-full"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate(`/dashboard/reservations/new?propertyId=${id}`)}
                  leftIcon={<Plus className="w-4 h-4" />}
                >
                  Rezervasyon Ekle
                </Button>
                <Button
                  className="w-full"
                  variant="outline"
                  size="sm"
                  onClick={() => navigate(`/dashboard/properties/${id}/units`)}
                  leftIcon={<BedDouble className="w-4 h-4" />}
                >
                  Birimleri Yönet
                </Button>
              </div>
            </Card>
          </div>
        </div>
      )}

      {activeTab === 'units' && (
        <Card>
          <div className="card-header flex items-center justify-between">
            <h3 className="text-lg font-semibold">Birimler</h3>
            <Button
              size="sm"
              onClick={() => navigate(`/dashboard/properties/${id}/units`)}
              leftIcon={<Plus className="w-4 h-4" />}
            >
              Birim Ekle
            </Button>
          </div>
          <div className="overflow-x-auto">
            <table className="table">
              <thead>
                <tr>
                  <th>Birim</th>
                  <th>No</th>
                  <th>Kat</th>
                  <th>Kapasite</th>
                  <th>Fiyat</th>
                  <th>Durum</th>
                </tr>
              </thead>
              <tbody>
                {units?.map((unit: any) => (
                  <tr key={unit.id}>
                    <td className="font-medium">{unit.name}</td>
                    <td>{unit.unitNumber || '-'}</td>
                    <td>{unit.floor}</td>
                    <td>{unit.maxAdults}Y {unit.maxChildren > 0 ? `/ ${unit.maxChildren}Ç` : ''}</td>
                    <td className="font-medium text-blue-600">
                      {formatCurrency(unit.basePrice, unit.currencyCode)}
                    </td>
                    <td>
                      <Badge color={unit.isActive ? 'green' : 'red'}>
                        {unit.isActive ? 'Aktif' : 'Pasif'}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {activeTab === 'images' && (
        <Card>
          <div className="card-header flex items-center justify-between">
            <h3 className="text-lg font-semibold">Fotoğraflar</h3>
            <Button size="sm" leftIcon={<Upload className="w-4 h-4" />}>
              Fotoğraf Yükle
            </Button>
          </div>
          <div className="card-body">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              {property.coverImageUrl ? (
                <div className="relative group rounded-xl overflow-hidden aspect-square bg-gray-100">
                  <img
                    src={property.coverImageUrl}
                    alt={property.name}
                    className="w-full h-full object-cover"
                  />
                  <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                    <Button size="sm" variant="ghost" className="text-white">
                      <Edit className="w-4 h-4" />
                    </Button>
                    <Button size="sm" variant="ghost" className="text-white">
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="text-center py-12 col-span-full text-gray-500">
                  <Image className="w-12 h-12 mx-auto mb-3 text-gray-300" />
                  <p>Henüz fotoğraf eklenmemiş</p>
                </div>
              )}
            </div>
          </div>
        </Card>
      )}

      {activeTab === 'reviews' && (
        <Card>
          <div className="card-body text-center py-12 text-gray-500">
            <Star className="w-12 h-12 mx-auto mb-3 text-gray-300" />
            <p>Henüz değerlendirme bulunmamaktadır</p>
          </div>
        </Card>
      )}

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={() => deleteMutation.mutate()}
        title="Mülkü Sil"
        message={`"${property.name}" mülkünü silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
        confirmLabel="Sil"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}