import { create } from 'zustand';

interface AppState {
  sidebarOpen: boolean;
  toggleSidebar: () => void;
  selectedPropertyId: string | null;
  setSelectedProperty: (id: string | null) => void;
  selectedCurrency: string;
  setSelectedCurrency: (currency: string) => void;
  dateRange: { start: Date; end: Date } | null;
  setDateRange: (range: { start: Date; end: Date } | null) => void;
}

export const useAppStore = create<AppState>((set) => ({
  sidebarOpen: true,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  
  selectedPropertyId: null,
  setSelectedProperty: (id) => set({ selectedPropertyId: id }),
  
  selectedCurrency: 'TRY',
  setSelectedCurrency: (currency) => set({ selectedCurrency: currency }),
  
  dateRange: null,
  setDateRange: (range) => set({ dateRange: range }),
}));
2. TÜM API SERVİSLERİ
