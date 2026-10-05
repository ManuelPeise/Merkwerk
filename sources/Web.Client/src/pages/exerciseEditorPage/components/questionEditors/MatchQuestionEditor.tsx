import React from 'react';
import { Box, Button, IconButton, Stack, Typography } from '@mui/material';
import { AddIcon, DeleteIcon } from 'src/components/icons/AppIcons';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IEditorMatchQuestion, IMatchPair } from 'src/lib/exercises/editorQuestion';
import { exerciseRules } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    question: IEditorMatchQuestion;
    uiTestId: string;
    onChange: (question: IEditorMatchQuestion) => void;
}

/** 2-8 pairs that belong together; the child sees the right side shuffled. */
const MatchQuestionEditor: React.FC<IProps> = (props) => {
    const { question, uiTestId, onChange } = props;
    const { getResource } = useTranslation();
    const { pairs } = question;

    const updatePair = (index: number, change: Partial<IMatchPair>) =>
        onChange({
            ...question,
            pairs: pairs.map((pair, i) => (i === index ? { ...pair, ...change } : pair)),
        });

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <Typography color="text.secondary">{getResource('captionMatchHint')}</Typography>
            {pairs.map((pair, index) => (
                <Box
                    key={index}
                    sx={{
                        display: 'grid',
                        gridTemplateColumns: { xs: '1fr auto', sm: '1fr 1fr auto' },
                        alignItems: 'end',
                        gap: 1,
                    }}
                >
                    <FormTextField
                        label={getResource('labelMatchLeft', { number: index + 1 })}
                        value={pair.left}
                        uiTestId={uiTestIdOf(uiTestId, `left-${index + 1}`)}
                        onChange={(left) => updatePair(index, { left })}
                    />
                    <FormTextField
                        label={getResource('labelMatchRight', { number: index + 1 })}
                        value={pair.right}
                        uiTestId={uiTestIdOf(uiTestId, `right-${index + 1}`)}
                        onChange={(right) => updatePair(index, { right })}
                    />
                    <IconButton
                        aria-label={getResource('labelRemoveMatchPair', { number: index + 1 })}
                        disabled={pairs.length <= exerciseRules.minPairs}
                        onClick={() =>
                            onChange({ ...question, pairs: pairs.filter((_, i) => i !== index) })
                        }
                        sx={{ mb: 0.5 }}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            ))}
            <Box>
                <Button
                    startIcon={<AddIcon />}
                    disabled={pairs.length >= exerciseRules.maxPairs}
                    onClick={() =>
                        onChange({ ...question, pairs: [...pairs, { left: '', right: '' }] })
                    }
                >
                    {getResource('labelAddMatchPair')}
                </Button>
            </Box>
        </Stack>
    );
};

export default MatchQuestionEditor;
