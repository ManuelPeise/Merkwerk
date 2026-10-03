import React from 'react';
import { FormControl, OutlinedInput, Stack } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';

interface IProps extends IFormFieldProps<string> {
    /** Number of digits. Default 4. */
    length?: number;
    /** Show dots instead of digits. Default true. */
    masked?: boolean;
    autoFocus?: boolean;
}

const nonDigits = /\D/g;

/**
 * PIN input with one box per digit: moves on after each digit, Backspace goes back,
 * pasting or autofilling the whole PIN fills all boxes. The value is always a string of digits.
 */
const FormPinField: React.FC<IProps> = (props) => {
    const {
        label,
        value,
        name,
        disabled,
        required,
        errorText,
        helperText,
        length = 4,
        masked = true,
        autoFocus,
        onChange,
    } = props;

    const { getResource } = useTranslation();
    const labelId = React.useId();
    const hasMessage = Boolean(errorText ?? helperText);
    const inputRefs = React.useRef<(HTMLInputElement | null)[]>([]);

    const pin = value.slice(0, length);

    const focusBox = (index: number) => {
        inputRefs.current[Math.max(0, Math.min(index, length - 1))]?.focus();
    };

    const handleChange = (index: number, rawValue: string) => {
        const digits = rawValue.replace(nonDigits, '');

        // Box cleared (e.g. Backspace on a filled box): cut the PIN at this position.
        if (digits === '') {
            onChange(pin.slice(0, index));
            return;
        }

        // One digit typed, or several pasted/autofilled: fill from this box onwards.
        const nextPin = (pin.slice(0, index) + digits).slice(0, length);
        onChange(nextPin);
        focusBox(nextPin.length);
    };

    const handleKeyDown = (index: number, event: React.KeyboardEvent) => {
        if (event.key === 'Backspace' && !pin[index] && index > 0) {
            event.preventDefault();
            onChange(pin.slice(0, index - 1));
            focusBox(index - 1);
        }
    };

    const handleFocus = (index: number, event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        // Keep the PIN without gaps: always continue in the first empty box.
        if (index > pin.length) {
            focusBox(pin.length);
            return;
        }

        // Typing into a filled box replaces its digit.
        event.target.select();
    };

    return (
        <FormFieldContainer
            label={label}
            labelId={labelId}
            disabled={disabled}
            required={required}
            errorText={errorText}
            helperText={helperText}
        >
            <Stack
                direction="row"
                spacing={1.5}
                role="group"
                aria-labelledby={labelId}
                aria-describedby={hasMessage ? `${labelId}-message` : undefined}
            >
                {Array.from({ length }, (_, index) => (
                    // One FormControl per box: MUI allows only one input per FormControl.
                    <FormControl key={index} error={Boolean(errorText)} disabled={disabled}>
                        <OutlinedInput
                            inputRef={(element: HTMLInputElement | null) => {
                                inputRefs.current[index] = element;
                            }}
                            value={pin[index] ?? ''}
                            type={masked ? 'password' : 'text'}
                            autoFocus={autoFocus && index === 0}
                            onChange={(event) => handleChange(index, event.target.value)}
                            onKeyDown={(event) => handleKeyDown(index, event)}
                            onFocus={(event) => handleFocus(index, event)}
                            // Square boxes with the same height as every other field.
                            sx={{ width: 64, '& input': { textAlign: 'center', fontSize: '1.75rem', px: 0 } }}
                            slotProps={{
                                input: {
                                    inputMode: 'numeric',
                                    pattern: '[0-9]*',
                                    autoComplete: index === 0 ? 'one-time-code' : 'off',
                                    'aria-label': getResource('labelPinDigit', { position: index + 1, length }),
                                },
                            }}
                        />
                    </FormControl>
                ))}
            </Stack>
            {name && <input type="hidden" name={name} value={pin} />}
        </FormFieldContainer>
    );
};

export default FormPinField;
