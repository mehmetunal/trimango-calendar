import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { useAuthStore } from './stores/authStore';

// Layouts
import AdminLayout from './components/layout/AdminLayout';
import TenantLayout from './components/layout/TenantLayout';
import AgencyLayout from './components/layout/AgencyLayout';

// Auth Pages
import LoginPage from './pages/auth/Login';
import RegisterPage from './pages/auth/Register';
import ForgotPassword from './pages/auth/ForgotPassword';

// Admin Pages
import AdminDashboard from './pages/admin/Dashboard';
import TenantManagement from './pages/admin/Tenants/TenantList';
import AgencyManagement from './pages/admin/Agencies/AgencyList';

// Tenant Pages
import TenantDashboard from './pages/tenant/Dashboard';
import PropertyList from './pages/tenant/Properties/PropertyList';
import PropertyForm from './pages/tenant/Properties/PropertyForm';
import PropertyDetail from './pages/tenant/Properties/PropertyDetail';
import UnitManagement from './pages/tenant/Units/UnitManagement';
import ReservationList from './pages/tenant/Reservations/ReservationList';
import ReservationDetail from './pages/tenant/Reservations/ReservationDetail';
import ReservationCalendar from './pages/tenant/Reservations/ReservationCalendar';
import CalendarManagement from './pages/tenant/Calendar/CalendarManagement';
import SeasonRates from './pages/tenant/Pricing/SeasonRates';
import CurrencyManagement from './pages/tenant/Pricing/CurrencyManagement';
import Authorizations from './pages/tenant/Agencies/Authorizations';
import ReportsPage from './pages/tenant/Reports/ReportsPage';
import WidgetSettings from './pages/tenant/Widgets/WidgetSettings';
import ReviewsPage from './pages/tenant/Reviews/ReviewsPage';
import SettingsPage from './pages/tenant/Settings/SettingsPage';

// Agency Pages
import AgencyDashboard from './pages/agency/Dashboard';
import AgencyMyProperties from './pages/agency/MyProperties';
import AgencyPropertyDetail from './pages/agency/PropertyDetail';
import AgencyCalendar from './pages/agency/Calendar';
import AgencyReservationList from './pages/agency/Reservations/ReservationList';
import AgencyCreateReservation from './pages/agency/Reservations/CreateReservation';
import AgencyReports from './pages/agency/Reports';
import AgencySettings from './pages/agency/Settings';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

// Protected Route wrapper
function ProtectedRoute({ children, allowedRoles }: { children: React.ReactNode; allowedRoles: string[] }) {
  const { user, isAuthenticated } = useAuthStore();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  if (user && !allowedRoles.includes(user.role)) {
    // Role göre yönlendir
    if (user.role === 'Admin') return <Navigate to="/admin" replace />;
    if (user.role === 'TenantOwner') return <Navigate to="/dashboard" replace />;
    if (user.role === 'AgencyUser') return <Navigate to="/agency" replace />;
    return <Navigate to="/login" replace />;
  }
  
  return <>{children}</>;
}

// Public Route wrapper
function PublicRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, user } = useAuthStore();
  
  if (isAuthenticated && user) {
    if (user.role === 'Admin') return <Navigate to="/admin" replace />;
    if (user.role === 'TenantOwner') return <Navigate to="/dashboard" replace />;
    if (user.role === 'AgencyUser') return <Navigate to="/agency" replace />;
  }
  
  return <>{children}</>;
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: {
              borderRadius: '12px',
              padding: '12px 16px',
              fontSize: '14px',
            },
          }}
        />
        
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
          <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />
          <Route path="/forgot-password" element={<PublicRoute><ForgotPassword /></PublicRoute>} />
          
          {/* Admin Routes */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <AdminLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<AdminDashboard />} />
            <Route path="tenants" element={<TenantManagement />} />
            <Route path="tenants/:id" element={<TenantManagement />} />
            <Route path="agencies" element={<AgencyManagement />} />
            <Route path="agencies/:id" element={<AgencyManagement />} />
            <Route path="subscriptions" element={<div>Abonelikler</div>} />
            <Route path="reports" element={<div>Raporlar</div>} />
            <Route path="activity" element={<div>Aktivite Log</div>} />
            <Route path="settings" element={<div>Sistem Ayarları</div>} />
          </Route>
          
          {/* Tenant Routes */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute allowedRoles={['TenantOwner']}>
                <TenantLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<TenantDashboard />} />
            <Route path="properties" element={<PropertyList />} />
            <Route path="properties/new" element={<PropertyForm />} />
            <Route path="properties/:id" element={<PropertyDetail />} />
            <Route path="properties/:id/edit" element={<PropertyForm />} />
            <Route path="properties/:propertyId/units" element={<UnitManagement />} />
            <Route path="reservations" element={<ReservationList />} />
            <Route path="reservations/new" element={<ReservationList />} />
            <Route path="reservations/:id" element={<ReservationDetail />} />
            <Route path="reservations/calendar" element={<ReservationCalendar />} />
            <Route path="calendar" element={<CalendarManagement />} />
            <Route path="pricing" element={<SeasonRates />} />
            <Route path="currencies" element={<CurrencyManagement />} />
            <Route path="agencies" element={<Authorizations />} />
            <Route path="authorizations" element={<Authorizations />} />
            <Route path="widgets" element={<WidgetSettings />} />
            <Route path="reviews" element={<ReviewsPage />} />
            <Route path="reports" element={<ReportsPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
          
          {/* Agency Routes */}
          <Route
            path="/agency"
            element={
              <ProtectedRoute allowedRoles={['AgencyUser']}>
                <AgencyLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<AgencyDashboard />} />
            <Route path="properties" element={<AgencyMyProperties />} />
            <Route path="properties/:propertyId" element={<AgencyPropertyDetail />} />
            <Route path="calendar" element={<AgencyCalendar />} />
            <Route path="calendar/:propertyId" element={<AgencyCalendar />} />
            <Route path="reservations" element={<AgencyReservationList />} />
            <Route path="reservations/new" element={<AgencyCreateReservation />} />
            <Route path="reservations/:id" element={<AgencyReservationList />} />
            <Route path="reports" element={<AgencyReports />} />
            <Route path="settings" element={<AgencySettings />} />
          </Route>
          
          {/* Default redirect */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
Ana Giriş Dosyası
