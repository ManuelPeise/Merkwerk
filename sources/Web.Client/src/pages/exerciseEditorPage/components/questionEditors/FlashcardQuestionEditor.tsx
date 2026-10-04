import React from 'react';
import { Box } from '@mui/material';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IEditorFlashcardQuestion } from 'src/lib/exercises/editorQuestion';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    question: IEditorFlashcardQuestion;
    uiTestId: string;
    onChange: (question: IEditorFlashcardQuestion) => void;
}

/** Front and back; the child turns the card and says whether it knew the back. */
const FlashcardQuestionEditor: React.FC<IProps> = (props) => {
    const { question, uiTestId, onChange } = props;
    const { getResource } = useTranslation();

    return (
        <Box
            data-testid={uiTestId}
            sx={{ display: 'grid', gap: 2, gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' } }}
        >
            <FormTextField
                label={getResource('labelFlashcardFront')}
                required
                value={question.front}
                uiTestId={uiTestIdOf(uiTestId, 'front')}
                onChange={(front) => onChange({ ...question, front })}
            />
            <FormTextField
                label={getResource('labelFlashcardBack')}
                required
                value={question.back}
                uiTestId={uiTestIdOf(uiTestId, 'back')}
                onChange={(back) => onChange({ ...question, back })}
            />
        </Box>
    );
};

export default FlashcardQuestionEditor;
