import { createBrowserRouter, Navigate } from 'react-router-dom';
import AdminLayout from 'src/components/layout/AdminLayout';
import KidsLayout from 'src/components/layout/KidsLayout';
import PublicLayout from 'src/components/layout/PublicLayout';
import { adultRoles, roles } from 'src/lib/auth/roles';
import DeviceRoute from 'src/navigation/DeviceRoute';
import SetupGate from 'src/navigation/SetupGate';
import ProtectedRoute from 'src/navigation/ProtectedRoute';
import PublicRoute from 'src/navigation/PublicRoute';
import { routes } from 'src/navigation/routes';
import AdminPage from 'src/pages/adminPage/AdminPage';
import ConfirmEmailPage from 'src/pages/authentication/confirmEmailPage/ConfirmEmailPage';
import ErrorPage from 'src/pages/errorPage/ErrorPage';
import ForgotPasswordPage from 'src/pages/authentication/forgotPasswordPage/ForgotPasswordPage';
import InvitationPage from 'src/pages/authentication/invitationPage/InvitationPage';
import LandingPage from 'src/pages/landingPage/LandingPage';
import LoginPage from 'src/pages/authentication/loginPage/LoginPage';
import ProfilesPage from 'src/pages/profilesPage/ProfilesPage';
import ResetPasswordPage from 'src/pages/authentication/resetPasswordPage/ResetPasswordPage';
import SetupPage from 'src/pages/authentication/setupPage/SetupPage';
import PairDevicePage from 'src/pages/pairDevicePage/PairDevicePage';
import PracticeHomePage from 'src/pages/practiceHomePage/PracticeHomePage';

// Every route belongs to exactly one layout. Pages marked LP-124 / LP-125 get their routes with those tickets.
export const router = createBrowserRouter([
    {
        // Outermost: redirects to /setup until the instance is set up.
        element: <SetupGate />,
        errorElement: <ErrorPage />,
        children: [
            {
                path: routes.start,
                // Any error while loading or rendering a page shows a friendly page instead of React Router's default.
                errorElement: <ErrorPage />,
                children: [
                    {
                        // Slim header with language switch, centred content, no menu.
                        element: <PublicLayout />,
                        children: [
                            // Everyone.
                            { index: true, element: <LandingPage /> },

                            // Only when NOT signed in.
                            {
                                element: <PublicRoute />,
                                children: [
                                    { path: routes.login, element: <LoginPage /> },
                                    {
                                        path: routes.forgotPassword,
                                        element: <ForgotPasswordPage />,
                                    },
                                    { path: routes.resetPassword, element: <ResetPasswordPage /> },
                                ],
                            },
                            { path: routes.setup, element: <SetupPage /> },
                            { path: routes.invitation, element: <InvitationPage /> },
                            { path: routes.confirmEmail, element: <ConfirmEmailPage /> },
                            { path: routes.practicePair, element: <PairDevicePage /> },
                        ],
                    },
                    {
                        // Parents' area: header bar and navigation drawer.
                        element: <ProtectedRoute roles={adultRoles} />,
                        children: [
                            {
                                element: <AdminLayout />,
                                children: [{ path: routes.admin, element: <AdminPage /> }],
                            },
                        ],
                    },
                    {
                        // Children's area: large header, no menu.
                        element: <KidsLayout />,
                        children: [
                            {
                                // Unpaired devices go to "pair device" first.
                                element: <DeviceRoute />,
                                children: [
                                    { path: routes.practiceProfiles, element: <ProfilesPage /> },
                                    {
                                        // No child signed in (or session expired): back to the profile selection.
                                        element: (
                                            <ProtectedRoute
                                                roles={[roles.learner]}
                                                signedOutTo={routes.practiceProfiles}
                                            />
                                        ),
                                        children: [
                                            {
                                                path: routes.practice,
                                                element: <PracticeHomePage />,
                                            },
                                        ],
                                    },
                                ],
                            },
                        ],
                    },
                    { path: '*', element: <Navigate to={routes.start} replace /> },
                ],
            },
        ],
    },
]);
