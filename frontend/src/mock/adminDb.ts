export type Tenant = { id: string; name: string; email: string; active: boolean; createdAt: string };
export type Agency = { id: string; name: string; email: string; active: boolean; createdAt: string };
export type Subscription = { id: string; tenantId: string; plan: 'Basic' | 'Pro' | 'Enterprise'; status: 'Active' | 'Paused' | 'Cancelled' };

type Db = { tenants: Tenant[]; agencies: Agency[]; subscriptions: Subscription[]; settings: { appName: string; supportEmail: string } };

const KEY = 'admin-mock-db';

const seed: Db = {
  tenants: [
    { id: 't1', name: 'Sahil Group', email: 'tenant1@trimango.local', active: true, createdAt: new Date().toISOString() },
    { id: 't2', name: 'Ege Hotels', email: 'tenant2@trimango.local', active: true, createdAt: new Date().toISOString() },
  ],
  agencies: [
    { id: 'a1', name: 'Blue Agency', email: 'agency1@trimango.local', active: true, createdAt: new Date().toISOString() },
    { id: 'a2', name: 'Sun Travel', email: 'agency2@trimango.local', active: false, createdAt: new Date().toISOString() },
  ],
  subscriptions: [
    { id: 's1', tenantId: 't1', plan: 'Pro', status: 'Active' },
    { id: 's2', tenantId: 't2', plan: 'Basic', status: 'Paused' },
  ],
  settings: { appName: 'HotelPlatform', supportEmail: 'support@trimango.local' },
};

function read(): Db {
  const raw = localStorage.getItem(KEY);
  if (!raw) {
    localStorage.setItem(KEY, JSON.stringify(seed));
    return seed;
  }
  return JSON.parse(raw) as Db;
}

function write(db: Db) { localStorage.setItem(KEY, JSON.stringify(db)); }

export const adminDb = {
  listTenants: () => read().tenants,
  addTenant: (name: string, email: string) => { const db = read(); db.tenants.push({ id: crypto.randomUUID(), name, email, active: true, createdAt: new Date().toISOString() }); write(db); },
  updateTenant: (id: string, patch: Partial<Tenant>) => { const db = read(); db.tenants = db.tenants.map(t => t.id === id ? { ...t, ...patch } : t); write(db); },
  deleteTenant: (id: string) => { const db = read(); db.tenants = db.tenants.filter(t => t.id !== id); write(db); },

  listAgencies: () => read().agencies,
  addAgency: (name: string, email: string) => { const db = read(); db.agencies.push({ id: crypto.randomUUID(), name, email, active: true, createdAt: new Date().toISOString() }); write(db); },
  updateAgency: (id: string, patch: Partial<Agency>) => { const db = read(); db.agencies = db.agencies.map(a => a.id === id ? { ...a, ...patch } : a); write(db); },
  deleteAgency: (id: string) => { const db = read(); db.agencies = db.agencies.filter(a => a.id !== id); write(db); },

  listSubscriptions: () => read().subscriptions,
  updateSubscription: (id: string, patch: Partial<Subscription>) => { const db = read(); db.subscriptions = db.subscriptions.map(s => s.id === id ? { ...s, ...patch } : s); write(db); },

  getSettings: () => read().settings,
  saveSettings: (settings: Db['settings']) => { const db = read(); db.settings = settings; write(db); },
};
