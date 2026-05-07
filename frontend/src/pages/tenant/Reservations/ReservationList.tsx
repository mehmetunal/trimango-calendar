// src/pages/tenant/Reservations/ReservationList.tsx
import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Search,
  Filter,
  Download,
  Plus,
  Calendar,
  ChevronDown,
  Eye,
  CheckCircle,
  XCircle,
  LogOut,
  MoreVertical,
} from 'lucide-react';
import { reservationApi } from '../../../api/reservation.api';
import { Button, Input, Select, Table, Badge, Pagination, Modal } from '../../../components/ui';
import { useDebounce } from '../../../hooks/useDebounce';
import { formatCurrency, formatDate, formatDateTime } from '../../../utils/format';
import { RESERVATION_STATUSES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function ReservationList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  // Filters
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dateFilter, setDateFilter] = useState('all');
  const debouncedSearch = useDebounce(search);
  
  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  
  // Selected reservation for actions
  const [selectedReservation, setSelectedReservation] = useState<any>(null);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  // Query params
  const queryParams = useMemo(() => ({
    page,
    pageSize,
    searchTerm: debouncedSearch || undefined,
    status: statusFilter || undefined,
  }), [page, pageSize, debouncedSearch, statusFilter]);

  const { data, isLoading } = useQuery({
    queryKey: ['reservations', queryParams],
    queryFn: () => reservationApi.getAll(queryParams),
    placeholderData: (previousData) => previousData,
  });

  // Mutations
  const checkInMutation = useMutation({
    mutationFn: (id: string) => reservationApi.checkIn(id),
    onSuccess: () => {
      toast.success('Check-in yapıldı');
      queryClient.invalidateQueries({ queryKey: ['reservations'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const checkOutMutation = useMutation({
    mutationFn: (id: string) => reservationApi.checkOut(id),
    onSuccess: () => {
      toast.success('Check-out yapıldı');
      queryClient.invalidateQueries({ queryKey: ['reservations'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const cancelMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) =>
      reservationApi.cancel(id, reason),
    onSuccess: () => {
      toast.success('Rezervasyon iptal edildi');
      setShowCancelModal(false);
      setCancelReason('');
      queryClient.invalidateQueries({ queryKey: ['reservations'] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const columns = [
    {
      key: 'reservationNumber',
      header: 'Rez. No',
      sortable: true,
      render: (item: any) => (
        <span className="font-medium text-blue-600">{item.reservationNumber}</span>
      ),
    },
    {
      key: 'guestName',
      header: 'Misafir',
      sortable: true,
      render: (item: any) => (
        <div>
          <div className="font-medium">{item.guestName}</div>
          <div className="text-xs text-gray-500">{item.guestEmail}</div>
        </div>
      ),
    },
    {
      key: 'propertyName',
      header: 'Mülk/Birim',
      render: (item: any) => (
        <div>
          <div>{item.propertyName}</div>
          <div className="text-xs text-gray-500">{item.unitName}</div>
        </div>
      ),
    },
    {
      key: 'checkIn',
      header: 'Giriş-Çıkış',
      sortable: true,
      render: (item: any) => (
        <div>
          <div className="text-sm">Giriş: {formatDate(item.checkIn)}</div>
          <div className="text-sm">Çıkış: {formatDate(item.checkOut)}</div>
          <div className="text-xs text-gray-500">{item.totalNights} gece</div>
        </div>
      ),
    },
    {
      key: 'totalAmount',
      header: 'Tutar',
      sortable: true,
      render: (item: any) => (
        <div className="font-medium">
          {formatCurrency(item.totalAmount, item.currencyCode)}
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Durum',
      sortable: true,
      render: (item: any) => {
        const status = RESERVATION_STATUSES[item.status as keyof typeof RESERVATION_STATUSES];
        return (
          <span className={`inline-flex px-2.5 py-1 text-xs font-medium rounded-full ${status.bgColor} ${status.textColor}`}>
            {status.label}
          </span>
        );
      },
    },
    {
      key: 'actions',
      header: '',
      render: (item: any) => (
        <div className="flex items-center gap-1">
          {item.status === 'Confirmed' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={(e) => {
                e.stopPropagation();
                checkInMutation.mutate(item.id);
              }}
              title="Check-in"
            >
              <CheckCircle className="w-4 h-4 text-green-600" />
            </Button>
          )}
          {item.status === 'CheckedIn' && (
            <Button
              size="sm"
              variant="ghost"
              onClick={(e) => {
                e.stopPropagation();
                checkOutMutation.mutate(item.id);
              }}
              title="Check-out"
            >
              <LogOut className="w-4 h-4 text-blue-600" />
            </Button>
          )}
          <Button
            size="sm"
            variant="ghost"
            onClick={(e) => {
              e.stopPropagation();
              setSelectedReservation(item);
              setShowDetailModal(true);
            }}
            title="Detay"
          >
            <Eye className="w-4 h-4" />
          </Button>
          {(item.status === 'Pending' || item.status === 'Confirmed') && (
            <Button
              size="sm"
              variant="ghost"
              onClick={(e) => {
                e.stopPropagation();
                setSelectedReservation(item);
                setShowCancelModal(true);
              }}
              title="İptal"
            >
              <XCircle className="w-4 h-4 text-red-600" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Rezervasyonlar</h1>
          <p className="text-sm text-gray-500 mt-1">
            {data?.totalCount || 0} rezervasyon bulundu
          </p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="outline" size="sm" leftIcon={<Download className="w-4 h-4" />}>
            Excel Export
          </Button>
          <Button size="sm" leftIcon={<Plus className="w-4 h-4" />} onClick={() => navigate('/dashboard/reservations/new')}>
            Yeni Rezervasyon
          </Button>
          <Button size="sm" variant="outline" leftIcon={<Calendar className="w-4 h-4" />} onClick={() => navigate('/dashboard/reservations/calendar')}>
            Takvim
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl border p-4">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Misafir adı, rez. no ara..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
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
            value={dateFilter}
            onChange={(e) => setDateFilter(e.target.value)}
            options={[
              { value: 'all', label: 'Tüm Tarihler' },
              { value: 'today', label: 'Bugün' },
              { value: 'tomorrow', label: 'Yarın' },
              { value: 'week', label: 'Bu Hafta' },
              { value: 'month', label: 'Bu Ay' },
            ]}
          />

          <Button variant="outline" leftIcon={<Filter className="w-4 h-4" />}>
            Diğer Filtreler
          </Button>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border overflow-hidden">
        <Table
          columns={columns}
          data={data?.items || []}
          isLoading={isLoading}
          onRowClick={(item) => {
            setSelectedReservation(item);
            setShowDetailModal(true);
          }}
          emptyMessage="Henüz rezervasyon bulunmamaktadır"
        />
      </div>

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
              <StatusBadge status={selectedReservation.status} />
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
                  <p className="font-medium">{selectedReservation.guestEmail}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Telefon</label>
                  <p className="font-medium">{formatPhoneNumber(selectedReservation.guestPhone)}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Misafir Sayısı</label>
                  <p className="font-medium">{selectedReservation.adults} Yetişkin, {selectedReservation.children} Çocuk</p>
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
                  <label className="text-xs text-gray-500">Giriş Tarihi</label>
                  <p className="font-medium">{formatDateTime(selectedReservation.checkIn)}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Çıkış Tarihi</label>
                  <p className="font-medium">{formatDateTime(selectedReservation.checkOut)}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Gece Sayısı</label>
                  <p className="font-medium">{selectedReservation.totalNights} gece</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500">Kaynak</label>
                  <p className="font-medium">{selectedReservation.source}</p>
                </div>
              </div>
            </div>

            {/* Payment Info */}
            <div>
              <h3 className="text-sm font-medium text-gray-500 uppercase mb-3">Ödeme Bilgileri</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-xs text-gray-500">Toplam Tutar</label>
                  <p className="font-bold text-lg">
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

            {/* Special Requests */}
            {selectedReservation.specialRequests && (
              <div>
                <h3 className="text-sm font-medium text-gray-500 uppercase mb-3">Özel İstekler</h3>
                <p className="text-sm bg-yellow-50 p-3 rounded-lg">{selectedReservation.specialRequests}</p>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Cancel Modal */}
      <Modal
        isOpen={showCancelModal}
        onClose={() => setShowCancelModal(false)}
        title="Rezervasyon İptali"
        size="sm"
        footer={
          <>
            <Button variant="outline" onClick={() => setShowCancelModal(false)}>
              Vazgeç
            </Button>
            <Button
              variant="danger"
              onClick={() => {
                if (selectedReservation && cancelReason) {
                  cancelMutation.mutate({ id: selectedReservation.id, reason: cancelReason });
                }
              }}
              isLoading={cancelMutation.isPending}
              disabled={!cancelReason.trim()}
            >
              İptal Et
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <p className="text-sm text-gray-600">
            <strong>{selectedReservation?.reservationNumber}</strong> numaralı rezervasyonu iptal etmek istediğinize emin misiniz?
          </p>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              İptal Sebebi *
            </label>
            <textarea
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-red-500 focus:border-red-500"
              placeholder="İptal sebebini açıklayın..."
              required
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}


function formatPhoneNumber(phone?: string) {
  return phone || '-';
}

function StatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = { Pending: 'Beklemede', Confirmed: 'Onaylandı', CheckedIn: 'Check-in', CheckedOut: 'Check-out', Cancelled: 'İptal' };
  return <span className="inline-flex px-2 py-1 text-xs rounded-full bg-gray-100 text-gray-700">{map[status] || status}</span>;
}
