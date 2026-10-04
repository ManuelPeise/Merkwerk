import React from 'react';
import { Card, Stack, Typography } from '@mui/material';

interface IProps {
    title: string;
    subtitle?: string;
    uiTestId: string;
    children: React.ReactNode;
}

/** Card for every auth page: title (h1), optional subtitle, content. Centred, max. 440 px wide. */
const AuthCard: React.FC<IProps> = (props) => {
    const { title, subtitle, uiTestId, children } = props;

    return (
        <Card
            sx={{ width: '100%', maxWidth: 440, mx: 'auto', my: 'auto', p: { xs: 3, sm: 4 } }}
            data-testid={uiTestId}
        >
            <Stack spacing={3}>
                <Stack spacing={1}>
                    <Typography variant="h1">{title}</Typography>
                    {subtitle && <Typography color="text.secondary">{subtitle}</Typography>}
                </Stack>
                {children}
            </Stack>
        </Card>
    );
};

export default AuthCard;
