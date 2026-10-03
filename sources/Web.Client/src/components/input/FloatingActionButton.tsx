import React from 'react';
import { Fab } from '@mui/material';

interface IProps {
    icon: React.ReactNode;
    /** Accessible name (already translated) – the button shows only an icon. */
    label: string;
    color?: 'primary' | 'secondary';
    onClick: () => void;
}

const FloatingActionButton: React.FC<IProps> = (props) => {
    const { icon, label, color = 'primary', onClick } = props;

    return (
        <Fab
            color={color}
            aria-label={label}
            onClick={onClick}
            sx={{ position: 'fixed', bottom: 24, right: 24 }}
        >
            {icon}
        </Fab>
    );
};

export default FloatingActionButton;
