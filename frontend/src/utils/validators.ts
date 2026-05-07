// src/utils/validators.ts
export const validators = {
  email: (value: string): string | undefined => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!value) return 'Email zorunludur';
    if (!emailRegex.test(value)) return 'Geçerli bir email adresi giriniz';
    return undefined;
  },

  password: (value: string): string | undefined => {
    if (!value) return 'Şifre zorunludur';
    if (value.length < 8) return 'Şifre en az 8 karakter olmalıdır';
    if (!/[A-Z]/.test(value)) return 'En az bir büyük harf içermelidir';
    if (!/[a-z]/.test(value)) return 'En az bir küçük harf içermelidir';
    if (!/[0-9]/.test(value)) return 'En az bir rakam içermelidir';
    return undefined;
  },

  phone: (value: string): string | undefined => {
    const phoneRegex = /^[0-9+\-\s]{10,20}$/;
    if (!value) return 'Telefon zorunludur';
    if (!phoneRegex.test(value)) return 'Geçerli bir telefon numarası giriniz';
    return undefined;
  },

  tcKimlik: (value: string): string | undefined => {
    if (!value) return undefined;
    if (!/^[0-9]{11}$/.test(value)) return 'TC Kimlik No 11 haneli olmalıdır';
    
    // TC Kimlik No algoritması
    const digits = value.split('').map(Number);
    const sum1 = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
    const sum2 = digits[1] + digits[3] + digits[5] + digits[7];
    const check1 = (sum1 * 7 - sum2) % 10;
    const check2 = (digits.slice(0, 10).reduce((a, b) => a + b, 0)) % 10;
    
    if (check1 !== digits[9] || check2 !== digits[10]) {
      return 'Geçersiz TC Kimlik No';
    }
    return undefined;
  },

  required: (value: any, fieldName: string = 'Bu alan'): string | undefined => {
    if (!value || (typeof value === 'string' && !value.trim())) {
      return `${fieldName} zorunludur`;
    }
    return undefined;
  },

  minLength: (value: string, min: number, fieldName: string = 'Bu alan'): string | undefined => {
    if (value && value.length < min) {
      return `${fieldName} en az ${min} karakter olmalıdır`;
    }
    return undefined;
  },

  maxLength: (value: string, max: number, fieldName: string = 'Bu alan'): string | undefined => {
    if (value && value.length > max) {
      return `${fieldName} en fazla ${max} karakter olmalıdır`;
    }
    return undefined;
  },

  numberRange: (value: number, min: number, max: number, fieldName: string = 'Bu alan'): string | undefined => {
    if (value < min || value > max) {
      return `${fieldName} ${min} ile ${max} arasında olmalıdır`;
    }
    return undefined;
  },
};