// src/pages/tenant/Pricing/CurrencyManagement.tsx
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  DollarSign,
  RefreshCw,
  TrendingUp,
  TrendingDown,
  Globe,
  Plus,
  Edit,
  Trash2,
  Check,
  X,
} from 'lucide-react';
import { pricingApi } from '../../../api/pricing.api';
import { Button, Card, Badge, Modal, ConfirmDialog } from '../../../components/ui';
import { formatCurrency, formatDate } from '../../../utils/format';
import { CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

export default function CurrencyManagement() {
  const queryClient = useQueryClient();

  const { data: currencies, isLoading } = useQuery({
    queryKey: ['currencies'],
    queryFn: () => pricingApi.getCurrencies(),
  });

  const { data: exchangeRates } = useQuery({
    queryKey: ['exchangeRates'],
    queryFn: () => pricingApi.getExchangeRate('TRY', 'USD'),
  });

  const updateRatesMutation = useMutation({
    mutationFn: async () => {
      toast.success('Kurlar güncelleniyor...');
      return true;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['exchangeRates'] });
    },
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Para Birimleri</h1>
          <p className="text-sm text-gray-500 mt-1">Döviz kuru ve para birimi yönetimi</p>
        </div>
        <Button
          onClick={() => updateRatesMutation.mutate()}
          leftIcon={<RefreshCw className="w-4 h-4" />}
          isLoading={updateRatesMutation.isPending}
        >
          Kurları Güncelle
        </Button>
      </div>

      {/* Currency Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {CURRENCIES.map((currency) => {
          const active = currencies?.find((c: any) => c.code === currency.code && c.isActive);
          return (
            <Card key={currency.code} className={`${!active ? 'opacity-60' : ''}`}>
              <div className="p-5">
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center">
                      <span className="text-2xl font-bold text-blue-600">{currency.symbol}</span>
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900">{currency.name}</h3>
                      <p className="text-sm text-gray-500">{currency.code}</p>
                    </div>
                  </div>
                  <Badge color={active ? 'green' : 'gray'}>
                    {active ? 'Aktif' : 'Pasif'}
                  </Badge>
                </div>

                {currency.code !== 'TRY' && exchangeRates && (
                  <div className="pt-3 border-t">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-500">1 {currency.code}</span>
                      <span className="font-medium">
                        {formatCurrency(exchangeRates.rate, 'TRY')}
                      </span>
                    </div>
                  </div>
                )}
              </div>
            </Card>
          );
        })}
      </div>

      {/* Exchange Rate Table */}
      <Card>
        <div className="card-header">
          <h3 className="text-lg font-semibold">Güncel Döviz Kurları</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="table">
            <thead>
              <tr>
                <th>Para Birimi</th>
                <th>Alış</th>
                <th>Satış</th>
                <th>Değişim</th>
                <th>Son Güncelleme</th>
              </tr>
            </thead>
            <tbody>
              {CURRENCIES.filter(c => c.code !== 'TRY').map((currency) => (
                <tr key={currency.code}>
                  <td>
                    <div className="flex items-center gap-2">
                      <span className="text-lg">{currency.symbol}</span>
                      <div>
                        <div className="font-medium">{currency.code}</div>
                        <div className="text-xs text-gray-500">{currency.name}</div>
                      </div>
                    </div>
                  </td>
                  <td className="font-medium">
                    {formatCurrency(30.45, 'TRY')}
                  </td>
                  <td className="font-medium">
                    {formatCurrency(30.55, 'TRY')}
                  </td>
                  <td>
                    <span className="inline-flex items-center gap-1 text-sm text-green-600">
                      <TrendingUp className="w-3.5 h-3.5" />
                      %0.5
                    </span>
                  </td>
                  <td className="text-sm text-gray-500">
                    {formatDate(new Date())}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
