import { createTheme } from '@mui/material';

/** Subject colors (LP-109) – the same values as color.subject.* in shared/design-tokens/tokens.json. */
export interface ISubjectPalette {
    german: string;
    english: string;
    math: string;
    purple: string;
    orange: string;
    magenta: string;
    slate: string;
    olive: string;
}

declare module '@mui/material/styles' {
    interface Palette {
        subject: ISubjectPalette;
    }

    interface PaletteOptions {
        subject?: ISubjectPalette;
    }
}

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
        // White icons and text on every subject color have at least 4.5:1 (checked in Architecture.Tests).
        subject: {
            german: '#C0504D',
            english: '#3A75A8',
            math: '#3B7F49',
            purple: '#7A4FA3',
            orange: '#A3560F',
            magenta: '#A8336C',
            slate: '#51606E',
            olive: '#5E6B1E',
        },
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
