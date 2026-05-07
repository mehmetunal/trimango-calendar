// src/pages/tenant/Reservations/ReservationDetail.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Calendar,
  Users,
  User,
  Mail,
  Phone,
  MapPin,
  Clock,
  DollarSign,
  CreditCard,
  CheckCircle,
  XCircle,
  LogOut,
  Edit,
  Printer,
  Send,
  AlertCircle,
  MessageSquare,
  ChevronRight,
} from 'lucide-react';
import { reservationApi } from '../../../api/reservation.api';
import { Button, Card, Badge, Modal } from '../../../components/ui';
import { formatCurrency, formatDate, formatDateTime, formatTime, getNights } from '../../../utils/format';
import { RESERVATION_STATUSES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function ReservationDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');

  const { data: reservation, isLoading } = useQuery({
    queryKey: ['reservation', id],
    queryFn: () => reservationApi.getById(id!),
    enabled: !!id,
  });

  const checkInMutation = useMutation({
    mutationFn: () => reservationApi.checkIn(id!),
    onSuccess: () => {
      toast.success('Check-in yapıldı');
      queryClient.invalidateQueries({ queryKey: ['reservation', id] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const checkOutMutation = useMutation({
    mutationFn: (isLate: boolean) => reservationApi.checkOut(id!, isLate),
    onSuccess: () => {
      toast.success('Check-out yapıldı');
      queryClient.invalidateQueries({ queryKey: ['reservation', id] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  const cancelMutation = useMutation({
    mutationFn: (reason: string) => reservationApi.cancel(id!, reason),
    onSuccess: () => {
      toast.success('Rezervasyon iptal edildi');
      setShowCancelModal(false);
      queryClient.invalidateQueries({ queryKey: ['reservation', id] });
    },
    onError: (error: any) => toast.error(error.message),
  });

  if (isLoading) {
    return (
      <div className="space-y-6 animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/4" />
        <div className="h-64 bg-gray-200 rounded-xl" />
      </div>
    );
  }

  if (!reservation) {
    return (
      <div className="text-center py-20">
        <Calendar className="w-20 h-20 text-gray-300 mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-gray-900 mb-2">Rezervasyon bulunamadı</h2>
        <Button onClick={() => navigate('/dashboard/reservations')}>
          Rezervasyonlara Dön
        </Button>
      </div>
    );
  }

  const statusConfig = RESERVATION_STATUSES[reservation.status as keyof typeof RESERVATION_STATUSES];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate('/dashboard/reservations')}
            className="p-2 rounded-lg hover:bg-gray-100"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-2xl font-bold text-gray-900">
                {reservation.reservationNumber}
              </h1>
              <Badge color={statusConfig?.color as any}>
                {statusConfig?.label}
              </Badge>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              Oluşturulma: {formatDateTime(reservation.createdAt)}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {reservation.status === 'Confirmed' && (
            <Button
              size="sm"
              onClick={() => checkInMutation.mutate()}
              isLoading={checkInMutation.isPending}
              leftIcon={<CheckCircle className="w-4 h-4" />}
            >
              Check-in
            </Button>
          )}
          {reservation.status === 'CheckedIn' && (
            <Button
              size="sm"
              onClick={() => checkOutMutation.mutate(false)}
              isLoading={checkOutMutation.isPending}
              leftIcon={<LogOut className="w-4 h-4" />}
            >
              Check-out
            </Button>
          )}
          {(reservation.status === 'Pending' || reservation.status === 'Confirmed') && (
            <Button
              variant="danger"
              size="sm"
              onClick={() => setShowCancelModal(true)}
              leftIcon={<XCircle className="w-4 h-4" />}
            >
              İptal Et
            </Button>
          )}
          <Button variant="outline" size="sm" leftIcon={<Printer className="w-4 h-4" />}>
            Yazdır
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          {/* Stay Info */}
          <Card>
            <div className="card-header">
              <h3 className="text-lg font-semibold">Konaklama Bilgileri</h3>
            </div>
            <div className="card-body">
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <label className="text-xs text-gray-500 uppercase">Mülk</label>
                  <p className="font-medium text-gray-900">{reservation.propertyName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Birim</label>
                  <p className="font-medium text-gray-900">{reservation.unitName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Giriş Tarihi</label>
                  <p className="font-medium text-gray-900">{formatDate(reservation.checkIn)}</p>
                  <p className="text-xs text-gray-500">{reservation.checkIn || '14:00'}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Çıkış Tarihi</label>
                  <p className="font-medium text-gray-900">{formatDate(reservation.checkOut)}</p>
                  <p className="text-xs text-gray-500">{reservation.checkOut || '12:00'}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Gece Sayısı</label>
                  <p className="font-medium text-gray-900">{reservation.totalNights} Gece</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Misafir Sayısı</label>
                  <p className="font-medium text-gray-900">
                    {reservation.adults} Yetişkin
                    {reservation.children > 0 && `, ${reservation.children} Çocuk`}
                  </p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Kaynak</label>
                  <p className="font-medium text-gray-900">{reservation.source}</p>
                </div>
              </div>
            </div>
          </Card>

          {/* Guest Info */}
          <Card>
            <div className="card-header">
              <h3 className="text-lg font-semibold">Misafir Bilgileri</h3>
            </div>
            <div className="card-body">
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <label className="text-xs text-gray-500 uppercase">Ad Soyad</label>
                  <p className="font-medium text-gray-900">{reservation.guestName}</p>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Email</label>
                  <div className="flex items-center gap-2">
                    <Mail className="w-4 h-4 text-gray-400" />
                    <a href={`mailto:${reservation.guestEmail}`} className="text-blue-600">
                      {reservation.guestEmail}
                    </a>
                  </div>
                </div>
                <div>
                  <label className="text-xs text-gray-500 uppercase">Telefon</label>
                  <div className="flex items-center gap-2">
                    <Phone className="w-4 h-4 text-gray-400" />
                    <a href={`tel:${reservation.guestPhone}`}>{reservation.guestPhone}</a>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* Special Requests */}
          {reservation.specialRequests && (
            <Card>
              <div className="card-header">
                <h3 className="text-lg font-semibold">Özel İstekler</h3>
              </div>
              <div className="card-body">
                <div className="flex items-start gap-2 p-3 bg-yellow-50 rounded-lg">
                  <MessageSquare className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                  <p className="text-sm text-yellow-800">{reservation.specialRequests}</p>
                </div>
              </div>
            </Card>
          )}
        </div>

        {/* Sidebar - Payment */}
        <div className="space-y-6">
          <Card>
            <div className="card-header">
              <h3 className="text-lg font-semibold">Ödeme Bilgileri</h3>
            </div>
            <div className="card-body space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-500">Toplam Tutar</span>
                <span className="text-lg font-bold text-blue-600">
                  {formatCurrency(reservation.totalAmount, reservation.currencyCode)}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-500">Ödenen</span>
                <span className="font-medium text-green-600">
                  {formatCurrency(reservation.paidAmount, reservation.currencyCode)}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-500">Kalan</span>
                <span className="font-medium text-red-600">
                  {formatCurrency(reservation.remainingAmount, reservation.currencyCode)}
                </span>
              </div>
              
              <div className="border-t pt-4">
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className="bg-blue-500 h-2 rounded-full"
                    style={{
                      width: `${reservation.totalAmount > 0 ? (reservation.paidAmount / reservation.totalAmount) * 100 : 0}%`
                    }}
                  />
                </div>
                <p className="text-xs text-gray-500 mt-1 text-center">
                  %{reservation.totalAmount > 0 ? Math.round((reservation.paidAmount / reservation.totalAmount) * 100) : 0} ödendi
                </p>
              </div>
            </div>
          </Card>

          {/* Status Timeline */}
          <Card>
            <div className="card-header">
              <h3 className="text-lg font-semibold">Durum Geçmişi</h3>
            </div>
            <div className="card-body">
              <div className="timeline">
                <div className="timeline-item completed">
                  <p className="text-sm font-medium">Rezervasyon Oluşturuldu</p>
                  <p className="text-xs text-gray-500">{formatDateTime(reservation.createdAt)}</p>
                </div>
                {reservation.status !== 'Pending' && (
                  <div className="timeline-item completed">
                    <p className="text-sm font-medium">Onaylandı</p>
                    <p className="text-xs text-gray-500">{formatDateTime(reservation.createdAt)}</p>
                  </div>
                )}
                {reservation.status === 'CheckedIn' || reservation.status === 'CheckedOut' ? (
                  <div className="timeline-item completed">
                    <p className="text-sm font-medium">Check-in Yapıldı</p>
                    <p className="text-xs text-gray-500">{formatDateTime(reservation.createdAt)}</p>
                  </div>
                ) : null}
                {reservation.status === 'CheckedOut' && (
                  <div className="timeline-item completed">
                    <p className="text-sm font-medium">Check-out Yapıldı</p>
                    <p className="text-xs text-gray-500">{formatDateTime(reservation.createdAt)}</p>
                  </div>
                )}
                {reservation.status === 'Cancelled' && (
                  <div className="timeline-item cancelled">
                    <p className="text-sm font-medium">İptal Edildi</p>
                    <p className="text-xs text-gray-500">{formatDateTime(reservation.createdAt)}</p>
                  </div>
                )}
              </div>
            </div>
          </Card>
        </div>
      </div>

      {/* Cancel Modal */}
      <Modal
        isOpen={showCancelModal}
        onClose={() => setShowCancelModal(false)}
        title="Rezervasyonu İptal Et"
        size="sm"
        footer={
          <div className="flex justify-end gap-3">
            <Button variant="outline" onClick={() => setShowCancelModal(false)}>
              Vazgeç
            </Button>
            <Button
              variant="danger"
              onClick={() => cancelMutation.mutate(cancelReason)}
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
            <AlertCircle className="w-5 h-5 text-red-600 flex-shrink-0" />
            <p className="text-sm text-red-800">
              Bu işlem geri alınamaz. Rezervasyon iptal edildiğinde misafire bilgi verilecektir.
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