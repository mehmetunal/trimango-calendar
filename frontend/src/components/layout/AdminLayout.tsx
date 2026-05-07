// src/components/layout/AdminLayout.tsx
import { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { clsx } from 'clsx';
import { useAuthStore } from '../../stores/authStore';
import {
  LayoutDashboard,
  Building2,
  Users,
  CreditCard,
  BarChart3,
  Settings,
  LogOut,
  Menu,
  Bell,
  ChevronDown,
} from 'lucide-react';

const menuItems = [
  { path: '/admin', icon: LayoutDashboard, label: 'Dashboard' },
  { path: '/admin/tenants', icon: Building2, label: 'Bayiler (Tenant)' },
  { path: '/admin/agencies', icon: Users, label: 'Acenteler' },
  { path: '/admin/subscriptions', icon: CreditCard, label: 'Abonelikler' },
  { path: '/admin/reports', icon: BarChart3, label: 'Raporlar' },
  { path: '/admin/settings', icon: Settings, label: 'Ayarlar' },
];

export default function AdminLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [notificationsOpen, setNotificationsOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuthStore();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside
        className={clsx(
          'bg-white border-r border-gray-200 transition-all duration-300',
          sidebarOpen ? 'w-64' : 'w-20'
        )}
      >
        <div className="flex items-center justify-between h-16 px-4 border-b">
          {sidebarOpen ? (
            <h1 className="text-xl font-bold text-blue-600">HotelPlatform</h1>
          ) : (
            <h1 className="text-xl font-bold text-blue-600">HP</h1>
          )}
          <button onClick={() => setSidebarOpen(!sidebarOpen)}>
            <Menu className="w-5 h-5" />
          </button>
        </div>

        <nav className="p-2 space-y-1">
          {menuItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <button
                key={item.path}
                onClick={() => navigate(item.path)}
                className={clsx(
                  'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors',
                  isActive
                    ? 'bg-blue-50 text-blue-600'
                    : 'text-gray-600 hover:bg-gray-100'
                )}
              >
                <item.icon className="w-5 h-5 flex-shrink-0" />
                {sidebarOpen && <span className="text-sm font-medium">{item.label}</span>}
              </button>
            );
          })}
        </nav>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="h-16 bg-white border-b flex items-center justify-between px-6">
          <h2 className="text-lg font-semibold text-gray-800">
            Admin Panel
          </h2>
          
          <div className="flex items-center gap-4">
            <div className="relative">
              <button
                onClick={() => setNotificationsOpen((s) => !s)}
                className="relative p-2 text-gray-400 hover:text-gray-600"
              >
                <Bell className="w-5 h-5" />
                <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full" />
              </button>
              {notificationsOpen && (
                <div className="absolute right-0 mt-2 w-72 bg-white border rounded-xl shadow-lg z-50">
                  <div className="p-3 border-b text-sm font-semibold">Bildirimler</div>
                  <div className="p-3 text-sm text-gray-600">Yeni kayıt ve sistem bildirimleri burada görünecek.</div>
                </div>
              )}
            </div>
            
            <div className="relative">
              <button
                onClick={() => setUserMenuOpen((s) => !s)}
                className="flex items-center gap-2 cursor-pointer"
              >
                <div className="w-8 h-8 bg-blue-600 rounded-full flex items-center justify-center text-white text-sm font-medium">
                  {user?.firstName?.[0] || 'A'}
                </div>
                <span className="text-sm font-medium">{user?.firstName || 'Admin'}</span>
                <ChevronDown className="w-4 h-4" />
              </button>
              {userMenuOpen && (
                <div className="absolute right-0 mt-2 w-48 bg-white border rounded-xl shadow-lg z-50">
                  <button
                    onClick={() => {
                      navigate('/admin/settings');
                      setUserMenuOpen(false);
                    }}
                    className="w-full text-left px-4 py-2 text-sm hover:bg-gray-50"
                  >
                    Ayarlar
                  </button>
                  <button
                    onClick={handleLogout}
                    className="w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                  >
                    Çıkış Yap
                  </button>
                </div>
              )}
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
