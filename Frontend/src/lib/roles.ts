// Mirrors ClaimShield.Api/Constants/RoleConstants.cs. UI-routing
// convenience only - the API re-derives and enforces the real role
// from public.profiles on every request regardless of what the
// frontend believes.

export const RoleId = {
  Customer: 1,
  Repairer: 2,
  Surveyor: 3,
  Approver: 4,
  Admin: 5,
} as const

export type RoleIdValue = (typeof RoleId)[keyof typeof RoleId]

export const RoleName: Record<RoleIdValue, string> = {
  [RoleId.Customer]: 'Customer',
  [RoleId.Repairer]: 'Repairer',
  [RoleId.Surveyor]: 'Surveyor',
  [RoleId.Approver]: 'Approver',
  [RoleId.Admin]: 'Admin',
}
