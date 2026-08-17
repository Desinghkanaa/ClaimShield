export interface DonutChartItem {
  label: string
  value: number
  color: string
}

// r = 15.915 makes the circle's circumference exactly 100, so each
// segment's percentage-of-total maps directly to stroke-dasharray units.
const RADIUS = 15.915
const CIRCUMFERENCE = 100

export function DonutChart({ items }: { items: DonutChartItem[] }) {
  const total = items.reduce((sum, i) => sum + i.value, 0)

  let offset = 0
  const segments = items.map((item) => {
    const percent = total > 0 ? (item.value / total) * 100 : 0
    const segment = { ...item, percent, offset }
    offset += percent
    return segment
  })

  return (
    <div className="donut-chart">
      <svg viewBox="0 0 42 42" className="donut-chart-svg">
        <circle
          cx="21"
          cy="21"
          r={RADIUS}
          fill="transparent"
          stroke="var(--color-border)"
          strokeWidth="6"
        />
        {segments
          .filter((s) => s.percent > 0)
          .map((s) => (
            <circle
              key={s.label}
              cx="21"
              cy="21"
              r={RADIUS}
              fill="transparent"
              stroke={s.color}
              strokeWidth="6"
              strokeDasharray={`${s.percent} ${CIRCUMFERENCE - s.percent}`}
              strokeDashoffset={25 - s.offset}
            />
          ))}
        <text x="21" y="23" textAnchor="middle" className="donut-chart-total">
          {total}
        </text>
      </svg>
      <ul className="donut-chart-legend">
        {items.map((item) => (
          <li key={item.label}>
            <span className="donut-chart-swatch" style={{ background: item.color }} />
            {item.label}: {item.value}
          </li>
        ))}
      </ul>
    </div>
  )
}
