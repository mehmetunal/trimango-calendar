import { LineChart, BarChart } from '../../../components/charts';

export default function AdminReports(){
  const revenue = [{label:'Ocak', value:120000},{label:'Subat', value:140000},{label:'Mart', value:132000}];
  const occupancy = [{label:'Pzt', value:62},{label:'Sal', value:70},{label:'Car', value:74}];
  return <div className='space-y-4'>
    <h1 className='text-xl font-semibold'>Raporlar</h1>
    <div className='grid grid-cols-1 lg:grid-cols-2 gap-4'>
      <div className='bg-white rounded-xl border p-4'><h2 className='font-medium mb-2'>Gelir</h2><BarChart data={revenue} dataKey='value' /></div>
      <div className='bg-white rounded-xl border p-4'><h2 className='font-medium mb-2'>Doluluk</h2><LineChart data={occupancy} dataKey='value' /></div>
    </div>
  </div>;
}
