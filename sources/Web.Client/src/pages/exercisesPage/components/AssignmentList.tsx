import React from 'react';
import {
    Box,
    IconButton,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Typography,
} from '@mui/material';
import { ChildIcon, DeleteIcon, EditIcon, FamilyIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IAssignment } from 'src/lib/api/assignments/assignmentsTypes';
import type { IGroup } from 'src/lib/api/groups/groupsTypes';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import { formatDueDate } from 'src/lib/assignments/dueDates';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    assignments: IAssignment[];
    learners: ILearner[];
    groups: IGroup[];
    disabled: boolean;
    uiTestId: string;
    onEdit: (assignment: IAssignment) => void;
    onRevoke: (assignment: IAssignment) => void;
}

/** Who has the exercise: one row per child or group with due date, allowed aids and own settings (LP-114). */
const AssignmentList: React.FC<IProps> = (props) => {
    const { assignments, learners, groups, disabled, uiTestId, onEdit, onRevoke } = props;
    const { getResource, language } = useTranslation();

    const nameOf = (assignment: IAssignment): string =>
        assignment.learnerId !== null
            ? (learners.find((learner) => learner.id === assignment.learnerId)?.displayName ?? '')
            : getResource('captionAssignmentGroup', {
                  name: groups.find((group) => group.id === assignment.groupId)?.name ?? '',
              });

    const detailsOf = (assignment: IAssignment): string =>
        [
            assignment.dueDate
                ? getResource('captionAssignmentDue', {
                      date: formatDueDate(assignment.dueDate, language),
                  })
                : getResource('captionAssignmentNoDueDate'),
            assignment.allowDotArray ? getResource('captionAidDotArray') : null,
            assignment.allowTimesTableMatrix ? getResource('captionAidTimesTableMatrix') : null,
            assignment.generator ? getResource('captionAssignmentOwnSettings') : null,
        ]
            .filter((part) => part !== null)
            .join(' · ');

    if (assignments.length === 0) {
        return (
            <Typography color="text.secondary" data-testid={uiTestId}>
                {getResource('captionNoAssignments')}
            </Typography>
        );
    }

    return (
        <List disablePadding data-testid={uiTestId}>
            {assignments.map((assignment) => {
                const name = nameOf(assignment);

                return (
                    <ListItem
                        key={assignment.id}
                        disableGutters
                        data-testid={uiTestIdOf(uiTestId, `item-${assignment.id}`)}
                        secondaryAction={
                            <Box sx={{ display: 'flex', gap: 0.5 }}>
                                <IconButton
                                    disabled={disabled}
                                    aria-label={getResource('labelEditAssignment', { name })}
                                    onClick={() => onEdit(assignment)}
                                >
                                    <EditIcon />
                                </IconButton>
                                <IconButton
                                    disabled={disabled}
                                    aria-label={getResource('labelRevokeAssignment', { name })}
                                    onClick={() => onRevoke(assignment)}
                                >
                                    <DeleteIcon />
                                </IconButton>
                            </Box>
                        }
                        sx={{ pr: 12 }}
                    >
                        <ListItemIcon sx={{ minWidth: 40 }}>
                            {assignment.learnerId !== null ? <ChildIcon /> : <FamilyIcon />}
                        </ListItemIcon>
                        <ListItemText
                            primary={name}
                            secondary={detailsOf(assignment)}
                            slotProps={{ primary: { sx: { fontWeight: 700 } } }}
                        />
                    </ListItem>
                );
            })}
        </List>
    );
};

export default AssignmentList;
