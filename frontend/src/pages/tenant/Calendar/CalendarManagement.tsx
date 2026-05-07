import { useState, useRef, useCallback, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import FullCalendar from '@fullcalendar/react';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import trLocale from '@fullcalendar/core/locales/tr';
import {
  Plus,
  Search,
  Filter,
  Download,
  Calendar as CalendarIcon,
  Lock,
  Unlock,
  Wrench,
  Home,
  AlertCircle,
  Eye,
  EyeOff,
  Trash2,
  Edit,
  ChevronLeft,
  ChevronRight,
  RotateCcw,
  Settings,
  Users,
  DollarSign,
  X,
  Check,
  Clock,
  MapPin,
  Phone,
  Mail,
  Info,
} from 'lucide-react';
import { calendarApi } from '../../../api/calendar.api';
import { propertyApi } from '../../../api/property.api';
import { reservationApi } from '../../../api/reservation.api';
import { Button, Input, Select, Modal, Card, Badge, ConfirmDialog, Pagination } from '../../../components/ui';
import { useDebounce } from '../../../hooks/useDebounce';
import { formatCurrency, formatDate, formatDateTime, formatTime, getNights } from '../../../utils/format';
import { PROPERTY_TYPES, RESERVATION_STATUSES } from '../../../utils/constants';
import toast from 'react-hot-toast';

