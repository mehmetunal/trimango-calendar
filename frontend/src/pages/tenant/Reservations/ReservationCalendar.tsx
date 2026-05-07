import { useState, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import trLocale from '@fullcalendar/core/locales/tr';
import { reservationApi } from '../../../api/reservation.api';
import { Modal } from '../../../components/ui';

export default function ReservationCalendar() {
  const calendarRef = useRef<FullCalendar>(null);
  const [selectedReservation, setSelectedReservation] = useState<any>(null);
  const [dateRange, setDateRange] = useState({
    start: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
    end: new Date(Date.now() + 60 * 24 * 60 * 60 * 1000),
  });

  const { data: reservations } = useQuery({
    queryKey: ['reservations', 'calendar', dateRange],
    queryFn: () => reservationApi.getCalendar(dateRange.start, dateRange.end),
  });

  const events = reservations?.map((res: any) => ({
    id: res.id,
    title: `${res.guestName} - ${res.unitName}`,
    start: res.checkIn,
    end: res.checkOut,
    backgroundColor: getStatusColor(res.status),
    borderColor: getStatusColor(res.status),
    extendedProps: res,
  })) || [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Rezervasyon Takvimi</h1>
        
        {/* Filter buttons */}
        <div className="flex items-center gap-2">
          <Button size="sm" variant="outline" onClick={() => calendarRef.current?.getApi().today()}>
            Bugün
          </Button>
          <Button size="sm" variant="outline" onClick={() => calendarRef.current?.getApi().prev()}>
            ←
          </Button>
          <Button size="sm" variant="outline" onClick={() => calendarRef.current?.getApi().next()}>
            →
          </Button>
        </div>
      </div>

      {/* Legend */}
      <div className="flex items-center gap-4 p-3 bg-white rounded-lg border">
        <LegendItem color="#10B981" label="Onaylı" />
        <LegendItem color="#3B82F6" label="Check-in Yapıldı" />
        <LegendItem color="#F59E0B" label="Beklemede" />
        <LegendItem color="#EF4444" label="İptal" />
        <LegendItem color="#6B7280" label="Check-out" />
      </div>

      <div className="bg-white rounded-xl border p-4">
        <FullCalendar
          ref={calendarRef}
          plugins={[dayGridPlugin, interactionPlugin]}
          initialView="dayGridMonth"
          locales={[trLocale]}
          locale="tr"
          headerToolbar={{
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,dayGridWeek'
          }}
          events={events}
          eventClick={(info) => setSelectedReservation(info.event.extendedProps)}
          eventTimeFormat={{
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
          }}
          height="auto"
          firstDay={1}
          buttonText={{
            today: 'Bugün',
            month: 'Ay',
            week: 'Hafta',
          }}
        />
      </div>

      {/* Reservation Detail Modal */}
      <Modal
        isOpen={!!selectedReservation}
        onClose={() => setSelectedReservation(null)}
        title="Rezervasyon Detayı"
        size="lg"
      >
        {selectedReservation && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm text-gray-500">Rezervasyon No</label>
                <p className="font-medium">{selectedReservation.reservationNumber}</p>
              </div>
              <div>
                <label className="text-sm text-gray-500">Durum</label>
                <StatusBadge status={selectedReservation.status} />
              </div>
              <div>
                <label className="text-sm text-gray-500">Misafir</label>
                <p className="font-medium">{selectedReservation.guestName}</p>
              </div>
              <div>
                <label className="text-sm text-gray-500">İletişim</label>
                <p className="font-medium">{selectedReservation.guestEmail}</p>
                <p className="text-sm">{selectedReservation.guestPhone}</p>
              </div>
              <div>
                <label className="text-sm text-gray-500">Giriş Tarihi</label>
                <p className="font-medium">{formatDate(selectedReservation.checkIn)}</p>
              </div>
              <div>
                <label className="text-sm text-gray-500">Çıkış Tarihi</label>
                <p className="font-medium">{formatDate(selectedReservation.checkOut)}</p>
              </div>
              <div>
                <label className="text-sm text-gray-500">Toplam Tutar</label>
                <p className="font-medium">
                  {formatCurrency(selectedReservation.totalAmount, selectedReservation.currencyCode)}
                </p>
              </div>
              <div>
                <label className="text-sm text-gray-500">Gece Sayısı</label>
                <p className="font-medium">{selectedReservation.totalNights}</p>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

function getStatusColor(status: string): string {
  const colors: Record<string, string> = {
    Confirmed: '#10B981',
    CheckedIn: '#3B82F6',
    Pending: '#F59E0B',
    Cancelled: '#EF4444',
    CheckedOut: '#6B7280',
  };
  return colors[status] || '#6B7280';
}

function LegendItem({ color, label }: { color: string; label: string }) {
  return (
    <div className="flex items-center gap-2">
      <div className="w-3 h-3 rounded" style={{ backgroundColor: color }} />
      <span className="text-sm text-gray-600">{label}</span>
    </div>
  );
}
Mülk Formu
