import React from 'react';
import { Box, Button, IconButton, Stack } from '@mui/material';
import { AddIcon, DeleteIcon } from 'src/components/icons/AppIcons';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IEditorChoiceQuestion } from 'src/lib/exercises/editorQuestion';
import { exerciseRules } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    question: IEditorChoiceQuestion;
    uiTestId: string;
    onChange: (question: IEditorChoiceQuestion) => void;
}

/** 2-8 answers; tick the right one(s). Without "several right" ticking one unticks the others. */
const ChoiceQuestionEditor: React.FC<IProps> = (props) => {
    const { question, uiTestId, onChange } = props;
    const { getResource } = useTranslation();
    const { options, correctIndexes, multipleAnswers } = question;

    const setCorrect = (index: number, isCorrect: boolean) => {
        const others = multipleAnswers ? correctIndexes.filter((i) => i !== index) : [];
        onChange({ ...question, correctIndexes: isCorrect ? [...others, index] : others });
    };

    const removeOption = (index: number) =>
        onChange({
            ...question,
            options: options.filter((_, i) => i !== index),
            // Indexes behind the removed option move up by one.
            correctIndexes: correctIndexes
                .filter((i) => i !== index)
                .map((i) => (i > index ? i - 1 : i)),
        });

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <FormCheckbox
                label={getResource('labelMultipleAnswers')}
                checked={multipleAnswers}
                uiTestId={uiTestIdOf(uiTestId, 'multiple')}
                onChange={(checked) =>
                    onChange({
                        ...question,
                        multipleAnswers: checked,
                        correctIndexes: checked ? correctIndexes : correctIndexes.slice(0, 1),
                    })
                }
            />
            {options.map((option, index) => (
                <Box
                    key={index}
                    sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'flex-end', gap: 1 }}
                >
                    <Box sx={{ flexGrow: 1, minWidth: 200 }}>
                        <FormTextField
                            label={getResource('labelAnswerOption', { number: index + 1 })}
                            value={option}
                            uiTestId={uiTestIdOf(uiTestId, `option-${index + 1}`)}
                            onChange={(value) =>
                                onChange({
                                    ...question,
                                    options: options.map((o, i) => (i === index ? value : o)),
                                })
                            }
                        />
                    </Box>
                    <FormCheckbox
                        label={getResource('labelAnswerOptionCorrect', { number: index + 1 })}
                        checked={correctIndexes.includes(index)}
                        uiTestId={uiTestIdOf(uiTestId, `correct-${index + 1}`)}
                        onChange={(checked) => setCorrect(index, checked)}
                    />
                    <IconButton
                        aria-label={getResource('labelRemoveAnswerOption', { number: index + 1 })}
                        disabled={options.length <= exerciseRules.minOptions}
                        onClick={() => removeOption(index)}
                        sx={{ mb: 0.5 }}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            ))}
            <Box>
                <Button
                    startIcon={<AddIcon />}
                    disabled={options.length >= exerciseRules.maxOptions}
                    onClick={() => onChange({ ...question, options: [...options, ''] })}
                >
                    {getResource('labelAddAnswerOption')}
                </Button>
            </Box>
        </Stack>
    );
};

export default ChoiceQuestionEditor;
