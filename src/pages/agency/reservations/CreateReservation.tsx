import { useState, useEffect, useMemo } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Calendar,
  Users,
  Search,
  Building2,
  DollarSign,
  CreditCard,
  User,
  Mail,
  Phone,
  MapPin,
  Info,
  Check,
  ArrowLeft,
  Save,
} from 'lucide-react';
import { agencyApi } from '../../../api/agency.api';
import { reservationApi } from '../../../api/reservation.api';
import { pricingApi } from '../../../api/pricing.api';
import { Button, Input, Select, Card, Modal } from '../../../components/ui';
import { formatCurrency, formatDate, getNights } from '../../../utils/format';
import { CURRENCIES } from '../../../utils/constants';
import { useAuthStore } from '../../../stores/authStore';
import toast from 'react-hot-toast';

const reservationSchema = z.object({
  propertyId: z.string().min(1, 'Mülk seçiniz'),
  unitId: z.string().min(1, 'Birim seçiniz'),
  checkIn: z.string().min(1, 'Giriş tarihi seçiniz'),
  checkOut: z.string().min(1, 'Çıkış tarihi seçiniz'),
  firstName: z.string().min(2, 'Ad en az 2 karakter olmalı'),
  lastName: z.string().min(2, 'Soyad en az 2 karakter olmalı'),
  email: z.string().email('Geçerli email giriniz'),
  phone: z.string().min(10, 'Geçerli telefon giriniz'),
  adults: z.number().min(1).max(20),
  children: z.number().min(0).max(10).default(0),
  infants: z.number().min(0).max(5).default(0),
  currencyCode: z.string().default('TRY'),
  specialRequests: z.string().optional(),
  tcKimlikNo: z.string().optional(),
  passportNumber: z.string().optional(),
  nationality: z.string().default('Türkiye'),
});

type ReservationFormData = z.infer<typeof reservationSchema>;

