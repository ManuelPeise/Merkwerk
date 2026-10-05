import React from 'react';
import { Alert, Box, Card, CardContent, IconButton, Stack, Typography } from '@mui/material';
import {
    DeleteIcon,
    MoveDownIcon,
    MoveUpIcon,
    VisibilityIcon,
    VisibilityOffIcon,
} from 'src/components/icons/AppIcons';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { EditorQuestion } from 'src/lib/exercises/editorQuestion';
import { toQuestion } from 'src/lib/exercises/editorQuestions';
import { questionTypeLabels } from 'src/lib/exercises/questionTypes';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import QuestionPreview from 'src/pages/exerciseEditorPage/components/QuestionPreview';
import ChoiceQuestionEditor from 'src/pages/exerciseEditorPage/components/questionEditors/ChoiceQuestionEditor';
import ClozeQuestionEditor from 'src/pages/exerciseEditorPage/components/questionEditors/ClozeQuestionEditor';
import FlashcardQuestionEditor from 'src/pages/exerciseEditorPage/components/questionEditors/FlashcardQuestionEditor';
import MatchQuestionEditor from 'src/pages/exerciseEditorPage/components/questionEditors/MatchQuestionEditor';
import TextQuestionEditor from 'src/pages/exerciseEditorPage/components/questionEditors/TextQuestionEditor';

interface IProps {
    question: EditorQuestion;
    /** 1-based position, shown and read aloud. */
    number: number;
    isFirst: boolean;
    isLast: boolean;
    problems: NotificationKey[];
    uiTestId: string;
    onChange: (question: EditorQuestion) => void;
    onMove: (direction: -1 | 1) => void;
    onRemove: () => void;
}

/** One question: type, prompt, the type's own fields, move/delete, and a preview from the child's view. */
const QuestionCard: React.FC<IProps> = (props) => {
    const { question, number, isFirst, isLast, problems, uiTestId, onChange, onMove, onRemove } =
        props;
    const { getResource } = useTranslation();
    const [showPreview, setShowPreview] = React.useState(false);

    const fieldsTestId = uiTestIdOf(uiTestId, 'fields');

    const renderFields = () => {
        switch (question.type) {
            case 'choice':
                return (
                    <ChoiceQuestionEditor
                        question={question}
                        uiTestId={fieldsTestId}
                        onChange={onChange}
                    />
                );
            case 'text':
                return (
                    <TextQuestionEditor
                        question={question}
                        uiTestId={fieldsTestId}
                        onChange={onChange}
                    />
                );
            case 'cloze':
                return (
                    <ClozeQuestionEditor
                        question={question}
                        uiTestId={fieldsTestId}
                        onChange={onChange}
                    />
                );
            case 'match':
                return (
                    <MatchQuestionEditor
                        question={question}
                        uiTestId={fieldsTestId}
                        onChange={onChange}
                    />
                );
            case 'flashcard':
                return (
                    <FlashcardQuestionEditor
                        question={question}
                        uiTestId={fieldsTestId}
                        onChange={onChange}
                    />
                );
        }
    };

    return (
        <Card variant="outlined" component="section" data-testid={uiTestId}>
            <CardContent>
                <Stack spacing={2}>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1 }}>
                        <Typography
                            variant="h2"
                            component="h3"
                            sx={{ flexGrow: 1, fontSize: '1.25rem' }}
                        >
                            {getResource('captionQuestionNumber', {
                                number,
                                type: getResource(questionTypeLabels[question.type]),
                            })}
                        </Typography>
                        <IconButton
                            aria-label={getResource(
                                showPreview ? 'labelHidePreview' : 'labelShowPreview',
                                { number },
                            )}
                            aria-pressed={showPreview}
                            onClick={() => setShowPreview(!showPreview)}
                        >
                            {showPreview ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                        <IconButton
                            aria-label={getResource('labelMoveQuestionUp', { number })}
                            disabled={isFirst}
                            onClick={() => onMove(-1)}
                        >
                            <MoveUpIcon />
                        </IconButton>
                        <IconButton
                            aria-label={getResource('labelMoveQuestionDown', { number })}
                            disabled={isLast}
                            onClick={() => onMove(1)}
                        >
                            <MoveDownIcon />
                        </IconButton>
                        <IconButton
                            aria-label={getResource('labelDeleteQuestion', { number })}
                            onClick={onRemove}
                        >
                            <DeleteIcon />
                        </IconButton>
                    </Box>

                    {problems.length > 0 && (
                        <Alert severity="warning" data-testid={uiTestIdOf(uiTestId, 'problems')}>
                            {problems.map((problem) => (
                                <Typography key={problem}>{getResource(problem)}</Typography>
                            ))}
                        </Alert>
                    )}

                    <FormTextField
                        label={getResource('labelQuestionPrompt')}
                        required
                        value={question.prompt}
                        helperText={getResource('captionQuestionPromptHint')}
                        uiTestId={uiTestIdOf(uiTestId, 'prompt')}
                        onChange={(prompt) => onChange({ ...question, prompt })}
                    />

                    {renderFields()}

                    {showPreview && (
                        <QuestionPreview
                            question={toQuestion(question)}
                            uiTestId={uiTestIdOf(uiTestId, 'preview')}
                        />
                    )}
                </Stack>
            </CardContent>
        </Card>
    );
};

export default QuestionCard;
