// src/pages/tenant/Widgets/WidgetSettings.tsx
import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Plus,
  Copy,
  Code,
  Eye,
  Settings,
  Globe,
  Palette,
  Image,
  MessageSquare,
  DollarSign,
  Calendar,
  Check,
  X,
  RefreshCw,
  ExternalLink,
  Smartphone,
  Monitor,
  Tablet,
  ChevronDown,
  Upload,
  Trash2,
  Save,
  CheckCircle,
  AlertCircle,
  Info,
} from 'lucide-react';
import { widgetApi } from '../../../api/widget.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Modal, Card, Tabs, Badge, ConfirmDialog } from '../../../components/ui';
import { formatDate, formatCurrency } from '../../../utils/format';
import { PROPERTY_TYPES, CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

// Types
interface BookingWidget {
  id: string;
  propertyId: string;
  propertyName: string;
  widgetKey: string;
  theme: string;
  primaryColor: string;
  secondaryColor: string;
  fontFamily: string;
  showPropertyImages: boolean;
  showAmenities: boolean;
  showReviews: boolean;
  showPriceBreakdown: boolean;
  position: 'Left' | 'Right' | 'Center' | 'FullPage' | 'Embed';
  customCSS: string;
  metaTitle: string;
  metaDescription: string;
  sharingImage: string;
  requirePayment: boolean;
  minAdvanceDays: number;
  maxAdvanceDays: number;
  defaultLanguage: string;
  availableLanguages: string[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  integrations: WidgetIntegration[];
}

interface WidgetIntegration {
  id: string;
  domain: string;
  isActive: boolean;
  createdAt: string;
}

interface WidgetFormData {
  propertyId: string;
  theme: string;
  primaryColor: string;
  secondaryColor: string;
  fontFamily: string;
  showPropertyImages: boolean;
  showAmenities: boolean;
  showReviews: boolean;
  showPriceBreakdown: boolean;
  position: string;
  requirePayment: boolean;
  minAdvanceDays: number;
  maxAdvanceDays: number;
  defaultLanguage: string;
  availableLanguages: string[];
  metaTitle: string;
  metaDescription: string;
}

const THEMES = [
  { value: 'default', label: 'Varsayılan', preview: 'bg-gradient-to-r from-blue-500 to-blue-600' },
  { value: 'modern', label: 'Modern', preview: 'bg-gradient-to-r from-gray-700 to-gray-900' },
  { value: 'minimal', label: 'Minimal', preview: 'bg-gradient-to-r from-white to-gray-100 border' },
  { value: 'elegant', label: 'Zarif', preview: 'bg-gradient-to-r from-amber-500 to-amber-700' },
  { value: 'nature', label: 'Doğa', preview: 'bg-gradient-to-r from-emerald-500 to-teal-600' },
];

const FONT_FAMILIES = [
  { value: 'Inter, sans-serif', label: 'Inter (Modern)' },
  { value: 'Poppins, sans-serif', label: 'Poppins (Yuvarlak)' },
  { value: 'Roboto, sans-serif', label: 'Roboto (Klasik)' },
  { value: 'Playfair Display, serif', label: 'Playfair (Zarif)' },
  { value: 'Montserrat, sans-serif', label: 'Montserrat (Kalın)' },
];

const LANGUAGES = [
  { value: 'tr', label: '🇹🇷 Türkçe' },
  { value: 'en', label: '🇬🇧 English' },
  { value: 'de', label: '🇩🇪 Deutsch' },
  { value: 'ru', label: '🇷🇺 Русский' },
  { value: 'ar', label: '🇸🇦 العربية' },
];

const COLORS = [
  '#2563EB', '#1D4ED8', '#3B82F6', '#60A5FA', // Blue
  '#059669', '#047857', '#10B981', '#34D399', // Green
  '#DC2626', '#B91C1C', '#EF4444', '#F87171', // Red
  '#D97706', '#B45309', '#F59E0B', '#FBBF24', // Amber
  '#7C3AED', '#6D28D9', '#8B5CF6', '#A78BFA', // Purple
  '#DB2777', '#BE185D', '#EC4899', '#F472B6', // Pink
];

export default function WidgetSettings() {
  const queryClient = useQueryClient();
  
  // States
  const [activeTab, setActiveTab] = useState<'list' | 'create' | 'edit'>('list');
  const [selectedWidget, setSelectedWidget] = useState<BookingWidget | null>(null);
  const [showPreview, setShowPreview] = useState(false);
  const [previewDevice, setPreviewDevice] = useState<'desktop' | 'tablet' | 'mobile'>('desktop');
  const [deleteWidget, setDeleteWidget] = useState<BookingWidget | null>(null);
  const [showIntegrationModal, setShowIntegrationModal] = useState(false);
  const [newDomain, setNewDomain] = useState('');
  
  // Form
  const [formData, setFormData] = useState<WidgetFormData>({
    propertyId: '',
    theme: 'default',
    primaryColor: '#2563EB',
    secondaryColor: '#1D4ED8',
    fontFamily: 'Inter, sans-serif',
    showPropertyImages: true,
    showAmenities: true,
    showReviews: true,
    showPriceBreakdown: true,
    position: 'Right',
    requirePayment: false,
    minAdvanceDays: 0,
    maxAdvanceDays: 365,
    defaultLanguage: 'tr',
    availableLanguages: ['tr', 'en'],
    metaTitle: '',
    metaDescription: '',
  });

  // Queries
  const { data: widgets, isLoading: widgetsLoading } = useQuery({
    queryKey: ['widgets'],
    queryFn: () => widgetApi.getAll(),
  });

  const { data: properties } = useQuery({
    queryKey: ['properties', 'all'],
    queryFn: () => propertyApi.getAll({ pageSize: 1000, isActive: true }),
  });

  // Mutations
  const createWidgetMutation = useMutation({
    mutationFn: (data: any) => widgetApi.create(data),
    onSuccess: () => {
      toast.success('Widget oluşturuldu');
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
      setActiveTab('list');
      resetForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  const updateWidgetMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => widgetApi.update(id, data),
    onSuccess: () => {
      toast.success('Widget güncellendi');
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
      setActiveTab('list');
      setSelectedWidget(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const deleteWidgetMutation = useMutation({
    mutationFn: (id: string) => widgetApi.delete(id),
    onSuccess: () => {
      toast.success('Widget silindi');
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
      setDeleteWidget(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const toggleWidgetMutation = useMutation({
    mutationFn: (id: string) => widgetApi.toggleActive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
      toast.success('Widget durumu güncellendi');
    },
    onError: (error: any) => toast.error(error.message),
  });

  const addDomainMutation = useMutation({
    mutationFn: ({ widgetId, domain }: { widgetId: string; domain: string }) =>
      widgetApi.addDomain(widgetId, domain),
    onSuccess: () => {
      toast.success('Domain eklendi');
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
      setNewDomain('');
      setShowIntegrationModal(false);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const removeDomainMutation = useMutation({
    mutationFn: ({ widgetId, integrationId }: { widgetId: string; integrationId: string }) =>
      widgetApi.removeDomain(widgetId, integrationId),
    onSuccess: () => {
      toast.success('Domain kaldırıldı');
      queryClient.invalidateQueries({ queryKey: ['widgets'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const resetForm = () => {
    setFormData({
      propertyId: '',
      theme: 'default',
      primaryColor: '#2563EB',
      secondaryColor: '#1D4ED8',
      fontFamily: 'Inter, sans-serif',
      showPropertyImages: true,
      showAmenities: true,
      showReviews: true,
      showPriceBreakdown: true,
      position: 'Right',
      requirePayment: false,
      minAdvanceDays: 0,
      maxAdvanceDays: 365,
      defaultLanguage: 'tr',
      availableLanguages: ['tr', 'en'],
      metaTitle: '',
      metaDescription: '',
    });
  };

  const handleEdit = (widget: BookingWidget) => {
    setSelectedWidget(widget);
    setFormData({
      propertyId: widget.propertyId,
      theme: widget.theme,
      primaryColor: widget.primaryColor,
      secondaryColor: widget.secondaryColor,
      fontFamily: widget.fontFamily,
      showPropertyImages: widget.showPropertyImages,
      showAmenities: widget.showAmenities,
      showReviews: widget.showReviews,
      showPriceBreakdown: widget.showPriceBreakdown,
      position: widget.position,
      requirePayment: widget.requirePayment,
      minAdvanceDays: widget.minAdvanceDays,
      maxAdvanceDays: widget.maxAdvanceDays,
      defaultLanguage: widget.defaultLanguage,
      availableLanguages: widget.availableLanguages,
      metaTitle: widget.metaTitle || '',
      metaDescription: widget.metaDescription || '',
    });
    setActiveTab('edit');
  };

  const handleCopyEmbed = (widgetKey: string) => {
    const embedCode = `<div id="hp-booking-widget" data-widget-key="${widgetKey}"></div>
<script src="https://yourdomain.com/widget.js"></script>`;
    
    navigator.clipboard.writeText(embedCode).then(() => {
      toast.success('Embed kodu kopyalandı');
    });
  };

  const getPositionLabel = (position: string) => {
    const labels: Record<string, string> = {
      Left: 'Sol Alt',
      Right: 'Sağ Alt',
      Center: 'Ortada Modal',
      FullPage: 'Tam Sayfa',
      Embed: 'Embed (iframe)',
    };
    return labels[position] || position;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Booking Widget</h1>
          <p className="text-sm text-gray-500 mt-1">
            Web sitenize entegre edilebilir online rezervasyon widget'ı
          </p>
        </div>
        
        {activeTab === 'list' && (
          <Button
            onClick={() => { resetForm(); setActiveTab('create'); }}
            leftIcon={<Plus className="w-4 h-4" />}
          >
            Yeni Widget Oluştur
          </Button>
        )}
      </div>

      {/* Widget List */}
      {activeTab === 'list' && (
        <div className="space-y-4">
          {widgetsLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {[...Array(4)].map((_, i) => (
                <Card key={i} className="p-6 animate-pulse">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-4" />
                  <div className="h-4 bg-gray-200 rounded w-1/2 mb-2" />
                  <div className="h-4 bg-gray-200 rounded w-1/4" />
                </Card>
              ))}
            </div>
          ) : widgets?.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {widgets.map((widget: BookingWidget) => (
                <Card key={widget.id} className={`${!widget.isActive ? 'opacity-60' : ''}`}>
                  <div className="p-6">
                    <div className="flex items-start justify-between mb-4">
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="font-semibold text-gray-900">{widget.propertyName}</h3>
                          <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                            widget.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                          }`}>
                            {widget.isActive ? 'Aktif' : 'Pasif'}
                          </span>
                        </div>
                        <p className="text-xs text-gray-500 font-mono">Key: {widget.widgetKey}</p>
                      </div>
                      <div className={`px-3 py-1 rounded-lg text-xs font-medium ${
                        widget.theme === 'default' ? 'bg-blue-100 text-blue-700' :
                        widget.theme === 'modern' ? 'bg-gray-100 text-gray-700' :
                        widget.theme === 'minimal' ? 'bg-gray-100 text-gray-700' :
                        'bg-amber-100 text-amber-700'
                      }`}>
                        {THEMES.find(t => t.value === widget.theme)?.label || widget.theme}
                      </div>
                    </div>

                    {/* Widget Preview */}
                    <div className="relative mb-4 p-4 rounded-lg bg-gray-50 border">
                      <div className="flex items-center gap-2 mb-3">
                        <div className="w-3 h-3 rounded-full bg-red-400" />
                        <div className="w-3 h-3 rounded-full bg-yellow-400" />
                        <div className="w-3 h-3 rounded-full bg-green-400" />
                        <span className="text-xs text-gray-400 ml-2">Widget Önizleme</span>
                      </div>
                      <div className="space-y-2">
                        <div className="h-3 rounded" style={{ backgroundColor: widget.primaryColor, width: '60%' }} />
                        <div className="h-2 bg-gray-300 rounded w-3/4" />
                        <div className="h-2 bg-gray-300 rounded w-1/2" />
                        <div className="flex gap-2 mt-3">
                          <div className="h-8 w-20 rounded" style={{ backgroundColor: widget.primaryColor }} />
                          <div className="h-8 w-20 rounded bg-gray-200" />
                        </div>
                      </div>
                    </div>

                    {/* Widget Info */}
                    <div className="grid grid-cols-2 gap-3 mb-4 text-sm">
                      <div className="flex items-center gap-2">
                        <Globe className="w-4 h-4 text-gray-400" />
                        <span>{getPositionLabel(widget.position)}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <MessageSquare className="w-4 h-4 text-gray-400" />
                        <span>{widget.availableLanguages?.length || 0} dil</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <Calendar className="w-4 h-4 text-gray-400" />
                        <span>{widget.minAdvanceDays}-{widget.maxAdvanceDays} gün</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <Globe className="w-4 h-4 text-gray-400" />
                        <span>{widget.integrations?.length || 0} domain</span>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="flex items-center gap-2 pt-4 border-t">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleCopyEmbed(widget.widgetKey)}
                        leftIcon={<Code className="w-4 h-4" />}
                      >
                        Embed Kodu
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => window.open(`/widget/${widget.widgetKey}`, '_blank')}
                        leftIcon={<ExternalLink className="w-4 h-4" />}
                      >
                        Önizle
                      </Button>
                      <Button
                        size="sm"
                        variant="ghost"
                        onClick={() => handleEdit(widget)}
                        leftIcon={<Settings className="w-4 h-4" />}
                      >
                        Düzenle
                      </Button>
                      <div className="ml-auto flex items-center gap-1">
                        <button
                          onClick={() => toggleWidgetMutation.mutate(widget.id)}
                          className={`p-1.5 rounded-lg transition-colors ${
                            widget.isActive
                              ? 'hover:bg-red-50 text-gray-400 hover:text-red-600'
                              : 'hover:bg-green-50 text-gray-400 hover:text-green-600'
                          }`}
                          title={widget.isActive ? 'Deaktif Et' : 'Aktif Et'}
                        >
                          {widget.isActive ? <X className="w-4 h-4" /> : <Check className="w-4 h-4" />}
                        </button>
                        <button
                          onClick={() => setDeleteWidget(widget)}
                          className="p-1.5 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600"
                          title="Sil"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          ) : (
            <div className="text-center py-16">
              <Code className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz widget oluşturulmamış</h3>
              <p className="text-sm text-gray-500 mb-6">
                Web sitenize entegre edebileceğiniz online rezervasyon widget'ı oluşturun
              </p>
              <Button onClick={() => { resetForm(); setActiveTab('create'); }} leftIcon={<Plus className="w-4 h-4" />}>
                İlk Widget'ı Oluştur
              </Button>
            </div>
          )}
        </div>
      )}

      {/* Create/Edit Form */}
      {(activeTab === 'create' || activeTab === 'edit') && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Form */}
          <div className="lg:col-span-2 space-y-6">
            {/* Property Selection */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Mülk Seçimi</h3>
              <Select
                label="Mülk"
                value={formData.propertyId}
                onChange={(e) => setFormData({ ...formData, propertyId: e.target.value })}
                options={[
                  { value: '', label: 'Mülk seçin...' },
                  ...(properties?.items?.map((p: any) => ({
                    value: p.id,
                    label: `${PROPERTY_TYPES.find(t => t.value === p.type)?.icon || ''} ${p.name}`,
                  })) || []),
                ]}
                required
                disabled={activeTab === 'edit'}
              />
            </Card>

            {/* Theme & Appearance */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Tema ve Görünüm</h3>
              
              <div className="space-y-4">
                {/* Theme Selection */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Tema</label>
                  <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                    {THEMES.map((theme) => (
                      <button
                        key={theme.value}
                        type="button"
                        onClick={() => setFormData({ ...formData, theme: theme.value })}
                        className={`p-4 rounded-xl border-2 text-left transition-all ${
                          formData.theme === theme.value
                            ? 'border-blue-500 bg-blue-50'
                            : 'border-gray-200 hover:border-gray-300'
                        }`}
                      >
                        <div className={`h-2 rounded-full mb-2 ${theme.preview}`} />
                        <div className="text-sm font-medium">{theme.label}</div>
                      </button>
                    ))}
                  </div>
                </div>

                {/* Colors */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Renk Paleti</label>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-xs text-gray-500 mb-1 block">Ana Renk</label>
                      <div className="flex items-center gap-2">
                        <input
                          type="color"
                          value={formData.primaryColor}
                          onChange={(e) => setFormData({ ...formData, primaryColor: e.target.value })}
                          className="w-10 h-10 rounded-lg border cursor-pointer"
                        />
                        <Input
                          value={formData.primaryColor}
                          onChange={(e) => setFormData({ ...formData, primaryColor: e.target.value })}
                          className="flex-1"
                        />
                      </div>
                      <div className="flex flex-wrap gap-1 mt-2">
                        {COLORS.slice(0, 8).map((color) => (
                          <button
                            key={color}
                            type="button"
                            onClick={() => setFormData({ ...formData, primaryColor: color })}
                            className="w-6 h-6 rounded-full border-2 transition-all"
                            style={{
                              backgroundColor: color,
                              borderColor: formData.primaryColor === color ? '#3B82F6' : 'transparent',
                              transform: formData.primaryColor === color ? 'scale(1.2)' : 'scale(1)',
                            }}
                          />
                        ))}
                      </div>
                    </div>
                    <div>
                      <label className="text-xs text-gray-500 mb-1 block">İkincil Renk</label>
                      <div className="flex items-center gap-2">
                        <input
                          type="color"
                          value={formData.secondaryColor}
                          onChange={(e) => setFormData({ ...formData, secondaryColor: e.target.value })}
                          className="w-10 h-10 rounded-lg border cursor-pointer"
                        />
                        <Input
                          value={formData.secondaryColor}
                          onChange={(e) => setFormData({ ...formData, secondaryColor: e.target.value })}
                          className="flex-1"
                        />
                      </div>
                    </div>
                  </div>
                </div>

                {/* Font */}
                <Select
                  label="Yazı Tipi"
                  value={formData.fontFamily}
                  onChange={(e) => setFormData({ ...formData, fontFamily: e.target.value })}
                  options={FONT_FAMILIES}
                />

                {/* Position */}
                <Select
                  label="Widget Konumu"
                  value={formData.position}
                  onChange={(e) => setFormData({ ...formData, position: e.target.value })}
                  options={[
                    { value: 'Right', label: 'Sağ Alt Köşe' },
                    { value: 'Left', label: 'Sol Alt Köşe' },
                    { value: 'Center', label: 'Ortada Modal Pencere' },
                    { value: 'FullPage', label: 'Tam Sayfa' },
                    { value: 'Embed', label: 'Gömülü (iframe)' },
                  ]}
                />
              </div>
            </Card>

            {/* Display Options */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Görüntüleme Seçenekleri</h3>
              <div className="space-y-3">
                {[
                  { key: 'showPropertyImages', label: 'Mülk Fotoğrafları', desc: 'Widget içinde mülk fotoğraflarını göster' },
                  { key: 'showAmenities', label: 'Özellikler', desc: 'Oda özellikleri ve imkanları göster' },
                  { key: 'showReviews', label: 'Değerlendirmeler', desc: 'Misafir değerlendirmelerini göster' },
                  { key: 'showPriceBreakdown', label: 'Fiyat Kırılımı', desc: 'Detaylı fiyat dökümünü göster' },
                ].map((option) => (
                  <label
                    key={option.key}
                    className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50"
                  >
                    <input
                      type="checkbox"
                      checked={(formData as any)[option.key]}
                      onChange={(e) => setFormData({ ...formData, [option.key]: e.target.checked })}
                      className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <div>
                      <div className="text-sm font-medium">{option.label}</div>
                      <div className="text-xs text-gray-500">{option.desc}</div>
                    </div>
                  </label>
                ))}
              </div>
            </Card>

            {/* Booking Settings */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Rezervasyon Ayarları</h3>
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <Input
                    label="Minimum Gün Öncesi"
                    type="number"
                    min={0}
                    max={365}
                    value={formData.minAdvanceDays}
                    onChange={(e) => setFormData({ ...formData, minAdvanceDays: parseInt(e.target.value) })}
                    helperText="0 = Bugün için de rezervasyon yapılabilir"
                  />
                  <Input
                    label="Maksimum Gün İleri"
                    type="number"
                    min={1}
                    max={730}
                    value={formData.maxAdvanceDays}
                    onChange={(e) => setFormData({ ...formData, maxAdvanceDays: parseInt(e.target.value) })}
                    helperText="365 = 1 yıl sonrasına kadar"
                  />
                </div>

                <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
                  <input
                    type="checkbox"
                    checked={formData.requirePayment}
                    onChange={(e) => setFormData({ ...formData, requirePayment: e.target.checked })}
                    className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <div>
                    <div className="text-sm font-medium">Ödeme Zorunlu</div>
                    <div className="text-xs text-gray-500">Rezervasyon sırasında ödeme alınması zorunlu olsun</div>
                  </div>
                </label>
              </div>
            </Card>

            {/* Language Settings */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">Dil Ayarları</h3>
              <div className="space-y-4">
                <Select
                  label="Varsayılan Dil"
                  value={formData.defaultLanguage}
                  onChange={(e) => setFormData({ ...formData, defaultLanguage: e.target.value })}
                  options={LANGUAGES}
                />
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Desteklenen Diller</label>
                  <div className="flex flex-wrap gap-2">
                    {LANGUAGES.map((lang) => (
                      <button
                        key={lang.value}
                        type="button"
                        onClick={() => {
                          const current = formData.availableLanguages;
                          const updated = current.includes(lang.value)
                            ? current.filter(l => l !== lang.value)
                            : [...current, lang.value];
                          setFormData({ ...formData, availableLanguages: updated });
                        }}
                        className={`inline-flex items-center gap-1.5 px-3 py-1.5 text-sm rounded-full border transition-colors ${
                          formData.availableLanguages.includes(lang.value)
                            ? 'bg-blue-50 border-blue-300 text-blue-700'
                            : 'bg-white border-gray-200 text-gray-600 hover:bg-gray-50'
                        }`}
                      >
                        {formData.availableLanguages.includes(lang.value) && (
                          <Check className="w-3.5 h-3.5" />
                        )}
                        {lang.label}
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </Card>

            {/* SEO Settings */}
            <Card className="p-6">
              <h3 className="text-lg font-semibold mb-4">SEO ve Paylaşım Ayarları</h3>
              <div className="space-y-4">
                <Input
                  label="Meta Başlık"
                  value={formData.metaTitle}
                  onChange={(e) => setFormData({ ...formData, metaTitle: e.target.value })}
                  placeholder="Rezervasyon Yap - Otel Adı"
                />
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Meta Açıklama</label>
                  <textarea
                    rows={3}
                    value={formData.metaDescription}
                    onChange={(e) => setFormData({ ...formData, metaDescription: e.target.value })}
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
                    placeholder="Otelimizde online rezervasyon yapın, en iyi fiyat garantisi..."
                    maxLength={160}
                  />
                  <p className="text-xs text-gray-500 mt-1">
                    {(formData.metaDescription || '').length}/160 karakter
                  </p>
                </div>
              </div>
            </Card>
          </div>

          {/* Preview Panel */}
          <div className="lg:col-span-1">
            <div className="sticky top-6 space-y-4">
              <Card className="p-4">
                <div className="flex items-center justify-between mb-3">
                  <h3 className="font-semibold text-sm">Canlı Önizleme</h3>
                  <div className="flex items-center gap-1">
                    <button
                      onClick={() => setPreviewDevice('desktop')}
                      className={`p-1.5 rounded transition-colors ${
                        previewDevice === 'desktop' ? 'bg-blue-100 text-blue-600' : 'text-gray-400 hover:text-gray-600'
                      }`}
                    >
                      <Monitor className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => setPreviewDevice('tablet')}
                      className={`p-1.5 rounded transition-colors ${
                        previewDevice === 'tablet' ? 'bg-blue-100 text-blue-600' : 'text-gray-400 hover:text-gray-600'
                      }`}
                    >
                      <Tablet className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => setPreviewDevice('mobile')}
                      className={`p-1.5 rounded transition-colors ${
                        previewDevice === 'mobile' ? 'bg-blue-100 text-blue-600' : 'text-gray-400 hover:text-gray-600'
                      }`}
                    >
                      <Smartphone className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                {/* Preview Frame */}
                <div
                  className={`bg-gray-100 rounded-lg overflow-hidden border mx-auto transition-all ${
                    previewDevice === 'mobile' ? 'w-[320px]' :
                    previewDevice === 'tablet' ? 'w-full' :
                    'w-full'
                  }`}
                >
                  {/* Preview Header */}
                  <div className="bg-gray-200 px-3 py-2 flex items-center gap-1.5">
                    <div className="w-2.5 h-2.5 rounded-full bg-red-400" />
                    <div className="w-2.5 h-2.5 rounded-full bg-yellow-400" />
                    <div className="w-2.5 h-2.5 rounded-full bg-green-400" />
                  </div>
                  
                  {/* Widget Preview */}
                  <div className="p-4" style={{ fontFamily: formData.fontFamily }}>
                    <div
                      className="rounded-xl shadow-lg overflow-hidden"
                      style={{ backgroundColor: 'white' }}
                    >
                      {/* Widget Header */}
                      <div
                        className="p-4 text-white"
                        style={{ backgroundColor: formData.primaryColor }}
                      >
                        <h4 className="font-semibold text-sm">
                          {formData.metaTitle || 'Rezervasyon Yap'}
                        </h4>
                      </div>
                      
                      {/* Search Form */}
                      <div className="p-4 space-y-3">
                        <div>
                          <label className="block text-xs font-medium text-gray-600 mb-1">Giriş Tarihi</label>
                          <div className="w-full px-3 py-2 text-sm border rounded-lg bg-gray-50">
                            Tarih seçin
                          </div>
                        </div>
                        <div className="grid grid-cols-2 gap-2">
                          <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Çıkış</label>
                            <div className="w-full px-3 py-2 text-sm border rounded-lg bg-gray-50">
                              Tarih
                            </div>
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Kişi</label>
                            <div className="w-full px-3 py-2 text-sm border rounded-lg bg-gray-50">
                              2 Yetişkin
                            </div>
                          </div>
                        </div>
                        
                        {/* Price Display */}
                        {formData.showPriceBreakdown && (
                          <div className="pt-3 border-t">
                            <div className="flex justify-between text-xs text-gray-500 mb-1">
                              <span>2 gece konaklama</span>
                              <span>₺1,500</span>
                            </div>
                            <div className="flex justify-between text-xs text-gray-500 mb-1">
                              <span>Vergiler</span>
                              <span>₺180</span>
                            </div>
                            <div className="flex justify-between font-semibold text-sm pt-2 border-t mt-2">
                              <span>Toplam</span>
                              <span style={{ color: formData.primaryColor }}>₺1,680</span>
                            </div>
                          </div>
                        )}
                        
                        <button
                          className="w-full py-2.5 text-sm font-medium text-white rounded-lg"
                          style={{ backgroundColor: formData.primaryColor }}
                        >
                          Rezervasyon Yap
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Position Indicator */}
                <div className="mt-3 flex items-center gap-2 text-xs text-gray-500">
                  <Info className="w-3.5 h-3.5" />
                  Konum: {getPositionLabel(formData.position)}
                </div>
              </Card>

              {/* Actions */}
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  className="flex-1"
                  onClick={() => { setActiveTab('list'); setSelectedWidget(null); }}
                >
                  İptal
                </Button>
                <Button
                  className="flex-1"
                  onClick={() => {
                    if (activeTab === 'edit' && selectedWidget) {
                      updateWidgetMutation.mutate({ id: selectedWidget.id, data: formData });
                    } else {
                      createWidgetMutation.mutate(formData);
                    }
                  }}
                  isLoading={createWidgetMutation.isPending || updateWidgetMutation.isPending}
                  leftIcon={<Save className="w-4 h-4" />}
                >
                  {activeTab === 'edit' ? 'Güncelle' : 'Oluştur'}
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Integration Modal */}
      <Modal
        isOpen={showIntegrationModal}
        onClose={() => setShowIntegrationModal(false)}
        title="Domain Yönetimi"
        size="md"
      >
        {selectedWidget && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Widget'ın çalışacağı domainler
              </label>
              <div className="space-y-2 mb-4">
                {selectedWidget.integrations?.map((integration: WidgetIntegration) => (
                  <div key={integration.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                    <div className="flex items-center gap-2">
                      <Globe className="w-4 h-4 text-gray-400" />
                      <span className="text-sm font-medium">{integration.domain}</span>
                      {!integration.isActive && (
                        <span className="text-xs text-red-500">(Pasif)</span>
                      )}
                    </div>
                    <button
                      onClick={() => removeDomainMutation.mutate({
                        widgetId: selectedWidget.id,
                        integrationId: integration.id,
                      })}
                      className="p-1 rounded hover:bg-red-50 text-gray-400 hover:text-red-600"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>

              <div className="flex gap-2">
                <Input
                  placeholder="ornek: otel.com"
                  value={newDomain}
                  onChange={(e) => setNewDomain(e.target.value)}
                  className="flex-1"
                />
                <Button
                  onClick={() => {
                    if (newDomain && selectedWidget) {
                      addDomainMutation.mutate({
                        widgetId: selectedWidget.id,
                        domain: newDomain,
                      });
                    }
                  }}
                  isLoading={addDomainMutation.isPending}
                >
                  Ekle
                </Button>
              </div>
              <p className="text-xs text-gray-500 mt-2">
                Widget sadece bu domainlerde çalışır. Yıldız (*) kullanarak alt domainleri de ekleyebilirsiniz.
                Örn: *.otel.com
              </p>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteWidget}
        onClose={() => setDeleteWidget(null)}
        onConfirm={() => deleteWidget && deleteWidgetMutation.mutate(deleteWidget.id)}
        title="Widget'ı Sil"
        message={`"${deleteWidget?.propertyName}" için oluşturulan widget'ı silmek istediğinize emin misiniz? Bu widget'ı kullanan tüm web sitelerinde rezervasyon formu çalışmayacaktır.`}
        confirmLabel="Sil"
        variant="danger"
        isLoading={deleteWidgetMutation.isPending}
      />
    </div>
  );
}