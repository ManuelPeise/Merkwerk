import React from 'react';
import { Checkbox, FormControl, FormControlLabel, FormHelperText } from '@mui/material';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    /** Already translated; may contain a link. */
    label: React.ReactNode;
    checked: boolean;
    name?: string;
    disabled?: boolean;
    required?: boolean;
    errorText?: string;
    uiTestId: string;
    onChange: (checked: boolean) => void;
}

/** Checkbox with the same error and required behaviour as the other form fields. */
const FormCheckbox: React.FC<IProps> = (props) => {
    const { label, checked, name, disabled, required, errorText, uiTestId, onChange } = props;

    const inputId = React.useId();

    return (
        <FormControl
            error={Boolean(errorText)}
            disabled={disabled}
            required={required}
            data-testid={uiTestId}
        >
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
                                'data-testid': uiTestIdOf(uiTestId, 'input'),
                            } as React.InputHTMLAttributes<HTMLInputElement>,
                        }}
                    />
                }
            />
            {errorText && (
                <FormHelperText
                    id={`${inputId}-message`}
                    data-testid={uiTestIdOf(uiTestId, 'error')}
                >
                    {errorText}
                </FormHelperText>
            )}
        </FormControl>
    );
};

export default FormCheckbox;
