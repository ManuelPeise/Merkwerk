import React from 'react';
import { Box, Button } from '@mui/material';
import { BackspaceIcon, CheckIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps {
    /** Digits typed so far. The keypad is controlled; it never shows the value itself. */
    value: string;
    onChange: (value: string) => void;
    /** Called by the "done" key. */
    onSubmit: () => void;
    /** Maximum number of digits. Default 6. */
    maxLength?: number;
    disabled?: boolean;
    testId?: string;
}

const digitRows = [
    ['1', '2', '3'],
    ['4', '5', '6'],
    ['7', '8', '9'],
];

const keySx = { minWidth: 64, minHeight: 64, fontSize: '1.75rem', fontWeight: 700 } as const;

/**
 * On-screen number pad for children. The keys are buttons, not an input,
 * so the device keyboard never opens. Text keys come with icons (no colour-only meaning).
 */
const NumberKeypad: React.FC<IProps> = (props) => {
    const { value, onChange, onSubmit, maxLength = 6, disabled, testId } = props;
    const { getResource } = useTranslation();

    const pressDigit = (digit: string) => {
        if (value.length < maxLength) {
            onChange(value + digit);
        }
    };

    const digitKey = (digit: string) => (
        <Button
            key={digit}
            variant="outlined"
            disabled={disabled}
            onClick={() => pressDigit(digit)}
            data-testid={testIdOf(testId, `key-${digit}`)}
            sx={keySx}
        >
            {digit}
        </Button>
    );

    return (
        <Box
            role="group"
            data-testid={testId}
            sx={{
                display: 'grid',
                gridTemplateColumns: 'repeat(3, minmax(64px, 1fr))',
                gap: 1.5,
                maxWidth: 320,
                mx: 'auto',
            }}
        >
            {digitRows.flat().map(digitKey)}
            <Button
                variant="outlined"
                disabled={disabled || value === ''}
                aria-label={getResource('labelDelete')}
                onClick={() => onChange(value.slice(0, -1))}
                data-testid={testIdOf(testId, 'backspace')}
                sx={keySx}
            >
                <BackspaceIcon />
            </Button>
            {digitKey('0')}
            <Button
                variant="contained"
                disabled={disabled || value === ''}
                aria-label={getResource('labelDone')}
                onClick={onSubmit}
                data-testid={testIdOf(testId, 'submit')}
                sx={keySx}
            >
                <CheckIcon />
            </Button>
        </Box>
    );
};

export default NumberKeypad;
