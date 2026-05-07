// src/pages/tenant/Agencies/Authorizations.tsx
import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Plus,
  Search,
  Filter,
  Shield,
  Building2,
  Trash2,
  Edit,
  Eye,
  EyeOff,
  Check,
  X,
  Clock,
  Calendar,
  Users,
  DollarSign,
  AlertTriangle,
  ChevronDown,
  MoreVertical,
  Power,
  Download,
} from 'lucide-react';
import { agencyApi } from '../../../api/agency.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Modal, Card, Badge, ConfirmDialog, Pagination } from '../../../components/ui';
import { useDebounce } from '../../../hooks/useDebounce';
import { formatCurrency, formatDate, formatDateTime } from '../../../utils/format';
import toast from 'react-hot-toast';

interface Agency {
  id: string;
  companyName: string;
  taxNumber: string;
  email: string;
  phone: string;
  contactPerson: string;
  type: string;
  typeDescription: string;
  defaultCommissionRate: number;
  authorizedPropertyCount: number;
  isVerified: boolean;
  isActive: boolean;
  createdAt: string;
}

interface Authorization {
  id: string;
  agencyId: string;
  agencyName: string;
  propertyId: string;
  propertyName: string;
  propertyType: string;
  level: string;
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  canModifyReservation: boolean;
  canCancelReservation: boolean;
  priceDisplay: string;
  customCommissionRate: number | null;
  defaultMarkupRate: number | null;
  maxMarkupRate: number | null;
  hasAllotment: boolean;
  totalAllotment: number | null;
  usedAllotment: number;
  isActive: boolean;
  validFrom: string | null;
  validTo: string | null;
  grantedAt: string;
  notes: string;
}

interface AuthorizationFormData {
  agencyId: string;
  propertyId: string;
  level: string;
  allowedUnitIds: string[] | null;
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  canModifyReservation: boolean;
  canCancelReservation: boolean;
  priceDisplay: string;
  customCommissionRate: number | null;
  defaultMarkupRate: number | null;
  maxMarkupRate: number | null;
  hasAllotment: boolean;
  totalAllotment: number | null;
  validFrom: string;
  validTo: string;
  notes: string;
}

