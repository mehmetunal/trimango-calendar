// src/components/layout/TenantLayout.tsx
import { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Building2,
  BedDouble,
  Calendar,
  ClipboardList,
  DollarSign,
  Users,
  Lock,
  BarChart3,
  Code,
  Settings,
  Bell,
  ChevronDown,
  Menu,
  LogOut,
  HelpCircle,
  ChevronLeft,
  Star,
  Globe,
} from 'lucide-react';
import { useAuthStore } from '../../stores/authStore';
import { useAppStore } from '../../stores/appStore';
import { clsx } from 'clsx';

type MenuItem = { path: string; icon: any; label: string; exact?: boolean };
type MenuGroup = { title: string; items: MenuItem[] };

const menuGroups: MenuGroup[] = [
  {
    title: 'GENEL',
    items: [
      { path: '/dashboard', icon: LayoutDashboard, label: 'Dashboard', exact: true },
    ],
  },
  {
    title: 'MÜLK YÖNETİMİ',
    items: [
      { path: '/dashboard/properties', icon: Building2, label: 'Mülklerim' },
      { path: '/dashboard/units', icon: BedDouble, label: 'Birimler' },
    ],
  },
  {
    title: 'REZERVASYON',
    items: [
      { path: '/dashboard/reservations', icon: ClipboardList, label: 'Rezervasyonlar' },
      { path: '/dashboard/calendar', icon: Calendar, label: 'Takvim & Blokaj' },
    ],
  },
  {
    title: 'FİYATLANDIRMA',
    items: [
      { path: '/dashboard/pricing', icon: DollarSign, label: 'Fiyatlar & Sezon' },
      { path: '/dashboard/currencies', icon: Globe, label: 'Para Birimleri' },
    ],
  },
  {
    title: 'ACENTE & KANAL',
    items: [
      { path: '/dashboard/agencies', icon: Users, label: 'Acente Yönetimi' },
      { path: '/dashboard/authorizations', icon: Lock, label: 'Yetkilendirmeler' },
    ],
  },
  {
    title: 'PAZARLAMA',
    items: [
      { path: '/dashboard/widgets', icon: Code, label: 'Booking Widget' },
      { path: '/dashboard/reviews', icon: Star, label: 'Değerlendirmeler' },
    ],
  },
  {
    title: 'RAPORLAR',
    items: [
      { path: '/dashboard/reports', icon: BarChart3, label: 'Raporlar' },
    ],
  },
];

