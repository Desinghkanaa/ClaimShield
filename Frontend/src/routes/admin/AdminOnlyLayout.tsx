import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { RoleId } from '../../lib/roles'

export function AdminOnlyLayout() {
  const { roleId } = useAuth()

  if (roleId !== RoleId.Admin) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
