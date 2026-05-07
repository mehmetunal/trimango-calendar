import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Download,
  Calendar,
  TrendingUp,
  TrendingDown,
  DollarSign,
  Users,
  BedDouble,
  Building2,
  Star,
  Filter,
  RefreshCw,
  FileText,
  Printer,
  Mail,
  ChevronDown,
  ArrowUp,
  ArrowDown,
  BarChart3,
  PieChart,
  Activity,
  Target,
  Award,
  Clock,
  AlertCircle,
} from 'lucide-react';
import { reportApi } from '../../../api/report.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Card, Tabs } from '../../../components/ui';
import {
  LineChart,
  BarChart,
  AreaChart,
  PieChart as RePieChart,
  ResponsiveContainer,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  Line,
  Bar,
  Area,
  Pie,
  Cell,
} from 'recharts';
import { formatCurrency, formatDate, formatDateTime } from '../../../utils/format';
import { CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

