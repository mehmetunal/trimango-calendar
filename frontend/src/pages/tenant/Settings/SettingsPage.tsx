// src/pages/tenant/Settings/SettingsPage.tsx
import { useState } from 'react';
import {
  User,
  Building2,
  Mail,
  Phone,
  Globe,
  Lock,
  Bell,
  CreditCard,
  Palette,
  FileText,
  Save,
  Eye,
  EyeOff,
  Moon,
  Sun,
} from 'lucide-react';
import { Button, Input, Select, Card, Tabs } from '../../../components/ui';
import toast from 'react-hot-toast';

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState('profile');

  const tabs = [
    { key: 'profile', label: 'Profil', icon: User },
    { key: 'company', label: 'Firma', icon: Building2 },
    { key: 'notifications', label: 'Bildirimler', icon: Bell },
    { key: 'appearance', label: 'Görünüm', icon: Palette },
    { key: 'billing', label: 'Fatura', icon: CreditCard },
    { key: 'security', label: 'Güvenlik', icon: Lock },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Ayarlar</h1>
        <p className="text-sm text-gray-500 mt-1">Hesap ve sistem ayarlarınızı yönetin</p>
      </div>

      <Tabs
        tabs={tabs}
        activeTab={activeTab}
        onChange={(tab) => setActiveTab(tab)}
      />

      {activeTab === 'profile' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Profil Bilgileri</h3>
          <div className="space-y-4 max-w-xl">
            <div className="grid grid-cols-2 gap-4">
              <Input label="Ad" value="Demo" />
              <Input label="Soyad" value="Kullanıcı" />
            </div>
            <Input label="Email" type="email" value="demo@trimangocalendar.com" />
            <Input label="Telefon" value="+90 555 123 4567" />
            <Button leftIcon={<Save className="w-4 h-4" />}>Kaydet</Button>
          </div>
        </Card>
      )}

      {activeTab === 'notifications' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Bildirim Tercihleri</h3>
          <div className="space-y-3 max-w-xl">
            {[
              { title: 'Yeni Rezervasyon', desc: 'Yeni rezervasyon oluşturulduğunda', enabled: true },
              { title: 'Check-in Hatırlatma', desc: 'Check-in tarihinden 1 gün önce', enabled: true },
              { title: 'İptal Bildirimi', desc: 'Rezervasyon iptal edildiğinde', enabled: true },
              { title: 'Değerlendirme', desc: 'Yeni değerlendirme yapıldığında', enabled: false },
              { title: 'Pazarlama', desc: 'Kampanya ve duyurular', enabled: false },
            ].map((item, index) => (
              <div key={index} className="flex items-center justify-between py-2">
                <div>
                  <p className="text-sm font-medium">{item.title}</p>
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

      {activeTab === 'appearance' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Görünüm Ayarları</h3>
          <div className="space-y-4 max-w-xl">
            <div>
              <label className="form-label">Tema</label>
              <div className="grid grid-cols-3 gap-3">
                {[
                  { value: 'light', label: 'Açık', icon: Sun },
                  { value: 'dark', label: 'Koyu', icon: Moon },
                  { value: 'system', label: 'Sistem', icon: Palette },
                ].map((theme) => (
                  <button
                    key={theme.value}
                    className="flex flex-col items-center gap-2 p-4 rounded-xl border-2 border-gray-200 hover:border-blue-400 cursor-pointer"
                  >
                    <theme.icon className="w-6 h-6" />
                    <span className="text-sm font-medium">{theme.label}</span>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </Card>
      )}

      {activeTab === 'billing' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Fatura Bilgileri</h3>
          <div className="space-y-4 max-w-xl">
            <div className="p-4 bg-blue-50 rounded-xl">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-semibold text-blue-900">Free Plan</p>
                  <p className="text-sm text-blue-700">5 mülk, 50 rezervasyon/ay</p>
                </div>
                <Button size="sm" variant="outline">Yükselt</Button>
              </div>
            </div>
          </div>
        </Card>
      )}

      {activeTab === 'security' && (
        <Card className="p-6">
          <h3 className="text-lg font-semibold mb-4">Şifre Değiştir</h3>
          <div className="space-y-4 max-w-xl">
            <Input label="Mevcut Şifre" type="password" />
            <Input label="Yeni Şifre" type="password" />
            <Input label="Yeni Şifre Tekrar" type="password" />
            <Button leftIcon={<Lock className="w-4 h-4" />}>Şifreyi Güncelle</Button>
          </div>
        </Card>
      )}
    </div>
  );
}