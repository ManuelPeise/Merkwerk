import { createBrowserRouter, Navigate } from 'react-router-dom';
import AppLayout from 'src/components/layout/AppLayout';
import ProtectedRoute from 'src/navigation/ProtectedRoute';
import PublicRoute from 'src/navigation/PublicRoute';
import { routes } from 'src/navigation/routes';
import AdminPage from 'src/pages/adminPage/AdminPage';
import ErrorPage from 'src/pages/errorPage/ErrorPage';
import LandingPage from 'src/pages/landingPage/LandingPage';
import LoginPage from 'src/pages/loginPage/LoginPage';

export const router = createBrowserRouter([
    {
        path: routes.start,
        element: <AppLayout />,
        // Any error while loading or rendering a page shows a friendly page instead of React Router's default.
        errorElement: <ErrorPage />,
        children: [
            // Everyone.
            { index: true, element: <LandingPage /> },

            // Only when NOT signed in.
            {
                element: <PublicRoute />,
                children: [{ path: routes.login, element: <LoginPage /> }],
            },

            // Only when signed in (optionally restricted: <ProtectedRoute roles={['Member']} />).
            {
                element: <ProtectedRoute />,
                children: [{ path: routes.admin, element: <AdminPage /> }],
            },

            { path: '*', element: <Navigate to={routes.start} replace /> },
        ],
    },
]);
