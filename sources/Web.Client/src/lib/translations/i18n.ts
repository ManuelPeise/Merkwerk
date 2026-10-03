import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import commonDe from 'src/lib/translations/resources/de/common.de.json';
import commonEn from 'src/lib/translations/resources/en/common.en.json';

export const defaultNamespace = 'common';

export const supportedLanguages = ['de', 'en'] as const;

export type SupportedLanguage = (typeof supportedLanguages)[number];

export const resources = {
    de: { common: commonDe },
    en: { common: commonEn },
} as const;

/** UI preference only – no personal data (AGENTS.md §10). */
const languageStorageKey = 'merkwerk.language';

export const isSupportedLanguage = (value: string | null | undefined): value is SupportedLanguage =>
    (supportedLanguages as readonly string[]).includes(value ?? '');

const readStoredLanguage = (): SupportedLanguage | null => {
    try {
        const stored = localStorage.getItem(languageStorageKey);
        return isSupportedLanguage(stored) ? stored : null;
    } catch {
        // Storage can be blocked (private mode); fall back to the browser language.
        return null;
    }
};

const detectLanguage = (): SupportedLanguage =>
    readStoredLanguage() ?? (navigator.language.toLowerCase().startsWith('en') ? 'en' : 'de');

/** Switches the UI language, remembers it on this device and updates <html lang> (screen readers). */
export const changeLanguage = async (language: SupportedLanguage): Promise<void> => {
    try {
        localStorage.setItem(languageStorageKey, language);
    } catch {
        // Not remembered – still switch for this session.
    }

    document.documentElement.lang = language;
    await i18n.changeLanguage(language);
};

const initialLanguage = detectLanguage();
document.documentElement.lang = initialLanguage;

void i18n.use(initReactI18next).init({
    resources,
    lng: initialLanguage,
    fallbackLng: 'de',
    supportedLngs: supportedLanguages,
    ns: [defaultNamespace],
    defaultNS: defaultNamespace,
    // React already escapes output.
    interpolation: { escapeValue: false },
});

export default i18n;
