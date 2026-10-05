import React from 'react';
import { OutlinedInput } from '@mui/material';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps extends IFormFieldProps<string> {
    /** Earliest selectable day, "yyyy-MM-dd". */
    min?: string;
    uiTestId: string;
}

/** A calendar day with the browser's date picker. Value "yyyy-MM-dd", empty string = no date. */
const FormDateField: React.FC<IProps> = (props) => {
    const {
        label,
        value,
        name,
        disabled,
        required,
        errorText,
        helperText,
        min,
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
                type="date"
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                inputProps={{
                    min,
                    'aria-describedby': hasMessage ? `${inputId}-message` : undefined,
                    'data-testid': uiTestIdOf(uiTestId, 'input'),
                }}
            />
        </FormFieldContainer>
    );
};

export default FormDateField;
