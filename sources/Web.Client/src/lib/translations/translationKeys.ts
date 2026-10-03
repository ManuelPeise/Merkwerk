import type commonDe from 'src/lib/translations/resources/de/common.de.json';

/** All keys of the default namespace (German is the source language). */
export type TranslationKey = keyof typeof commonDe;

export type CaptionKey = Extract<TranslationKey, `caption${string}`>;

export type LabelKey = Extract<TranslationKey, `label${string}`>;

export type NotificationKey = Extract<TranslationKey, `notification${string}`>;
