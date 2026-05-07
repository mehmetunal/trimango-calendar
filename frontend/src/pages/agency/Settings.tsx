// src/pages/agency/Settings.tsx
import { useState } from 'react';
import {
  User,
  Building2,
  Mail,
  Phone,
  Globe,
  Lock,
  Bell,
  Save,
  Eye,
  EyeOff,
} from 'lucide-react';
import { Button, Input, Select, Card, Tabs } from '../../components/ui';
import { useAuthStore } from '../../stores/authStore';
import toast from 'react-hot-toast';

export default function AgencySettings() {
  const user = useAuthStore((state) => state.user);
  const [activeTab, setActiveTab] = useState('profile');

  const [profileData, setProfileData] = useState({
    firstName: user?.firstName || '',
    lastName: user?.lastName || '',
    email: user?.email || '',
    phone: '',
  });

  const [companyData, setCompanyData] = useState({
    companyName: '',
    taxNumber: '',
    taxOffice: '',
    address: '',
    city: '',
    country: 'Türkiye',
    website: '',
    contactPerson: '',
    contactPhone: '',
    contactEmail: '',
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  const [showPassword, setShowPassword] = useState(false);

  const handleProfileSave = (e: React.FormEvent) => {
    e.preventDefault();
    toast.success('Profil bilgileri güncellendi');
  };

  const handleCompanySave = (e: React.FormEvent) => {
    e.preventDefault();
    toast.success('Firma bilgileri güncellendi');
  };

  const handlePasswordSave = (e: React.FormEvent) => {
    e.preventDefault();
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      toast.error('Şifreler eşleşmiyor');
      return;
    }
    toast.success('Şifre güncellendi');
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Acente Ayarları</h1>
        <p className="text-sm text-gray-500 mt-1">Hesap ve firma ayarlarınızı yönetin</p>
      </div>

      <Tabs
        tabs={[
          { key: 'profile', label: 'Profil', icon: User },
          { key: 'company', label: 'Firma', icon: Building2 },
          { key: 'notifications', label: 'Bildirimler', icon: Bell },
          { key: 'security', label: 'Güvenlik', icon: Lock },
        ]}
        activeTab={activeTab}
        onChange={(tab) => setActiveTab(tab)}
      />

      {/* Profile Tab */}
      {activeTab === 'profile' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Profil Bilgileri</h3>
          <form onSubmit={handleProfileSave} className="space-y-4 max-w-xl">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Ad"
                value={profileData.firstName}
                onChange={(e) => setProfileData({ ...profileData, firstName: e.target.value })}
                leftIcon={<User className="w-4 h-4 text-gray-400" />}
              />
              <Input
                label="Soyad"
                value={profileData.lastName}
                onChange={(e) => setProfileData({ ...profileData, lastName: e.target.value })}
                leftIcon={<User className="w-4 h-4 text-gray-400" />}
              />
            </div>
            <Input
              label="Email"
              type="email"
              value={profileData.email}
              onChange={(e) => setProfileData({ ...profileData, email: e.target.value })}
              leftIcon={<Mail className="w-4 h-4 text-gray-400" />}
            />
            <Input
              label="Telefon"
              value={profileData.phone}
              onChange={(e) => setProfileData({ ...profileData, phone: e.target.value })}
              leftIcon={<Phone className="w-4 h-4 text-gray-400" />}
            />
            <Button type="submit" leftIcon={<Save className="w-4 h-4" />}>
              Kaydet
            </Button>
          </form>
        </Card>
      )}

      {/* Company Tab */}
      {activeTab === 'company' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Firma Bilgileri</h3>
          <form onSubmit={handleCompanySave} className="space-y-4 max-w-xl">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Firma Adı"
                value={companyData.companyName}
                onChange={(e) => setCompanyData({ ...companyData, companyName: e.target.value })}
                leftIcon={<Building2 className="w-4 h-4 text-gray-400" />}
              />
              <Input
                label="Vergi No"
                value={companyData.taxNumber}
                onChange={(e) => setCompanyData({ ...companyData, taxNumber: e.target.value })}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Vergi Dairesi"
                value={companyData.taxOffice}
                onChange={(e) => setCompanyData({ ...companyData, taxOffice: e.target.value })}
              />
              <Select
                label="Ülke"
                value={companyData.country}
                onChange={(e) => setCompanyData({ ...companyData, country: e.target.value })}
                options={[
                  { value: 'Türkiye', label: '🇹🇷 Türkiye' },
                  { value: 'Almanya', label: '🇩🇪 Almanya' },
                  { value: 'İngiltere', label: '🇬🇧 İngiltere' },
                ]}
              />
            </div>
            <Input
              label="Adres"
              value={companyData.address}
              onChange={(e) => setCompanyData({ ...companyData, address: e.target.value })}
            />
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Şehir"
                value={companyData.city}
                onChange={(e) => setCompanyData({ ...companyData, city: e.target.value })}
              />
              <Input
                label="Web Sitesi"
                value={companyData.website}
                onChange={(e) => setCompanyData({ ...companyData, website: e.target.value })}
                leftIcon={<Globe className="w-4 h-4 text-gray-400" />}
              />
            </div>
            <div className="grid grid-cols-3 gap-4">
              <Input
                label="Yetkili Kişi"
                value={companyData.contactPerson}
                onChange={(e) => setCompanyData({ ...companyData, contactPerson: e.target.value })}
              />
              <Input
                label="Yetkili Telefon"
                value={companyData.contactPhone}
                onChange={(e) => setCompanyData({ ...companyData, contactPhone: e.target.value })}
              />
              <Input
                label="Yetkili Email"
                type="email"
                value={companyData.contactEmail}
                onChange={(e) => setCompanyData({ ...companyData, contactEmail: e.target.value })}
              />
            </div>
            <Button type="submit" leftIcon={<Save className="w-4 h-4" />}>
              Firma Bilgilerini Kaydet
            </Button>
          </form>
        </Card>
      )}

      {/* Notifications Tab */}
      {activeTab === 'notifications' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Bildirim Tercihleri</h3>
          <div className="space-y-3 max-w-xl">
            {[
              { title: 'Yeni Yetkilendirme', desc: 'Yeni bir mülk için yetki verildiğinde', enabled: true },
              { title: 'Yetki İptali', desc: 'Yetkilendirme iptal edildiğinde', enabled: true },
              { title: 'Rezervasyon Onayı', desc: 'Rezervasyon onaylandığında', enabled: true },
              { title: 'Kontenjan Uyarısı', desc: 'Kontenjan %90 dolduğunda', enabled: true },
              { title: 'Fiyat Değişikliği', desc: 'Mülk fiyatları değiştiğinde', enabled: false },
              { title: 'Haftalık Özet', desc: 'Haftalık performans özeti', enabled: true },
            ].map((item, index) => (
              <div key={index} className="flex items-center justify-between py-3 border-b last:border-b-0">
                <div>
                  <p className="text-sm font-medium text-gray-900">{item.title}</p>
                  <p className="text-xs text-gray-500">{item.desc}</p>
                </div>
                <label className="switch">
                  <input type="checkbox" className="switch-input" defaultChecked={item.enabled} />
                  <span className="switch-track" />
                  <span className="switch-thumb" />
                </label>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Security Tab */}
      {activeTab === 'security' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Şifre Değiştir</h3>
          <form onSubmit={handlePasswordSave} className="space-y-4 max-w-xl">
            <div>
              <label className="form-label">Mevcut Şifre</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                <input
                  type={showPassword ? 'text' : 'password'}
                  value={passwordData.currentPassword}
                  onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                  className="form-input pl-10 pr-10"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400"
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>
            <Input
              label="Yeni Şifre"
              type="password"
              value={passwordData.newPassword}
              onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
              leftIcon={<Lock className="w-4 h-4 text-gray-400" />}
            />
            <Input
              label="Yeni Şifre Tekrar"
              type="password"
              value={passwordData.confirmPassword}
              onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
              leftIcon={<Lock className="w-4 h-4 text-gray-400" />}
            />
            <Button type="submit" leftIcon={<Save className="w-4 h-4" />}>
              Şifreyi Güncelle
            </Button>
          </form>
        </Card>
      )}
    </div>
  );
}