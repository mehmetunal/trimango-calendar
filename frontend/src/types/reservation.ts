import { ReservationStatus } from "./common";

export interface Reservation {
  id: string;
  reservationNumber: string;
  unitId: string;
  unitName: string;
  propertyName: string;
  propertyType: string;
  guestId: string;
  guestName: string;
  guestEmail: string;
  guestPhone: string;
  checkIn: string;
  checkOut: string;
  totalNights: number;
  actualCheckIn?: string | null;
  actualCheckOut?: string | null;
  adults: number;
  children: number;
  status: ReservationStatus;
  statusDescription?: string;
  statusColor?: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  currencyCode: string;
  formattedTotal: string;
  formattedRemaining?: string;
  source: string;
  externalReference?: string;
  specialRequests: string;
  isCancelled: boolean;
  cancelledAt?: string | null;
  createdBy?: string;
  createdAt: string;
}

