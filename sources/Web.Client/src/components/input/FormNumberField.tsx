import React from 'react';
import { OutlinedInput } from '@mui/material';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps extends IFormFieldProps<number | null> {
    uiTestId: string;
}

/** Whole numbers >= 0. Empty field = null. */
const digitsOnly = /^\d*$/;

const FormNumberField: React.FC<IProps> = (props) => {
    const { label, value, name, disabled, required, errorText, helperText, uiTestId, onChange } =
        props;

    const inputId = React.useId();
    const hasMessage = Boolean(errorText ?? helperText);

    const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const nextValue = event.target.value;

        // Ignore anything that is not a digit, so the field never holds an invalid number.
        if (!digitsOnly.test(nextValue)) {
            return;
        }

        onChange(nextValue === '' ? null : Number(nextValue));
    };

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
                value={value ?? ''}
                name={name}
                // type="text" + inputMode: numeric keypad on phones, without the quirks of type="number".
                type="text"
                fullWidth
                onChange={handleChange}
                sx={{ '& input': { textAlign: 'right' } }}
                inputProps={{
                    inputMode: 'numeric',
                    pattern: '[0-9]*',
                    'aria-describedby': hasMessage ? `${inputId}-message` : undefined,
                    'data-testid': uiTestIdOf(uiTestId, 'input'),
                }}
            />
        </FormFieldContainer>
    );
};

export default FormNumberField;
