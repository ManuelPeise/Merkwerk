import React from 'react';
import { Box, FormControl, OutlinedInput } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import FormFieldContainer from 'src/components/input/FormFieldContainer';
import type { IFormFieldProps } from 'src/components/input/types/formFieldProps';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps extends IFormFieldProps<string> {
    /** Number of digits. Default 4. */
    length?: number;
    /** Show dots instead of digits. Default true. */
    masked?: boolean;
    autoFocus?: boolean;
    uiTestId: string;
}

const nonDigits = /\D/g;

/** Longer codes (6 digits) read in groups of three: 123 456 (LP-166). */
const groupSize = 3;

const isGroupStart = (index: number, length: number) =>
    length > 4 && length % groupSize === 0 && index > 0 && index % groupSize === 0;

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
        uiTestId,
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

    const handleFocus = (
        index: number,
        event: React.FocusEvent<HTMLInputElement | HTMLTextAreaElement>,
    ) => {
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
            uiTestId={uiTestId}
        >
            <Box
                role="group"
                aria-labelledby={labelId}
                aria-describedby={hasMessage ? `${labelId}-message` : undefined}
                sx={{ display: 'flex', gap: { xs: 1, sm: 2 } }}
            >
                {Array.from({ length }, (_, index) => (
                    // One FormControl per box: MUI allows only one input per FormControl.
                    <FormControl
                        key={index}
                        error={Boolean(errorText)}
                        disabled={disabled}
                        sx={isGroupStart(index, length) ? { ml: { xs: 1.5, sm: 2 } } : undefined}
                    >
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
                            // Square boxes with the same height as every other field; narrower on phones.
                            sx={{
                                width: { xs: 44, sm: 64 },
                                '& input': { textAlign: 'center', fontSize: '1.75rem', px: 0 },
                            }}
                            inputProps={{
                                inputMode: 'numeric',
                                pattern: '[0-9]*',
                                autoComplete: index === 0 ? 'one-time-code' : 'off',
                                'data-testid': uiTestIdOf(uiTestId, 'input'),
                                'aria-label': getResource('labelPinDigit', {
                                    position: index + 1,
                                    length,
                                }),
                            }}
                        />
                    </FormControl>
                ))}
            </Box>
            {name && <input type="hidden" name={name} value={pin} />}
        </FormFieldContainer>
    );
};

export default FormPinField;
