import React from 'react';
import { OutlinedInput } from '@mui/material';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';

interface IProps extends IFormFieldProps<string> {
    type?: 'text' | 'email';
    /** e.g. 'email', 'username', 'given-name'. Defaults to 'email' for type email. */
    autoComplete?: string;
}

const FormTextField: React.FC<IProps> = (props) => {
    const {
        label,
        value,
        name,
        disabled,
        required,
        errorText,
        helperText,
        type = 'text',
        autoComplete,
        onChange,
    } = props;

    const inputId = React.useId();
    const hasMessage = Boolean(errorText ?? helperText);

    return (
        <FormFieldContainer
            label={label}
            inputId={inputId}
            disabled={disabled}
            required={required}
            errorText={errorText}
            helperText={helperText}
        >
            <OutlinedInput
                id={inputId}
                value={value}
                name={name}
                type={type}
                autoComplete={autoComplete ?? (type === 'email' ? 'email' : undefined)}
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                slotProps={{
                    input: { 'aria-describedby': hasMessage ? `${inputId}-message` : undefined },
                }}
            />
        </FormFieldContainer>
    );
};

export default FormTextField;
