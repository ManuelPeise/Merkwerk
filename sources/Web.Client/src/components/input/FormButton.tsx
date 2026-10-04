import React from 'react';
import { Button } from '@mui/material';

interface IProps {
    label: string;
    /** save = main action (filled), cancel = secondary action (outlined). */
    intent?: 'save' | 'cancel';
    /** 'submit' inside a <form>, so Enter submits it. */
    type?: 'button' | 'submit';
    disabled?: boolean;
    testId?: string;
    onClick?: () => void;
}

const FormButton: React.FC<IProps> = (props) => {
    const { label, intent = 'save', type = 'button', disabled, testId, onClick } = props;

    return (
        <Button
            type={type}
            variant={intent === 'save' ? 'contained' : 'outlined'}
            color="primary"
            disabled={disabled}
            data-testid={testId}
            onClick={onClick}
        >
            {label}
        </Button>
    );
};

export default FormButton;
