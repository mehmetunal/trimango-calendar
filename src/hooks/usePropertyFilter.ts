import { useState, useCallback } from 'react';
import { useDebounce } from './useDebounce';

export function usePropertyFilter() {
  const [filters, setFilters] = useState({
    search: '',
    type: '',
    city: '',
    minPrice: '',
    maxPrice: '',
    amenities: [] as string[],
  });

  const debouncedSearch = useDebounce(filters.search);

  const setFilter = useCallback((key: string, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  }, []);

  const toggleAmenity = useCallback((amenity: string) => {
    setFilters((prev) => ({
      ...prev,
      amenities: prev.amenities.includes(amenity)
        ? prev.amenities.filter((a) => a !== amenity)
        : [...prev.amenities, amenity],
    }));
  }, []);

  const clearFilters = useCallback(() => {
    setFilters({
      search: '',
      type: '',
      city: '',
      minPrice: '',
      maxPrice: '',
      amenities: [],
    });
  }, []);

  return {
    filters,
    debouncedSearch,
    setFilter,
    toggleAmenity,
    clearFilters,
    hasActiveFilters:
      filters.search !== '' ||
      filters.type !== '' ||
      filters.city !== '' ||
      filters.amenities.length > 0,
  };
}
4. UTILITY FONKSİYONLARI
