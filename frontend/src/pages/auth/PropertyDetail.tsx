// src/pages/agency/PropertyDetail.tsx
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Building2,
  MapPin,
  Mail,
  Phone,
  BedDouble,
  Users,
  Calendar,
  DollarSign,
  Clock,
  Check,
  Shield,
  Eye,
  EyeOff,
  Plus,
  Star,
} from 'lucide-react';
import { agencyApi } from '../../api/agency.api';
import { Button, Card, Badge, Tabs } from '../../components/ui';
import { formatCurrency } from '../../utils/format';
import { PROPERTY_TYPES } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';

export default function AgencyPropertyDetail() {
  const { propertyId } = useParams<{ propertyId: string }>();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  const { data: property, isLoading } = useQuery({
    queryKey: ['agency', 'property-detail', agencyId, propertyId],
    queryFn: () => agencyApi.getPropertyDetail(agencyId!, propertyId!),
    enabled: !!agencyId && !!propertyId,
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
        <h2 className="text-xl font-semibold mb-2">Mülk bulunamadı</h2>
        <Button onClick={() => navigate('/agency/properties')}>Geri Dön</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button onClick={() => navigate('/agency/properties')} className="p-2 rounded-lg hover:bg-gray-100">
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{property.propertyName}</h1>
            <div className="flex items-center gap-1 text-sm text-gray-500 mt-1">
              <MapPin className="w-3.5 h-3.5" />
              {property.city}
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Button
            onClick={() => navigate(`/agency/calendar/${propertyId}`)}
            leftIcon={<Calendar className="w-4 h-4" />}
          >
            Takvimi Gör
          </Button>
          {property.canCreateReservation && (
            <Button
              onClick={() => navigate(`/agency/reservations/new?propertyId=${propertyId}`)}
              leftIcon={<Plus className="w-4 h-4" />}
            >
              Rezervasyon Yap
            </Button>
          )}
        </div>
      </div>

      {/* Authorization Info */}
      <Card className="p-4">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${property.canCreateReservation ? 'bg-green-100' : 'bg-red-100'}`}>
              <Shield className={`w-5 h-5 ${property.canCreateReservation ? 'text-green-600' : 'text-red-600'}`} />
            </div>
            <div>
              <p className="text-xs text-gray-500">Rezervasyon Yetkisi</p>
              <p className="text-sm font-medium">{property.canCreateReservation ? 'Var' : 'Yok'}</p>
            </div>
          </div>
          
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${property.canViewPrices ? 'bg-blue-100' : 'bg-red-100'}`}>
              <DollarSign className={`w-5 h-5 ${property.canViewPrices ? 'text-blue-600' : 'text-red-600'}`} />
            </div>
            <div>
              <p className="text-xs text-gray-500">Fiyat Görüntüleme</p>
              <p className="text-sm font-medium">{property.canViewPrices ? 'Var' : 'Yok'}</p>
            </div>
          </div>
          
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-purple-100">
              <DollarSign className="w-5 h-5 text-purple-600" />
            </div>
            <div>
              <p className="text-xs text-gray-500">Komisyon</p>
              <p className="text-sm font-medium">%{property.commissionRate}</p>
            </div>
          </div>
          
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-orange-100">
              <Users className="w-5 h-5 text-orange-600" />
            </div>
            <div>
              <p className="text-xs text-gray-500">Kontenjan</p>
              <p className="text-sm font-medium">
                {property.hasAllotment
                  ? `${property.remainingAllotment} / ${property.totalAllotment}`
                  : 'Sınırsız'}
              </p>
            </div>
          </div>
        </div>
      </Card>

      {/* Units */}
      <Card>
        <div className="card-header">
          <h3 className="text-lg font-semibold">Birimler</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="table">
            <thead>
              <tr>
                <th>Birim</th>
                <th>No</th>
                <th>Kapasite</th>
                {property.canViewPrices && <th>Fiyat</th>}
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {property.units?.map((unit: any) => (
                <tr key={unit.unitId}>
                  <td className="font-medium">{unit.unitName}</td>
                  <td>{unit.unitNumber || '-'}</td>
                  <td>{unit.maxAdults}Y {unit.maxChildren > 0 ? `/ ${unit.maxChildren}Ç` : ''}</td>
                  {property.canViewPrices && (
                    <td className="font-medium text-blue-600">
                      {unit.basePrice ? formatCurrency(unit.basePrice, unit.currencyCode) : 'Gizli'}
                    </td>
                  )}
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
    </div>
  );
}