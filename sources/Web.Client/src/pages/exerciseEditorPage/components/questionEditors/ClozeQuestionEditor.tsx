import React from 'react';
import { Stack, Typography } from '@mui/material';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IEditorClozeQuestion } from 'src/lib/exercises/editorQuestion';
import { parseCloze } from 'src/lib/exercises/editorQuestions';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    question: IEditorClozeQuestion;
    uiTestId: string;
    onChange: (question: IEditorClozeQuestion) => void;
}

/** One text with the gaps in square brackets, alternatives split by "|": "Der [Hund|Dackel] bellt." */
const ClozeQuestionEditor: React.FC<IProps> = (props) => {
    const { question, uiTestId, onChange } = props;
    const { getResource } = useTranslation();
    const gapCount = parseCloze(question.text).gaps.length;

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <FormTextField
                label={getResource('labelClozeText')}
                required
                value={question.text}
                helperText={getResource('captionClozeHint')}
                uiTestId={uiTestIdOf(uiTestId, 'text')}
                onChange={(text) => onChange({ ...question, text })}
            />
            <Typography color="text.secondary">
                {getResource('captionClozeGapCount', { count: gapCount })}
            </Typography>
            <FormCheckbox
                label={getResource('labelCaseSensitive')}
                checked={question.caseSensitive}
                uiTestId={uiTestIdOf(uiTestId, 'case-sensitive')}
                onChange={(caseSensitive) => onChange({ ...question, caseSensitive })}
            />
        </Stack>
    );
};

export default ClozeQuestionEditor;
