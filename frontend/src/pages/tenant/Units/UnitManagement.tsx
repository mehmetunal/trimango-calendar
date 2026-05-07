// src/pages/tenant/Units/UnitManagement.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import {
  Plus,
  Edit,
  Trash2,
  BedDouble,
  Users,
  DollarSign,
  Ruler,
  Eye,
  Power,
  ArrowUpDown,
  Save,
  X,
} from 'lucide-react';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Modal, Card, Badge, ConfirmDialog } from '../../../components/ui';
import { formatCurrency } from '../../../utils/format';
import toast from 'react-hot-toast';

interface UnitFormData {
  name: string;
  unitNumber: string;
  floor: number;
  maxAdults: number;
  maxChildren: number;
  maxInfants: number;
  basePrice: number;
  currencyCode: string;
  size: number | null;
  view: string;
  description: string;
}

export default function UnitManagement() {
  const { propertyId } = useParams<{ propertyId: string }>();
  const queryClient = useQueryClient();
  
  // States
  const [showForm, setShowForm] = useState(false);
  const [editingUnit, setEditingUnit] = useState<any>(null);
  const [deleteUnit, setDeleteUnit] = useState<any>(null);
  const [bulkEditMode, setBulkEditMode] = useState(false);
  const [selectedUnits, setSelectedUnits] = useState<string[]>([]);
  
  // Form state
  const [formData, setFormData] = useState<UnitFormData>({
    name: '',
    unitNumber: '',
    floor: 1,
    maxAdults: 2,
    maxChildren: 0,
    maxInfants: 0,
    basePrice: 0,
    currencyCode: 'TRY',
    size: null,
    view: '',
    description: '',
  });

  const { data: units, isLoading } = useQuery({
    queryKey: ['units', propertyId],
    queryFn: () => propertyApi.getUnits(propertyId!),
    enabled: !!propertyId,
  });

  const { data: property } = useQuery({
    queryKey: ['property', propertyId],
    queryFn: () => propertyApi.getById(propertyId!),
    enabled: !!propertyId,
  });

  // Create/Update mutation
  const saveMutation = useMutation({
    mutationFn: (data: any) => {
      if (editingUnit) {
        return propertyApi.updateUnit(editingUnit.id, data);
      }
      return propertyApi.createUnit(propertyId!, data);
    },
    onSuccess: () => {
      toast.success(editingUnit ? 'Birim güncellendi' : 'Birim oluşturuldu');
      queryClient.invalidateQueries({ queryKey: ['units', propertyId] });
      queryClient.invalidateQueries({ queryKey: ['property', propertyId] });
      resetForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: string) => propertyApi.deleteUnit(id),
    onSuccess: () => {
      toast.success('Birim silindi');
      queryClient.invalidateQueries({ queryKey: ['units', propertyId] });
      setDeleteUnit(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  // Toggle mutation
  const toggleMutation = useMutation({
    mutationFn: (id: string) => propertyApi.updateUnit(id, { isActive: false }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units', propertyId] });
    },
  });

  // Bulk price update
  const bulkPriceMutation = useMutation({
    mutationFn: async (data: { price: number; currencyCode: string }) => {
      const promises = selectedUnits.map(unitId =>
        propertyApi.updateUnit(unitId, { basePrice: data.price, currencyCode: data.currencyCode })
      );
      await Promise.all(promises);
    },
    onSuccess: () => {
      toast.success(`${selectedUnits.length} birimin fiyatı güncellendi`);
      queryClient.invalidateQueries({ queryKey: ['units', propertyId] });
      setBulkEditMode(false);
      setSelectedUnits([]);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const resetForm = () => {
    setFormData({
      name: '',
      unitNumber: '',
      floor: 1,
      maxAdults: 2,
      maxChildren: 0,
      maxInfants: 0,
      basePrice: 0,
      currencyCode: 'TRY',
      size: null,
      view: '',
      description: '',
    });
    setEditingUnit(null);
    setShowForm(false);
  };

  const handleEdit = (unit: any) => {
    setEditingUnit(unit);
    setFormData({
      name: unit.name,
      unitNumber: unit.unitNumber,
      floor: unit.floor,
      maxAdults: unit.maxAdults,
      maxChildren: unit.maxChildren,
      maxInfants: unit.maxInfants || 0,
      basePrice: unit.basePrice,
      currencyCode: unit.currencyCode,
      size: unit.size,
      view: unit.view || '',
      description: unit.description || '',
    });
    setShowForm(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    saveMutation.mutate(formData);
  };

  const toggleSelectAll = () => {
    if (selectedUnits.length === units?.length) {
      setSelectedUnits([]);
    } else {
      setSelectedUnits(units?.map(u => u.id) || []);
    }
  };

  const toggleSelectUnit = (unitId: string) => {
    setSelectedUnits(prev =>
      prev.includes(unitId)
        ? prev.filter(id => id !== unitId)
        : [...prev, unitId]
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {property?.name} - Birim Yönetimi
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            {units?.length || 0} birim • Toplam kapasite: {units?.reduce((sum, u) => sum + u.maxAdults + u.maxChildren, 0) || 0} kişi
          </p>
        </div>
        <div className="flex items-center gap-3">
          {!bulkEditMode ? (
            <>
              <Button variant="outline" size="sm" onClick={() => setBulkEditMode(true)}>
                Toplu Fiyat Güncelle
              </Button>
              <Button size="sm" leftIcon={<Plus className="w-4 h-4" />} onClick={() => setShowForm(true)}>
                Birim Ekle
              </Button>
            </>
          ) : (
            <>
              <span className="text-sm text-gray-600">
                {selectedUnits.length} birim seçildi
              </span>
              <Button variant="outline" size="sm" onClick={() => { setBulkEditMode(false); setSelectedUnits([]); }}>
                İptal
              </Button>
            </>
          )}
        </div>
      </div>

      {/* Bulk Price Edit Bar */}
      {bulkEditMode && selectedUnits.length > 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
          <div className="flex items-center gap-4">
            <span className="text-sm font-medium text-blue-900">Toplu Fiyat Güncelle:</span>
            <input
              type="number"
              placeholder="Yeni fiyat"
              className="px-3 py-1.5 border rounded-lg text-sm w-32"
              id="bulkPrice"
            />
            <select className="px-3 py-1.5 border rounded-lg text-sm">
              <option value="TRY">₺ TRY</option>
              <option value="USD">$ USD</option>
              <option value="EUR">€ EUR</option>
            </select>
            <Button
              size="sm"
              onClick={() => {
                const price = parseFloat((document.getElementById('bulkPrice') as HTMLInputElement).value);
                if (price > 0) {
                  bulkPriceMutation.mutate({ price, currencyCode: 'TRY' });
                }
              }}
              isLoading={bulkPriceMutation.isPending}
            >
              Güncelle
            </Button>
          </div>
        </div>
      )}

      {/* Units Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {units?.map((unit) => (
          <Card key={unit.id} className={`relative ${!unit.isActive ? 'opacity-60' : ''}`}>
            {/* Checkbox for bulk edit */}
            {bulkEditMode && (
              <div className="absolute top-3 left-3 z-10">
                <input
                  type="checkbox"
                  checked={selectedUnits.includes(unit.id)}
                  onChange={() => toggleSelectUnit(unit.id)}
                  className="w-5 h-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
              </div>
            )}

            <div className="p-5">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <h3 className="font-semibold text-gray-900">{unit.name}</h3>
                  {unit.unitNumber && (
                    <p className="text-sm text-gray-500">No: {unit.unitNumber}</p>
                  )}
                </div>
                <div className="flex items-center gap-1">
                  <button
                    onClick={() => handleEdit(unit)}
                    className="p-1.5 rounded-lg hover:bg-blue-50 text-gray-400 hover:text-blue-600"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => toggleMutation.mutate(unit.id)}
                    className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-gray-600"
                  >
                    <Power className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => setDeleteUnit(unit)}
                    className="p-1.5 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 mb-4">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <Users className="w-4 h-4 text-gray-400" />
                  <span>{unit.maxAdults} Y {unit.maxChildren > 0 && `/ ${unit.maxChildren} Ç`}</span>
                </div>
                {unit.size && (
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Ruler className="w-4 h-4 text-gray-400" />
                    <span>{unit.size} m²</span>
                  </div>
                )}
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <BedDouble className="w-4 h-4 text-gray-400" />
                  <span>Kat {unit.floor}</span>
                </div>
                {unit.view && (
                  <div className="flex items-center gap-2 text-sm text-gray-600">
                    <Eye className="w-4 h-4 text-gray-400" />
                    <span>{unit.view}</span>
                  </div>
                )}
              </div>

              <div className="pt-3 border-t">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-500">Gecelik fiyat</span>
                  <span className="text-lg font-bold text-blue-600">
                    {formatCurrency(unit.basePrice, unit.currencyCode)}
                  </span>
                </div>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {/* Empty State */}
      {!isLoading && units?.length === 0 && (
        <div className="text-center py-16">
          <BedDouble className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz birim eklenmemiş</h3>
          <p className="text-sm text-gray-500 mb-6">Bu mülke ait oda veya birim ekleyin</p>
          <Button onClick={() => setShowForm(true)} leftIcon={<Plus className="w-4 h-4" />}>
            İlk Birimi Ekle
          </Button>
        </div>
      )}

      {/* Unit Form Modal */}
      <Modal
        isOpen={showForm}
        onClose={resetForm}
        title={editingUnit ? 'Birim Düzenle' : 'Yeni Birim Ekle'}
        size="lg"
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={resetForm}>
              İptal
            </Button>
            <Button onClick={handleSubmit} isLoading={saveMutation.isPending}>
              {editingUnit ? 'Güncelle' : 'Ekle'}
            </Button>
          </div>
        }
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Birim Adı *"
              placeholder="Örn: Standart Oda"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
            <Input
              label="Birim No"
              placeholder="Örn: 101"
              value={formData.unitNumber}
              onChange={(e) => setFormData({ ...formData, unitNumber: e.target.value })}
            />
          </div>

          <div className="grid grid-cols-3 gap-4">
            <Input
              label="Kat"
              type="number"
              value={formData.floor}
              onChange={(e) => setFormData({ ...formData, floor: parseInt(e.target.value) })}
            />
            <Input
              label="Maks. Yetişkin"
              type="number"
              min={1}
              max={20}
              value={formData.maxAdults}
              onChange={(e) => setFormData({ ...formData, maxAdults: parseInt(e.target.value) })}
            />
            <Input
              label="Maks. Çocuk"
              type="number"
              min={0}
              max={10}
              value={formData.maxChildren}
              onChange={(e) => setFormData({ ...formData, maxChildren: parseInt(e.target.value) })}
            />
          </div>

          <div className="grid grid-cols-3 gap-4">
            <Input
              label="Gecelik Fiyat *"
              type="number"
              min={0}
              step="0.01"
              value={formData.basePrice}
              onChange={(e) => setFormData({ ...formData, basePrice: parseFloat(e.target.value) })}
              required
            />
            <Select
              label="Para Birimi"
              value={formData.currencyCode}
              onChange={(e) => setFormData({ ...formData, currencyCode: e.target.value })}
              options={[
                { value: 'TRY', label: '₺ TRY' },
                { value: 'USD', label: '$ USD' },
                { value: 'EUR', label: '€ EUR' },
                { value: 'GBP', label: '£ GBP' },
              ]}
            />
            <Input
              label="Büyüklük (m²)"
              type="number"
              value={formData.size || ''}
              onChange={(e) => setFormData({ ...formData, size: e.target.value ? parseFloat(e.target.value) : null })}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Manzara"
              placeholder="Örn: Deniz, Dağ, Şehir"
              value={formData.view}
              onChange={(e) => setFormData({ ...formData, view: e.target.value })}
            />
            <Input
              label="Bebek Kapasitesi"
              type="number"
              min={0}
              max={5}
              value={formData.maxInfants}
              onChange={(e) => setFormData({ ...formData, maxInfants: parseInt(e.target.value) })}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Açıklama</label>
            <textarea
              rows={3}
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500"
              placeholder="Birim hakkında açıklama..."
            />
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteUnit}
        onClose={() => setDeleteUnit(null)}
        onConfirm={() => deleteUnit && deleteMutation.mutate(deleteUnit.id)}
        title="Birimi Sil"
        message={`"${deleteUnit?.name}" birimini silmek istediğinize emin misiniz? Bu birime ait tüm rezervasyonlar da etkilenecektir.`}
        confirmLabel="Sil"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}