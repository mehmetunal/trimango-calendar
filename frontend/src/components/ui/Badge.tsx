// src/components/ui/Badge.tsx
import { clsx } from 'clsx';
import { HTMLAttributes } from 'react';

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  children: React.ReactNode;
  color?: 'green' | 'red' | 'yellow' | 'blue' | 'purple' | 'gray' | 'orange';
  size?: 'sm' | 'md' | 'lg';
  dot?: boolean;
}

const colorClasses: Record<string, string> = {
  green: 'bg-green-100 text-green-800',
  red: 'bg-red-100 text-red-800',
  yellow: 'bg-yellow-100 text-yellow-800',
  blue: 'bg-blue-100 text-blue-800',
  purple: 'bg-purple-100 text-purple-800',
  gray: 'bg-gray-100 text-gray-800',
  orange: 'bg-orange-100 text-orange-800',
};

const sizeClasses: Record<string, string> = {
  sm: 'px-2 py-0.5 text-xs',
  md: 'px-2.5 py-0.5 text-xs',
  lg: 'px-3 py-1 text-sm',
};

export default function Badge({ 
  children, 
  color = 'gray', 
  size = 'md', 
  dot = false, 
  className,
  ...props 
}: BadgeProps) {
  return (
    <span
      className={clsx(
        'inline-flex items-center font-medium rounded-full',
        colorClasses[color],
        sizeClasses[size],
        className
      )}
      {...props}
    >
      {dot && <span className="w-1.5 h-1.5 rounded-full bg-current mr-1.5" />}
      {children}
    </span>
  );
}