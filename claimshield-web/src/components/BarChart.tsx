export interface BarChartItem {
  label: string
  value: number
  color?: string
}

export function BarChart({ items }: { items: BarChartItem[] }) {
  const max = Math.max(1, ...items.map((i) => i.value))

  return (
    <div className="bar-chart">
      {items.map((item) => (
        <div className="bar-chart-row" key={item.label}>
          <span className="bar-chart-label">{item.label}</span>
          <div className="bar-chart-track">
            <div
              className="bar-chart-fill"
              style={{
                width: `${(item.value / max) * 100}%`,
                background: item.color ?? 'var(--color-primary)',
              }}
            />
          </div>
          <span className="bar-chart-value">{item.value}</span>
        </div>
      ))}
    </div>
  )
}
