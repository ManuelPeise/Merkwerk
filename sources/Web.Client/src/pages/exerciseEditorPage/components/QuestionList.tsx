import React from 'react';
import { Box, Button, Card, CardContent, Stack, Typography } from '@mui/material';
import { AddIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';
import type { QuestionType } from 'src/lib/api/exercises/exercisesTypes';
import type { EditorQuestion } from 'src/lib/exercises/editorQuestion';
import { exerciseRules } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import { questionTypeLabels, questionTypes } from 'src/lib/exercises/questionTypes';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import QuestionCard from 'src/pages/exerciseEditorPage/components/QuestionCard';

interface IProps {
    questions: EditorQuestion[];
    questionErrors: Record<string, NotificationKey[]>;
    uiTestId: string;
    onAdd: (type: QuestionType) => void;
    onChange: (question: EditorQuestion) => void;
    onMove: (key: string, direction: -1 | 1) => void;
    onRemove: (key: string) => void;
}

/** All questions of the exercise in display order, plus one "add" button per question type. */
const QuestionList: React.FC<IProps> = (props) => {
    const { questions, questionErrors, uiTestId, onAdd, onChange, onMove, onRemove } = props;
    const { getResource } = useTranslation();

    const isFull = questions.length >= exerciseRules.maxQuestions;

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            <Typography variant="h2">{getResource('captionQuestions')}</Typography>

            {questions.length === 0 && (
                <Typography color="text.secondary">{getResource('captionNoQuestions')}</Typography>
            )}

            {questions.map((question, index) => (
                <QuestionCard
                    key={question.key}
                    question={question}
                    number={index + 1}
                    isFirst={index === 0}
                    isLast={index === questions.length - 1}
                    problems={questionErrors[question.key] ?? []}
                    uiTestId={uiTestIdOf(uiTestId, `item-${index + 1}`)}
                    onChange={onChange}
                    onMove={(direction) => onMove(question.key, direction)}
                    onRemove={() => onRemove(question.key)}
                />
            ))}

            <Card variant="outlined">
                <CardContent>
                    <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                        {getResource('labelAddQuestion')}
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                        {questionTypes.map((type) => (
                            <Button
                                key={type}
                                variant="outlined"
                                startIcon={<AddIcon />}
                                disabled={isFull}
                                data-testid={uiTestIdOf(uiTestId, `add-${type}`)}
                                onClick={() => onAdd(type)}
                                sx={{ minHeight: 48 }}
                            >
                                {getResource(questionTypeLabels[type])}
                            </Button>
                        ))}
                    </Box>
                    {isFull && (
                        <Typography color="text.secondary" sx={{ mt: 1 }}>
                            {getResource('notificationTooManyQuestions')}
                        </Typography>
                    )}
                </CardContent>
            </Card>
        </Stack>
    );
};

export default QuestionList;
