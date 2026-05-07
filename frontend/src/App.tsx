import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import Login from './pages/auth/Login';
import { useAuthStore } from './stores/authStore';
import AdminLayout from './components/layout/AdminLayout';
import AgencyLayout from './components/layout/AgencyLayout';
import TenantLayout from './components/layout/TenantLayout';
import RegisterPage from './pages/auth/Register';
import AgencyDashboard from './pages/agency/Dashboard';
import AgencyMyProperties from './pages/agency/MyProperties';
import AgencyCalendar from './pages/agency/Calendar';
import AgencyCreateReservation from './pages/agency/reservations/CreateReservation';
import TenantDashboard from './pages/tenant/Dashboard';
import PropertyList from './pages/tenant/Properties/PropertyList';
import UnitManagement from './pages/tenant/Units/UnitManagement';
import TenantReservationList from './pages/tenant/Reservations/ReservationList';
import ReservationCalendar from './pages/tenant/Reservations/ReservationCalendar';
import SeasonRates from './pages/tenant/Pricing/SeasonRates';
import CurrencyManagement from './pages/tenant/Pricing/CurrencyManagement';
import Authorizations from './pages/tenant/Agencies/Authorizations';
import WidgetSettings from './pages/tenant/Widgets/WidgetSettings';
import ReviewsPage from './pages/tenant/Reviews/ReviewsPage';
import ReportsPage from './pages/tenant/Reports/ReportsPage';
import SettingsPage from './pages/tenant/Settings/SettingsPage';
import AgencyReports from './pages/agency/Reports';
import AgencySettings from './pages/agency/Settings';
import AgencyReservationList from './pages/Reservations/ReservationList';
import AdminDashboardPage from './pages/admin/Dashboard';
import TenantManagement from './pages/admin/Tenants/TenantList';
import AgencyManagement from './pages/admin/Agencies/AgencyList';
import SubscriptionList from './pages/admin/Subscriptions/SubscriptionList';
import AdminReports from './pages/admin/Reports/AdminReports';
import AdminSettings from './pages/admin/Settings/AdminSettings';

const queryClient = new QueryClient();

function AppRoutes() {
  const { isAuthenticated, user } = useAuthStore();

  return (
    <Routes>
      <Route
        path="/login"
        element={
          isAuthenticated
            ? <Navigate to={user?.role === 'Admin' ? '/admin' : user?.role === 'AgencyUser' ? '/agency' : '/dashboard'} replace />
            : <Login />
        }
      />
      <Route path="/register" element={<RegisterPage />} />
      <Route
        path="/admin/*"
        element={isAuthenticated ? <AdminLayout /> : <Navigate to="/login" replace />}
      >
        <Route index element={<AdminDashboardPage />} />
        <Route path="tenants" element={<TenantManagement />} />
        <Route path="agencies" element={<AgencyManagement />} />
        <Route path="subscriptions" element={<SubscriptionList />} />
        <Route path="reports" element={<AdminReports />} />
        <Route path="settings" element={<AdminSettings />} />
      </Route>
      <Route
        path="/agency/*"
        element={isAuthenticated ? <AgencyLayout /> : <Navigate to="/login" replace />}
      >
        <Route index element={<AgencyDashboard />} />
        <Route path="properties" element={<AgencyMyProperties />} />
        <Route path="calendar" element={<AgencyCalendar />} />
        <Route path="calendar/:propertyId" element={<AgencyCalendar />} />
        <Route path="reservations" element={<AgencyReservationList />} />
        <Route path="reservations/new" element={<AgencyCreateReservation />} />
        <Route path="reports" element={<AgencyReports />} />
        <Route path="settings" element={<AgencySettings />} />
      </Route>
      <Route
        path="/dashboard/*"
        element={isAuthenticated ? <TenantLayout /> : <Navigate to="/login" replace />}
      >
        <Route index element={<TenantDashboard />} />
        <Route path="properties" element={<PropertyList />} />
        <Route path="units" element={<UnitManagement />} />
        <Route path="reservations" element={<TenantReservationList />} />
        <Route path="calendar" element={<ReservationCalendar />} />
        <Route path="pricing" element={<SeasonRates />} />
        <Route path="currencies" element={<CurrencyManagement />} />
        <Route path="agencies" element={<Authorizations />} />
        <Route path="authorizations" element={<Authorizations />} />
        <Route path="widgets" element={<WidgetSettings />} />
        <Route path="reviews" element={<ReviewsPage />} />
        <Route path="reports" element={<ReportsPage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>
      <Route
        path="/"
        element={
          isAuthenticated
            ? <Navigate to={user?.role === 'Admin' ? '/admin' : user?.role === 'AgencyUser' ? '/agency' : '/dashboard'} replace />
            : <Navigate to="/login" replace />
        }
      />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
      <Toaster position="top-right" />
    </QueryClientProvider>
  );
}
