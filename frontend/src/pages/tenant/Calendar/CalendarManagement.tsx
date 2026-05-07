// src/pages/tenant/Calendar/CalendarManagement.tsx
import React, { useState, useRef, useCallback, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import trLocale from '@fullcalendar/core/locales/tr';
import {
  Plus,
  Search,
  Filter,
  Download,
  Calendar as CalendarIcon,
  Lock,
  Unlock,
  Wrench,
  Home,
  AlertCircle,
  Eye,
  EyeOff,
  Trash2,
  Edit,
  ChevronLeft,
  ChevronRight,
  RotateCcw,
  Settings,
  Users,
  DollarSign,
  X,
  Check,
  Clock,
  MapPin,
  Phone,
  Mail,
  Info,
} from 'lucide-react';
import { calendarApi } from '../../../api/calendar.api';
import { propertyApi } from '../../../api/property.api';
import { reservationApi } from '../../../api/reservation.api';
import { Button, Input, Select, Modal, Card, Badge, ConfirmDialog, Pagination } from '../../../components/ui';
import { useDebounce } from '../../../hooks/useDebounce';
import { formatCurrency, formatDate, formatDateTime, formatTime, getNights } from '../../../utils/format';
import { PROPERTY_TYPES, RESERVATION_STATUSES } from '../../../utils/constants';
import toast from 'react-hot-toast';

// Types
interface CalendarBlock {
  id: string;
  unitId: string;
  propertyId?: string;
  type: 'Maintenance' | 'ClosedSeason' | 'PrivateUse' | 'AllotmentFull' | 'Other';
  startDate: string;
  endDate: string;
  reason: string;
  notes: string;
  createdByTenantId?: string;
  createdByAgencyId?: string;
  isActive: boolean;
  createdAt: string;
}

interface DailyPrice {
  unitId: string;
  date: string;
  price: number;
  currencyCode: string;
  setByTenantId?: string;
  setByAgencyId?: string;
}

interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  backgroundColor: string;
  borderColor: string;
  textColor: string;
  extendedProps: {
    type: 'reservation' | 'block' | 'price_change';
    reservation?: any;
    block?: CalendarBlock;
    unitName?: string;
    guestName?: string;
    status?: string;
    amount?: number;
    currencyCode?: string;
  };
}

const BLOCK_TYPES = [
  { value: 'Maintenance', label: 'Bakım/Onarım', icon: Wrench, color: '#F59E0B' },
  { value: 'ClosedSeason', label: 'Kapalı Sezon', icon: Lock, color: '#6B7280' },
  { value: 'PrivateUse', label: 'Özel Kullanım', icon: Home, color: '#8B5CF6' },
  { value: 'AllotmentFull', label: 'Kontenjan Doldu', icon: Users, color: '#EF4444' },
  { value: 'Other', label: 'Diğer', icon: AlertCircle, color: '#9CA3AF' },
] as const;

