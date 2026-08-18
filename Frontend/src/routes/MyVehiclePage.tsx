import { useEffect, useState } from 'react'
import { ApiError, getMyCustomerProfile, getMyVehicles } from '../lib/api'
import type { VehicleResponseDto } from '../lib/types'
import { SkeletonBlock } from '../components/Skeleton'
import { ActiveBadge } from '../components/StatusBadge'

export function MyVehiclePage() {
  const [vehicles, setVehicles] = useState<VehicleResponseDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    getMyCustomerProfile()
      .then((customer) => getMyVehicles(customer.customerId))
      .then((data) => {
        if (!cancelled) setVehicles(data)
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Failed to load your vehicles.')
        }
      })

    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div>
      <h1>My Vehicle</h1>

      {error && <p className="error-text">{error}</p>}

      {!error && !vehicles && (
        <section className="card">
          <SkeletonBlock lines={5} />
        </section>
      )}

      {vehicles && vehicles.length === 0 && (
        <p>No vehicles are linked to your account yet.</p>
      )}

      {vehicles?.map((vehicle) => (
        <section className="card" key={vehicle.vehicleId}>
          <h2>
            {vehicle.registrationNumber} <ActiveBadge active={vehicle.isActive} />
          </h2>
          <dl className="fact-grid">
            <dt>Variant</dt>
            <dd>{vehicle.variant ?? '—'}</dd>

            <dt>Manufacturing year</dt>
            <dd>{vehicle.manufacturingYear}</dd>

            <dt>Colour</dt>
            <dd>{vehicle.vehicleColor ?? '—'}</dd>

            <dt>Chassis number</dt>
            <dd>{vehicle.chassisNumber}</dd>

            <dt>Engine number</dt>
            <dd>{vehicle.engineNumber}</dd>

            <dt>RC number</dt>
            <dd>{vehicle.rcNumber ?? '—'}</dd>

            <dt>RC status</dt>
            <dd>{vehicle.rcStatus ?? '—'}</dd>
          </dl>
        </section>
      ))}
    </div>
  )
}
