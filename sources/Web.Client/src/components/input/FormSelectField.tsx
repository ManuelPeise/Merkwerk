import React from 'react';
import { MenuItem, Select } from '@mui/material';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

export interface ISelectOption {
    value: string;
    /** Already translated. */
    label: string;
}

interface IProps extends IFormFieldProps<string> {
    options: ISelectOption[];
    /** Shown while nothing is selected (value ''). */
    placeholder?: string;
    uiTestId: string;
}

/** Drop-down with the same frame as the other form fields. Values are strings; convert numbers at the caller. */
const FormSelectField: React.FC<IProps> = (props) => {
    const {
        label,
        value,
        name,
        disabled,
        required,
        errorText,
        helperText,
        options,
        placeholder,
        uiTestId,
        onChange,
    } = props;

    const labelId = React.useId();
    const hasMessage = Boolean(errorText ?? helperText);

    return (
        <FormFieldContainer
            label={label}
            labelId={labelId}
            disabled={disabled}
            required={required}
            errorText={errorText}
            helperText={helperText}
            uiTestId={uiTestId}
        >
            <Select
                labelId={labelId}
                value={value}
                name={name}
                displayEmpty
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                renderValue={(selected) =>
                    options.find((option) => option.value === selected)?.label ?? placeholder ?? ''
                }
                SelectDisplayProps={{
                    'aria-describedby': hasMessage ? `${labelId}-message` : undefined,
                }}
                inputProps={{ 'data-testid': uiTestIdOf(uiTestId, 'input') }}
            >
                {options.map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                        {option.label}
                    </MenuItem>
                ))}
            </Select>
        </FormFieldContainer>
    );
};

export default FormSelectField;
