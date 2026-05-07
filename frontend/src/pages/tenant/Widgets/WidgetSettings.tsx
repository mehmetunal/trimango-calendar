import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Plus,
  Copy,
  Code,
  Eye,
  Settings,
  Globe,
  Palette,
  Image,
  MessageSquare,
  DollarSign,
  Calendar,
  Check,
  X,
  RefreshCw,
  ExternalLink,
  Smartphone,
  Monitor,
  Tablet,
  ChevronDown,
  Upload,
  Trash2,
  Save,
  CheckCircle,
  AlertCircle,
  Info,
} from 'lucide-react';
import { widgetApi } from '../../../api/widget.api';
import { propertyApi } from '../../../api/property.api';
import { Button, Input, Select, Modal, Card, Tabs, Badge, ConfirmDialog } from '../../../components/ui';
import { formatDate, formatCurrency } from '../../../utils/format';
import { PROPERTY_TYPES, CURRENCIES } from '../../../utils/constants';
import toast from 'react-hot-toast';

