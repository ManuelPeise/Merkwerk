import { createBrowserRouter, Navigate } from 'react-router-dom';
import AdminLayout from 'src/components/layout/AdminLayout';
import KidsLayout from 'src/components/layout/KidsLayout';
import PublicLayout from 'src/components/layout/PublicLayout';
import { adultRoles, roles } from 'src/lib/auth/roles';
import ProtectedRoute from 'src/navigation/ProtectedRoute';
import PublicRoute from 'src/navigation/PublicRoute';
import { routes } from 'src/navigation/routes';
import AdminPage from 'src/pages/adminPage/AdminPage';
import ErrorPage from 'src/pages/errorPage/ErrorPage';
import LandingPage from 'src/pages/landingPage/LandingPage';
import LoginPage from 'src/pages/loginPage/LoginPage';
import PracticeHomePage from 'src/pages/practiceHomePage/PracticeHomePage';

// Every route belongs to exactly one layout. Pages marked LP-124 / LP-125 get their routes with those tickets.
export const router = createBrowserRouter([
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
                        children: [{ path: routes.login, element: <LoginPage /> }],
                    },
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
                        element: <ProtectedRoute roles={[roles.learner]} />,
                        children: [{ path: routes.practice, element: <PracticeHomePage /> }],
                    },
                ],
            },
            { path: '*', element: <Navigate to={routes.start} replace /> },
        ],
    },
]);
