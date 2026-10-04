import React from 'react';
import { FormControl, FormHelperText, FormLabel } from '@mui/material';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps {
    label: string;
    /** id of the input the label belongs to (fields with one input). */
    inputId?: string;
    /** id of the label itself, for groups that use aria-labelledby (e.g. the PIN boxes). */
    labelId?: string;
    disabled?: boolean;
    required?: boolean;
    errorText?: string;
    helperText?: string;
    testId?: string;
    children: React.ReactNode;
}

/** id of the message below a field – inputs reference it via aria-describedby. */
const toMessageId = (id: string) => `${id}-message`;

/**
 * The same frame for every form field: label above, input(s), message below.
 * Disabled, required and error state reach the inputs through the FormControl context.
 */
const FormFieldContainer: React.FC<IProps> = (props) => {
    const { label, inputId, labelId, disabled, required, errorText, helperText, testId, children } =
        props;

    const message = errorText ?? helperText;
    const messageId = toMessageId(inputId ?? labelId ?? '');

    return (
        <FormControl
            fullWidth
            error={Boolean(errorText)}
            disabled={disabled}
            required={required}
            data-testid={testId}
        >
            <FormLabel htmlFor={inputId} id={labelId} sx={{ mb: 1 }}>
                {label}
            </FormLabel>
            {children}
            {message && (
                <FormHelperText
                    id={messageId}
                    data-testid={errorText ? testIdOf(testId, 'error') : undefined}
                >
                    {message}
                </FormHelperText>
            )}
        </FormControl>
    );
};

export default FormFieldContainer;
