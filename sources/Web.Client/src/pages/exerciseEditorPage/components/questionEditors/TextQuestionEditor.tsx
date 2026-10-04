import React from 'react';
import { Box, Stack, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { TextInputKind } from 'src/lib/api/exercises/exercisesTypes';
import type { IEditorTextQuestion } from 'src/lib/exercises/editorQuestion';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import TextListEditor from 'src/pages/exerciseEditorPage/components/TextListEditor';

interface IProps {
    question: IEditorTextQuestion;
    uiTestId: string;
    onChange: (question: IEditorTextQuestion) => void;
}

const maxAcceptedAnswers = 10;

/** A word, sentence or number to type; several accepted answers and the tolerance rules of LP-111. */
const TextQuestionEditor: React.FC<IProps> = (props) => {
    const { question, uiTestId, onChange } = props;
    const { getResource } = useTranslation();
    const isNumber = question.inputKind === 'number';

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <Box>
                <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                    {getResource('labelInputKind')}
                </Typography>
                <ToggleButtonGroup
                    exclusive
                    value={question.inputKind}
                    aria-label={getResource('labelInputKind')}
                    onChange={(_, inputKind: TextInputKind | null) => {
                        if (inputKind !== null) {
                            onChange({ ...question, inputKind });
                        }
                    }}
                >
                    <ToggleButton value="text" sx={{ minHeight: 48 }}>
                        {getResource('labelInputKindText')}
                    </ToggleButton>
                    <ToggleButton value="number" sx={{ minHeight: 48 }}>
                        {getResource('labelInputKindNumber')}
                    </ToggleButton>
                </ToggleButtonGroup>
            </Box>

            <TextListEditor
                values={question.acceptedAnswers}
                labelFor={(index) => getResource('labelAcceptedAnswer', { number: index + 1 })}
                removeLabelFor={(index) =>
                    getResource('labelRemoveAcceptedAnswer', { number: index + 1 })
                }
                addLabel={getResource('labelAddAcceptedAnswer')}
                min={1}
                max={maxAcceptedAnswers}
                uiTestId={uiTestIdOf(uiTestId, 'answers')}
                onChange={(acceptedAnswers) => onChange({ ...question, acceptedAnswers })}
            />

            {isNumber ? (
                <FormTextField
                    label={getResource('labelNumberTolerance')}
                    value={question.numberTolerance}
                    helperText={getResource('captionNumberToleranceHint')}
                    uiTestId={uiTestIdOf(uiTestId, 'tolerance')}
                    onChange={(numberTolerance) => onChange({ ...question, numberTolerance })}
                />
            ) : (
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
                    <FormCheckbox
                        label={getResource('labelCaseSensitive')}
                        checked={question.caseSensitive}
                        uiTestId={uiTestIdOf(uiTestId, 'case-sensitive')}
                        onChange={(caseSensitive) => onChange({ ...question, caseSensitive })}
                    />
                    <FormCheckbox
                        label={getResource('labelAllowTypo')}
                        checked={question.allowTypo}
                        uiTestId={uiTestIdOf(uiTestId, 'allow-typo')}
                        onChange={(allowTypo) => onChange({ ...question, allowTypo })}
                    />
                </Box>
            )}
        </Stack>
    );
};

export default TextQuestionEditor;
