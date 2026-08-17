import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react'
import type { Session } from '@supabase/supabase-js'
import { supabase } from '../lib/supabaseClient'
import type { RoleIdValue } from '../lib/roles'
import { RoleId, RoleName } from '../lib/roles'

interface AuthContextValue {
  session: Session | null
  loading: boolean
  roleId: RoleIdValue | null
  roleName: string | null
  displayName: string
  // True for every role except Customer, who must additionally verify a
  // second, app-level OTP after password login (Phase 12) before the
  // app shell renders for them. Backed by sessionStorage so a page
  // refresh mid-session doesn't force re-verification, but a fresh
  // sign-in (different user id) always starts unverified again.
  otpVerified: boolean
  markOtpVerified: () => void
  signIn: (email: string, password: string) => Promise<{ error: string | null }>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

const OTP_VERIFIED_KEY = 'claimshield_otp_verified_user_id'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null)
  const [loading, setLoading] = useState(true)
  const [otpVerifiedUserId, setOtpVerifiedUserId] = useState<string | null>(
    () => sessionStorage.getItem(OTP_VERIFIED_KEY),
  )

  useEffect(() => {
    supabase.auth.getSession().then(({ data }) => {
      setSession(data.session)
      setLoading(false)
    })

    const { data: subscription } = supabase.auth.onAuthStateChange(
      (_event, newSession) => {
        setSession(newSession)
      },
    )

    return () => subscription.subscription.unsubscribe()
  }, [])

  const roleId =
    (session?.user.user_metadata?.role_id as RoleIdValue | undefined) ?? null

  const otpVerified =
    roleId !== RoleId.Customer ||
    (session != null && otpVerifiedUserId === session.user.id)

  const value: AuthContextValue = {
    session,
    loading,
    roleId,
    roleName: roleId ? RoleName[roleId] : null,
    displayName:
      [
        session?.user.user_metadata?.first_name,
        session?.user.user_metadata?.last_name,
      ]
        .filter(Boolean)
        .join(' ') ||
      session?.user.email ||
      'User',
    otpVerified,
    markOtpVerified: () => {
      if (session) {
        sessionStorage.setItem(OTP_VERIFIED_KEY, session.user.id)
        setOtpVerifiedUserId(session.user.id)
      }
    },
    signIn: async (email, password) => {
      const { error } = await supabase.auth.signInWithPassword({
        email,
        password,
      })
      return { error: error?.message ?? null }
    },
    signOut: async () => {
      sessionStorage.removeItem(OTP_VERIFIED_KEY)
      setOtpVerifiedUserId(null)
      await supabase.auth.signOut()
    },
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
