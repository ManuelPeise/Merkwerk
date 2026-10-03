import { statelessApi } from 'src/lib/api/StatelessApi';
import type { ISession } from 'src/lib/api/authentication/authenticationTypes';
import type {
    IInvitationAcceptRequest,
    IInvitationDetails,
} from 'src/lib/api/invitations/invitationsTypes';

/** Endpoints of InvitationsController. `details` takes the token as query parameter (params: { token }). */
export const invitationsApi = {
    details: statelessApi.create<IInvitationDetails>({ serviceUrl: '/invitations/details' }),
    accept: statelessApi.create<ISession, IInvitationAcceptRequest>({
        serviceUrl: '/invitations/accept',
    }),
};
