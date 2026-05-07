export interface Property {
  id: string;
  tenantId: string;
  type: PropertyType;
  name: string;
  slug: string;
  description: string;
  shortDescription: string;
  email: string;
  phone: string;
  address: string;
  district: string;
  city: string;
  country: string;
  postalCode: string;
  latitude: number | null;
  longitude: number | null;
  checkInTime: string;
  checkOutTime: string;
  coverImageUrl: string;
  amenities: string[];
  averageRating: number;
  reviewCount: number;
  totalUnitCount: number;
  startingPrice: number;
  currencyCode: string;
  isActive: boolean;
  createdAt: string;
}

export interface Unit {
  id: string;
  propertyId: string;
  name: string;
  unitNumber: string;
  floor: number;
  maxAdults: number;
  maxChildren: number;
  maxInfants: number;
  basePrice: number;
  currencyCode: string;
  size: number | null;
  view: string;
  roomAmenities: string[];
  isActive: boolean;
}

