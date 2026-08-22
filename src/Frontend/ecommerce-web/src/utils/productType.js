// Mirrors Product.Domain.ProductType / PricingModel (backend enums serialize as these exact
// strings — see ProductConfiguration's HasConversion<string>()).
export const PRODUCT_TYPES = ['License', 'ApiAccess', 'SaaSSubscription', 'Project'];
export const PRICING_MODELS = ['OneTime', 'Monthly', 'Yearly'];

const TYPE_LABELS = {
  License: 'License',
  ApiAccess: 'API Access',
  SaaSSubscription: 'SaaS',
  Project: 'Project',
};

const TYPE_ICONS = {
  License: 'key',
  ApiAccess: 'code',
  SaaSSubscription: 'layers',
  Project: 'package',
};

const PRICING_LABELS = {
  OneTime: 'one-time',
  Monthly: '/ month',
  Yearly: '/ year',
};

export function productTypeLabel(type) {
  return TYPE_LABELS[type] ?? type;
}

export function productTypeIcon(type) {
  return TYPE_ICONS[type] ?? 'box';
}

export function pricingModelLabel(model) {
  return PRICING_LABELS[model] ?? model;
}

// Product.Domain.Product.CategoryId is still a required Guid on the backend, but this project has
// no Category CRUD/service of its own — categories were never more than an opaque grouping id.
// Rather than surface a raw-GUID input in the Admin form, one fixed id per ProductType is used as
// its category, so "category" quietly becomes "product type" without a backend migration.
const CATEGORY_IDS = {
  License: '11111111-1111-1111-1111-111111111111',
  ApiAccess: '22222222-2222-2222-2222-222222222222',
  SaaSSubscription: '33333333-3333-3333-3333-333333333333',
  Project: '44444444-4444-4444-4444-444444444444',
};

export function categoryIdForType(type) {
  return CATEGORY_IDS[type] ?? CATEGORY_IDS.Project;
}
