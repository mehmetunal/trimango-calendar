// src/utils/constants.ts
export const PROPERTY_TYPES = [
  { value: 'Hotel', label: 'Otel', icon: '🏨' },
  { value: 'ApartHotel', label: 'Apart Otel', icon: '🏢' },
  { value: 'Bungalov', label: 'Bungalov', icon: '🏡' },
  { value: 'Villa', label: 'Villa', icon: '🏘️' },
  { value: 'Ev', label: 'Ev', icon: '🏠' },
  { value: 'Oda', label: 'Oda', icon: '🚪' },
  { value: 'Pansiyon', label: 'Pansiyon', icon: '🛌' },
  { value: 'Resort', label: 'Resort', icon: '🏖️' },
  { value: 'ButikOtel', label: 'Butik Otel', icon: '✨' },
  { value: 'DagEvi', label: 'Dağ Evi', icon: '🏔️' },
] as const;

export const RESERVATION_STATUSES = {
  Pending: { label: 'Beklemede', color: 'yellow', bgColor: 'bg-yellow-100', textColor: 'text-yellow-800' },
  Confirmed: { label: 'Onaylandı', color: 'green', bgColor: 'bg-green-100', textColor: 'text-green-800' },
  CheckedIn: { label: 'Giriş Yapıldı', color: 'blue', bgColor: 'bg-blue-100', textColor: 'text-blue-800' },
  CheckedOut: { label: 'Çıkış Yapıldı', color: 'gray', bgColor: 'bg-gray-100', textColor: 'text-gray-800' },
  Cancelled: { label: 'İptal Edildi', color: 'red', bgColor: 'bg-red-100', textColor: 'text-red-800' },
  NoShow: { label: 'Gelmedi', color: 'orange', bgColor: 'bg-orange-100', textColor: 'text-orange-800' },
} as const;

export const CURRENCIES = [
  { code: 'TRY', symbol: '₺', name: 'Türk Lirası', locale: 'tr-TR' },
  { code: 'USD', symbol: '$', name: 'Amerikan Doları', locale: 'en-US' },
  { code: 'EUR', symbol: '€', name: 'Euro', locale: 'de-DE' },
  { code: 'GBP', symbol: '£', name: 'İngiliz Sterlini', locale: 'en-GB' },
] as const;

export const AMENITIES = [
  { key: 'wifi', label: 'WiFi', icon: '📶' },
  { key: 'pool', label: 'Havuz', icon: '🏊' },
  { key: 'spa', label: 'SPA', icon: '💆' },
  { key: 'parking', label: 'Otopark', icon: '🅿️' },
  { key: 'restaurant', label: 'Restoran', icon: '🍽️' },
  { key: 'bar', label: 'Bar', icon: '🍸' },
  { key: 'ac', label: 'Klima', icon: '❄️' },
  { key: 'heating', label: 'Isıtma', icon: '🔥' },
  { key: 'tv', label: 'TV', icon: '📺' },
  { key: 'minibar', label: 'Mini Bar', icon: '🍾' },
  { key: 'safe', label: 'Kasa', icon: '🔒' },
  { key: 'balcony', label: 'Balkon', icon: '🌅' },
  { key: 'garden', label: 'Bahçe', icon: '🌳' },
  { key: 'bbq', label: 'Barbekü', icon: '🍖' },
  { key: 'kitchen', label: 'Mutfak', icon: '🍳' },
  { key: 'washer', label: 'Çamaşır Makinesi', icon: '👕' },
  { key: 'dishwasher', label: 'Bulaşık Makinesi', icon: '🍽️' },
  { key: 'jacuzzi', label: 'Jakuzi', icon: '🛁' },
  { key: 'sauna', label: 'Sauna', icon: '🧖' },
  { key: 'fitness', label: 'Fitness', icon: '💪' },
  { key: 'meeting', label: 'Toplantı Odası', icon: '👥' },
  { key: 'reception', label: '7/24 Resepsiyon', icon: '🕐' },
  { key: 'roomService', label: 'Oda Servisi', icon: '🛎️' },
  { key: 'transfer', label: 'Havaalanı Transfer', icon: '✈️' },
  { key: 'accessible', label: 'Engelli Dostu', icon: '♿' },
  { key: 'petFriendly', label: 'Evcil Hayvan Kabul', icon: '🐾' },
] as const;

export const PAGE_SIZES = [10, 20, 50, 100] as const;