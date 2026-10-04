import commonDe from '../../src/lib/translations/resources/de/common.de.json' with { type: 'json' };

type Dictionary = typeof commonDe;
type TranslationKey = keyof Dictionary;

const placeholderRegex = /\{\{\s*(\w+)\s*\}\}/g;

export const t = (key: TranslationKey, values?: Record<string, string | number>): string => {
    const template = commonDe[key];

    if (!values) {
        return template;
    }

    return template.replace(placeholderRegex, (_full, placeholder: string) => {
        const value = values[placeholder];
        return value === undefined ? `{{${placeholder}}}` : String(value);
    });
};
