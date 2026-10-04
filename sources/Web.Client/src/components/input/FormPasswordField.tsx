import React from 'react';
import { IconButton, InputAdornment, OutlinedInput } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { VisibilityIcon, VisibilityOffIcon } from 'src/components/icons/AppIcons';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps extends IFormFieldProps<string> {
    /** 'current-password' for logins, 'new-password' when setting a password. */
    autoComplete?: 'current-password' | 'new-password';
    testId?: string;
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
        testId,
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
            testId={testId}
        >
            <OutlinedInput
                id={inputId}
                value={value}
                name={name}
                type={isVisible ? 'text' : 'password'}
                autoComplete={autoComplete}
                fullWidth
                onChange={(event) => onChange(event.target.value)}
                inputProps={{
                    'aria-describedby': hasMessage ? `${inputId}-message` : undefined,
                    'data-testid': testIdOf(testId, 'input'),
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
