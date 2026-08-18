export interface SparklinePoint {
  label: string
  value: number
}

const WIDTH = 300
const HEIGHT = 80
const PADDING = 4

export function Sparkline({ points }: { points: SparklinePoint[] }) {
  const max = Math.max(1, ...points.map((p) => p.value))
  const stepX = points.length > 1 ? (WIDTH - PADDING * 2) / (points.length - 1) : 0

  const coords = points.map((p, i) => {
    const x = PADDING + i * stepX
    const y = HEIGHT - PADDING - (p.value / max) * (HEIGHT - PADDING * 2)
    return { x, y, ...p }
  })

  const linePath = coords.map((c, i) => `${i === 0 ? 'M' : 'L'}${c.x},${c.y}`).join(' ')
  const areaPath =
    coords.length > 0
      ? `${linePath} L${coords[coords.length - 1].x},${HEIGHT - PADDING} L${coords[0].x},${HEIGHT - PADDING} Z`
      : ''

  const total = points.reduce((sum, p) => sum + p.value, 0)
  const first = points[0]?.label
  const last = points[points.length - 1]?.label

  return (
    <div className="sparkline">
      <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="sparkline-svg" preserveAspectRatio="none">
        {areaPath && <path d={areaPath} fill="var(--color-primary)" opacity="0.12" />}
        {linePath && (
          <path d={linePath} fill="none" stroke="var(--color-primary)" strokeWidth="2" />
        )}
      </svg>
      <div className="sparkline-footer">
        <span>{first}</span>
        <span>{total} claims in range</span>
        <span>{last}</span>
      </div>
    </div>
  )
}