export default function AgencyCreateReservation() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const user = useAuthStore((state) => state.user);
  const agencyId = user?.agencyId;

  // States
  const [step, setStep] = useState<'search' | 'details' | 'guest' | 'confirm'>('search');
  const [selectedProperty, setSelectedProperty] = useState<string>('');
  const [selectedUnit, setSelectedUnit] = useState<string>('');
  const [priceCalculation, setPriceCalculation] = useState<any>(null);
  const [isCalculatingPrice, setIsCalculatingPrice] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<ReservationFormData>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      propertyId: searchParams.get('propertyId') || '',
      checkIn: searchParams.get('checkIn') || '',
      adults: 2,
      children: 0,
      infants: 0,
      currencyCode: 'TRY',
      nationality: 'Türkiye',
    },
  });

  const watchCheckIn = watch('checkIn');
  const watchCheckOut = watch('checkOut');
  const watchAdults = watch('adults');
  const watchChildren = watch('children');
  const watchCurrency = watch('currencyCode');
  const watchUnitId = watch('unitId');

  // Queries
  const { data: myProperties } = useQuery({
    queryKey: ['agency', 'properties', agencyId],
    queryFn: () => agencyApi.getMyProperties(agencyId!),
    enabled: !!agencyId,
  });

  const { data: propertyDetail } = useQuery({
    queryKey: ['agency', 'property-detail', agencyId, selectedProperty],
    queryFn: () => agencyApi.getPropertyDetail(agencyId!, selectedProperty),
    enabled: !!agencyId && !!selectedProperty,
    onSuccess: (data) => {
      setSelectedUnit('');
      setPriceCalculation(null);
    },
  });

  // Calculate price when dates or unit changes
  useEffect(() => {
    if (watchUnitId && watchCheckIn && watchCheckOut && watchCheckOut > watchCheckIn) {
      calculatePrice();
    }
  }, [watchUnitId, watchCheckIn, watchCheckOut, watchAdults, watchChildren, watchCurrency]);

  const calculatePrice = async () => {
    setIsCalculatingPrice(true);
    try {
      const result = await pricingApi.calculatePrice({
        unitId: watchUnitId,
        checkIn: watchCheckIn,
        checkOut: watchCheckOut,
        adults: watchAdults,
        children: watchChildren,
        currencyCode: watchCurrency,
        agencyId: agencyId,
      });
      setPriceCalculation(result);
    } catch (error: any) {
      toast.error(error.message || 'Fiyat hesaplanamadı');
    } finally {
      setIsCalculatingPrice(false);
    }
  };

  const totalNights = watchCheckIn && watchCheckOut && watchCheckOut > watchCheckIn
    ? getNights(watchCheckIn, watchCheckOut)
    : 0;

  // Create reservation mutation
  const createMutation = useMutation({
    mutationFn: (data: any) => reservationApi.createAgencyReservation(agencyId!, data),
    onSuccess: (data) => {
      toast.success('Rezervasyon başarıyla oluşturuldu');
      navigate(`/agency/reservations/${data.id}`);
    },
    onError: (error: any) => toast.error(error.message),
  });

  const onSubmit = (data: ReservationFormData) => {
    const reservationData = {
      ...data,
      agencyId,
      totalAmount: priceCalculation?.grandTotal?.amount,
      totalNights,
      taxAmount: priceCalculation?.taxAmount?.amount,
      serviceFee: priceCalculation?.serviceFee?.amount,
    };
    createMutation.mutate(reservationData);
  };

  const canProceed = useMemo(() => {
    switch (step) {
      case 'search':
        return !!selectedProperty && !!selectedUnit && !!watchCheckIn && !!watchCheckOut && watchCheckOut > watchCheckIn;
      case 'details':
        return !!priceCalculation;
      case 'guest':
        return true;
      default:
        return false;
    }
  }, [step, selectedProperty, selectedUnit, watchCheckIn, watchCheckOut, priceCalculation]);

  const authorization = propertyDetail;
  const canCreateReservation = authorization?.canCreateReservation;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <button onClick={() => navigate(-1)} className="p-2 rounded-lg hover:bg-gray-100">
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Yeni Rezervasyon</h1>
          <p className="text-sm text-gray-500 mt-1">Acente rezervasyonu oluşturun</p>
        </div>
      </div>

      {/* Steps */}
      <div className="flex items-center gap-2">
        {['Mülk & Tarih', 'Detaylar', 'Misafir Bilgileri', 'Onay'].map((label, index) => (
          <div key={index} className="flex items-center">
            <div className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium ${
              step === ['search', 'details', 'guest', 'confirm'][index]
                ? 'bg-blue-100 text-blue-700'
                : 'bg-gray-100 text-gray-500'
            }`}>
              <span className="w-6 h-6 rounded-full bg-white flex items-center justify-center text-xs font-bold">
                {index + 1}
              </span>
              {label}
            </div>
            {index < 3 && <div className="w-8 h-px bg-gray-300 mx-1" />}
          </div>
        ))}
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        {/* Step 1: Property & Date Selection */}
        {step === 'search' && (
          <Card className="p-6 space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <Select
                label="Mülk"
                value={selectedProperty}
                onChange={(e) => {
                  setSelectedProperty(e.target.value);
                  setValue('propertyId', e.target.value);
                }}
                options={[
                  { value: '', label: 'Mülk seçin...' },
                  ...(myProperties?.map((p: any) => ({
                    value: p.propertyId,
                    label: p.propertyName,
                  })) || []),
                ]}
                disabled={!canCreateReservation}
              />

              <Select
                label="Birim"
                value={selectedUnit}
                onChange={(e) => {
                  setSelectedUnit(e.target.value);
                  setValue('unitId', e.target.value);
                }}
                options={[
                  { value: '', label: 'Birim seçin...' },
                  ...(propertyDetail?.units?.map((u: any) => ({
                    value: u.unitId,
                    label: `${u.unitName}${u.unitNumber ? ` (${u.unitNumber})` : ''} - Max: ${u.maxAdults}Y ${u.maxChildren}Ç`,
                  })) || []),
                ]}
                disabled={!selectedProperty}
              />
            </div>

            <div className="grid grid-cols-3 gap-4">
              <Input
                label="Giriş Tarihi"
                type="date"
                {...register('checkIn')}
                min={new Date().toISOString().split('T')[0]}
                error={errors.checkIn?.message}
              />
              <Input
                label="Çıkış Tarihi"
                type="date"
                {...register('checkOut')}
                min={watchCheckIn || new Date().toISOString().split('T')[0]}
                error={errors.checkOut?.message}
              />
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Gece</label>
                <div className="w-full px-3 py-2 border rounded-lg bg-gray-50 text-center font-medium">
                  {totalNights > 0 ? `${totalNights} Gece` : '-'}
                </div>
              </div>
            </div>

            <div className="flex justify-end">
              <Button
                type="button"
                onClick={() => canProceed && setStep('details')}
                disabled={!canProceed}
              >
                Devam Et
              </Button>
            </div>
          </Card>
        )}

        {/* Step 2: Details & Price */}
        {step === 'details' && (
          <Card className="p-6 space-y-6">
            <div className="grid grid-cols-3 gap-4">
              <Input
                label="Yetişkin"
                type="number"
                {...register('adults', { valueAsNumber: true })}
                min={1}
                max={propertyDetail?.units?.find((u: any) => u.unitId === selectedUnit)?.maxAdults || 20}
              />
              <Input
                label="Çocuk"
                type="number"
                {...register('children', { valueAsNumber: true })}
                min={0}
                max={propertyDetail?.units?.find((u: any) => u.unitId === selectedUnit)?.maxChildren || 10}
              />
              <Select
                label="Para Birimi"
                {...register('currencyCode')}
                options={CURRENCIES.map(c => ({ value: c.code, label: `${c.symbol} ${c.name}` }))}
              />
            </div>

            {/* Price Display */}
            {isCalculatingPrice ? (
              <div className="text-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto" />
                <p className="text-sm text-gray-500 mt-2">Fiyat hesaplanıyor...</p>
              </div>
            ) : priceCalculation ? (
              <div className="bg-gray-50 rounded-xl p-6 space-y-3">
                <h3 className="font-semibold text-lg">Fiyat Detayı</h3>
                
                <div className="space-y-2">
                  {priceCalculation.breakdown?.dailyPrices?.map((day: any, index: number) => (
                    <div key={index} className="flex justify-between text-sm">
                      <span>
                        {formatDate(day.date)}
                        {day.isWeekend && <span className="text-xs text-amber-600 ml-1">(Hafta Sonu)</span>}
                      </span>
                      <span className="font-medium">
                        {formatCurrency(day.actualPrice, watchCurrency)}
                      </span>
                    </div>
                  ))}
                </div>

                <div className="border-t pt-3 space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Oda Fiyatı</span>
                    <span>{priceCalculation.breakdown?.basePrice?.formattedPrice}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>Vergiler (%12)</span>
                    <span>{priceCalculation.taxAmount?.formattedPrice}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span>Servis Ücreti (%3)</span>
                    <span>{priceCalculation.serviceFee?.formattedPrice}</span>
                  </div>
                  {priceCalculation.breakdown?.promotionDiscount && (
                    <div className="flex justify-between text-sm text-green-600">
                      <span>İndirim</span>
                      <span>-{priceCalculation.breakdown.promotionDiscount.formattedPrice}</span>
                    </div>
                  )}
                </div>

                <div className="border-t pt-3 flex justify-between items-center">
                  <span className="font-semibold">Genel Toplam</span>
                  <span className="text-2xl font-bold text-blue-600">
                    {priceCalculation.grandTotal?.formattedPrice}
                  </span>
                </div>

                <p className="text-xs text-gray-500">
                  Ortalama: {priceCalculation.averageNightlyPrice?.formattedPrice} / gece
                </p>
              </div>
            ) : null}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Özel İstekler</label>
              <textarea
                rows={3}
                {...register('specialRequests')}
                className="w-full border rounded-lg px-3 py-2 text-sm"
                placeholder="Varsa özel istekleri belirtin..."
              />
            </div>

            <div className="flex justify-between">
              <Button type="button" variant="outline" onClick={() => setStep('search')}>
                Geri
              </Button>
              <Button type="button" onClick={() => canProceed && setStep('guest')}>
                Devam Et
              </Button>
            </div>
          </Card>
        )}

        {/* Step 3: Guest Information */}
        {step === 'guest' && (
          <Card className="p-6 space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Ad *"
                {...register('firstName')}
                error={errors.firstName?.message}
                leftIcon={<User className="w-4 h-4 text-gray-400" />}
              />
              <Input
                label="Soyad *"
                {...register('lastName')}
                error={errors.lastName?.message}
                leftIcon={<User className="w-4 h-4 text-gray-400" />}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Email *"
                type="email"
                {...register('email')}
                error={errors.email?.message}
                leftIcon={<Mail className="w-4 h-4 text-gray-400" />}
              />
              <Input
                label="Telefon *"
                {...register('phone')}
                error={errors.phone?.message}
                leftIcon={<Phone className="w-4 h-4 text-gray-400" />}
              />
            </div>

            <div className="grid grid-cols-3 gap-4">
              <Input
                label="TC Kimlik No"
                {...register('tcKimlikNo')}
                maxLength={11}
              />
              <Input
                label="Pasaport No"
                {...register('passportNumber')}
              />
              <Input
                label="Uyruk"
                {...register('nationality')}
              />
            </div>

            <div className="flex justify-between">
              <Button type="button" variant="outline" onClick={() => setStep('details')}>
                Geri
              </Button>
              <Button type="button" onClick={() => setStep('confirm')}>
                Devam Et
              </Button>
            </div>
          </Card>
        )}

        {/* Step 4: Confirmation */}
        {step === 'confirm' && (
          <Card className="p-6 space-y-6">
            <div className="text-center">
              <Check className="w-16 h-16 text-green-500 mx-auto mb-4" />
              <h2 className="text-xl font-bold">Rezervasyon Özeti</h2>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-xs text-gray-500">Mülk</label>
                <p className="font-medium">{propertyDetail?.propertyName}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Birim</label>
                <p className="font-medium">
                  {propertyDetail?.units?.find((u: any) => u.unitId === selectedUnit)?.unitName}
                </p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Tarih</label>
                <p className="font-medium">
                  {formatDate(watchCheckIn)} - {formatDate(watchCheckOut)}
                </p>
                <p className="text-sm text-gray-500">{totalNights} gece</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Misafir</label>
                <p className="font-medium">{watch('firstName')} {watch('lastName')}</p>
                <p className="text-sm text-gray-500">{watch('email')}</p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Kişi Sayısı</label>
                <p className="font-medium">
                  {watchAdults} Yetişkin{watchChildren > 0 && `, ${watchChildren} Çocuk`}
                </p>
              </div>
              <div>
                <label className="text-xs text-gray-500">Toplam Tutar</label>
                <p className="text-2xl font-bold text-blue-600">
                  {priceCalculation?.grandTotal?.formattedPrice}
                </p>
              </div>
            </div>

            <div className="flex justify-between">
              <Button type="button" variant="outline" onClick={() => setStep('guest')}>
                Geri
              </Button>
              <Button
                type="submit"
                isLoading={createMutation.isPending}
                leftIcon={<Save className="w-4 h-4" />}
              >
                Rezervasyonu Oluştur
              </Button>
            </div>
          </Card>
        )}
      </form>
    </div>
  );
}
Acente paneli sayfaları tamamlandı! Şimdi sırada Acente Layout ve Routing yapılandırması var. Devam ediyorum!

devam edelim
Harika! Şimdi tüm paneller için layout ve routing yapılandırmasını hazırlayalım.

🎨 LAYOUT VE ROUTING YAPILANDIRMASI
AgencyLayout.tsx
