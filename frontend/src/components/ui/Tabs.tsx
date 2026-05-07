// src/components/ui/Tabs.tsx
import { clsx } from 'clsx';
import { LucideIcon } from 'lucide-react';

interface Tab {
  key: string;
  label: string;
  icon?: LucideIcon;
  count?: number;
}

interface TabsProps {
  tabs: Tab[];
  activeTab: string;
  onChange: (key: string) => void;
  variant?: 'default' | 'pills' | 'underline';
  className?: string;
}

export default function Tabs({ tabs, activeTab, onChange, variant = 'underline', className }: TabsProps) {
  return (
    <div className={clsx(
      'flex gap-1',
      variant === 'underline' && 'border-b border-gray-200',
      variant === 'pills' && 'bg-gray-100 rounded-lg p-1',
      className
    )}>
      {tabs.map((tab) => {
        const Icon = tab.icon;
        return (
          <button
            key={tab.key}
            onClick={() => onChange(tab.key)}
            className={clsx(
              'flex items-center gap-2 px-4 py-3 text-sm font-medium transition-colors whitespace-nowrap',
              variant === 'underline' && [
                'border-b-2 -mb-[1px]',
                activeTab === tab.key
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
              ],
              variant === 'pills' && [
                'rounded-md',
                activeTab === tab.key
                  ? 'bg-white shadow-sm text-blue-600'
                  : 'text-gray-500 hover:text-gray-700',
              ],
            )}
          >
            {Icon && <Icon className="w-4 h-4" />}
            <span>{tab.label}</span>
            {tab.count !== undefined && (
              <span className={clsx(
                'inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 text-xs font-medium rounded-full',
                activeTab === tab.key
                  ? 'bg-blue-100 text-blue-600'
                  : 'bg-gray-100 text-gray-500'
              )}>
                {tab.count}
              </span>
            )}
          </button>
        );
      })}
    </div>
  );
}