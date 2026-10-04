import React from 'react';
import { Typography } from '@mui/material';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useTranslation } from 'src/hooks/useTranslation';
import { testIds } from 'src/lib/testing/testIds';

/** Placeholder for the children's area until LP-117 – only reachable for learners. */
const PracticeHomePage: React.FC = () => {
    const { getResource } = useTranslation();
    const { user } = useAuthentication();

    return (
        <Typography variant="h1" data-testid={testIds.practice.home}>
            {getResource('captionPracticeGreeting', { name: user?.name ?? '' })}
        </Typography>
    );
};

export default PracticeHomePage;
