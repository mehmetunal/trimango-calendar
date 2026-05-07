export type MockRole = 'Admin' | 'TenantOwner' | 'AgencyUser';

export interface MockUser {
  id: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: MockRole;
  tenantId?: string;
  agencyId?: string;
}

export const mockUsers: MockUser[] = [
  {
    id: 'admin-1',
    email: 'admin@trimango.local',
    password: 'Admin123!',
    firstName: 'System',
    lastName: 'Admin',
    role: 'Admin',
  },
  {
    id: 'tenant-1',
    email: 'owner@trimango.local',
    password: 'Owner123!',
    firstName: 'Mulk',
    lastName: 'Sahibi',
    role: 'TenantOwner',
    tenantId: 'tenant-001',
  },
  {
    id: 'agency-1',
    email: 'agency@trimango.local',
    password: 'Agency123!',
    firstName: 'Acente',
    lastName: 'Kullanici',
    role: 'AgencyUser',
    agencyId: 'agency-001',
  },
];

export function loginWithMockUser(email: string, password: string) {
  const user = mockUsers.find(
    (u) => u.email.toLowerCase() === email.toLowerCase() && u.password === password
  );

  if (!user) {
    throw new Error('Email veya sifre hatali');
  }

  return {
    user: {
      id: user.id,
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName,
      role: user.role,
      tenantId: user.tenantId,
      agencyId: user.agencyId,
    },
    token: `mock-token-${user.id}`,
    refreshToken: `mock-refresh-${user.id}`,
  };
}