export default function TenantLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [collapsedGroups, setCollapsedGroups] = useState<string[]>([]);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuthStore();
  const { selectedPropertyId } = useAppStore();

  const isActive = (path: string, exact?: boolean) => {
    if (exact) return location.pathname === path;
    return location.pathname.startsWith(path);
  };

  const toggleGroup = (title: string) => {
    setCollapsedGroups(prev =>
      prev.includes(title) ? prev.filter(t => t !== title) : [...prev, title]
    );
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className={clsx(
        'bg-white border-r border-gray-200 transition-all duration-300 flex flex-col z-30',
        sidebarOpen ? 'w-64' : 'w-20'
      )}>
        {/* Logo */}
        <div className="flex items-center justify-between h-16 px-4 border-b">
          {sidebarOpen ? (
            <div className="flex items-center gap-2">
              <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
                <Building2 className="w-5 h-5 text-white" />
              </div>
              <span className="text-lg font-bold text-gray-900">Yönetim Paneli</span>
            </div>
          ) : (
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center mx-auto">
              <Building2 className="w-5 h-5 text-white" />
            </div>
          )}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400"
          >
            <Menu className="w-5 h-5" />
          </button>
        </div>

        {/* Property Selector */}
        {sidebarOpen && (
          <div className="p-3 border-b">
            <select className="w-full text-sm border rounded-lg px-3 py-2 bg-gray-50">
              <option value="">Tüm Mülkler</option>
              <option value="1">Sahil Palace Hotel</option>
              <option value="2">Ege Bahçe Bungalov</option>
            </select>
          </div>
        )}

        {/* Navigation */}
        <nav className="flex-1 overflow-y-auto p-3 space-y-4">
          {menuGroups.map((group) => {
            const isCollapsed = collapsedGroups.includes(group.title);
            return (
              <div key={group.title}>
                {sidebarOpen && (
                  <button
                    onClick={() => toggleGroup(group.title)}
                    className="flex items-center justify-between w-full px-3 py-1 text-xs font-semibold text-gray-400 uppercase tracking-wider hover:text-gray-600"
                  >
                    <span>{group.title}</span>
                    <ChevronLeft className={clsx(
                      'w-3 h-3 transition-transform',
                      isCollapsed ? '-rotate-90' : ''
                    )} />
                  </button>
                )}
                {!isCollapsed && (
                  <div className="space-y-1 mt-1">
                    {group.items.map((item) => {
                      const active = isActive(item.path, item.exact);
                      return (
                        <button
                          key={item.path}
                          onClick={() => navigate(item.path)}
                          className={clsx(
                            'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors',
                            active
                              ? 'bg-blue-50 text-blue-600'
                              : 'text-gray-600 hover:bg-gray-100'
                          )}
                          title={!sidebarOpen ? item.label : undefined}
                        >
                          <item.icon className="w-5 h-5 flex-shrink-0" />
                          {sidebarOpen && <span className="text-sm font-medium">{item.label}</span>}
                          {active && sidebarOpen && (
                            <div className="ml-auto w-1.5 h-1.5 rounded-full bg-blue-600" />
                          )}
                        </button>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </nav>

        {/* User Info */}
        <div className="p-3 border-t">
          <button
            onClick={() => setUserMenuOpen(!userMenuOpen)}
            className={clsx(
              'w-full flex items-center gap-3 p-2 rounded-lg hover:bg-gray-100 transition-colors',
              sidebarOpen ? 'justify-start' : 'justify-center'
            )}
          >
            <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0">
              <span className="text-sm font-medium text-blue-700">
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </span>
            </div>
            {sidebarOpen && (
              <div className="flex-1 text-left min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate">
                  {user?.firstName} {user?.lastName}
                </p>
                <p className="text-xs text-gray-500 truncate">Mülk Sahibi</p>
              </div>
            )}
            {sidebarOpen && <ChevronDown className="w-4 h-4 text-gray-400 flex-shrink-0" />}
          </button>

          {userMenuOpen && (
            <div className="mt-2 p-2 bg-white rounded-lg shadow-lg border">
              <button
                onClick={() => { navigate('/dashboard/settings'); setUserMenuOpen(false); }}
                className="w-full flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg"
              >
                <Settings className="w-4 h-4" />
                Ayarlar
              </button>
              <button
                onClick={handleLogout}
                className="w-full flex items-center gap-2 px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg"
              >
                <LogOut className="w-4 h-4" />
                Çıkış Yap
              </button>
            </div>
          )}
        </div>
      </aside>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="h-16 bg-white border-b flex items-center justify-between px-6 flex-shrink-0">
          <div className="flex items-center gap-4">
            {/* Mobile menu toggle */}
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="p-2 rounded-lg hover:bg-gray-100 text-gray-400 lg:hidden"
            >
              <Menu className="w-5 h-5" />
            </button>
            <h2 className="text-lg font-semibold text-gray-800">
              {menuGroups.flatMap(g => g.items).find(item => isActive(item.path, item.exact))?.label || 'Panel'}
            </h2>
          </div>

          <div className="flex items-center gap-3">
            <button className="relative p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100">
              <Bell className="w-5 h-5" />
              <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-red-500 rounded-full" />
            </button>
            <button className="relative p-2 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100">
              <HelpCircle className="w-5 h-5" />
            </button>
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
