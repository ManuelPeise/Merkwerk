import React from 'react';
import { Alert, Box, Button, Paper, Stack, Typography } from '@mui/material';
import { RefreshIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';
import { exercisesApi } from 'src/lib/api/exercises/exercisesApi';
import type { IArithmeticSettings, IGeneratorPreview } from 'src/lib/api/exercises/exercisesTypes';
import { validateArithmeticSettings } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

interface IProps {
    settings: IArithmeticSettings;
    uiTestId: string;
}

/** Waits a moment after the last change, so typing a number range does not send a request per digit. */
const previewDelayMs = 300;

/**
 * Example tasks from the server's generator (POST exercises/generate-preview, LP-131). Children get other numbers
 * in every attempt - "New tasks" shows another example.
 */
const GeneratorPreview: React.FC<IProps> = (props) => {
    const { settings, uiTestId } = props;
    const { getResource } = useTranslation();

    const [preview, setPreview] = React.useState<IGeneratorPreview | null>(null);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [round, setRound] = React.useState(0);

    const settingsJson = JSON.stringify(settings);
    const isValid = validateArithmeticSettings(settings).length === 0;

    React.useEffect(() => {
        const generator = JSON.parse(settingsJson) as IArithmeticSettings;
        if (validateArithmeticSettings(generator).length > 0) {
            return;
        }

        const controller = new AbortController();
        const timer = window.setTimeout(() => {
            void exercisesApi.generatePreview
                .post({ body: { generator }, signal: controller.signal })
                .then((result) => {
                    if (result.error?.kind === 'canceled') {
                        return;
                    }

                    setPreview(result.data ?? null);
                    setErrorKey(result.error ? result.error.messageKey : null);
                });
        }, previewDelayMs);

        return () => {
            window.clearTimeout(timer);
            controller.abort();
        };
    }, [settingsJson, round]);

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 2 }}>
                <Typography variant="h2" sx={{ flexGrow: 1 }}>
                    {getResource('captionGeneratorPreview')}
                </Typography>
                <Button
                    variant="outlined"
                    startIcon={<RefreshIcon />}
                    disabled={!isValid}
                    onClick={() => setRound(round + 1)}
                >
                    {getResource('labelNewTasks')}
                </Button>
            </Box>
            <Typography color="text.secondary">
                {getResource('captionGeneratorPreviewHint')}
            </Typography>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            {isValid && preview && (
                <Box
                    data-testid={uiTestIdOf(uiTestId, 'tasks')}
                    sx={{
                        display: 'grid',
                        gap: 1.5,
                        gridTemplateColumns: 'repeat(auto-fill, minmax(160px, 1fr))',
                    }}
                >
                    {preview.questions.map((question, index) => (
                        <Paper
                            key={index}
                            variant="outlined"
                            sx={{
                                minHeight: 64,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                fontSize: '1.5rem',
                                fontWeight: 700,
                                borderRadius: 3,
                            }}
                        >
                            {question.payload.prompt}
                        </Paper>
                    ))}
                </Box>
            )}
        </Stack>
    );
};

export default GeneratorPreview;
