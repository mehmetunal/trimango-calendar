import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select } from '../../../components/ui';
import toast from 'react-hot-toast';

const propertySchema = z.object({
  type: z.enum(['Hotel', 'ApartHotel', 'Bungalov', 'Villa', 'Ev', 'Oda', 'Pansiyon', 'Resort', 'ButikOtel', 'DagEvi']),
  name: z.string().min(3, 'Mülk adı en az 3 karakter olmalı').max(300),
  description: z.string().optional(),
  email: z.string().email('Geçerli bir email giriniz').optional().or(z.literal('')),
  phone: z.string().optional(),
  address: z.string().min(5, 'Adres zorunludur'),
  district: z.string().optional(),
  city: z.string().min(2, 'Şehir zorunludur'),
  country: z.string().default('Türkiye'),
  checkInTime: z.string().default('14:00'),
  checkOutTime: z.string().default('12:00'),
  amenities: z.array(z.string()).default([]),
});

type PropertyFormData = z.infer<typeof propertySchema>;

const propertyTypes = [
  { value: 'Hotel', label: 'Otel' },
  { value: 'ApartHotel', label: 'Apart Otel' },
  { value: 'Bungalov', label: 'Bungalov' },
  { value: 'Villa', label: 'Villa' },
  { value: 'Ev', label: 'Ev' },
  { value: 'Oda', label: 'Oda' },
  { value: 'Pansiyon', label: 'Pansiyon' },
  { value: 'Resort', label: 'Resort' },
  { value: 'ButikOtel', label: 'Butik Otel' },
  { value: 'DagEvi', label: 'Dağ Evi' },
];

const amenityOptions = [
  'WiFi', 'Havuz', 'SPA', 'Otopark', 'Restoran', 'Bar',
  'Klima', 'Isıtma', 'TV', 'Mini Bar', 'Kasa', 'Balkon',
  'Bahçe', 'Barbekü', 'Mutfak', 'Çamaşır Makinesi',
  'Bulaşık Makinesi', 'Jakuzi', 'Sauna', 'Fitness',
  'Toplantı Odası', '7/24 Resepsiyon', 'Oda Servisi',
  'Havaalanı Transfer', 'Engelli Dostu', 'Evcil Hayvan Kabul',
];

export default function PropertyForm() {
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    watch,
    setValue,
  } = useForm<PropertyFormData>({
    resolver: zodResolver(propertySchema),
    defaultValues: {
      country: 'Türkiye',
      checkInTime: '14:00',
      checkOutTime: '12:00',
      amenities: [],
    },
  });

  const selectedAmenities = watch('amenities') || [];

  const toggleAmenity = (amenity: string) => {
    const current = [...selectedAmenities];
    const index = current.indexOf(amenity);
    if (index > -1) {
      current.splice(index, 1);
    } else {
      current.push(amenity);
    }
    setValue('amenities', current, { shouldValidate: true });
  };

  const onSubmit = async (data: PropertyFormData) => {
    try {
      await propertyApi.create(data);
      toast.success('Mülk başarıyla oluşturuldu');
      navigate('/dashboard/properties');
    } catch (error: any) {
      toast.error(error.message || 'Bir hata oluştu');
    }
  };

  return (
    <div className="max-w-3xl mx-auto">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Yeni Mülk Ekle</h1>
        <p className="text-sm text-gray-500 mt-1">Mülk bilgilerini doldurun</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Temel Bilgiler */}
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">Temel Bilgiler</h2>
          </CardHeader>
          <CardContent className="space-y-4">
            <Select
              label="Mülk Tipi"
              options={propertyTypes}
              {...register('type')}
              error={errors.type?.message}
            />

            <Input
              label="Mülk Adı"
              placeholder="Örn: Sahil Palace Hotel"
              {...register('name')}
              error={errors.name?.message}
            />

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Email"
                type="email"
                placeholder="info@hotel.com"
                {...register('email')}
                error={errors.email?.message}
              />
              <Input
                label="Telefon"
                placeholder="+90 212 555 0000"
                {...register('phone')}
                error={errors.phone?.message}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Açıklama</label>
              <textarea
                rows={4}
                placeholder="Mülk hakkında detaylı açıklama..."
                className="w-full border rounded-lg px-3 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                {...register('description')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Adres Bilgileri */}
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">Adres Bilgileri</h2>
          </CardHeader>
          <CardContent className="space-y-4">
            <Input
              label="Adres"
              placeholder="Açık adres"
              {...register('address')}
              error={errors.address?.message}
            />

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="İlçe"
                placeholder="İlçe"
                {...register('district')}
              />
              <Input
                label="Şehir"
                placeholder="Şehir"
                {...register('city')}
                error={errors.city?.message}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Ülke"
                {...register('country')}
              />
              <Input
                label="Posta Kodu"
                {...register('postalCode')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Konaklama Politikaları */}
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">Konaklama Politikaları</h2>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Check-in Saati"
                type="time"
                {...register('checkInTime')}
              />
              <Input
                label="Check-out Saati"
                type="time"
                {...register('checkOutTime')}
              />
            </div>
          </CardContent>
        </Card>

        {/* Özellikler */}
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">Özellikler ve İmkanlar</h2>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
              {amenityOptions.map((amenity) => (
                <label
                  key={amenity}
                  className={clsx(
                    'flex items-center gap-2 p-3 rounded-lg border cursor-pointer transition-all',
                    selectedAmenities.includes(amenity)
                      ? 'border-blue-500 bg-blue-50 text-blue-700'
                      : 'border-gray-200 hover:border-gray-300'
                  )}
                >
                  <input
                    type="checkbox"
                    checked={selectedAmenities.includes(amenity)}
                    onChange={() => toggleAmenity(amenity)}
                    className="sr-only"
                  />
                  <span className="text-sm">{amenity}</span>
                </label>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
          >
            İptal
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            Mülk Oluştur
          </Button>
        </div>
      </form>
    </div>
  );
}
Bu frontend yapısı size tam kapsamlı bir başlangıç sağlar. Özet olarak:

✅ Modern teknolojiler: React 18, TypeScript, Tailwind CSS, React Query
✅ 3 farklı panel: Admin, Tenant (mülk sahibi), Agency (acente)
✅ Zengin UI bileşenleri: Button, Modal, Table, Form elemanları
✅ Responsive tasarım: Mobil uyumlu
✅ Grafikler ve takvim: Recharts, FullCalendar entegrasyonu
✅ Form validasyonu: React Hook Form + Zod
✅ State yönetimi: Zustand (global), React Query (server state)
✅ Tip güvenliği: Tam TypeScript desteği

Projenin tamamını bu yapı üzerine inşa edebilirsiniz. Herhangi bir sayfanın veya bileşenin daha detaylı kodunu ister misiniz?

evet isterim tam anlamı ile işlevli olsun
Harika! Size eksiksiz, production-ready kodlar hazırlayacağım. Tüm sayfaları, bileşenleri, hook'ları ve yardımcı fonksiyonları detaylıca yazalım.

📚 TÜM PROJE DOSYALARI
1. STORE (ZUSTAND) YAPILARI
