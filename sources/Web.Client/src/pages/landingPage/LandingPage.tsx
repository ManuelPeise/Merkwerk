import React from 'react';
import { Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';

/** Start page for everyone. Placeholder until the role choice (child / parent) is designed. */
const LandingPage: React.FC = () => {
    const { getResource } = useTranslation();

    return <Typography variant="h1">{getResource('captionStartPage')}</Typography>;
};

export default LandingPage;
