import React from 'react';
import { Box, Card, CardActionArea, Typography } from '@mui/material';
import { Link } from 'react-router-dom';

interface IProps {
    /** Icon or avatar, shown above the label. */
    icon: React.ReactNode;
    /** Translated label. */
    label: string;
    /** Translated description. */
    description?: string;
    /** Route the whole tile links to. */
    to: string;
    color?: 'primary' | 'secondary';
}

/** Large, fully clickable tile (one focus stop). Icon plus text, never colour alone. */
const ChoiceTile: React.FC<IProps> = (props) => {
    const { icon, label, description, to, color = 'primary' } = props;

    return (
        <Card sx={{ height: '100%' }}>
            <CardActionArea
                component={Link}
                to={to}
                sx={{
                    height: '100%',
                    minWidth: 160,
                    minHeight: { xs: 160, sm: 280 },
                    p: { xs: 3, md: 5 },
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: 2,
                    textAlign: 'center',
                    '&.Mui-focusVisible, &:focus-visible': {
                        outline: '3px solid',
                        outlineColor: `${color}.main`,
                        outlineOffset: -3,
                    },
                }}
            >
                <Box
                    sx={{
                        color: `${color}.main`,
                        display: 'flex',
                        '& svg': { fontSize: 64 },
                    }}
                >
                    {icon}
                </Box>
                <Typography variant="h6" component="span" sx={{ fontWeight: 700 }}>
                    {label}
                </Typography>
                {description && (
                    <Typography color="text.secondary" component="span">
                        {description}
                    </Typography>
                )}
            </CardActionArea>
        </Card>
    );
};

export default ChoiceTile;
