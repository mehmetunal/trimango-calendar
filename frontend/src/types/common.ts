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
  | 'Pending' | 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled' | 'NoShow';

export type CurrencyCode = 'TRY' | 'USD' | 'EUR' | 'GBP';

