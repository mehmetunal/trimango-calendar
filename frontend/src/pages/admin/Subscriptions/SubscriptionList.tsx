import { adminDb } from '../../../mock/adminDb';
import { useState } from 'react';

export default function SubscriptionList() {
  const [rows, setRows] = useState(adminDb.listSubscriptions());
  const refresh = () => setRows(adminDb.listSubscriptions());
  return <div className='space-y-4'>
    <h1 className='text-xl font-semibold'>Abonelikler</h1>
    <table className='table'><thead><tr><th>Tenant</th><th>Plan</th><th>Durum</th><th></th></tr></thead><tbody>
      {rows.map(r=><tr key={r.id}><td>{r.tenantId}</td><td>{r.plan}</td><td>{r.status}</td><td className='space-x-2'>
        <button onClick={()=>{adminDb.updateSubscription(r.id,{status:r.status==='Active'?'Paused':'Active'}); refresh();}}>Durum Değiştir</button>
      </td></tr>)}
    </tbody></table>
  </div>;
}
