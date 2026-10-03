import type { FC } from 'react';
import { CssBaseline, ThemeProvider } from '@mui/material';
import { RouterProvider } from 'react-router-dom';
import { AuthenticationProvider } from 'src/components/providers/AuthenticationContextProvider';
import { theme } from 'src/lib/theme/theme';
import { router } from 'src/navigation/router';

const App: FC = () => (
    <ThemeProvider theme={theme}>
        <CssBaseline />
        <AuthenticationProvider>
            <RouterProvider router={router} />
        </AuthenticationProvider>
    </ThemeProvider>
);

export default App;
