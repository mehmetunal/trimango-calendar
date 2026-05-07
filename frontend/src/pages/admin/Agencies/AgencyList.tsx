import { useState } from 'react';
import { adminDb } from '../../../mock/adminDb';

export default function AgencyList() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [rows, setRows] = useState(adminDb.listAgencies());
  const refresh = () => setRows(adminDb.listAgencies());
  return <div className='space-y-4'>
    <h1 className='text-xl font-semibold'>Acente Yönetimi</h1>
    <div className='flex gap-2'>
      <input className='border rounded px-3 py-2' placeholder='Ad' value={name} onChange={e=>setName(e.target.value)} />
      <input className='border rounded px-3 py-2' placeholder='Email' value={email} onChange={e=>setEmail(e.target.value)} />
      <button className='btn btn-primary px-3' onClick={()=>{ if(name&&email){ adminDb.addAgency(name,email); setName(''); setEmail(''); refresh(); }}}>Ekle</button>
    </div>
    <table className='table'> <thead><tr><th>Ad</th><th>Email</th><th>Durum</th><th></th></tr></thead><tbody>
      {rows.map(r=><tr key={r.id}><td>{r.name}</td><td>{r.email}</td><td>{r.active?'Aktif':'Pasif'}</td><td className='space-x-2'>
        <button onClick={()=>{adminDb.updateAgency(r.id,{active:!r.active}); refresh();}}>Toggle</button>
        <button className='text-red-600' onClick={()=>{adminDb.deleteAgency(r.id); refresh();}}>Sil</button>
      </td></tr>)}
    </tbody></table>
  </div>;
}
