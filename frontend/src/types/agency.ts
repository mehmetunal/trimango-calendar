export interface AgencyAuthorization {
  id: string;
  agencyId: string;
  agencyName: string;
  propertyId: string;
  propertyName: string;
  level: 'ViewOnly' | 'PriceAndAvailability' | 'CanReserve' | 'FullAccess';
  canViewPrices: boolean;
  canSetPrices: boolean;
  canCreateReservation: boolean;
  canModifyReservation: boolean;
  canCancelReservation: boolean;
  priceDisplay: 'Net' | 'Commission' | 'Markup';
  customCommissionRate: number | null;
  defaultMarkupRate: number | null;
  hasAllotment: boolean;
  totalAllotment: number | null;
  usedAllotment: number;
  isActive: boolean;
  grantedAt: string;
}