export default function AgencyManagement() {
  const queryClient = useQueryClient();
  
  // States
  const [activeTab, setActiveTab] = useState<'agencies' | 'authorizations'>('agencies');
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const debouncedSearch = useDebounce(search);
  
  // Authorization form
  const [showAuthForm, setShowAuthForm] = useState(false);
  const [editingAuth, setEditingAuth] = useState<Authorization | null>(null);
  const [authFormData, setAuthFormData] = useState<AuthorizationFormData>({
    agencyId: '',
    propertyId: '',
    level: 'ViewOnly',
    allowedUnitIds: null,
    canViewPrices: true,
    canSetPrices: false,
    canCreateReservation: true,
    canModifyReservation: false,
    canCancelReservation: false,
    priceDisplay: 'Net',
    customCommissionRate: null,
    defaultMarkupRate: null,
    maxMarkupRate: null,
    hasAllotment: false,
    totalAllotment: null,
    validFrom: '',
    validTo: '',
    notes: '',
  });
  
  // Revoke confirmation
  const [revokeAuth, setRevokeAuth] = useState<Authorization | null>(null);
  
  // Detail modal
  const [selectedAuth, setSelectedAuth] = useState<Authorization | null>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);

  // Queries
  const queryParams = useMemo(() => ({
    page,
    pageSize: 20,
    search: debouncedSearch || undefined,
    type: typeFilter || undefined,
    isActive: statusFilter ? statusFilter === 'active' : undefined,
  }), [page, debouncedSearch, typeFilter, statusFilter]);

  const { data: agencies, isLoading: agenciesLoading } = useQuery({
    queryKey: ['agencies', queryParams],
    queryFn: () => agencyApi.getAll(queryParams),
    enabled: activeTab === 'agencies',
  });

  const { data: authorizations, isLoading: authorizationsLoading } = useQuery({
    queryKey: ['authorizations'],
    queryFn: () => agencyApi.getAllAuthorizations(),
    enabled: activeTab === 'authorizations',
  });

  const { data: properties } = useQuery({
    queryKey: ['properties', 'all'],
    queryFn: () => propertyApi.getAll({ pageSize: 1000 }),
  });

  // Mutations
  const grantAuthMutation = useMutation({
    mutationFn: (data: any) => {
      if (editingAuth) {
        return agencyApi.updateAuthorization(editingAuth.id, data);
      }
      return agencyApi.grantAuthorization(data);
    },
    onSuccess: () => {
      toast.success(editingAuth ? 'Yetkilendirme güncellendi' : 'Yetkilendirme verildi');
      queryClient.invalidateQueries({ queryKey: ['authorizations'] });
      queryClient.invalidateQueries({ queryKey: ['agencies'] });
      resetAuthForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  const revokeMutation = useMutation({
    mutationFn: (authId: string) => agencyApi.revokeAuthorization(authId),
    onSuccess: () => {
      toast.success('Yetkilendirme iptal edildi');
      queryClient.invalidateQueries({ queryKey: ['authorizations'] });
      queryClient.invalidateQueries({ queryKey: ['agencies'] });
      setRevokeAuth(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const resetAuthForm = () => {
    setAuthFormData({
      agencyId: '',
      propertyId: '',
      level: 'ViewOnly',
      allowedUnitIds: null,
      canViewPrices: true,
      canSetPrices: false,
      canCreateReservation: true,
      canModifyReservation: false,
      canCancelReservation: false,
      priceDisplay: 'Net',
      customCommissionRate: null,
      defaultMarkupRate: null,
      maxMarkupRate: null,
      hasAllotment: false,
      totalAllotment: null,
      validFrom: '',
      validTo: '',
      notes: '',
    });
    setEditingAuth(null);
    setShowAuthForm(false);
  };

  const handleEditAuth = (auth: Authorization) => {
    setEditingAuth(auth);
    setAuthFormData({
      agencyId: auth.agencyId,
      propertyId: auth.propertyId,
      level: auth.level,
      allowedUnitIds: null,
      canViewPrices: auth.canViewPrices,
      canSetPrices: auth.canSetPrices,
      canCreateReservation: auth.canCreateReservation,
      canModifyReservation: auth.canModifyReservation,
      canCancelReservation: auth.canCancelReservation,
      priceDisplay: auth.priceDisplay,
      customCommissionRate: auth.customCommissionRate,
      defaultMarkupRate: auth.defaultMarkupRate,
      maxMarkupRate: auth.maxMarkupRate,
      hasAllotment: auth.hasAllotment,
      totalAllotment: auth.totalAllotment,
      validFrom: auth.validFrom?.split('T')[0] || '',
      validTo: auth.validTo?.split('T')[0] || '',
      notes: auth.notes || '',
    });
    setShowAuthForm(true);
  };

  const getLevelBadge = (level: string) => {
    const configs: Record<string, { color: string; label: string; icon: any }> = {
      ViewOnly: { color: 'bg-gray-100 text-gray-800', label: 'Sadece Görüntüleme', icon: Eye },
      PriceAndAvailability: { color: 'bg-blue-100 text-blue-800', label: 'Fiyat ve Müsaitlik', icon: DollarSign },
      CanReserve: { color: 'bg-green-100 text-green-800', label: 'Rezervasyon Yapabilir', icon: Check },
      FullAccess: { color: 'bg-purple-100 text-purple-800', label: 'Tam Yetki', icon: Shield },
    };
    const config = configs[level] || configs.ViewOnly;
    const Icon = config.icon;
    return (
      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 text-xs font-medium rounded-full ${config.color}`}>
        <Icon className="w-3 h-3" />
        {config.label}
      </span>
    );
  };

  const getPriceDisplayBadge = (display: string) => {
    const configs: Record<string, { color: string; label: string }> = {
      Net: { color: 'bg-green-100 text-green-800', label: 'Net Fiyat' },
      Commission: { color: 'bg-orange-100 text-orange-800', label: 'Komisyon Dahil' },
      Markup: { color: 'bg-purple-100 text-purple-800', label: 'Markup Fiyat' },
    };
    const config = configs[display] || configs.Net;
    return (
      <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${config.color}`}>
        {config.label}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Acente Yönetimi</h1>
          <p className="text-sm text-gray-500 mt-1">
            Acenteleri ve yetkilendirmeleri yönetin
          </p>
        </div>
        <Button
          onClick={() => setShowAuthForm(true)}
          leftIcon={<Plus className="w-4 h-4" />}
        >
          Yeni Yetkilendirme
        </Button>
      </div>

      {/* Tabs */}
      <div className="border-b">
        <div className="flex gap-6">
          <button
            onClick={() => setActiveTab('agencies')}
            className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'agencies'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Acenteler ({agencies?.totalCount || 0})
          </button>
          <button
            onClick={() => setActiveTab('authorizations')}
            className={`pb-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'authorizations'
                ? 'border-blue-600 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            Yetkilendirmeler ({authorizations?.length || 0})
          </button>
        </div>
      </div>

      {/* Agencies Tab */}
      {activeTab === 'agencies' && (
        <>
          {/* Filters */}
          <div className="bg-white rounded-xl border p-4">
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type="text"
                  placeholder="Acente ara..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <Select
                value={typeFilter}
                onChange={(e) => setTypeFilter(e.target.value)}
                options={[
                  { value: '', label: 'Tüm Tipler' },
                  { value: 'TravelAgency', label: 'Seyahat Acentası' },
                  { value: 'TourOperator', label: 'Tur Operatörü' },
                  { value: 'OTA', label: 'Online Acente (OTA)' },
                  { value: 'Corporate', label: 'Kurumsal' },
                ]}
              />
              <Select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                options={[
                  { value: '', label: 'Tüm Durumlar' },
                  { value: 'active', label: 'Aktif' },
                  { value: 'inactive', label: 'Pasif' },
                  { value: 'verified', label: 'Onaylı' },
                  { value: 'unverified', label: 'Onaysız' },
                ]}
              />
            </div>
          </div>

          {/* Agencies Table */}
          <div className="bg-white rounded-xl border overflow-hidden">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Acente</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">İletişim</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tip</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Yetkili Mülk</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Komisyon</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">Durum</th>
                  <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase">İşlem</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {agencies?.items?.map((agency: Agency) => (
                  <tr key={agency.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center">
                          <Building2 className="w-5 h-5 text-blue-600" />
                        </div>
                        <div>
                          <div className="font-medium text-gray-900">{agency.companyName}</div>
                          <div className="text-xs text-gray-500">{agency.taxNumber}</div>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <div className="text-sm">
                        <div>{agency.email}</div>
                        <div className="text-gray-500">{agency.phone}</div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm">{agency.typeDescription}</span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className="inline-flex items-center gap-1 text-sm font-medium">
                        <Building2 className="w-4 h-4 text-gray-400" />
                        {agency.authorizedPropertyCount}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className="text-sm font-medium">%{agency.defaultCommissionRate}</span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <div className="flex flex-col items-center gap-1">
                        <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                          agency.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                          {agency.isActive ? 'Aktif' : 'Pasif'}
                        </span>
                        {!agency.isVerified && (
                          <span className="inline-flex items-center gap-1 text-xs text-yellow-600">
                            <AlertTriangle className="w-3 h-3" />
                            Onaysız
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex items-center justify-center gap-1">
                        <button
                          onClick={() => {
                            setAuthFormData(prev => ({ ...prev, agencyId: agency.id }));
                            setShowAuthForm(true);
                          }}
                          className="p-1.5 rounded-lg hover:bg-blue-50 text-gray-400 hover:text-blue-600"
                          title="Yetkilendir"
                        >
                          <Shield className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {agencies && agencies.totalPages > 1 && (
            <Pagination
              currentPage={agencies.page}
              totalPages={agencies.totalPages}
              onPageChange={setPage}
              totalCount={agencies.totalCount}
            />
          )}
        </>
      )}

      {/* Authorizations Tab */}
      {activeTab === 'authorizations' && (
        <div className="space-y-4">
          {authorizations?.map((auth: Authorization) => (
            <Card key={auth.id} className={`${!auth.isActive ? 'opacity-60 bg-gray-50' : ''}`}>
              <div className="p-5">
                <div className="flex items-start justify-between">
                  {/* Agency & Property Info */}
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="flex items-center gap-2">
                        <Building2 className="w-5 h-5 text-blue-600" />
                        <div>
                          <h3 className="font-semibold text-gray-900">{auth.propertyName}</h3>
                          <p className="text-xs text-gray-500">{auth.propertyType}</p>
                        </div>
                      </div>
                      <span className="text-gray-300">→</span>
                      <div className="flex items-center gap-2">
                        <Users className="w-5 h-5 text-green-600" />
                        <div>
                          <h3 className="font-semibold text-gray-900">{auth.agencyName}</h3>
                        </div>
                      </div>
                    </div>

                    {/* Authorization Details */}
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                      <div>
                        <label className="text-xs text-gray-500">Yetki Seviyesi</label>
                        <div className="mt-1">{getLevelBadge(auth.level)}</div>
                      </div>
                      <div>
                        <label className="text-xs text-gray-500">Fiyat Gösterimi</label>
                        <div className="mt-1">{getPriceDisplayBadge(auth.priceDisplay)}</div>
                      </div>
                      <div>
                        <label className="text-xs text-gray-500">Komisyon</label>
                        <p className="text-sm font-medium mt-1">
                          %{auth.customCommissionRate || 10}
                        </p>
                      </div>
                      <div>
                        <label className="text-xs text-gray-500">Kontenjan</label>
                        <p className="text-sm font-medium mt-1">
                          {auth.hasAllotment ? (
                            <span className="text-blue-600">
                              {auth.usedAllotment}/{auth.totalAllotment}
                            </span>
                          ) : (
                            <span className="text-gray-400">Sınırsız</span>
                          )}
                        </p>
                      </div>
                    </div>

                    {/* Permissions */}
                    <div className="flex flex-wrap gap-2 mt-3">
                      {auth.canViewPrices && (
                        <span className="inline-flex items-center gap-1 text-xs px-2 py-1 bg-blue-50 text-blue-700 rounded-full">
                          <Eye className="w-3 h-3" /> Fiyat Görüntüleme
                        </span>
                      )}
                      {auth.canSetPrices && (
                        <span className="inline-flex items-center gap-1 text-xs px-2 py-1 bg-purple-50 text-purple-700 rounded-full">
                          <Edit className="w-3 h-3" /> Fiyat Belirleme
                        </span>
                      )}
                      {auth.canCreateReservation && (
                        <span className="inline-flex items-center gap-1 text-xs px-2 py-1 bg-green-50 text-green-700 rounded-full">
                          <Check className="w-3 h-3" /> Rezervasyon
                        </span>
                      )}
                      {auth.canModifyReservation && (
                        <span className="inline-flex items-center gap-1 text-xs px-2 py-1 bg-orange-50 text-orange-700 rounded-full">
                          <Edit className="w-3 h-3" /> Değişiklik
                        </span>
                      )}
                      {auth.canCancelReservation && (
                        <span className="inline-flex items-center gap-1 text-xs px-2 py-1 bg-red-50 text-red-700 rounded-full">
                          <X className="w-3 h-3" /> İptal
                        </span>
                      )}
                    </div>

                    {/* Date Range */}
                    {(auth.validFrom || auth.validTo) && (
                      <div className="flex items-center gap-2 mt-3 text-xs text-gray-500">
                        <Calendar className="w-3.5 h-3.5" />
                        {auth.validFrom ? formatDate(auth.validFrom) : 'Başlangıç yok'}
                        <span>-</span>
                        {auth.validTo ? formatDate(auth.validTo) : 'Bitiş yok'}
                      </div>
                    )}
                  </div>

                  {/* Actions */}
                  <div className="flex items-center gap-2 ml-4">
                    <button
                      onClick={() => {
                        setSelectedAuth(auth);
                        setShowDetailModal(true);
                      }}
                      className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-gray-600"
                      title="Detay"
                    >
                      <Eye className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleEditAuth(auth)}
                      className="p-2 rounded-lg hover:bg-blue-50 text-gray-400 hover:text-blue-600"
                      title="Düzenle"
                    >
                      <Edit className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => setRevokeAuth(auth)}
                      className="p-2 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600"
                      title="İptal Et"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
            </Card>
          ))}

          {!authorizationsLoading && authorizations?.length === 0 && (
            <div className="text-center py-16">
              <Shield className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz yetkilendirme yok</h3>
              <p className="text-sm text-gray-500 mb-6">Acentelere mülkleriniz için yetki verin</p>
              <Button onClick={() => setShowAuthForm(true)} leftIcon={<Plus className="w-4 h-4" />}>
                İlk Yetkilendirmeyi Yap
              </Button>
            </div>
          )}
        </div>
      )}

      {/* Authorization Form Modal */}
      <Modal
        isOpen={showAuthForm}
        onClose={resetAuthForm}
        title={editingAuth ? 'Yetkilendirmeyi Düzenle' : 'Yeni Yetkilendirme'}
        size="xl"
        footer={
          <div className="flex justify-end gap-3 w-full">
            <Button variant="outline" onClick={resetAuthForm}>
              İptal
            </Button>
            <Button
              onClick={() => grantAuthMutation.mutate(authFormData)}
              isLoading={grantAuthMutation.isPending}
            >
              {editingAuth ? 'Güncelle' : 'Yetkilendir'}
            </Button>
          </div>
        }
      >
        <div className="space-y-6">
          {/* Agency & Property Selection */}
          <div className="grid grid-cols-2 gap-4">
            <Select
              label="Acente *"
              value={authFormData.agencyId}
              onChange={(e) => setAuthFormData({ ...authFormData, agencyId: e.target.value })}
              options={[
                { value: '', label: 'Acente seçin...' },
                ...(agencies?.items?.map((a: Agency) => ({
                  value: a.id,
                  label: a.companyName,
                })) || []),
              ]}
              required
            />
            <Select
              label="Mülk *"
              value={authFormData.propertyId}
              onChange={(e) => setAuthFormData({ ...authFormData, propertyId: e.target.value })}
              options={[
                { value: '', label: 'Mülk seçin...' },
                ...(properties?.items?.map((p: any) => ({
                  value: p.id,
                  label: p.name,
                })) || []),
              ]}
              required
            />
          </div>

          {/* Authorization Level */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Yetki Seviyesi *</label>
            <div className="grid grid-cols-2 gap-3">
              {[
                { value: 'ViewOnly', label: 'Sadece Görüntüleme', desc: 'Sadece mülk ve müsaitlikleri görebilir', icon: Eye },
                { value: 'PriceAndAvailability', label: 'Fiyat ve Müsaitlik', desc: 'Fiyatları görebilir, müsaitlik kontrolü yapabilir', icon: DollarSign },
                { value: 'CanReserve', label: 'Rezervasyon Yapabilir', desc: 'Rezervasyon oluşturabilir ve yönetebilir', icon: Check },
                { value: 'FullAccess', label: 'Tam Yetki', desc: 'Tüm işlemleri yapabilir, fiyat belirleyebilir', icon: Shield },
              ].map((level) => {
                const Icon = level.icon;
                return (
                  <button
                    key={level.value}
                    type="button"
                    onClick={() => setAuthFormData({ ...authFormData, level: level.value })}
                    className={`flex items-start gap-3 p-4 rounded-xl border-2 text-left transition-all ${
                      authFormData.level === level.value
                        ? 'border-blue-500 bg-blue-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <div className={`p-2 rounded-lg ${
                      authFormData.level === level.value ? 'bg-blue-100' : 'bg-gray-100'
                    }`}>
                      <Icon className={`w-5 h-5 ${
                        authFormData.level === level.value ? 'text-blue-600' : 'text-gray-500'
                      }`} />
                    </div>
                    <div>
                      <div className="font-medium text-sm">{level.label}</div>
                      <div className="text-xs text-gray-500 mt-0.5">{level.desc}</div>
                    </div>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Permissions */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-3">İzinler</label>
            <div className="grid grid-cols-2 gap-3">
              <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
                <input
                  type="checkbox"
                  checked={authFormData.canViewPrices}
                  onChange={(e) => setAuthFormData({ ...authFormData, canViewPrices: e.target.checked })}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <div className="text-sm font-medium">Fiyat Görüntüleme</div>
                  <div className="text-xs text-gray-500">Birim fiyatlarını görebilir</div>
                </div>
              </label>
              <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
                <input
                  type="checkbox"
                  checked={authFormData.canSetPrices}
                  onChange={(e) => setAuthFormData({ ...authFormData, canSetPrices: e.target.checked })}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <div className="text-sm font-medium">Fiyat Belirleme</div>
                  <div className="text-xs text-gray-500">Kendi fiyatlarını belirleyebilir</div>
                </div>
              </label>
              <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
                <input
                  type="checkbox"
                  checked={authFormData.canCreateReservation}
                  onChange={(e) => setAuthFormData({ ...authFormData, canCreateReservation: e.target.checked })}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <div className="text-sm font-medium">Rezervasyon Yapma</div>
                  <div className="text-xs text-gray-500">Yeni rezervasyon oluşturabilir</div>
                </div>
              </label>
              <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
                <input
                  type="checkbox"
                  checked={authFormData.canCancelReservation}
                  onChange={(e) => setAuthFormData({ ...authFormData, canCancelReservation: e.target.checked })}
                  className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <div>
                  <div className="text-sm font-medium">İptal Etme</div>
                  <div className="text-xs text-gray-500">Rezervasyon iptal edebilir</div>
                </div>
              </label>
            </div>
          </div>

          {/* Price & Commission Settings */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-3">Fiyatlandırma Ayarları</label>
            <div className="grid grid-cols-2 gap-4">
              <Select
                label="Fiyat Gösterim Tipi"
                value={authFormData.priceDisplay}
                onChange={(e) => setAuthFormData({ ...authFormData, priceDisplay: e.target.value })}
                options={[
                  { value: 'Net', label: 'Net Fiyat (Oda fiyatı)' },
                  { value: 'Commission', label: 'Komisyon Dahil' },
                  { value: 'Markup', label: 'Markup (Üstüne fark koyabilir)' },
                ]}
              />
              <Input
                label="Komisyon Oranı (%)"
                type="number"
                min={0}
                max={100}
                value={authFormData.customCommissionRate || ''}
                onChange={(e) => setAuthFormData({
                  ...authFormData,
                  customCommissionRate: e.target.value ? parseFloat(e.target.value) : null,
                })}
                placeholder="Varsayılan: %10"
              />
              {authFormData.priceDisplay === 'Markup' && (
                <>
                  <Input
                    label="Varsayılan Markup (%)"
                    type="number"
                    min={0}
                    max={100}
                    value={authFormData.defaultMarkupRate || ''}
                    onChange={(e) => setAuthFormData({
                      ...authFormData,
                      defaultMarkupRate: e.target.value ? parseFloat(e.target.value) : null,
                    })}
                  />
                  <Input
                    label="Maks. Markup (%)"
                    type="number"
                    min={0}
                    max={100}
                    value={authFormData.maxMarkupRate || ''}
                    onChange={(e) => setAuthFormData({
                      ...authFormData,
                      maxMarkupRate: e.target.value ? parseFloat(e.target.value) : null,
                    })}
                  />
                </>
              )}
            </div>
          </div>

          {/* Allotment */}
          <div>
            <label className="flex items-center gap-3 mb-3">
              <input
                type="checkbox"
                checked={authFormData.hasAllotment}
                onChange={(e) => setAuthFormData({
                  ...authFormData,
                  hasAllotment: e.target.checked,
                  totalAllotment: e.target.checked ? authFormData.totalAllotment || 10 : null,
                })}
                className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm font-medium text-gray-700">Kontenjan Sınırlaması</span>
            </label>
            {authFormData.hasAllotment && (
              <Input
                label="Toplam Kontenjan"
                type="number"
                min={1}
                value={authFormData.totalAllotment || ''}
                onChange={(e) => setAuthFormData({
                  ...authFormData,
                  totalAllotment: parseInt(e.target.value),
                })}
              />
            )}
          </div>

          {/* Date Range */}
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Geçerlilik Başlangıç"
              type="date"
              value={authFormData.validFrom}
              onChange={(e) => setAuthFormData({ ...authFormData, validFrom: e.target.value })}
            />
            <Input
              label="Geçerlilik Bitiş"
              type="date"
              value={authFormData.validTo}
              onChange={(e) => setAuthFormData({ ...authFormData, validTo: e.target.value })}
            />
          </div>

          {/* Notes */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Notlar</label>
            <textarea
              rows={2}
              value={authFormData.notes}
              onChange={(e) => setAuthFormData({ ...authFormData, notes: e.target.value })}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="Yetkilendirme ile ilgili notlar..."
            />
          </div>
        </div>
      </Modal>

      {/* Authorization Detail Modal */}
      <Modal
        isOpen={showDetailModal}
        onClose={() => setShowDetailModal(false)}
        title="Yetkilendirme Detayı"
        size="lg"
      >
        {selectedAuth && (
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs text-gray-500">Acente</label>
                <p className="font-medium">{selectedAuth.agencyName}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Mülk</label>
                <p className="font-medium">{selectedAuth.propertyName}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Yetki Seviyesi</label>
                <div className="mt-1">{getLevelBadge(selectedAuth.level)}</div>
              </div>
              <div>
                <label className="text-xs text-gray-500">Fiyat Gösterimi</label>
                <div className="mt-1">{getPriceDisplayBadge(selectedAuth.priceDisplay)}</div>
              </div>
              <div>
                <label className="text-xs text-gray-500">Komisyon</label>
                <p className="font-medium">%{selectedAuth.customCommissionRate || 'Varsayılan'}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Kontenjan</label>
                <p className="font-medium">
                  {selectedAuth.hasAllotment
                    ? `${selectedAuth.usedAllotment} / ${selectedAuth.totalAllotment}`
                    : 'Sınırsız'}
                </p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Veriliş Tarihi</label>
                <p className="font-medium">{formatDateTime(selectedAuth.grantedAt)}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Durum</label>
                <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                  selectedAuth.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                }`}>
                  {selectedAuth.isActive ? 'Aktif' : 'İptal Edilmiş'}
                </span>
              </div>
            </div>

            {selectedAuth.notes && (
              <div>
                <label className="text-xs text-gray-500">Notlar</label>
                <p className="text-sm mt-1 bg-gray-50 p-3 rounded-lg">{selectedAuth.notes}</p>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Revoke Confirmation */}
      <ConfirmDialog
        isOpen={!!revokeAuth}
        onClose={() => setRevokeAuth(null)}
        onConfirm={() => revokeAuth && revokeMutation.mutate(revokeAuth.id)}
        title="Yetkilendirmeyi İptal Et"
        message={`"${revokeAuth?.agencyName}" acentesinin "${revokeAuth?.propertyName}" mülkü için yetkilendirmesini iptal etmek istediğinize emin misiniz?`}
        confirmLabel="İptal Et"
        variant="danger"
        isLoading={revokeMutation.isPending}
      />
    </div>
  );
}
