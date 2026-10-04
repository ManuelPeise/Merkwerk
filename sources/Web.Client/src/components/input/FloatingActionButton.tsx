import React from 'react';
import { Fab } from '@mui/material';

interface IProps {
    icon: React.ReactNode;
    /** Accessible name (already translated) - the button shows only an icon. */
    label: string;
    color?: 'primary' | 'secondary';
    uiTestId: string;
    onClick: () => void;
}

const FloatingActionButton: React.FC<IProps> = (props) => {
    const { icon, label, color = 'primary', uiTestId, onClick } = props;

    return (
        <Fab
            color={color}
            aria-label={label}
            data-testid={uiTestId}
            onClick={onClick}
            sx={{ position: 'fixed', bottom: 24, right: 24 }}
        >
            {icon}
        </Fab>
    );
};

export default FloatingActionButton;
