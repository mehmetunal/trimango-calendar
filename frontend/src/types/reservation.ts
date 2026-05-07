export interface Reservation {
  id: string;
  reservationNumber: string;
  unitId: string;
  unitName: string;
  propertyName: string;
  guestId: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  checkIn: string;
  checkOut: string;
  totalNights: number;
  adults: number;
  children: number;
  status: ReservationStatus;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  currencyCode: string;
  formattedTotal: string;
  source: string;
  specialRequests: string;
  isCancelled: boolean;
  createdAt: string;
}

