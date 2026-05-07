// src/types/common.ts
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface SelectOption {
  value: string;
  label: string;
}

export type PropertyType = 
  | 'Hotel' | 'ApartHotel' | 'Bungalov' | 'Villa' 
  | 'Ev' | 'Oda' | 'Pansiyon' | 'Resort' | 'ButikOtel' | 'DagEvi';

export type ReservationStatus = 
  | 'Pending'
  | 'Confirmed'
  | 'AwaitingPayment'
  | 'CheckedIn'
  | 'CheckedOut'
  | 'Cancelled'
  | 'NoShow'
  | 'Completed';

export type CurrencyCode = 'TRY' | 'USD' | 'EUR' | 'GBP';
