// src/pages/tenant/Reviews/ReviewsPage.tsx
import { useState } from 'react';
import {
  Star,
  User,
  Calendar,
  MessageSquare,
  Search,
  Filter,
  Check,
  X,
  Reply,
  Trash2,
  Eye,
  EyeOff,
  ThumbsUp,
  ThumbsDown,
} from 'lucide-react';
import { Button, Input, Select, Card, Badge, Pagination } from '../../../components/ui';
import { formatDate } from '../../../utils/format';

export default function ReviewsPage() {
  const [filter, setFilter] = useState('all');
  const [search, setSearch] = useState('');

  // Demo reviews
  const reviews = [
    {
      id: '1',
      guestName: 'Ahmet Yılmaz',
      rating: 5,
      comment: 'Harika bir konaklama deneyimiydi. Oda çok temiz ve konforluydu. Personel çok ilgiliydi.',
      date: '2024-01-15',
      propertyName: 'Sahil Palace Hotel',
      unitName: 'Deluxe Oda',
      isApproved: true,
      hasResponse: true,
      response: 'Değerli yorumunuz için teşekkür ederiz. Sizi tekrar ağırlamaktan mutluluk duyarız.',
    },
    {
      id: '2',
      guestName: 'Ayşe Demir',
      rating: 4,
      comment: 'Genel olarak iyiydi. Kahvaltı çeşitliliği artırılabilir.',
      date: '2024-01-10',
      propertyName: 'Ege Bahçe Bungalov',
      unitName: 'Aile Bungalovu',
      isApproved: true,
      hasResponse: false,
      response: '',
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Değerlendirmeler</h1>
          <p className="text-sm text-gray-500 mt-1">Misafir değerlendirmelerini yönetin</p>
        </div>
      </div>

      {/* Filters */}
      <Card className="p-4">
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Değerlendirme ara..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="form-input pl-10"
            />
          </div>
          <Select
            value={filter}
            onChange={(e) => setFilter(e.target.value)}
            options={[
              { value: 'all', label: 'Tümü' },
              { value: 'approved', label: 'Onaylanmış' },
              { value: 'pending', label: 'Onay Bekleyen' },
              { value: 'responded', label: 'Cevaplanmış' },
            ]}
          />
        </div>
      </Card>

      {/* Reviews List */}
      <div className="space-y-4">
        {reviews.map((review) => (
          <Card key={review.id}>
            <div className="p-6">
              <div className="flex items-start justify-between mb-4">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <div className="flex">
                      {[1, 2, 3, 4, 5].map((star) => (
                        <Star
                          key={star}
                          className={`w-4 h-4 ${
                            star <= review.rating
                              ? 'text-yellow-400 fill-current'
                              : 'text-gray-300'
                          }`}
                        />
                      ))}
                    </div>
                    <span className="text-sm font-medium text-gray-900">{review.guestName}</span>
                    <Badge color={review.isApproved ? 'green' : 'yellow'}>
                      {review.isApproved ? 'Onaylı' : 'Bekliyor'}
                    </Badge>
                  </div>
                  <div className="flex items-center gap-3 text-xs text-gray-500">
                    <div className="flex items-center gap-1">
                      <Calendar className="w-3.5 h-3.5" />
                      {formatDate(review.date)}
                    </div>
                    <span>•</span>
                    <span>{review.propertyName}</span>
                    <span>•</span>
                    <span>{review.unitName}</span>
                  </div>
                </div>
              </div>

              <p className="text-gray-700 mb-4">{review.comment}</p>

              {review.hasResponse && (
                <div className="ml-4 pl-4 border-l-2 border-blue-200 mb-4">
                  <div className="flex items-center gap-2 mb-1">
                    <Reply className="w-4 h-4 text-blue-500" />
                    <span className="text-xs font-medium text-blue-600">İşletme Yanıtı</span>
                  </div>
                  <p className="text-sm text-gray-600">{review.response}</p>
                </div>
              )}

              <div className="flex items-center gap-2 pt-3 border-t">
                <Button size="sm" variant="ghost">
                  <Check className="w-4 h-4 text-green-500" />
                </Button>
                <Button size="sm" variant="ghost">
                  <X className="w-4 h-4 text-red-500" />
                </Button>
                {!review.hasResponse && (
                  <Button size="sm" variant="ghost">
                    <Reply className="w-4 h-4" />
                    Yanıtla
                  </Button>
                )}
                <Button size="sm" variant="ghost" className="ml-auto">
                  <Trash2 className="w-4 h-4" />
                </Button>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}