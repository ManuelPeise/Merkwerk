import React from 'react';
import {
    Alert,
    Box,
    Card,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Stack,
    Typography,
} from '@mui/material';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { EditIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import SubjectBadge from 'src/components/subjects/SubjectBadge';
import { useIsOrgAdmin } from 'src/hooks/useIsOrgAdmin';
import { useTranslation } from 'src/hooks/useTranslation';
import { subjectsApi } from 'src/lib/api/subjects/subjectsApi';
import type { ISubject } from 'src/lib/api/subjects/subjectsTypes';
import { getSubjectLanguageLabel } from 'src/lib/subjects/subjectStyles';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import SubjectDialog from 'src/pages/subjectsPage/components/SubjectDialog';

type DialogState = { kind: 'closed' } | { kind: 'edit'; subject?: ISubject };

/** /admin/subjects – subjects with color and icon (LP-109). Every adult sees them, admins create and change them. */
const SubjectsPage: React.FC = () => {
    const { getResource } = useTranslation();
    const isAdmin = useIsOrgAdmin();

    const [subjects, setSubjects] = React.useState<ISubject[] | null>(null);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [dialog, setDialog] = React.useState<DialogState>({ kind: 'closed' });
    // Incremented to reload the list after a change.
    const [reloadKey, setReloadKey] = React.useState(0);

    React.useEffect(() => {
        const controller = new AbortController();

        void subjectsApi.list.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setSubjects(result.data ?? []);
            setErrorKey(result.error ? result.error.messageKey : null);
        });

        return () => controller.abort();
    }, [reloadKey]);

    const handleSaved = () => {
        setDialog({ kind: 'closed' });
        setReloadKey((key) => key + 1);
    };

    const languageOf = (subject: ISubject): string => {
        const labelKey = getSubjectLanguageLabel(subject.languageCode);
        return getResource('captionSubjectLanguage', {
            language: labelKey ? getResource(labelKey) : subject.languageCode,
        });
    };

    return (
        <Stack spacing={4}>
            <Box
                sx={{
                    display: 'flex',
                    alignItems: 'flex-start',
                    justifyContent: 'space-between',
                    gap: 2,
                }}
            >
                <Stack spacing={1}>
                    <Typography variant="h1">{getResource('captionSubjects')}</Typography>
                    <Typography color="text.secondary">
                        {getResource('captionSubjectsDescription')}
                    </Typography>
                </Stack>
                {isAdmin && (
                    <FormButton
                        label={getResource('labelAddSubject')}
                        onClick={() => setDialog({ kind: 'edit' })}
                    />
                )}
            </Box>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            {subjects === null ? (
                <LoadingIndicator />
            ) : subjects.length === 0 ? (
                <Typography color="text.secondary">{getResource('captionNoSubjects')}</Typography>
            ) : (
                <Card>
                    <List>
                        {subjects.map((subject) => (
                            <ListItem
                                key={subject.id}
                                secondaryAction={
                                    isAdmin && (
                                        <IconButton
                                            aria-label={getResource('labelEditSubject', {
                                                name: subject.name,
                                            })}
                                            onClick={() => setDialog({ kind: 'edit', subject })}
                                        >
                                            <EditIcon />
                                        </IconButton>
                                    )
                                }
                            >
                                <ListItemText
                                    disableTypography
                                    primary={<SubjectBadge subject={subject} />}
                                    secondary={
                                        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                                            {languageOf(subject)}
                                        </Typography>
                                    }
                                />
                            </ListItem>
                        ))}
                    </List>
                </Card>
            )}

            {dialog.kind === 'edit' && (
                <SubjectDialog
                    open
                    subject={dialog.subject}
                    onClose={() => setDialog({ kind: 'closed' })}
                    onSaved={handleSaved}
                />
            )}
        </Stack>
    );
};

export default SubjectsPage;
