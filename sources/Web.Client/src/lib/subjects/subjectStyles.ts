import { createElement, type ComponentType, type ReactElement } from 'react';
import type { SvgIconProps } from '@mui/material';
import {
    SubjectArtIcon,
    SubjectBookIcon,
    SubjectEnglishIcon,
    SubjectGermanIcon,
    SubjectGlobeIcon,
    SubjectLanguageIcon,
    SubjectMathIcon,
    SubjectMusicIcon,
    SubjectPuzzleIcon,
    SubjectScienceIcon,
    SubjectSportIcon,
    SubjectStarIcon,
} from 'src/components/icons/AppIcons';
import type { LabelKey } from 'src/lib/translations/translationKeys';

/**
 * The fixed choice for subjects (LP-109) – mirrors Logic.Content/Subjects/SubjectRules.cs. Color keys are palette
 * paths of the theme (`theme.palette.subject.*`), so `sx={{ bgcolor: subject.color }}` works directly.
 */
export const subjectColors: readonly { key: string; labelKey: LabelKey }[] = [
    { key: 'subject.german', labelKey: 'labelSubjectColorRed' },
    { key: 'subject.english', labelKey: 'labelSubjectColorBlue' },
    { key: 'subject.math', labelKey: 'labelSubjectColorGreen' },
    { key: 'subject.purple', labelKey: 'labelSubjectColorPurple' },
    { key: 'subject.orange', labelKey: 'labelSubjectColorOrange' },
    { key: 'subject.magenta', labelKey: 'labelSubjectColorMagenta' },
    { key: 'subject.slate', labelKey: 'labelSubjectColorSlate' },
    { key: 'subject.olive', labelKey: 'labelSubjectColorOlive' },
];

export const subjectIcons: readonly {
    key: string;
    icon: ComponentType<SvgIconProps>;
    labelKey: LabelKey;
}[] = [
    { key: 'german', icon: SubjectGermanIcon, labelKey: 'labelSubjectIconGerman' },
    { key: 'english', icon: SubjectEnglishIcon, labelKey: 'labelSubjectIconEnglish' },
    { key: 'math', icon: SubjectMathIcon, labelKey: 'labelSubjectIconMath' },
    { key: 'book', icon: SubjectBookIcon, labelKey: 'labelSubjectIconBook' },
    { key: 'music', icon: SubjectMusicIcon, labelKey: 'labelSubjectIconMusic' },
    { key: 'science', icon: SubjectScienceIcon, labelKey: 'labelSubjectIconScience' },
    { key: 'art', icon: SubjectArtIcon, labelKey: 'labelSubjectIconArt' },
    { key: 'sport', icon: SubjectSportIcon, labelKey: 'labelSubjectIconSport' },
    { key: 'globe', icon: SubjectGlobeIcon, labelKey: 'labelSubjectIconGlobe' },
    { key: 'language', icon: SubjectLanguageIcon, labelKey: 'labelSubjectIconLanguage' },
    { key: 'puzzle', icon: SubjectPuzzleIcon, labelKey: 'labelSubjectIconPuzzle' },
    { key: 'star', icon: SubjectStarIcon, labelKey: 'labelSubjectIconStar' },
];

export const subjectLanguages: readonly { code: string; labelKey: LabelKey }[] = [
    { code: 'de', labelKey: 'labelSubjectLanguageDe' },
    { code: 'en', labelKey: 'labelSubjectLanguageEn' },
    { code: 'fr', labelKey: 'labelSubjectLanguageFr' },
    { code: 'es', labelKey: 'labelSubjectLanguageEs' },
    { code: 'it', labelKey: 'labelSubjectLanguageIt' },
];

/** Icon component for an icon key; unknown keys (newer server) get the star. */
const getSubjectIcon = (key: string): ComponentType<SvgIconProps> =>
    subjectIcons.find((entry) => entry.key === key)?.icon ?? SubjectStarIcon;

/**
 * The icon element for an icon key. Returns an element, not a component: picking a component during render would
 * recreate it on every render (react-hooks/static-components).
 */
export const renderSubjectIcon = (key: string): ReactElement => createElement(getSubjectIcon(key));

/** Theme palette path for a color key; unknown keys fall back to the secondary text color. */
export const getSubjectColor = (key: string): string =>
    subjectColors.some((entry) => entry.key === key) ? key : 'text.secondary';

/** Translation key for a language code, if it is one of ours. */
export const getSubjectLanguageLabel = (code: string): LabelKey | undefined =>
    subjectLanguages.find((entry) => entry.code === code)?.labelKey;
