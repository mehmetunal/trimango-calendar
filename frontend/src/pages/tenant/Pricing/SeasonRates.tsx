// src/pages/tenant/Pricing/SeasonRates.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Plus,
  Edit,
  Trash2,
  Calendar,
  DollarSign,
  Sun,
  Moon,
  Star,
  Clock,
  Filter,
  Search,
} from 'lucide-react';
import { pricingApi } from '../../../api/pricing.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Card, Badge, Modal, ConfirmDialog } from '../../../components/ui';
import { formatCurrency, formatDate } from '../../../utils/format';
import { CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function SeasonRates() {
  const queryClient = useQueryClient();
  
  const [selectedPropertyId, setSelectedPropertyId] = useState('');
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingRate, setEditingRate] = useState<any>(null);
  const [deleteRate, setDeleteRate] = useState<any>(null);
  
  const [formData, setFormData] = useState({
    name: '',
    startDate: '',
    endDate: '',
    weekdayPrice: 0,
    weekendPrice: null as number | null,
    specialDayPrice: null as number | null,
    currencyCode: 'TRY',
    minStayDays: 1,
    maxStayDays: 30,
    cancellationPolicy: 'Flexible',
    freeCancellationDays: 7,
    cancellationFee: null as number | null,
  });

  const { data: properties } = useQuery({
    queryKey: ['properties'],
    queryFn: () => propertyApi.getAll({ pageSize: 1000, isActive: true }),
  });

  const { data: units } = useQuery({
    queryKey: ['units', selectedPropertyId],
    queryFn: () => propertyApi.getUnits(selectedPropertyId),
    enabled: !!selectedPropertyId,
  });

  const { data: seasonRates, isLoading } = useQuery({
    queryKey: ['seasonRates', selectedUnitId],
    queryFn: () => pricingApi.getSeasonRates(selectedUnitId),
    enabled: !!selectedUnitId,
  });

  const saveMutation = useMutation({
    mutationFn: (data: any) => {
      if (editingRate) {
        return pricingApi.updateSeasonRate(editingRate.id, data);
      }
      return pricingApi.createSeasonRate(data);
    },
    onSuccess: () => {
      toast.success(editingRate ? 'Sezon güncellendi' : 'Sezon oluşturuldu');
      queryClient.invalidateQueries({ queryKey: ['seasonRates', selectedUnitId] });
      resetForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => pricingApi.deleteSeasonRate(id),
    onSuccess: () => {
      toast.success('Sezon silindi');
      queryClient.invalidateQueries({ queryKey: ['seasonRates', selectedUnitId] });
      setDeleteRate(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const resetForm = () => {
    setFormData({
      name: '',
      startDate: '',
      endDate: '',
      weekdayPrice: 0,
      weekendPrice: null,
      specialDayPrice: null,
      currencyCode: 'TRY',
      minStayDays: 1,
      maxStayDays: 30,
      cancellationPolicy: 'Flexible',
      freeCancellationDays: 7,
      cancellationFee: null,
    });
    setEditingRate(null);
    setShowForm(false);
  };

  const handleEdit = (rate: any) => {
    setEditingRate(rate);
    setFormData({
      name: rate.name,
      startDate: rate.startDate.split('T')[0],
      endDate: rate.endDate.split('T')[0],
      weekdayPrice: rate.weekdayPrice,
      weekendPrice: rate.weekendPrice,
      specialDayPrice: rate.specialDayPrice,
      currencyCode: rate.currencyCode,
      minStayDays: rate.minStayDays,
      maxStayDays: rate.maxStayDays,
      cancellationPolicy: rate.cancellationPolicy || 'Flexible',
      freeCancellationDays: rate.freeCancellationDays || 7,
      cancellationFee: rate.cancellationFee,
    });
    setShowForm(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    saveMutation.mutate({ ...formData, unitId: selectedUnitId });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Sezon Fiyatları</h1>
          <p className="text-sm text-gray-500 mt-1">Sezonluk fiyatlandırma yönetimi</p>
        </div>
        <Button
          onClick={() => setShowForm(true)}
          disabled={!selectedUnitId}
          leftIcon={<Plus className="w-4 h-4" />}
        >
          Sezon Ekle
        </Button>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            label="Mülk"
            value={selectedPropertyId}
            onChange={(e) => {
              setSelectedPropertyId(e.target.value);
              setSelectedUnitId('');
            }}
            options={[
              { value: '', label: 'Mülk seçin...' },
              ...(properties?.items?.map((p: any) => ({
                value: p.id,
                label: p.name,
              })) || []),
            ]}
          />
          <Select
            label="Birim"
            value={selectedUnitId}
            onChange={(e) => setSelectedUnitId(e.target.value)}
            options={[
              { value: '', label: 'Birim seçin...' },
              ...(units?.map((u: any) => ({
                value: u.id,
                label: `${u.name} (${formatCurrency(u.basePrice, u.currencyCode)})`,
              })) || []),
            ]}
            disabled={!selectedPropertyId}
          />
        </div>
      </Card>

      {/* Season Rates List */}
      {selectedUnitId && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {seasonRates?.map((rate) => (
            <Card key={rate.id} className={`${!rate.isActive ? 'opacity-60' : ''}`}>
              <div className="p-5">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <h3 className="font-semibold text-gray-900">{rate.name}</h3>
                    <p className="text-xs text-gray-500">
                      {formatDate(rate.startDate)} - {formatDate(rate.endDate)}
                    </p>
                  </div>
                  <Badge color={rate.isActive ? 'green' : 'gray'}>
                    {rate.isActive ? 'Aktif' : 'Pasif'}
                  </Badge>
                </div>

                <div className="space-y-2 mb-4">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-500">Hafta içi</span>
                    <span className="font-medium">
                      {formatCurrency(rate.weekdayPrice, rate.currencyCode)}
                    </span>
                  </div>
                  {rate.weekendPrice && (
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-500">Hafta sonu</span>
                      <span className="font-medium text-amber-600">
                        {formatCurrency(rate.weekendPrice, rate.currencyCode)}
                      </span>
                    </div>
                  )}
                  {rate.specialDayPrice && (
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-500">Özel gün</span>
                      <span className="font-medium text-purple-600">
                        {formatCurrency(rate.specialDayPrice, rate.currencyCode)}
                      </span>
                    </div>
                  )}
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-gray-500">Min. gece</span>
                    <span className="font-medium">{rate.minStayDays}</span>
                  </div>
                </div>

                <div className="flex items-center gap-2 pt-3 border-t">
                  <Button size="sm" variant="ghost" onClick={() => handleEdit(rate)}>
                    <Edit className="w-4 h-4" />
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setDeleteRate(rate)}>
                    <Trash2 className="w-4 h-4 text-red-500" />
                  </Button>
                </div>
              </div>
            </Card>
          ))}

          {!isLoading && seasonRates?.length === 0 && (
            <div className="col-span-full text-center py-12 text-gray-500">
              <Calendar className="w-12 h-12 mx-auto mb-3 text-gray-300" />
              <p>Henüz sezon tanımlanmamış</p>
            </div>
          )}
        </div>
      )}

      {/* Season Form Modal */}
      <Modal
        isOpen={showForm}
        onClose={resetForm}
        title={editingRate ? 'Sezonu Düzenle' : 'Yeni Sezon'}
        size="lg"
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={resetForm}>İptal</Button>
            <Button onClick={handleSubmit} isLoading={saveMutation.isPending}>
              {editingRate ? 'Güncelle' : 'Ekle'}
            </Button>
          </div>
        }
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Sezon Adı *"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              placeholder="Örn: Yaz Sezonu 2024"
              required
            />
            <Select
              label="Para Birimi"
              value={formData.currencyCode}
              onChange={(e) => setFormData({ ...formData, currencyCode: e.target.value })}
              options={CURRENCIES.map(c => ({ value: c.code, label: `${c.symbol} ${c.name}` }))}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Başlangıç *"
              type="date"
              value={formData.startDate}
              onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
              required
            />
            <Input
              label="Bitiş *"
              type="date"
              value={formData.endDate}
              onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
              required
            />
          </div>

          <div className="grid grid-cols-3 gap-4">
            <Input
              label="Hafta İçi Fiyat *"
              type="number"
              min={0}
              step="0.01"
              value={formData.weekdayPrice}
              onChange={(e) => setFormData({ ...formData, weekdayPrice: parseFloat(e.target.value) })}
              required
            />
            <Input
              label="Hafta Sonu Fiyat"
              type="number"
              min={0}
              step="0.01"
              value={formData.weekendPrice || ''}
              onChange={(e) => setFormData({ ...formData, weekendPrice: e.target.value ? parseFloat(e.target.value) : null })}
            />
            <Input
              label="Özel Gün Fiyatı"
              type="number"
              min={0}
              step="0.01"
              value={formData.specialDayPrice || ''}
              onChange={(e) => setFormData({ ...formData, specialDayPrice: e.target.value ? parseFloat(e.target.value) : null })}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Min. Gece"
              type="number"
              min={1}
              value={formData.minStayDays}
              onChange={(e) => setFormData({ ...formData, minStayDays: parseInt(e.target.value) })}
            />
            <Input
              label="Maks. Gece"
              type="number"
              min={1}
              value={formData.maxStayDays}
              onChange={(e) => setFormData({ ...formData, maxStayDays: parseInt(e.target.value) })}
            />
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteRate}
        onClose={() => setDeleteRate(null)}
        onConfirm={() => deleteRate && deleteMutation.mutate(deleteRate.id)}
        title="Sezonu Sil"
        message={`"${deleteRate?.name}" sezonunu silmek istediğinize emin misiniz?`}
        confirmLabel="Sil"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}