import React from 'react';
import { ToggleButton, ToggleButtonGroup } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { isSupportedLanguage } from 'src/lib/translations/i18n';

const LanguageSwitch: React.FC = () => {
    const { language, getResource, toggleLanguage } = useTranslation();

    const handleChange = (_event: React.MouseEvent<HTMLElement>, value: string | null) => {
        if (isSupportedLanguage(value)) {
            toggleLanguage(value);
        }
    };

    return (
        <ToggleButtonGroup
            exclusive
            size="small"
            value={language}
            onChange={handleChange}
            aria-label={getResource('labelLanguage')}
        >
            <ToggleButton
                value="de"
                aria-label={getResource('labelLanguageGerman')}
                sx={{ minHeight: 48 }}
            >
                DE
            </ToggleButton>
            <ToggleButton
                value="en"
                aria-label={getResource('labelLanguageEnglish')}
                sx={{ minHeight: 48 }}
            >
                EN
            </ToggleButton>
        </ToggleButtonGroup>
    );
};

export default LanguageSwitch;
