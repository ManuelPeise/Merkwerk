import React from 'react';
import { Fab } from '@mui/material';

interface IProps {
    icon: React.ReactNode;
    /** Accessible name (already translated) – the button shows only an icon. */
    label: string;
    color?: 'primary' | 'secondary';
    testId?: string;
    onClick: () => void;
}

const FloatingActionButton: React.FC<IProps> = (props) => {
    const { icon, label, color = 'primary', testId, onClick } = props;

    return (
        <Fab
            color={color}
            aria-label={label}
            data-testid={testId}
            onClick={onClick}
            sx={{ position: 'fixed', bottom: 24, right: 24 }}
        >
            {icon}
        </Fab>
    );
};

export default FloatingActionButton;
