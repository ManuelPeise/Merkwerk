import React from 'react';
import { Button } from '@mui/material';

interface IProps {
    label: string;
    /** save = main action (filled), cancel = secondary action (outlined). */
    intent?: 'save' | 'cancel';
    /** 'submit' inside a <form>, so Enter submits it. */
    type?: 'button' | 'submit';
    disabled?: boolean;
    uiTestId: string;
    onClick?: () => void;
}

const FormButton: React.FC<IProps> = (props) => {
    const { label, intent = 'save', type = 'button', disabled, uiTestId, onClick } = props;

    return (
        <Button
            type={type}
            variant={intent === 'save' ? 'contained' : 'outlined'}
            color="primary"
            disabled={disabled}
            data-testid={uiTestId}
            onClick={onClick}
            // Labels stay on one line; a header row wraps the button below its heading instead (LP-166).
            sx={{ whiteSpace: 'nowrap', flexShrink: 0 }}
        >
            {label}
        </Button>
    );
};

export default FormButton;
