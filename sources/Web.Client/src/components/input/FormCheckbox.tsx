import React from 'react';
import { Checkbox, FormControl, FormControlLabel, FormHelperText } from '@mui/material';

interface IProps {
    /** Already translated; may contain a link. */
    label: React.ReactNode;
    checked: boolean;
    name?: string;
    disabled?: boolean;
    required?: boolean;
    errorText?: string;
    onChange: (checked: boolean) => void;
}

/** Checkbox with the same error and required behaviour as the other form fields. */
const FormCheckbox: React.FC<IProps> = (props) => {
    const { label, checked, name, disabled, required, errorText, onChange } = props;

    const inputId = React.useId();

    return (
        <FormControl error={Boolean(errorText)} disabled={disabled} required={required}>
            <FormControlLabel
                label={label}
                control={
                    <Checkbox
                        id={inputId}
                        name={name}
                        checked={checked}
                        onChange={(event) => onChange(event.target.checked)}
                        // 48 px touch target.
                        sx={{ p: 1.5 }}
                        slotProps={{
                            input: {
                                'aria-describedby': errorText ? `${inputId}-message` : undefined,
                            },
                        }}
                    />
                }
            />
            {errorText && <FormHelperText id={`${inputId}-message`}>{errorText}</FormHelperText>}
        </FormControl>
    );
};

export default FormCheckbox;
