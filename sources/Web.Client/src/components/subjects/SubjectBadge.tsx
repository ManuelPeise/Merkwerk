import React from 'react';
import { Box, Typography } from '@mui/material';
import type { ISubject } from 'src/lib/api/subjects/subjectsTypes';
import { getSubjectColor, renderSubjectIcon } from 'src/lib/subjects/subjectStyles';

interface IProps {
    subject: Pick<ISubject, 'name' | 'color' | 'icon'>;
    /** `small` for lists (icon beside the name), `large` for tiles (64 px icon above the name). */
    size?: 'small' | 'large';
    testId?: string;
}

/**
 * The one way a subject is shown – adults' and children's area alike (LP-109). Icon in the subject color plus the
 * name: the color is never the only hint.
 */
const SubjectBadge: React.FC<IProps> = (props) => {
    const { subject, size = 'small', testId } = props;
    const chipSize = size === 'large' ? 64 : 40;

    return (
        <Box
            data-testid={testId}
            sx={{
                display: 'inline-flex',
                flexDirection: size === 'large' ? 'column' : 'row',
                alignItems: 'center',
                gap: size === 'large' ? 1 : 1.5,
            }}
        >
            <Box
                aria-hidden
                sx={{
                    width: chipSize,
                    height: chipSize,
                    flexShrink: 0,
                    borderRadius: 3,
                    bgcolor: getSubjectColor(subject.color),
                    color: 'common.white',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    '& svg': { fontSize: chipSize * 0.6 },
                }}
            >
                {renderSubjectIcon(subject.icon)}
            </Box>
            <Typography component="span" sx={{ fontWeight: 700 }}>
                {subject.name}
            </Typography>
        </Box>
    );
};

export default SubjectBadge;
