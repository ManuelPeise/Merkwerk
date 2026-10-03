import 'i18next';
import type commonDe from 'src/lib/translations/resources/de/common.de.json';

// Typed keys: t('captionWelcome') is checked against the German resources (source language).
declare module 'i18next' {
    interface CustomTypeOptions {
        defaultNS: 'common';
        resources: {
            common: typeof commonDe;
        };
    }
}
