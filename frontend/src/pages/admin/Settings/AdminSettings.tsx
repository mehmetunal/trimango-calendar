import { useState } from 'react';
import { adminDb } from '../../../mock/adminDb';

export default function AdminSettings(){
  const initial = adminDb.getSettings();
  const [appName, setAppName] = useState(initial.appName);
  const [supportEmail, setSupportEmail] = useState(initial.supportEmail);
  return <div className='space-y-4 max-w-xl'>
    <h1 className='text-xl font-semibold'>Sistem Ayarları</h1>
    <div>
      <label className='form-label'>Uygulama Adı</label>
      <input className='form-input' value={appName} onChange={e=>setAppName(e.target.value)} />
    </div>
    <div>
      <label className='form-label'>Destek Email</label>
      <input className='form-input' value={supportEmail} onChange={e=>setSupportEmail(e.target.value)} />
    </div>
    <button className='btn btn-primary px-4' onClick={()=>adminDb.saveSettings({appName, supportEmail})}>Kaydet</button>
  </div>;
}
