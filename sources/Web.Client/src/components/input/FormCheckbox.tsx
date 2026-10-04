import React from 'react';
import { Checkbox, FormControl, FormControlLabel, FormHelperText } from '@mui/material';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps {
    /** Already translated; may contain a link. */
    label: React.ReactNode;
    checked: boolean;
    name?: string;
    disabled?: boolean;
    required?: boolean;
    errorText?: string;
    testId?: string;
    onChange: (checked: boolean) => void;
}

/** Checkbox with the same error and required behaviour as the other form fields. */
const FormCheckbox: React.FC<IProps> = (props) => {
    const { label, checked, name, disabled, required, errorText, testId, onChange } = props;

    const inputId = React.useId();

    return (
        <FormControl
            error={Boolean(errorText)}
            disabled={disabled}
            required={required}
            data-testid={testId}
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
                                'data-testid': testIdOf(testId, 'input'),
                            } as React.InputHTMLAttributes<HTMLInputElement>,
                        }}
                    />
                }
            />
            {errorText && (
                <FormHelperText id={`${inputId}-message`} data-testid={testIdOf(testId, 'error')}>
                    {errorText}
                </FormHelperText>
            )}
        </FormControl>
    );
};

export default FormCheckbox;
