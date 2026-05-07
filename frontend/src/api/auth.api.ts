// src/api/auth.api.ts
import api from './axios';
import { useAuthStore } from '../stores/authStore';
import { loginWithMockUser } from './mockAuth';

interface LoginCredentials {
  email: string;
  password: string;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantId?: string;
}

interface AuthResponse {
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    tenantId?: string;
    agencyId?: string;
  };
  token: string;
  refreshToken: string;
}

interface JwtPayload {
  sub?: string;
  email?: string;
  unique_name?: string;
  given_name?: string;
  family_name?: string;
  role?: string;
  tenantId?: string;
  agencyId?: string;
  [key: string]: unknown;
}

function parseJwt(token: string): JwtPayload | null {
  try {
    const base64 = token.split('.')[1];
    if (!base64) return null;
    const json = decodeURIComponent(
      atob(base64.replace(/-/g, '+').replace(/_/g, '/'))
        .split('')
        .map((c) => `%${(`00${c.charCodeAt(0).toString(16)}`).slice(-2)}`)
        .join('')
    );
    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}

function buildUserFromToken(accessToken: string) {
  const payload = parseJwt(accessToken);
  const fullName = String(payload?.unique_name || '').trim();
  const [firstName = '', ...rest] = fullName.split(' ');
  const lastName = rest.join(' ');
  const role = String(payload?.role || 'TenantOwner') as 'Admin' | 'TenantOwner' | 'AgencyUser';

  return {
    id: String(payload?.sub || ''),
    email: String(payload?.email || ''),
    firstName: String(payload?.given_name || firstName || ''),
    lastName: String(payload?.family_name || lastName || ''),
    role,
    tenantId: payload?.tenantId ? String(payload.tenantId) : undefined,
    agencyId: payload?.agencyId ? String(payload.agencyId) : undefined,
  };
}

export const authApi = {
  login: async (credentials: LoginCredentials): Promise<AuthResponse> => {
    if (import.meta.env.VITE_USE_MOCK === 'true') {
      const response = loginWithMockUser(credentials.email, credentials.password);
      const { user, token, refreshToken } = response;
      useAuthStore.getState().login(user as any, token, refreshToken);
      return response;
    }

    const response = await api.post('/auth/login', credentials);
    const { accessToken, refreshToken } = response.data;
    const user = buildUserFromToken(accessToken);
    const token = accessToken;
    useAuthStore.getState().login(user, token, refreshToken);
    return { user, token, refreshToken };
  },

  register: async (data: RegisterData): Promise<AuthResponse> => {
    const response = await api.post('/auth/register', data);
    const { accessToken, refreshToken } = response.data;
    const user = buildUserFromToken(accessToken);
    const token = accessToken;
    useAuthStore.getState().login(user, token, refreshToken);
    return { user, token, refreshToken };
  },

  logout: async (): Promise<void> => {
    useAuthStore.getState().logout();
  },

  refreshToken: async (refreshToken: string): Promise<{ token: string; refreshToken: string }> => {
    const accessToken = useAuthStore.getState().token || '';
    const response = await api.post('/auth/refresh-token', { accessToken, refreshToken });
    return {
      token: response.data.accessToken,
      refreshToken: response.data.refreshToken,
    };
  },

  forgotPassword: async (email: string): Promise<void> => {
    await api.post('/auth/forgot-password', { email });
  },

  resetPassword: async (email: string, token: string, newPassword: string): Promise<void> => {
    await api.post('/auth/reset-password', { email, token, newPassword });
  },

  getProfile: async () => {
    const token = useAuthStore.getState().token;
    return token ? buildUserFromToken(token) : null;
  },

  updateProfile: async (data: Partial<RegisterData>) => {
    return data;
  },
};