export default function CalendarManagement() {
  const queryClient = useQueryClient();
  const calendarRef = useRef<FullCalendar>(null);
  
  // States
  const [selectedPropertyId, setSelectedPropertyId] = useState<string>('');
  const [selectedUnits, setSelectedUnits] = useState<string[]>([]);
  const [viewMode, setViewMode] = useState<'calendar' | 'list'>('calendar');
  const [calendarView, setCalendarView] = useState<string>('dayGridMonth');
  const [dateRange, setDateRange] = useState({
    start: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
    end: new Date(new Date().getFullYear(), new Date().getMonth() + 2, 0),
  });
  
  // Block form
  const [showBlockForm, setShowBlockForm] = useState(false);
  const [editingBlock, setEditingBlock] = useState<CalendarBlock | null>(null);
  const [blockFormData, setBlockFormData] = useState({
    unitId: '',
    propertyId: '',
    type: 'Maintenance' as CalendarBlock['type'],
    startDate: '',
    endDate: '',
    reason: '',
    notes: '',
    applyToAllUnits: false,
  });
  
  // Price form
  const [showPriceForm, setShowPriceForm] = useState(false);
  const [priceFormData, setPriceFormData] = useState({
    unitId: '',
    startDate: '',
    endDate: '',
    price: 0,
    currencyCode: 'TRY',
    applyToWeekends: false,
    weekendPrice: 0,
  });
  
  // Bulk operations
  const [showBulkBlockForm, setShowBulkBlockForm] = useState(false);
  const [bulkBlockData, setBulkBlockData] = useState({
    propertyId: '',
    type: 'Maintenance' as CalendarBlock['type'],
    startDate: '',
    endDate: '',
    reason: '',
    notes: '',
  });
  
  // Detail modals
  const [selectedEvent, setSelectedEvent] = useState<CalendarEvent | null>(null);
  const [showEventDetail, setShowEventDetail] = useState(false);
  
  // Delete confirmation
  const [deleteBlock, setDeleteBlock] = useState<CalendarBlock | null>(null);
  
  // Filter
  const [showOnlyActive, setShowOnlyActive] = useState(true);
  const [filterType, setFilterType] = useState<string>('all');

  // Queries
  const { data: properties } = useQuery({
    queryKey: ['properties', 'all'],
    queryFn: () => propertyApi.getAll({ pageSize: 1000, isActive: true }),
  });

  const { data: units } = useQuery({
    queryKey: ['units', selectedPropertyId],
    queryFn: () => propertyApi.getUnits(selectedPropertyId!),
    enabled: !!selectedPropertyId,
  });

  const { data: blocks, isLoading: blocksLoading } = useQuery({
    queryKey: ['blocks', selectedPropertyId, selectedUnits, dateRange],
    queryFn: () => calendarApi.getBlocks({
      propertyId: selectedPropertyId || undefined,
      unitIds: selectedUnits.length > 0 ? selectedUnits : undefined,
      startDate: dateRange.start,
      endDate: dateRange.end,
    }),
    enabled: !!selectedPropertyId,
  });

  const { data: reservations } = useQuery({
    queryKey: ['reservations', 'calendar', selectedPropertyId, dateRange],
    queryFn: () => reservationApi.getCalendar(dateRange.start, dateRange.end, selectedPropertyId || undefined),
    enabled: !!selectedPropertyId,
  });

  const { data: prices } = useQuery({
    queryKey: ['prices', 'calendar', selectedPropertyId, dateRange],
    queryFn: () => calendarApi.getPrices({
      propertyId: selectedPropertyId || undefined,
      startDate: dateRange.start,
      endDate: dateRange.end,
    }),
    enabled: !!selectedPropertyId,
  });

  // Mutations
  const createBlockMutation = useMutation({
    mutationFn: (data: any) => calendarApi.createBlock(data),
    onSuccess: () => {
      toast.success('Tarihler bloke edildi');
      queryClient.invalidateQueries({ queryKey: ['blocks'] });
      resetBlockForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  const updateBlockMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => calendarApi.updateBlock(id, data),
    onSuccess: () => {
      toast.success('Blokaj güncellendi');
      queryClient.invalidateQueries({ queryKey: ['blocks'] });
      resetBlockForm();
    },
    onError: (error: any) => toast.error(error.message),
  });

  const deleteBlockMutation = useMutation({
    mutationFn: (id: string) => calendarApi.deleteBlock(id),
    onSuccess: () => {
      toast.success('Blokaj kaldırıldı');
      queryClient.invalidateQueries({ queryKey: ['blocks'] });
      setDeleteBlock(null);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const setPriceMutation = useMutation({
    mutationFn: (data: any) => calendarApi.setDailyPrice(data),
    onSuccess: () => {
      toast.success('Fiyat güncellendi');
      queryClient.invalidateQueries({ queryKey: ['prices'] });
      setShowPriceForm(false);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const bulkBlockMutation = useMutation({
    mutationFn: (data: any) => calendarApi.createBulkBlocks(data),
    onSuccess: () => {
      toast.success('Toplu blokaj yapıldı');
      queryClient.invalidateQueries({ queryKey: ['blocks'] });
      setShowBulkBlockForm(false);
    },
    onError: (error: any) => toast.error(error.message),
  });

  // Event handlers
  const resetBlockForm = () => {
    setBlockFormData({
      unitId: '',
      propertyId: selectedPropertyId,
      type: 'Maintenance',
      startDate: '',
      endDate: '',
      reason: '',
      notes: '',
      applyToAllUnits: false,
    });
    setEditingBlock(null);
    setShowBlockForm(false);
  };

  const handleEditBlock = (block: CalendarBlock) => {
    setEditingBlock(block);
    setBlockFormData({
      unitId: block.unitId,
      propertyId: block.propertyId || '',
      type: block.type,
      startDate: block.startDate.split('T')[0],
      endDate: block.endDate.split('T')[0],
      reason: block.reason,
      notes: block.notes || '',
      applyToAllUnits: false,
    });
    setShowBlockForm(true);
  };

  const handleDateSelect = (selectInfo: any) => {
    const startStr = selectInfo.startStr;
    const endStr = selectInfo.endStr;
    
    // End date'den 1 gün çıkar (FullCalendar bitişi exclusive yapar)
    const endDate = new Date(endStr);
    endDate.setDate(endDate.getDate() - 1);
    
    setBlockFormData({
      ...blockFormData,
      startDate: startStr,
      endDate: endDate.toISOString().split('T')[0],
      unitId: selectInfo.view.type.includes('Day') ? selectedUnits[0] || '' : '',
    });
    setShowBlockForm(true);
  };

  const handleEventClick = (clickInfo: any) => {
    const event = clickInfo.event;
    setSelectedEvent({
      id: event.id,
      title: event.title,
      start: event.startStr,
      end: event.endStr,
      backgroundColor: event.backgroundColor,
      borderColor: event.borderColor,
      textColor: event.textColor,
      extendedProps: event.extendedProps,
    });
    setShowEventDetail(true);
  };

  const navigateToday = () => {
    calendarRef.current?.getApi().today();
  };

  const navigatePrev = () => {
    calendarRef.current?.getApi().prev();
  };

  const navigateNext = () => {
    calendarRef.current?.getApi().next();
  };

  const changeView = (view: string) => {
    setCalendarView(view);
    calendarRef.current?.getApi().changeView(view);
  };

  // Build calendar events
  const events: CalendarEvent[] = useMemo(() => {
    const allEvents: CalendarEvent[] = [];
    
    // Reservation events
    reservations?.forEach((res: any) => {
      const statusConfig = RESERVATION_STATUSES[res.status as keyof typeof RESERVATION_STATUSES];
      allEvents.push({
        id: `res-${res.id}`,
        title: `${res.guestName} - ${res.unitName || ''}`,
        start: res.checkIn,
        end: res.checkOut,
        backgroundColor: statusConfig?.color === 'green' ? '#10B981' :
                        statusConfig?.color === 'blue' ? '#3B82F6' :
                        statusConfig?.color === 'yellow' ? '#F59E0B' :
                        statusConfig?.color === 'red' ? '#EF4444' : '#6B7280',
        borderColor: 'transparent',
        textColor: '#FFFFFF',
        extendedProps: {
          type: 'reservation',
          reservation: res,
          unitName: res.unitName,
          guestName: res.guestName,
          status: res.status,
          amount: res.totalAmount,
          currencyCode: res.currencyCode,
        },
      });
    });
    
    // Block events
    blocks?.forEach((block: CalendarBlock) => {
      if (!block.isActive && showOnlyActive) return;
      if (filterType !== 'all' && block.type !== filterType) return;
      
      const typeConfig = BLOCK_TYPES.find(t => t.value === block.type);
      allEvents.push({
        id: `block-${block.id}`,
        title: `🔒 ${block.reason || typeConfig?.label || 'Bloke'}`,
        start: block.startDate,
        end: new Date(new Date(block.endDate).getTime() + 86400000).toISOString().split('T')[0],
        backgroundColor: typeConfig?.color || '#9CA3AF',
        borderColor: 'transparent',
        textColor: '#FFFFFF',
        extendedProps: {
          type: 'block',
          block,
        },
      });
    });
    
    return allEvents;
  }, [reservations, blocks, showOnlyActive, filterType]);

  // Statistics
  const stats = useMemo(() => {
    if (!units) return null;
    
    const today = new Date().toISOString().split('T')[0];
    const todayReservations = reservations?.filter((r: any) => 
      r.checkIn <= today && r.checkOut > today
    ) || [];
    
    const todayBlocks = blocks?.filter((b: CalendarBlock) => 
      b.startDate <= today && b.endDate >= today && b.isActive
    ) || [];
    
    const totalUnits = units.length;
    const occupiedUnits = todayReservations.length;
    const blockedUnits = todayBlocks.length;
    const availableUnits = totalUnits - occupiedUnits - blockedUnits;
    
    return {
      totalUnits,
      occupiedUnits,
      blockedUnits,
      availableUnits,
      occupancyRate: totalUnits > 0 ? ((occupiedUnits / totalUnits) * 100).toFixed(1) : '0',
    };
  }, [units, reservations, blocks]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Takvim & Blokaj Yönetimi</h1>
          <p className="text-sm text-gray-500 mt-1">
            Müsaitlik takvimini yönetin, blokaj ve fiyat güncellemeleri yapın
          </p>
        </div>
        
        <div className="flex flex-wrap items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowBulkBlockForm(true)}
            leftIcon={<Lock className="w-4 h-4" />}
          >
            Toplu Blokaj
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowPriceForm(true)}
            leftIcon={<DollarSign className="w-4 h-4" />}
          >
            Fiyat Güncelle
          </Button>
          <Button
            size="sm"
            onClick={() => setShowBlockForm(true)}
            leftIcon={<Plus className="w-4 h-4" />}
          >
            Blokaj Ekle
          </Button>
        </div>
      </div>

      {/* Property & Unit Selector */}
      <div className="bg-white rounded-xl border p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Mülk Seçin</label>
            <Select
              value={selectedPropertyId}
              onChange={(e) => {
                setSelectedPropertyId(e.target.value);
                setSelectedUnits([]);
              }}
              options={[
                { value: '', label: 'Mülk seçin...' },
                ...(properties?.items?.map((p: any) => ({
                  value: p.id,
                  label: `${PROPERTY_TYPES.find(t => t.value === p.type)?.icon || ''} ${p.name}`,
                })) || []),
              ]}
            />
          </div>
          
          {units && units.length > 0 && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Birim Filtresi ({selectedUnits.length > 0 ? `${selectedUnits.length} seçili` : 'Tümü'})
              </label>
              <div className="flex flex-wrap gap-2">
                <button
                  onClick={() => setSelectedUnits(selectedUnits.length === units.length ? [] : units.map((u: any) => u.id))}
                  className={`px-3 py-1.5 text-xs font-medium rounded-full border transition-colors ${
                    selectedUnits.length === units.length
                      ? 'bg-blue-100 border-blue-300 text-blue-700'
                      : 'bg-gray-50 border-gray-200 text-gray-600 hover:bg-gray-100'
                  }`}
                >
                  {selectedUnits.length === units.length ? 'Tümünü Kaldır' : 'Tümünü Seç'}
                </button>
                {units.map((unit: any) => (
                  <button
                    key={unit.id}
                    onClick={() => {
                      setSelectedUnits(prev =>
                        prev.includes(unit.id)
                          ? prev.filter(id => id !== unit.id)
                          : [...prev, unit.id]
                      );
                    }}
                    className={`px-3 py-1.5 text-xs font-medium rounded-full border transition-colors ${
                      selectedUnits.includes(unit.id)
                        ? 'bg-blue-100 border-blue-300 text-blue-700'
                        : 'bg-gray-50 border-gray-200 text-gray-600 hover:bg-gray-100'
                    }`}
                  >
                    {unit.name}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Stats Bar */}
      {stats && selectedPropertyId && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <Card className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-100 rounded-lg">
                <Home className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.totalUnits}</p>
                <p className="text-xs text-gray-500">Toplam Birim</p>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-green-100 rounded-lg">
                <Check className="w-5 h-5 text-green-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.availableUnits}</p>
                <p className="text-xs text-gray-500">Müsait</p>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-purple-100 rounded-lg">
                <Users className="w-5 h-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.occupiedUnits}</p>
                <p className="text-xs text-gray-500">Dolu</p>
              </div>
            </div>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-orange-100 rounded-lg">
                <Lock className="w-5 h-5 text-orange-600" />
              </div>
              <div>
                <p className="text-2xl font-bold text-gray-900">{stats.blockedUnits}</p>
                <p className="text-xs text-gray-500">Bloke</p>
              </div>
            </div>
          </Card>
        </div>
      )}

      {/* Calendar Controls */}
      {selectedPropertyId && (
        <div className="bg-white rounded-xl border">
          <div className="flex items-center justify-between p-4 border-b">
            <div className="flex items-center gap-3">
              <button
                onClick={navigatePrev}
                className="p-2 rounded-lg hover:bg-gray-100"
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={navigateToday}
                className="px-3 py-1.5 text-sm font-medium bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200"
              >
                Bugün
              </button>
              <button
                onClick={navigateNext}
                className="p-2 rounded-lg hover:bg-gray-100"
              >
                <ChevronRight className="w-5 h-5" />
              </button>
              <h2 className="text-lg font-semibold text-gray-900" id="calendar-title">
                {/* FullCalendar başlığı buraya dinamik gelecek */}
              </h2>
            </div>
            
            <div className="flex items-center gap-2">
              {/* View toggles */}
              <div className="flex bg-gray-100 rounded-lg p-1">
                {[
                  { view: 'dayGridMonth', label: 'Ay', icon: CalendarIcon },
                  { view: 'timeGridWeek', label: 'Hafta', icon: Clock },
                  { view: 'timeGridDay', label: 'Gün', icon: CalendarIcon },
                  { view: 'listWeek', label: 'Liste', icon: Info },
                ].map(({ view, label, icon: Icon }) => (
                  <button
                    key={view}
                    onClick={() => changeView(view)}
                    className={`flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium rounded-md transition-colors ${
                      calendarView === view
                        ? 'bg-white shadow-sm text-blue-600'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    <Icon className="w-4 h-4" />
                    <span className="hidden sm:inline">{label}</span>
                  </button>
                ))}
              </div>
              
              {/* Filter toggles */}
              <button
                onClick={() => setShowOnlyActive(!showOnlyActive)}
                className={`p-2 rounded-lg border transition-colors ${
                  showOnlyActive ? 'bg-blue-50 border-blue-200 text-blue-600' : 'bg-white border-gray-200 text-gray-400'
                }`}
                title={showOnlyActive ? 'Tüm blokajları göster' : 'Sadece aktif blokajları göster'}
              >
                {showOnlyActive ? <Eye className="w-4 h-4" /> : <EyeOff className="w-4 h-4" />}
              </button>
              
              <Select
                value={filterType}
                onChange={(e) => setFilterType(e.target.value)}
                options={[
                  { value: 'all', label: 'Tüm Tipler' },
                  ...BLOCK_TYPES.map(t => ({ value: t.value, label: t.label })),
                ]}
                className="w-40"
              />
            </div>
          </div>
          
          {/* Calendar */}
          <div className="p-4">
            <FullCalendar
              ref={calendarRef}
              plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin, listPlugin]}
              initialView={calendarView}
              locales={[trLocale]}
              locale="tr"
              headerToolbar={false}
              events={events}
              selectable={true}
              select={handleDateSelect}
              eventClick={handleEventClick}
              selectMirror={true}
              dayMaxEvents={3}
              weekends={true}
              firstDay={1}
              height="auto"
              datesSet={(dateInfo) => {
                setDateRange({
                  start: dateInfo.start,
                  end: dateInfo.end,
                });
                
                // Takvim başlığını güncelle
                const titleEl = document.getElementById('calendar-title');
                if (titleEl) {
                  titleEl.textContent = dateInfo.view.title;
                }
              }}
              eventDidMount={(info) => {
                // Tooltip ekle
                const props = info.event.extendedProps;
                let tooltip = info.event.title;
                if (props.type === 'reservation' && props.reservation) {
                  tooltip = `${props.guestName}\n${props.unitName}\n${formatDate(info.event.startStr)} - ${formatDate(info.event.endStr)}\n${formatCurrency(props.amount || 0, props.currencyCode || 'TRY')}`;
                } else if (props.type === 'block' && props.block) {
                  tooltip = `${props.block.reason}\n${formatDate(props.block.startDate)} - ${formatDate(props.block.endDate)}`;
                }
                info.el.title = tooltip;
              }}
            />
          </div>
          
          {/* Legend */}
          <div className="flex flex-wrap items-center gap-4 p-4 border-t bg-gray-50 rounded-b-xl">
            <span className="text-xs font-medium text-gray-500">LEJANT:</span>
            {Object.entries(RESERVATION_STATUSES).map(([key, val]) => (
              <div key={key} className="flex items-center gap-1.5">
                <div
                  className="w-3 h-3 rounded"
                  style={{
                    backgroundColor:
                      val.color === 'green' ? '#10B981' :
                      val.color === 'blue' ? '#3B82F6' :
                      val.color === 'yellow' ? '#F59E0B' :
                      val.color === 'red' ? '#EF4444' : '#6B7280',
                  }}
                />
                <span className="text-xs text-gray-600">{val.label}</span>
              </div>
            ))}
            <div className="w-px h-4 bg-gray-300" />
            {BLOCK_TYPES.map((type) => (
              <div key={type.value} className="flex items-center gap-1.5">
                <div className="w-3 h-3 rounded" style={{ backgroundColor: type.color }} />
                <span className="text-xs text-gray-600">{type.label}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Empty State */}
      {!selectedPropertyId && (
        <div className="text-center py-20">
          <CalendarIcon className="w-20 h-20 text-gray-300 mx-auto mb-4" />
          <h3 className="text-xl font-medium text-gray-900 mb-2">Takvimi görüntülemek için mülk seçin</h3>
          <p className="text-sm text-gray-500">
            Yukarıdan bir mülk seçerek takvim yönetimine başlayabilirsiniz
          </p>
        </div>
      )}

      {/* Block Form Modal */}
      <Modal
        isOpen={showBlockForm}
        onClose={resetBlockForm}
        title={editingBlock ? 'Blokajı Düzenle' : 'Yeni Blokaj'}
        size="md"
        footer={
          <div className="flex justify-end gap-3 w-full">
            <Button variant="outline" onClick={resetBlockForm}>
              İptal
            </Button>
            <Button
              onClick={() => {
                const data = {
                  ...blockFormData,
                  propertyId: blockFormData.applyToAllUnits ? selectedPropertyId : undefined,
                };
                if (editingBlock) {
                  updateBlockMutation.mutate({ id: editingBlock.id, data });
                } else {
                  createBlockMutation.mutate(data);
                }
              }}
              isLoading={createBlockMutation.isPending || updateBlockMutation.isPending}
            >
              {editingBlock ? 'Güncelle' : 'Bloke Et'}
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          {!editingBlock && units && units.length > 1 && (
            <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
              <input
                type="checkbox"
                checked={blockFormData.applyToAllUnits}
                onChange={(e) => setBlockFormData({
                  ...blockFormData,
                  applyToAllUnits: e.target.checked,
                  unitId: e.target.checked ? '' : blockFormData.unitId,
                })}
                className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <div>
                <div className="text-sm font-medium">Tüm birimlere uygula</div>
                <div className="text-xs text-gray-500">Seçili mülkteki tüm birimler bloke edilir</div>
              </div>
            </label>
          )}

          {!blockFormData.applyToAllUnits && (
            <Select
              label="Birim *"
              value={blockFormData.unitId}
              onChange={(e) => setBlockFormData({ ...blockFormData, unitId: e.target.value })}
              options={[
                { value: '', label: 'Birim seçin...' },
                ...(units?.map((u: any) => ({
                  value: u.id,
                  label: `${u.name}${u.unitNumber ? ` (${u.unitNumber})` : ''}`,
                })) || []),
              ]}
              required
            />
          )}

          <Select
            label="Blokaj Tipi *"
            value={blockFormData.type}
            onChange={(e) => setBlockFormData({ ...blockFormData, type: e.target.value as any })}
            options={BLOCK_TYPES.map(t => ({
              value: t.value,
              label: t.label,
            }))}
          />

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Başlangıç Tarihi *"
              type="date"
              value={blockFormData.startDate}
              onChange={(e) => setBlockFormData({ ...blockFormData, startDate: e.target.value })}
              min={new Date().toISOString().split('T')[0]}
              required
            />
            <Input
              label="Bitiş Tarihi *"
              type="date"
              value={blockFormData.endDate}
              onChange={(e) => setBlockFormData({ ...blockFormData, endDate: e.target.value })}
              min={blockFormData.startDate || new Date().toISOString().split('T')[0]}
              required
            />
          </div>

          <Input
            label="Sebep"
            value={blockFormData.reason}
            onChange={(e) => setBlockFormData({ ...blockFormData, reason: e.target.value })}
            placeholder="Örn: Yıllık bakım, tadilat..."
          />

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Notlar</label>
            <textarea
              rows={3}
              value={blockFormData.notes}
              onChange={(e) => setBlockFormData({ ...blockFormData, notes: e.target.value })}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="Ek notlar..."
            />
          </div>
        </div>
      </Modal>

      {/* Bulk Block Modal */}
      <Modal
        isOpen={showBulkBlockForm}
        onClose={() => setShowBulkBlockForm(false)}
        title="Toplu Blokaj"
        size="md"
        footer={
          <div className="flex justify-end gap-3 w-full">
            <Button variant="outline" onClick={() => setShowBulkBlockForm(false)}>
              İptal
            </Button>
            <Button
              onClick={() => bulkBlockMutation.mutate(bulkBlockData)}
              isLoading={bulkBlockMutation.isPending}
            >
              Tümünü Bloke Et
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <Select
            label="Mülk *"
            value={bulkBlockData.propertyId}
            onChange={(e) => setBulkBlockData({ ...bulkBlockData, propertyId: e.target.value })}
            options={[
              { value: '', label: 'Mülk seçin...' },
              ...(properties?.items?.map((p: any) => ({
                value: p.id,
                label: p.name,
              })) || []),
            ]}
            required
          />

          <Select
            label="Blokaj Tipi"
            value={bulkBlockData.type}
            onChange={(e) => setBulkBlockData({ ...bulkBlockData, type: e.target.value as any })}
            options={BLOCK_TYPES.map(t => ({ value: t.value, label: t.label }))}
          />

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Başlangıç *"
              type="date"
              value={bulkBlockData.startDate}
              onChange={(e) => setBulkBlockData({ ...bulkBlockData, startDate: e.target.value })}
              required
            />
            <Input
              label="Bitiş *"
              type="date"
              value={bulkBlockData.endDate}
              onChange={(e) => setBulkBlockData({ ...bulkBlockData, endDate: e.target.value })}
              required
            />
          </div>

          <Input
            label="Sebep"
            value={bulkBlockData.reason}
            onChange={(e) => setBulkBlockData({ ...bulkBlockData, reason: e.target.value })}
          />
        </div>
      </Modal>

      {/* Price Form Modal */}
      <Modal
        isOpen={showPriceForm}
        onClose={() => setShowPriceForm(false)}
        title="Fiyat Güncelle"
        size="md"
        footer={
          <div className="flex justify-end gap-3 w-full">
            <Button variant="outline" onClick={() => setShowPriceForm(false)}>
              İptal
            </Button>
            <Button
              onClick={() => setPriceMutation.mutate(priceFormData)}
              isLoading={setPriceMutation.isPending}
            >
              Fiyatı Güncelle
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <Select
            label="Birim *"
            value={priceFormData.unitId}
            onChange={(e) => setPriceFormData({ ...priceFormData, unitId: e.target.value })}
            options={[
              { value: '', label: 'Birim seçin...' },
              ...(units?.map((u: any) => ({
                value: u.id,
                label: `${u.name} (Mevcut: ${formatCurrency(u.basePrice, u.currencyCode)})`,
              })) || []),
            ]}
            required
          />

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Başlangıç *"
              type="date"
              value={priceFormData.startDate}
              onChange={(e) => setPriceFormData({ ...priceFormData, startDate: e.target.value })}
              required
            />
            <Input
              label="Bitiş *"
              type="date"
              value={priceFormData.endDate}
              onChange={(e) => setPriceFormData({ ...priceFormData, endDate: e.target.value })}
              required
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Fiyat *"
              type="number"
              min={0}
              step="0.01"
              value={priceFormData.price || ''}
              onChange={(e) => setPriceFormData({ ...priceFormData, price: parseFloat(e.target.value) })}
              required
            />
            <Select
              label="Para Birimi"
              value={priceFormData.currencyCode}
              onChange={(e) => setPriceFormData({ ...priceFormData, currencyCode: e.target.value })}
              options={[
                { value: 'TRY', label: '₺ TRY' },
                { value: 'USD', label: '$ USD' },
                { value: 'EUR', label: '€ EUR' },
                { value: 'GBP', label: '£ GBP' },
              ]}
            />
          </div>

          <label className="flex items-center gap-3 p-3 rounded-lg border cursor-pointer hover:bg-gray-50">
            <input
              type="checkbox"
              checked={priceFormData.applyToWeekends}
              onChange={(e) => setPriceFormData({ ...priceFormData, applyToWeekends: e.target.checked })}
              className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
            />
            <div>
              <div className="text-sm font-medium">Hafta sonu için farklı fiyat</div>
            </div>
          </label>

          {priceFormData.applyToWeekends && (
            <Input
              label="Hafta Sonu Fiyatı"
              type="number"
              min={0}
              step="0.01"
              value={priceFormData.weekendPrice || ''}
              onChange={(e) => setPriceFormData({ ...priceFormData, weekendPrice: parseFloat(e.target.value) })}
            />
          )}
        </div>
      </Modal>

      {/* Event Detail Modal */}
      <Modal
        isOpen={showEventDetail}
        onClose={() => setShowEventDetail(false)}
        title={selectedEvent?.extendedProps.type === 'reservation' ? 'Rezervasyon Detayı' : 'Blokaj Detayı'}
        size="md"
        footer={
          selectedEvent?.extendedProps.type === 'block' ? (
            <div className="flex justify-end gap-3 w-full">
              <Button
                variant="outline"
                onClick={() => {
                  if (selectedEvent?.extendedProps.block) {
                    handleEditBlock(selectedEvent.extendedProps.block);
                    setShowEventDetail(false);
                  }
                }}
                leftIcon={<Edit className="w-4 h-4" />}
              >
                Düzenle
              </Button>
              <Button
                variant="danger"
                onClick={() => {
                  if (selectedEvent?.extendedProps.block) {
                    setDeleteBlock(selectedEvent.extendedProps.block);
                    setShowEventDetail(false);
                  }
                }}
                leftIcon={<Trash2 className="w-4 h-4" />}
              >
                Kaldır
              </Button>
            </div>
          ) : (
            <div className="flex justify-end gap-3 w-full">
              <Button variant="outline" onClick={() => setShowEventDetail(false)}>
                Kapat
              </Button>
            </div>
          )
        }
      >
        {selectedEvent && (
          <div className="space-y-4">
            {selectedEvent.extendedProps.type === 'reservation' && selectedEvent.extendedProps.reservation && (
              <>
                <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg">
                  <div>
                    <span className="text-sm text-gray-500">Rezervasyon No</span>
                    <p className="font-bold">{selectedEvent.extendedProps.reservation.reservationNumber}</p>
                  </div>
                  <StatusBadge status={selectedEvent.extendedProps.status} />
                </div>
                
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="text-xs text-gray-500">Misafir</label>
                    <p className="font-medium">{selectedEvent.extendedProps.guestName}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Birim</label>
                    <p className="font-medium">{selectedEvent.extendedProps.unitName}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Giriş</label>
                    <p className="font-medium">{formatDate(selectedEvent.start)}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Çıkış</label>
                    <p className="font-medium">{formatDate(selectedEvent.end)}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Gece</label>
                    <p className="font-medium">{getNights(selectedEvent.start, selectedEvent.end)}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Tutar</label>
                    <p className="font-bold text-blue-600">
                      {formatCurrency(selectedEvent.extendedProps.amount || 0, selectedEvent.extendedProps.currencyCode || 'TRY')}
                    </p>
                  </div>
                </div>
              </>
            )}
            
            {selectedEvent.extendedProps.type === 'block' && selectedEvent.extendedProps.block && (
              <>
                <div className="flex items-center gap-3 p-3 rounded-lg" style={{
                  backgroundColor: `${BLOCK_TYPES.find(t => t.value === selectedEvent.extendedProps.block?.type)?.color}15`
                }}>
                  {React.createElement(
                    BLOCK_TYPES.find(t => t.value === selectedEvent.extendedProps.block?.type)?.icon || AlertCircle,
                    { className: "w-6 h-6", style: { color: BLOCK_TYPES.find(t => t.value === selectedEvent.extendedProps.block?.type)?.color } }
                  )}
                  <div>
                    <p className="font-semibold">
                      {BLOCK_TYPES.find(t => t.value === selectedEvent.extendedProps.block?.type)?.label}
                    </p>
                    <p className="text-sm">{selectedEvent.extendedProps.block.reason}</p>
                  </div>
                </div>
                
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="text-xs text-gray-500">Başlangıç</label>
                    <p className="font-medium">{formatDate(selectedEvent.extendedProps.block.startDate)}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Bitiş</label>
                    <p className="font-medium">{formatDate(selectedEvent.extendedProps.block.endDate)}</p>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Durum</label>
                    <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                      selectedEvent.extendedProps.block.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-red-100 text-red-800'
                    }`}>
                      {selectedEvent.extendedProps.block.isActive ? 'Aktif' : 'Pasif'}
                    </span>
                  </div>
                  <div>
                    <label className="text-xs text-gray-500">Oluşturulma</label>
                    <p className="font-medium">{formatDate(selectedEvent.extendedProps.block.createdAt)}</p>
                  </div>
                </div>
                
                {selectedEvent.extendedProps.block.notes && (
                  <div>
                    <label className="text-xs text-gray-500">Notlar</label>
                    <p className="text-sm mt-1 bg-gray-50 p-3 rounded-lg">{selectedEvent.extendedProps.block.notes}</p>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </Modal>

      {/* Delete Block Confirmation */}
      <ConfirmDialog
        isOpen={!!deleteBlock}
        onClose={() => setDeleteBlock(null)}
        onConfirm={() => deleteBlock && deleteBlockMutation.mutate(deleteBlock.id)}
        title="Blokajı Kaldır"
        message={`"${deleteBlock?.reason || 'Bu blokajı'}" kaldırmak istediğinize emin misiniz?`}
        confirmLabel="Kaldır"
        variant="danger"
        isLoading={deleteBlockMutation.isPending}
      />
    </div>
  );
}

// Status Badge bileşeni
function StatusBadge({ status }: { status: string }) {
  const config = RESERVATION_STATUSES[status as keyof typeof RESERVATION_STATUSES];
  if (!config) return null;
  
  return (
    <span className={`inline-flex px-2.5 py-1 text-xs font-medium rounded-full ${config.bgColor} ${config.textColor}`}>
      {config.label}
    </span>
  );
}