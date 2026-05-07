// src/pages/agency/Calendar.tsx
import { useState, useRef, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import trLocale from '@fullcalendar/core/locales/tr';
import {
  ChevronLeft,
  ChevronRight,
  Calendar as CalendarIcon,
  Plus,
  Eye,
  Lock,
  Home,
  Wrench,
  AlertCircle,
  Info,
  DollarSign,
  Users,
} from 'lucide-react';
import { calendarApi } from '../../api/calendar.api';
import { reservationApi } from '../../api/reservation.api';
import { agencyApi } from '../../api/agency.api';
import { Button, Modal, Card, Badge } from '../../components/ui';
import { formatCurrency, formatDate, formatDateTime } from '../../utils/format';
import { RESERVATION_STATUSES } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';
import toast from 'react-hot-toast';

export default function AgencyCalendar() {
  const { propertyId } = useParams<{ propertyId: string }>();
  const navigate = useNavigate();
  const calendarRef = useRef<FullCalendar>(null);
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  // States
  const [dateRange, setDateRange] = useState({
    start: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
    end: new Date(new Date().getFullYear(), new Date().getMonth() + 2, 0),
  });
  const [selectedEvent, setSelectedEvent] = useState<any>(null);
  const [showEventDetail, setShowEventDetail] = useState(false);
  const [selectedDate, setSelectedDate] = useState<{ start: string; end: string } | null>(null);

  // Queries
  const { data: calendarData } = useQuery({
    queryKey: ['agency', 'calendar', agencyId, propertyId, dateRange],
    queryFn: () => calendarApi.getAgencyCalendar(agencyId!, propertyId!, dateRange.start, dateRange.end),
    enabled: !!agencyId && !!propertyId,
  });

  const { data: authorization } = useQuery({
    queryKey: ['agency', 'authorization', agencyId, propertyId],
    queryFn: () => agencyApi.getAuthorizationDetail(agencyId!, propertyId!),
    enabled: !!agencyId && !!propertyId,
  });

  const { data: myProperties } = useQuery({
    queryKey: ['agency', 'properties', agencyId],
    queryFn: () => agencyApi.getMyProperties(agencyId!),
    enabled: !!agencyId,
  });

  useEffect(() => {
    if (!propertyId && myProperties?.items?.length) {
      navigate(`/agency/calendar/${myProperties.items[0].propertyId}`, { replace: true });
    }
  }, [propertyId, myProperties, navigate]);

  // Build events
  const events = useMemo(() => {
    if (!calendarData?.units) return [];
    
    const allEvents: any[] = [];
    
    calendarData.units.forEach((unit: any) => {
      unit.dailyData?.forEach((day: any) => {
        if (day.status === 'Reserved' && day.reservation) {
          allEvents.push({
            id: `res-${day.reservation.id}`,
            title: `${day.guestName || 'Rezerve'} - ${unit.unitName}`,
            start: day.date,
            end: new Date(new Date(day.date).getTime() + 86400000).toISOString().split('T')[0],
            backgroundColor: '#3B82F6',
            borderColor: 'transparent',
            textColor: '#FFFFFF',
            extendedProps: {
              type: 'reservation',
              unitName: unit.unitName,
              guestName: day.guestName,
              reservationNumber: day.reservationNumber,
            },
          });
        } else if (day.status === 'Blocked') {
          allEvents.push({
            id: `block-${unit.unitId}-${day.date}`,
            title: `🔒 ${day.blockReason || 'Bloke'}`,
            start: day.date,
            end: new Date(new Date(day.date).getTime() + 86400000).toISOString().split('T')[0],
            backgroundColor: '#F59E0B',
            borderColor: 'transparent',
            textColor: '#FFFFFF',
            extendedProps: {
              type: 'block',
              unitName: unit.unitName,
              blockReason: day.blockReason,
            },
          });
        }
      });
    });
    
    return allEvents;
  }, [calendarData]);

  const canCreateReservation = authorization?.canCreateReservation;
  const canViewPrices = authorization?.canViewPrices;
  const priceDisplay = authorization?.priceDisplay;
  const commissionRate = authorization?.customCommissionRate ?? 10;

  const handleDateClick = (info: any) => {
    if (!canCreateReservation) {
      toast.error('Bu mülk için rezervasyon yetkiniz bulunmamaktadır');
      return;
    }
    
    // Kontenjan kontrolü
    if (authorization?.hasAllotment && authorization.remainingAllotment <= 0) {
      toast.error('Kontenjanınız dolmuş');
      return;
    }
    
    navigate(`/agency/reservations/new?propertyId=${propertyId}&checkIn=${info.dateStr}`);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {calendarData?.propertyName || 'Mülk Takvimi'}
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Müsaitlik durumunu görüntüleyin ve rezervasyon oluşturun
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          {canCreateReservation && (
            <Button
              onClick={() => navigate(`/agency/reservations/new?propertyId=${propertyId}`)}
              leftIcon={<Plus className="w-4 h-4" />}
            >
              Yeni Rezervasyon
            </Button>
          )}
        </div>
      </div>

      {/* Authorization Info */}
      {authorization && (
        <Card className="p-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="flex items-center gap-3">
              <div className={`p-2 rounded-lg ${canCreateReservation ? 'bg-green-100' : 'bg-red-100'}`}>
                <Info className={`w-5 h-5 ${canCreateReservation ? 'text-green-600' : 'text-red-600'}`} />
              </div>
              <div>
                <p className="text-xs text-gray-500">Rezervasyon Yetkisi</p>
                <p className="text-sm font-medium">{canCreateReservation ? 'Var' : 'Yok'}</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              <div className={`p-2 rounded-lg ${canViewPrices ? 'bg-blue-100' : 'bg-red-100'}`}>
                <DollarSign className={`w-5 h-5 ${canViewPrices ? 'text-blue-600' : 'text-red-600'}`} />
              </div>
              <div>
                <p className="text-xs text-gray-500">Fiyat Görüntüleme</p>
                <p className="text-sm font-medium">{canViewPrices ? 'Var' : 'Yok'}</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-purple-100">
                <DollarSign className="w-5 h-5 text-purple-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500">Fiyat Tipi</p>
                <p className="text-sm font-medium">
                  {priceDisplay === 'Net' ? 'Net Fiyat' :
                   priceDisplay === 'Commission' ? 'Komisyon Dahil' :
                   'Markup'}
                </p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              <div className="p-2 rounded-lg bg-green-100">
                <DollarSign className="w-5 h-5 text-green-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500">Komisyon</p>
                <p className="text-sm font-medium">%{commissionRate}</p>
              </div>
            </div>
          </div>
          
          {authorization.hasAllotment && (
            <div className="mt-4 pt-4 border-t">
              <div className="flex items-center justify-between text-sm mb-2">
                <span className="text-gray-500">Kontenjan Kullanımı</span>
                <span className="font-medium">
                  {authorization.usedAllotment} / {authorization.totalAllotment}
                  {' '}
                  <span className="text-green-600">({authorization.remainingAllotment} kaldı)</span>
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full ${
                    authorization.remainingAllotment > 0 ? 'bg-green-500' : 'bg-red-500'
                  }`}
                  style={{
                    width: `${authorization.totalAllotment > 0 ? (authorization.usedAllotment / authorization.totalAllotment * 100) : 0}%`
                  }}
                />
              </div>
            </div>
          )}
        </Card>
      )}

      {/* Calendar */}
      <Card>
        <div className="flex items-center justify-between p-4 border-b">
          <div className="flex items-center gap-3">
            <button
              onClick={() => calendarRef.current?.getApi().prev()}
              className="p-2 rounded-lg hover:bg-gray-100"
            >
              <ChevronLeft className="w-5 h-5" />
            </button>
            <button
              onClick={() => calendarRef.current?.getApi().today()}
              className="px-3 py-1.5 text-sm font-medium bg-blue-100 text-blue-700 rounded-lg"
            >
              Bugün
            </button>
            <button
              onClick={() => calendarRef.current?.getApi().next()}
              className="p-2 rounded-lg hover:bg-gray-100"
            >
              <ChevronRight className="w-5 h-5" />
            </button>
            <h2 className="text-lg font-semibold" id="cal-title" />
          </div>
        </div>
        
        <div className="p-4">
          <FullCalendar
            ref={calendarRef}
            plugins={[dayGridPlugin, interactionPlugin]}
            initialView="dayGridMonth"
            locales={[trLocale]}
            locale="tr"
            headerToolbar={false}
            events={events}
            dateClick={handleDateClick}
            eventClick={(info) => {
              setSelectedEvent(info.event);
              setShowEventDetail(true);
            }}
            height="auto"
            firstDay={1}
            datesSet={(dateInfo) => {
              setDateRange((prev) => {
                const prevStart = prev.start.getTime();
                const prevEnd = prev.end.getTime();
                const nextStart = dateInfo.start.getTime();
                const nextEnd = dateInfo.end.getTime();
                if (prevStart === nextStart && prevEnd === nextEnd) {
                  return prev;
                }
                return { start: dateInfo.start, end: dateInfo.end };
              });
              const titleEl = document.getElementById('cal-title');
              if (titleEl) titleEl.textContent = dateInfo.view.title;
            }}
          />
        </div>
        
        {/* Legend */}
        <div className="flex flex-wrap items-center gap-4 p-4 border-t bg-gray-50 rounded-b-xl">
          <span className="text-xs font-medium text-gray-500">LEJANT:</span>
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-blue-500" />
            <span className="text-xs text-gray-600">Rezerve</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-amber-500" />
            <span className="text-xs text-gray-600">Bloke</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-white border border-gray-300" />
            <span className="text-xs text-gray-600">Müsait (Tıklayarak rezervasyon yapın)</span>
          </div>
        </div>
      </Card>

      {/* Unit List with Availability */}
      {calendarData?.units && (
        <Card>
          <div className="px-6 py-4 border-b">
            <h3 className="text-lg font-semibold">Birimler ve Fiyatlar</h3>
          </div>
          <div className="divide-y">
            {calendarData.units.map((unit: any) => (
              <div key={unit.unitId} className="px-6 py-4 hover:bg-gray-50">
                <div className="flex items-center justify-between">
                  <div>
                    <h4 className="font-medium text-gray-900">{unit.unitName}</h4>
                    {unit.unitNumber && (
                      <p className="text-xs text-gray-500">No: {unit.unitNumber}</p>
                    )}
                  </div>
                  
                  {canViewPrices && unit.dailyData?.[0]?.agencyPrice && (
                    <div className="text-right">
                      <p className="text-lg font-bold text-blue-600">
                        {formatCurrency(unit.dailyData[0].agencyPrice, unit.dailyData[0].currencyCode || 'TRY')}
                      </p>
                      <p className="text-xs text-gray-500">/ gece</p>
                    </div>
                  )}
                </div>
                
                {/* Availability bar for next 7 days */}
                <div className="flex gap-1 mt-2">
                  {unit.dailyData?.slice(0, 7).map((day: any, index: number) => (
                    <div
                      key={index}
                      className={`flex-1 h-1.5 rounded-full ${
                        day.status === 'Available' ? 'bg-green-400' :
                        day.status === 'Reserved' ? 'bg-blue-400' :
                        day.status === 'Blocked' ? 'bg-amber-400' :
                        'bg-gray-300'
                      }`}
                      title={`${formatDate(day.date)}: ${
                        day.status === 'Available' ? 'Müsait' :
                        day.status === 'Reserved' ? 'Rezerve' :
                        day.status === 'Blocked' ? 'Bloke' : 'Bilinmiyor'
                      }`}
                    />
                  ))}
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Event Detail Modal */}
      <Modal
        isOpen={showEventDetail}
        onClose={() => setShowEventDetail(false)}
        title="Detay"
        size="sm"
      >
        {selectedEvent && (
          <div className="space-y-3">
            <div>
              <label className="text-xs text-gray-500">Tarih</label>
              <p className="font-medium">{formatDate(selectedEvent.start)}</p>
            </div>
            <div>
              <label className="text-xs text-gray-500">Birim</label>
              <p className="font-medium">{selectedEvent.extendedProps.unitName}</p>
            </div>
            <div>
              <label className="text-xs text-gray-500">Durum</label>
              <span className={`inline-flex px-2 py-0.5 text-xs font-medium rounded-full ${
                selectedEvent.extendedProps.type === 'reservation'
                  ? 'bg-blue-100 text-blue-800'
                  : 'bg-amber-100 text-amber-800'
              }`}>
                {selectedEvent.extendedProps.type === 'reservation' ? 'Rezerve' : 'Bloke'}
              </span>
            </div>
            {selectedEvent.extendedProps.type === 'reservation' && (
              <>
                <div>
                  <label className="text-xs text-gray-500">Misafir</label>
                  <p className="font-medium">{selectedEvent.extendedProps.guestName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Rezervasyon No</label>
                  <p className="font-medium">{selectedEvent.extendedProps.reservationNumber}</p>
                </div>
              </>
            )}
            {selectedEvent.extendedProps.type === 'block' && (
              <div>
                <label className="text-xs text-gray-500">Sebep</label>
                <p className="font-medium">{selectedEvent.extendedProps.blockReason}</p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
