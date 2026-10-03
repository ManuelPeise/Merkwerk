import React from 'react';
import { IconButton, InputAdornment, OutlinedInput } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { VisibilityIcon, VisibilityOffIcon } from 'src/components/icons/AppIcons';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';

interface IProps extends IFormFieldProps<string> {
    /** 'current-password' for logins, 'new-password' when setting a password. */
    autoComplete?: 'current-password' | 'new-password';
}

const FormPasswordField: React.FC<IProps> = (props) => {
    const {
        label,
        value,
        name,
        disabled,
        required,
        errorText,
        helperText,
        autoComplete = 'current-password',
        onChange,
    } = props;

    const { getResource } = useTranslation();
    const inputId = React.useId();
    const hasMessage = Boolean(errorText ?? helperText);
    const [isVisible, setIsVisible] = React.useState(false);

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
                type={isVisible ? 'text' : 'password'}
                autoComplete={autoComplete}
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                slotProps={{
                    input: { 'aria-describedby': hasMessage ? `${inputId}-message` : undefined },
                }}
                endAdornment={
                    <InputAdornment position="end">
                        <IconButton
                            edge="end"
                            aria-label={getResource(
                                isVisible ? 'labelHidePassword' : 'labelShowPassword',
                            )}
                            onClick={() => setIsVisible((visible) => !visible)}
                            disabled={disabled}
                        >
                            {isVisible ? <VisibilityOffIcon /> : <VisibilityIcon />}
                        </IconButton>
                    </InputAdornment>
                }
            />
        </FormFieldContainer>
    );
};

export default FormPasswordField;
