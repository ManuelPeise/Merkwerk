import React from 'react';
import { Stack, Typography } from '@mui/material';
import { useIsOrgAdmin } from 'src/hooks/useIsOrgAdmin';
import { useTranslation } from 'src/hooks/useTranslation';
import { testIds } from 'src/lib/testing/testIds';
import AdultsSection from 'src/pages/familyPage/components/AdultsSection';
import GroupsSection from 'src/pages/familyPage/components/GroupsSection';
import LearnersSection from 'src/pages/familyPage/components/LearnersSection';

/** /admin/family – children, groups and adults of the family (LP-105, LP-108). Everyone sees it, only admins change it. */
const FamilyPage: React.FC = () => {
    const { getResource } = useTranslation();
    const isAdmin = useIsOrgAdmin();
    // Bumped when children change, so the groups show current names and avatars.
    const [learnersVersion, setLearnersVersion] = React.useState(0);

    return (
        <Stack spacing={5} data-testid={testIds.family.page}>
            <Typography variant="h1">{getResource('captionFamily')}</Typography>
            <LearnersSection
                isAdmin={isAdmin}
                onChanged={() => setLearnersVersion((version) => version + 1)}
            />
            <GroupsSection isAdmin={isAdmin} learnersVersion={learnersVersion} />
            <AdultsSection isAdmin={isAdmin} />
        </Stack>
    );
};

export default FamilyPage;
