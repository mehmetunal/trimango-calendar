// src/pages/agency/Reservations/ReservationList.tsx
import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Filter,
  Plus,
  Calendar,
  Eye,
  CheckCircle,
  XCircle,
  Download,
  ChevronDown,
  Building2,
  Mail,
  Phone,
  Clock,
  DollarSign,
} from 'lucide-react';
import { reservationApi } from '../../api/reservation.api';
import { Button, Input, Select, Card, Badge, Pagination, Modal, ConfirmDialog } from '../../components/ui';
import { useDebounce } from '../../hooks/useDebounce';
import { formatCurrency, formatDate, formatDateTime } from '../../utils/format';
import { RESERVATION_STATUSES, PROPERTY_TYPES } from '../../utils/constants';
import { useAuthStore } from '../../stores/authStore';
import toast from 'react-hot-toast';

export default function AgencyReservationList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const user = useAuthStore((state: { user: any; }) => state.user);
  const agencyId = user?.agencyId;

  // States
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [propertyFilter, setPropertyFilter] = useState('');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const debouncedSearch = useDebounce(search);

  // Detail modal
  const [selectedReservation, setSelectedReservation] = useState<any>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);

  // Cancel
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelReservationId, setCancelReservationId] = useState<string | null>(null);

  const queryParams = useMemo(() => ({
    page,
    pageSize,
    searchTerm: debouncedSearch || undefined,
    status: statusFilter || undefined,
    propertyId: propertyFilter || undefined,
  }), [page, pageSize, debouncedSearch, statusFilter, propertyFilter]);

  const { data, isLoading } = useQuery({
    queryKey: ['agency', 'reservations', agencyId, queryParams],
    queryFn: () => reservationApi.getAgencyReservations(agencyId!, queryParams),
    enabled: !!agencyId,
    placeholderData: (previousData) => previousData,
  });

  // Cancel mutation
  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      reservationApi.cancelAgencyReservation(agencyId!, id, reason),
    onSuccess: () => {
      toast.success('Rezervasyon iptal edildi');
      setShowCancelModal(false);
      setCancelReason('');
      setCancelReservationId(null);
      queryClient.invalidateQueries({ queryKey: ['agency', 'reservations'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const getStatusBadge = (status: string) => {
    const config = RESERVATION_STATUSES[status as keyof typeof RESERVATION_STATUSES];
    if (!config) return null;
    return (
      <span className={`inline-flex items-center px-2.5 py-1 text-xs font-medium rounded-full ${config.bgColor} ${config.textColor}`}>
        {config.label}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Rezervasyonlarım</h1>
          <p className="text-sm text-gray-500 mt-1">
            {data?.totalCount || 0} rezervasyon bulundu
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="outline" size="sm" leftIcon={<Download className="w-4 h-4" />}>
            Excel
          </Button>
          <Button size="sm" leftIcon={<Plus className="w-4 h-4" />} onClick={() => navigate('/agency/reservations/new')}>
            Yeni Rezervasyon
          </Button>
          <Button size="sm" variant="outline" leftIcon={<Calendar className="w-4 h-4" />} onClick={() => navigate('/agency/calendar')}>
            Takvim
          </Button>
        </div>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Misafir adı, rez. no ara..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="form-input pl-10"
            />
          </div>

          <Select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            options={[
              { value: '', label: 'Tüm Durumlar' },
              ...Object.entries(RESERVATION_STATUSES).map(([key, val]) => ({
                value: key,
                label: val.label,
              })),
            ]}
          />

          <Select
            value={propertyFilter}
            onChange={(e) => setPropertyFilter(e.target.value)}
            options={[
              { value: '', label: 'Tüm Mülkler' },
              // Mülkler API'den gelecek
            ]}
          />
        </div>
      </Card>

      {/* Table */}
      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="table">
            <thead>
              <tr>
                <th>Rez. No</th>
                <th>Misafir</th>
                <th>Mülk/Birim</th>
                <th>Tarih</th>
                <th>Tutar</th>
                <th>Durum</th>
                <th>İşlem</th>
              </tr>
            </thead>
            <tbody>
              {data?.items?.map((reservation: any) => (
                <tr
                  key={reservation.id}
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => {
                    setSelectedReservation(reservation);
                    setShowDetailModal(true);
                  }}
                >
                  <td className="font-medium text-blue-600">
                    {reservation.reservationNumber}
                  </td>
                  <td>
                    <div>
                      <div className="font-medium">{reservation.guestName}</div>
                      <div className="text-xs text-gray-500">{reservation.guestEmail}</div>
                    </div>
                  </td>
                  <td>
                    <div>
                      <div>{reservation.propertyName}</div>
                      <div className="text-xs text-gray-500">{reservation.unitName}</div>
                    </div>
                  </td>
                  <td>
                    <div className="text-sm">
                      <div>Giriş: {formatDate(reservation.checkIn)}</div>
                      <div>Çıkış: {formatDate(reservation.checkOut)}</div>
                      <div className="text-xs text-gray-500">{reservation.totalNights} gece</div>
                    </div>
                  </td>
                  <td className="font-medium">
                    {formatCurrency(reservation.totalAmount, reservation.currencyCode)}
                  </td>
                  <td>{getStatusBadge(reservation.status)}</td>
                  <td>
                    <div className="flex items-center gap-1">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          setSelectedReservation(reservation);
                          setShowDetailModal(true);
                        }}
                        className="p-1.5 rounded-lg hover:bg-gray-100"
                        title="Detay"
                      >
                        <Eye className="w-4 h-4" />
                      </button>
                      {(reservation.status === 'Pending' || reservation.status === 'Confirmed') && (
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            setCancelReservationId(reservation.id);
                            setShowCancelModal(true);
                          }}
                          className="p-1.5 rounded-lg hover:bg-red-50 text-red-500"
                          title="İptal"
                        >
                          <XCircle className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {!isLoading && data?.items?.length === 0 && (
          <div className="text-center py-16">
            <Calendar className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">Henüz rezervasyon yok</h3>
            <p className="text-sm text-gray-500 mb-6">İlk rezervasyonunuzu oluşturun</p>
            <Button onClick={() => navigate('/agency/reservations/new')} leftIcon={<Plus className="w-4 h-4" />}>
              Rezervasyon Yap
            </Button>
          </div>
        )}
      </Card>

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

      {/* Detail Modal */}
      <Modal
        isOpen={showDetailModal}
        onClose={() => setShowDetailModal(false)}
        title="Rezervasyon Detayı"
        size="lg"
      >
        {selectedReservation && (
          <div className="space-y-6">
            {/* Status */}
            <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
              <div>
                <span className="text-sm text-gray-500">Rezervasyon No</span>
                <p className="text-lg font-bold">{selectedReservation.reservationNumber}</p>
              </div>
              {getStatusBadge(selectedReservation.status)}
            </div>

            {/* Guest Info */}
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase mb-3">Misafir Bilgileri</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-xs text-gray-500">Ad Soyad</label>
                  <p className="font-medium">{selectedReservation.guestName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Email</label>
                  <div className="flex items-center gap-2">
                    <Mail className="w-4 h-4 text-gray-400" />
                    <a href={`mailto:${selectedReservation.guestEmail}`} className="text-blue-600">
                      {selectedReservation.guestEmail}
                    </a>
                  </div>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Telefon</label>
                  <div className="flex items-center gap-2">
                    <Phone className="w-4 h-4 text-gray-400" />
                    <span>{selectedReservation.guestPhone}</span>
                  </div>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Misafir Sayısı</label>
                  <p className="font-medium">
                    {selectedReservation.adults} Yetişkin
                    {selectedReservation.children > 0 && `, ${selectedReservation.children} Çocuk`}
                  </p>
                </div>
              </div>
            </div>

            {/* Stay Info */}
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase mb-3">Konaklama Bilgileri</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-xs text-gray-500">Mülk</label>
                  <p className="font-medium">{selectedReservation.propertyName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Birim</label>
                  <p className="font-medium">{selectedReservation.unitName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Giriş</label>
                  <p className="font-medium">{formatDate(selectedReservation.checkIn)}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Çıkış</label>
                  <p className="font-medium">{formatDate(selectedReservation.checkOut)}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Gece</label>
                  <p className="font-medium">{selectedReservation.totalNights}</p>
                </div>
              </div>
            </div>

            {/* Payment */}
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase mb-3">Ödeme</h3>
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="text-xs text-gray-500">Toplam</label>
                  <p className="text-lg font-bold text-blue-600">
                    {formatCurrency(selectedReservation.totalAmount, selectedReservation.currencyCode)}
                  </p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Ödenen</label>
                  <p className="font-medium text-green-600">
                    {formatCurrency(selectedReservation.paidAmount, selectedReservation.currencyCode)}
                  </p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Kalan</label>
                  <p className="font-medium text-red-600">
                    {formatCurrency(selectedReservation.remainingAmount, selectedReservation.currencyCode)}
                  </p>
                </div>
              </div>
            </div>
          </div>
        )}
      </Modal>

      {/* Cancel Modal */}
      <Modal
        isOpen={showCancelModal}
        onClose={() => {
          setShowCancelModal(false);
          setCancelReason('');
          setCancelReservationId(null);
        }}
        title="Rezervasyonu İptal Et"
        size="sm"
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => setShowCancelModal(false)}>
              Vazgeç
            </Button>
            <Button
              variant="danger"
              onClick={() => {
                if (cancelReservationId && cancelReason.trim()) {
                  cancelMutation.mutate({ id: cancelReservationId, reason: cancelReason });
                }
              }}
              isLoading={cancelMutation.isPending}
              disabled={!cancelReason.trim()}
            >
              İptal Et
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          <div className="flex items-center gap-3 p-3 bg-red-50 rounded-lg">
            <XCircle className="w-5 h-5 text-red-600 flex-shrink-0" />
            <p className="text-sm text-red-800">
              Bu işlem geri alınamaz. Rezervasyon iptal edildiğinde mülk sahibine bilgi verilecektir.
            </p>
          </div>
          <div>
            <label className="form-label">İptal Sebebi *</label>
            <textarea
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              className="form-input"
              placeholder="İptal sebebini açıklayın..."
              required
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}