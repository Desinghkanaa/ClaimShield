using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimShield.Api.Migrations
{
    /// <inheritdoc />
    public partial class SupabaseAuthIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // -----------------------------------------------------
            // public.profiles.id is the same UUID as auth.users.id,
            // set by the trigger below - not expressible as a
            // normal EF relationship since auth.users isn't part
            // of this DbContext's model.
            // -----------------------------------------------------

            migrationBuilder.Sql(@"
                ALTER TABLE public.profiles
                    ADD CONSTRAINT profiles_id_fkey
                    FOREIGN KEY (id)
                    REFERENCES auth.users (id)
                    ON DELETE CASCADE;
            ");

            // -----------------------------------------------------
            // Auto-creates the public.profiles row whenever
            // Supabase Auth inserts a new auth.users row (e.g. via
            // the Admin API in SupabaseAdminService). RoleId/
            // FirstName/LastName/PhoneNumber come from the
            // user_metadata supplied at creation time.
            // -----------------------------------------------------

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION public.handle_new_auth_user()
                RETURNS trigger
                LANGUAGE plpgsql
                SECURITY DEFINER SET search_path = public
                AS $$
                BEGIN
                  INSERT INTO public.profiles (id, email, first_name, last_name, phone_number, role_id, is_active)
                  VALUES (
                    NEW.id,
                    NEW.email,
                    COALESCE(NEW.raw_user_meta_data->>'first_name', ''),
                    NEW.raw_user_meta_data->>'last_name',
                    NEW.raw_user_meta_data->>'phone_number',
                    COALESCE((NEW.raw_user_meta_data->>'role_id')::int, 1),
                    true
                  );
                  RETURN NEW;
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;

                CREATE TRIGGER on_auth_user_created
                    AFTER INSERT ON auth.users
                    FOR EACH ROW EXECUTE FUNCTION public.handle_new_auth_user();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;");

            migrationBuilder.Sql(
                "DROP FUNCTION IF EXISTS public.handle_new_auth_user();");

            migrationBuilder.Sql(
                "ALTER TABLE public.profiles DROP CONSTRAINT IF EXISTS profiles_id_fkey;");
        }
    }
}
