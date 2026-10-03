import React from 'react';
import { Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';

/** Placeholder for the children's area � only reachable for learners (ProtectedRoute). */
const PracticeHomePage: React.FC = () => {
    const { getResource } = useTranslation();

    return <Typography variant="h1">{getResource('captionPracticeHome')}</Typography>;
};

export default PracticeHomePage;
