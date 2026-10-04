import React from 'react';
import { OutlinedInput } from '@mui/material';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps extends IFormFieldProps<string> {
    type?: 'text' | 'email';
    /** e.g. 'email', 'username', 'given-name'. Defaults to 'email' for type email. */
    autoComplete?: string;
    uiTestId: string;
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
        uiTestId,
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
            uiTestId={uiTestId}
        >
            <OutlinedInput
                id={inputId}
                value={value}
                name={name}
                type={type}
                autoComplete={autoComplete ?? (type === 'email' ? 'email' : undefined)}
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                inputProps={{
                    'aria-describedby': hasMessage ? `${inputId}-message` : undefined,
                    'data-testid': uiTestIdOf(uiTestId, 'input'),
                }}
            />
        </FormFieldContainer>
    );
};

export default FormTextField;
