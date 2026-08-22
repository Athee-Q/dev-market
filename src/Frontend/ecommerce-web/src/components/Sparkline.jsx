// Minimal inline-SVG sparkline — no charting dependency. `values` should be in chronological order.
export function Sparkline({ values, width = 180, height = 40 }) {
  if (!values || values.length < 2) return null;

  const max = Math.max(...values);
  const min = Math.min(...values);
  const range = max - min || 1;
  const stepX = width / (values.length - 1);

  const points = values.map((v, i) => [i * stepX, height - ((v - min) / range) * height]);
  const line = points.map(([x, y], i) => `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`).join(' ');
  const fill = `${line} L${width},${height} L0,${height} Z`;

  return (
    <svg className="sparkline" width={width} height={height} viewBox={`0 0 ${width} ${height}`} preserveAspectRatio="none">
      <path className="fill" d={fill} />
      <path className="line" d={line} />
    </svg>
  );
}
