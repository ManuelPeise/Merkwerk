import React from 'react';
import { Stack, Typography } from '@mui/material';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useTranslation } from 'src/hooks/useTranslation';
import { roles } from 'src/lib/auth/roles';
import AdultsSection from 'src/pages/familyPage/components/AdultsSection';
import LearnersSection from 'src/pages/familyPage/components/LearnersSection';

/** /admin/family – children and adults of the family (LP-105). Everyone sees it, only admins change it. */
const FamilyPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { user } = useAuthentication();
    const isAdmin = user?.role === roles.orgAdmin;

    return (
        <Stack spacing={5}>
            <Typography variant="h1">{getResource('captionFamily')}</Typography>
            <LearnersSection isAdmin={isAdmin} />
            <AdultsSection isAdmin={isAdmin} />
        </Stack>
    );
};

export default FamilyPage;
