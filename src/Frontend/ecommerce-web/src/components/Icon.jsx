// Small hand-drawn icon set (not a library) — keeps the redesigned UI off emoji without pulling
// in an icon package. Every icon is a 24x24 stroked outline, same visual weight throughout.
const PATHS = {
  dashboard: 'M4 4h7v7H4V4zm9 0h7v4h-7V4zm0 7h7v9h-7v-9zM4 14h7v6H4v-6z',
  box: 'M3 8l9-5 9 5-9 5-9-5zm0 0v9l9 5m9-14v9l-9 5M3 8l9 5m9-5l-9 5',
  key: 'M15 7a4 4 0 100 8 4 4 0 000-8zm-1.5 5.5L4 22m4-4l3 3m2-6l3 3',
  card: 'M3 6h18a1 1 0 011 1v10a1 1 0 01-1 1H3a1 1 0 01-1-1V7a1 1 0 011-1zM2 10h20M6 15h4',
  user: 'M12 12a4 4 0 100-8 4 4 0 000 8zm-7 9a7 7 0 0114 0',
  shield: 'M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3z',
  bell: 'M6 10a6 6 0 1112 0c0 4 1.5 5.5 1.5 5.5H4.5S6 14 6 10zM9.5 18.5a2.5 2.5 0 005 0',
  logout: 'M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9',
  copy: 'M8 8V4a1 1 0 011-1h10a1 1 0 011 1v10a1 1 0 01-1 1h-4M4 8h11a1 1 0 011 1v11a1 1 0 01-1 1H4a1 1 0 01-1-1V9a1 1 0 011-1z',
  external: 'M14 4h6v6M20 4L10 14M19 13v6a1 1 0 01-1 1H5a1 1 0 01-1-1V6a1 1 0 011-1h6',
  plus: 'M12 5v14M5 12h14',
  chevron: 'M6 9l6 6 6-6',
  search: 'M11 19a8 8 0 100-16 8 8 0 000 16zM21 21l-4.35-4.35',
  cart: 'M3 4h2l2.6 12.4a1 1 0 001 .8h9.7a1 1 0 001-.78L21 8H6M9 21a1 1 0 100-2 1 1 0 000 2zm9 0a1 1 0 100-2 1 1 0 000 2z',
  package: 'M12 3l8 4.5v9L12 21l-8-4.5v-9L12 3zm0 9v9m0-9L4 7.5M12 12l8-4.5',
  check: 'M5 13l4 4L19 7',
  code: 'M9 8L4 12l5 4m6-8l5 4-5 4M14 4l-4 16',
  rocket: 'M6 15c-2 1-3 5-3 5s4-1 5-3m4-2l4-4a5 5 0 000-7 5 5 0 00-7 0l-4 4m3 3l-3-3m3 3l4 4m-7-7l4 4',
  clock: 'M12 8v4l3 3M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
  layers: 'M12 2l9 5-9 5-9-5 9-5zm-9 9l9 5 9-5m-18 4l9 5 9-5',
};

export function Icon({ name, size = 18, strokeWidth = 1.8, className = '', ...rest }) {
  const d = PATHS[name];
  if (!d) return null;
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={`icon icon-${name} ${className}`}
      aria-hidden="true"
      {...rest}
    >
      <path d={d} />
    </svg>
  );
}
