import React from 'react';
import type { TOptions } from 'i18next';
import { useTranslation as useI18nextTranslation } from 'react-i18next';
import {
    changeLanguage,
    isSupportedLanguage,
    type SupportedLanguage,
} from 'src/lib/translations/i18n';
import type { TranslationKey } from 'src/lib/translations/translationKeys';

interface ITranslation {
    /** Current UI language. */
    language: SupportedLanguage;
    /** Text for a resource key; values fill placeholders such as {{name}}. Typos in keys are compile errors. */
    getResource: (key: TranslationKey, values?: TOptions) => string;
    /** Switches the UI language and remembers the choice on this device. */
    toggleLanguage: (language: SupportedLanguage) => void;
}

/**
 * The only way components get texts (AGENTS.md §7). Wraps react-i18next, so the library stays
 * replaceable and every component re-renders when the language changes.
 */
export const useTranslation = (): ITranslation => {
    const { t, i18n } = useI18nextTranslation();

    const getResource = React.useCallback(
        (key: TranslationKey, values?: TOptions): string => (values ? t(key, values) : t(key)),
        [t],
    );

    const toggleLanguage = React.useCallback((language: SupportedLanguage) => {
        void changeLanguage(language);
    }, []);

    const language = isSupportedLanguage(i18n.resolvedLanguage) ? i18n.resolvedLanguage : 'de';

    return { language, getResource, toggleLanguage };
};
