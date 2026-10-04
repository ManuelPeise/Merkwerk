import React from 'react';
import { Chip } from '@mui/material';
import { ArchiveIcon, CheckIcon, EditIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IExerciseSummary } from 'src/lib/api/exercises/exercisesTypes';

interface IProps {
    exercise: Pick<IExerciseSummary, 'state' | 'latestVersion' | 'hasUnpublishedChanges'>;
    uiTestId: string;
}

/** Status of an exercise as text plus icon (never colour alone): draft, published vN (with changes), archived. */
const ExerciseStateChip: React.FC<IProps> = (props) => {
    const { exercise, uiTestId } = props;
    const { getResource } = useTranslation();

    if (exercise.state === 'archived') {
        return (
            <Chip
                size="small"
                icon={<ArchiveIcon />}
                label={getResource('captionExerciseArchived')}
                data-testid={uiTestId}
            />
        );
    }

    if (exercise.state === 'draft') {
        return (
            <Chip
                size="small"
                variant="outlined"
                icon={<EditIcon />}
                label={getResource('captionExerciseDraft')}
                data-testid={uiTestId}
            />
        );
    }

    return (
        <Chip
            size="small"
            color={exercise.hasUnpublishedChanges ? 'warning' : 'success'}
            icon={exercise.hasUnpublishedChanges ? <EditIcon /> : <CheckIcon />}
            label={getResource(
                exercise.hasUnpublishedChanges
                    ? 'captionExercisePublishedWithChanges'
                    : 'captionExercisePublished',
                { version: exercise.latestVersion },
            )}
            data-testid={uiTestId}
        />
    );
};

export default ExerciseStateChip;
