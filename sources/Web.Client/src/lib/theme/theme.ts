import { createTheme } from '@mui/material';

/**
 * Design direction A "Ruhiger Lernraum" (LP-008).
 * Andika must be self-hosted (no font CDN in the children's client); until the woff2 files are
 * in place the browser falls back to the system font.
 */
export const theme = createTheme({
    palette: {
        mode: 'light',
        primary: { main: '#1F6F78', dark: '#174F56', light: '#DCEBEC', contrastText: '#FFFFFF' },
        secondary: { main: '#3E7CB1', contrastText: '#FFFFFF' },
        success: { main: '#1E6B43', light: '#E6F4EC', contrastText: '#FFFFFF' },
        warning: { main: '#8A4B0F', light: '#FFF1E0', contrastText: '#FFFFFF' },
        error: { main: '#C0504D', contrastText: '#FFFFFF' },
        background: { default: '#FAF8F4', paper: '#FFFFFF' },
        text: { primary: '#1E2328', secondary: '#5B636B' },
        divider: '#E3DED5',
    },
    shape: { borderRadius: 16 },
    typography: {
        fontFamily: "Andika, 'Segoe UI', system-ui, sans-serif",
        fontSize: 16,
        h1: { fontSize: '2.5rem', fontWeight: 700 },
        h2: { fontSize: '1.75rem', fontWeight: 700 },
        button: { textTransform: 'none', fontWeight: 700 },
    },
    components: {
        // One look for every form field (see FormFieldContainer): label above, white outlined input,
        // 64 px high (touch target for children). Spacing between fields comes from the form (Stack spacing).
        MuiFormLabel: {
            styleOverrides: { root: { fontWeight: 700, color: '#1E2328', fontSize: '1rem' } },
        },
        MuiOutlinedInput: {
            styleOverrides: {
                root: { minHeight: 64, fontSize: '1.125rem', backgroundColor: '#FFFFFF' },
            },
        },
        MuiFormHelperText: {
            styleOverrides: { root: { marginLeft: 0, fontSize: '0.9375rem' } },
        },
        MuiTextField: {
            defaultProps: { variant: 'outlined', fullWidth: true },
        },
        MuiButton: {
            defaultProps: { disableElevation: true },
            // Large touch targets for children (LP-007).
            styleOverrides: { root: { minHeight: 56, paddingInline: 24, fontSize: '1.125rem' } },
        },
        // Flat surfaces. No border here: AppBar, Drawer, Alert and Menu are Papers too.
        MuiPaper: {
            defaultProps: { elevation: 0 },
        },
        // Cards (task tiles etc.) get the outline of design direction A.
        MuiCard: {
            defaultProps: { variant: 'outlined' },
            styleOverrides: { root: { border: '2px solid #E3DED5', borderRadius: 20 } },
        },
    },
});
