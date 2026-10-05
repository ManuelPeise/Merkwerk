import React from 'react';
import { Box, Paper, Stack, Typography } from '@mui/material';
import NumberKeypad from 'src/components/kids/NumberKeypad';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IQuestion } from 'src/lib/api/exercises/exercisesTypes';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    question: IQuestion;
    uiTestId: string;
}

/** Large tile like the ones children tap (>= 64 px high). */
const tileSx = {
    minHeight: 64,
    px: 2,
    py: 1.5,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    textAlign: 'center',
    fontSize: '1.5rem',
    fontWeight: 700,
    borderRadius: 3,
} as const;

/** Empty answer box in the text flow. */
const gapSx = {
    display: 'inline-block',
    minWidth: 96,
    height: '1.6em',
    mx: 1,
    verticalAlign: 'bottom',
    border: '2px dashed',
    borderColor: 'primary.main',
    borderRadius: 2,
} as const;

const noop = () => undefined;

/**
 * How the child will see the question (LP-112): large text, tiles, gaps, keypad - without interaction and
 * without solutions. The interactive player (LP-118) replaces this.
 */
const QuestionPreview: React.FC<IProps> = (props) => {
    const { question, uiTestId } = props;
    const { getResource } = useTranslation();
    const { payload } = question;

    const renderAnswerArea = () => {
        switch (payload.type) {
            case 'choice':
                return (
                    <Box
                        sx={{
                            display: 'grid',
                            gap: 2,
                            gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, minmax(0, 1fr))' },
                        }}
                    >
                        {payload.options.map((option, index) => (
                            <Paper key={index} variant="outlined" sx={tileSx}>
                                {option || '…'}
                            </Paper>
                        ))}
                    </Box>
                );
            case 'text':
                return payload.inputKind === 'number' ? (
                    <Stack spacing={2} sx={{ alignItems: 'center' }}>
                        <Paper variant="outlined" sx={{ ...tileSx, minWidth: 160 }}>
                            ?
                        </Paper>
                        <NumberKeypad
                            value=""
                            disabled
                            uiTestId={uiTestIdOf(uiTestId, 'keypad')}
                            onChange={noop}
                            onSubmit={noop}
                        />
                    </Stack>
                ) : (
                    <Paper variant="outlined" sx={{ ...tileSx, justifyContent: 'flex-start' }}>
                        <Box component="span" sx={{ ...gapSx, mx: 0, flexGrow: 1 }} />
                    </Paper>
                );
            case 'cloze':
                return (
                    <Typography sx={{ fontSize: '1.5rem', lineHeight: 2 }}>
                        {payload.parts.map((part, index) => (
                            <React.Fragment key={index}>
                                {part}
                                {index < payload.parts.length - 1 && (
                                    <Box component="span" sx={gapSx} />
                                )}
                            </React.Fragment>
                        ))}
                    </Typography>
                );
            case 'match': {
                // The player shuffles the right side; the preview just shows it in another order.
                const right = [...payload.right.slice(1), ...payload.right.slice(0, 1)];

                return (
                    <Box sx={{ display: 'grid', gap: 2, gridTemplateColumns: '1fr 1fr' }}>
                        <Stack spacing={2}>
                            {payload.left.map((entry, index) => (
                                <Paper key={index} variant="outlined" sx={tileSx}>
                                    {entry || '…'}
                                </Paper>
                            ))}
                        </Stack>
                        <Stack spacing={2}>
                            {right.map((entry, index) => (
                                <Paper key={index} variant="outlined" sx={tileSx}>
                                    {entry || '…'}
                                </Paper>
                            ))}
                        </Stack>
                    </Box>
                );
            }
            case 'flashcard':
                return (
                    <Stack spacing={1} sx={{ alignItems: 'center' }}>
                        <Paper
                            elevation={3}
                            sx={{ ...tileSx, minHeight: 160, minWidth: 240, fontSize: '2rem' }}
                        >
                            {payload.front || '…'}
                        </Paper>
                        <Typography color="text.secondary">
                            {getResource('captionFlashcardTurnHint')}
                        </Typography>
                    </Stack>
                );
        }
    };

    return (
        <Box
            data-testid={uiTestId}
            aria-label={getResource('captionChildPreview')}
            role="figure"
            sx={{ bgcolor: 'background.default', borderRadius: 3, p: { xs: 2, sm: 3 } }}
        >
            <Typography component="p" color="text.secondary" sx={{ mb: 1 }}>
                {getResource('captionChildPreview')}
            </Typography>
            <Stack spacing={3}>
                <Typography component="p" sx={{ fontSize: '1.75rem', fontWeight: 700 }}>
                    {payload.prompt || '…'}
                </Typography>
                {renderAnswerArea()}
            </Stack>
        </Box>
    );
};

export default QuestionPreview;
