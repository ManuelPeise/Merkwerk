import { useAuthentication } from 'src/hooks/useAuthentication';
import { roles } from 'src/lib/auth/roles';

/**
 * True for admins of the family (owner included). Only hides actions in the UI - the server checks every request
 * (LP-107, permission matrix in AGENTS.md).
 */
export const useIsOrgAdmin = (): boolean => {
    const { user } = useAuthentication();
    return user?.role === roles.orgAdmin;
};
