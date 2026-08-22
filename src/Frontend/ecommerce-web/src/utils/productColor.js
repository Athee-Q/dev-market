// Products have no image field in the backend yet — these give each product a stable, distinct
// "cover" so the catalog doesn't read as a bare list of text rows, without needing a real asset
// pipeline. Deterministic per product id, so the same product always gets the same look.

const GRADIENTS = [
  ['#6366f1', '#8b5cf6'],
  ['#ec4899', '#f43f5e'],
  ['#f59e0b', '#f97316'],
  ['#10b981', '#059669'],
  ['#06b6d4', '#3b82f6'],
  ['#8b5cf6', '#d946ef'],
  ['#0ea5e9', '#22d3ee'],
  ['#f43f5e', '#fb923c'],
];

function hashString(value) {
  let hash = 0;
  for (let i = 0; i < value.length; i++) {
    hash = (hash << 5) - hash + value.charCodeAt(i);
    hash |= 0;
  }
  return Math.abs(hash);
}

export function productGradient(seed) {
  const [from, to] = GRADIENTS[hashString(String(seed)) % GRADIENTS.length];
  return `linear-gradient(135deg, ${from}, ${to})`;
}

export function productInitials(name) {
  if (!name) return '?';
  return name
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((word) => word[0]?.toUpperCase() ?? '')
    .join('');
}
