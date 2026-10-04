import React from 'react';
import { Alert, Box, Stack, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import FormNumberField from 'src/components/input/FormNumberField';
import { useTranslation } from 'src/hooks/useTranslation';
import type {
    ArithmeticOperation,
    IArithmeticSettings,
    PlaceholderMode,
    TenTransition,
} from 'src/lib/api/exercises/exercisesTypes';
import { exerciseRules } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { LabelKey, NotificationKey } from 'src/lib/translations/translationKeys';

interface IProps {
    settings: IArithmeticSettings;
    problems: NotificationKey[];
    uiTestId: string;
    onChange: (settings: IArithmeticSettings) => void;
}

const operations: { value: ArithmeticOperation; labelKey: LabelKey }[] = [
    { value: 'add', labelKey: 'labelOperationAdd' },
    { value: 'subtract', labelKey: 'labelOperationSubtract' },
    { value: 'multiply', labelKey: 'labelOperationMultiply' },
    { value: 'divide', labelKey: 'labelOperationDivide' },
];

const presetRanges = [10, 20, 100, 1000];

const tenTransitions: { value: TenTransition; labelKey: LabelKey }[] = [
    { value: 'any', labelKey: 'labelTenTransitionAny' },
    { value: 'without', labelKey: 'labelTenTransitionWithout' },
    { value: 'with', labelKey: 'labelTenTransitionWith' },
];

const placeholders: { value: PlaceholderMode; labelKey: LabelKey }[] = [
    { value: 'none', labelKey: 'labelPlaceholderNone' },
    { value: 'mixed', labelKey: 'labelPlaceholderMixed' },
    { value: 'only', labelKey: 'labelPlaceholderOnly' },
];

/** Settings of the arithmetic generator (LP-131). */
const GeneratorForm: React.FC<IProps> = (props) => {
    const { settings, problems, uiTestId, onChange } = props;
    const { getResource } = useTranslation();

    const [isCustomRange, setIsCustomRange] = React.useState(
        () => !presetRanges.includes(settings.numberRange),
    );

    const update = (change: Partial<IArithmeticSettings>) => onChange({ ...settings, ...change });

    const caption = (labelKey: LabelKey) => (
        <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
            {getResource(labelKey)}
        </Typography>
    );

    return (
        <Stack spacing={3} data-testid={uiTestId}>
            <Typography variant="h2">{getResource('captionGeneratorSettings')}</Typography>

            {problems.length > 0 && (
                <Alert severity="warning">
                    {problems.map((problem) => (
                        <Typography key={problem}>{getResource(problem)}</Typography>
                    ))}
                </Alert>
            )}

            <Box>
                {caption('labelOperations')}
                <ToggleButtonGroup
                    value={settings.operations}
                    aria-label={getResource('labelOperations')}
                    sx={{ flexWrap: 'wrap' }}
                    data-testid={uiTestIdOf(uiTestId, 'operations')}
                    onChange={(_, value: ArithmeticOperation[]) => update({ operations: value })}
                >
                    {operations.map((operation) => (
                        <ToggleButton
                            key={operation.value}
                            value={operation.value}
                            sx={{ minHeight: 56 }}
                        >
                            {getResource(operation.labelKey)}
                        </ToggleButton>
                    ))}
                </ToggleButtonGroup>
            </Box>

            <Box>
                {caption('labelNumberRange')}
                <ToggleButtonGroup
                    exclusive
                    value={isCustomRange ? 'custom' : String(settings.numberRange)}
                    aria-label={getResource('labelNumberRange')}
                    sx={{ flexWrap: 'wrap' }}
                    data-testid={uiTestIdOf(uiTestId, 'range')}
                    onChange={(_, value: string | null) => {
                        if (value === null) {
                            return;
                        }

                        setIsCustomRange(value === 'custom');
                        if (value !== 'custom') {
                            update({ numberRange: Number(value) });
                        }
                    }}
                >
                    {presetRanges.map((range) => (
                        <ToggleButton
                            key={range}
                            value={String(range)}
                            sx={{ minHeight: 56, minWidth: 64 }}
                        >
                            {getResource('labelNumberRangeUpTo', { value: range })}
                        </ToggleButton>
                    ))}
                    <ToggleButton value="custom" sx={{ minHeight: 56 }}>
                        {getResource('labelNumberRangeCustom')}
                    </ToggleButton>
                </ToggleButtonGroup>
                {isCustomRange && (
                    <Box sx={{ mt: 2, maxWidth: 240 }}>
                        <FormNumberField
                            label={getResource('labelNumberRangeMax')}
                            value={settings.numberRange}
                            helperText={getResource('captionNumberRangeHint', {
                                min: exerciseRules.minNumberRange,
                                max: exerciseRules.maxNumberRange,
                            })}
                            uiTestId={uiTestIdOf(uiTestId, 'range-max')}
                            onChange={(value) => update({ numberRange: value ?? 0 })}
                        />
                    </Box>
                )}
            </Box>

            <Box>
                {caption('labelTenTransition')}
                <ToggleButtonGroup
                    exclusive
                    value={settings.tenTransition}
                    aria-label={getResource('labelTenTransition')}
                    sx={{ flexWrap: 'wrap' }}
                    data-testid={uiTestIdOf(uiTestId, 'ten-transition')}
                    onChange={(_, value: TenTransition | null) => {
                        if (value !== null) {
                            update({ tenTransition: value });
                        }
                    }}
                >
                    {tenTransitions.map((entry) => (
                        <ToggleButton key={entry.value} value={entry.value} sx={{ minHeight: 56 }}>
                            {getResource(entry.labelKey)}
                        </ToggleButton>
                    ))}
                </ToggleButtonGroup>
                <Typography color="text.secondary" sx={{ mt: 1 }}>
                    {getResource('captionTenTransitionHint')}
                </Typography>
            </Box>

            <Box>
                {caption('labelPlaceholder')}
                <ToggleButtonGroup
                    exclusive
                    value={settings.placeholder}
                    aria-label={getResource('labelPlaceholder')}
                    sx={{ flexWrap: 'wrap' }}
                    data-testid={uiTestIdOf(uiTestId, 'placeholder')}
                    onChange={(_, value: PlaceholderMode | null) => {
                        if (value !== null) {
                            update({ placeholder: value });
                        }
                    }}
                >
                    {placeholders.map((entry) => (
                        <ToggleButton key={entry.value} value={entry.value} sx={{ minHeight: 56 }}>
                            {getResource(entry.labelKey)}
                        </ToggleButton>
                    ))}
                </ToggleButtonGroup>
            </Box>

            <Box sx={{ maxWidth: 240 }}>
                <FormNumberField
                    label={getResource('labelTaskCount')}
                    value={settings.taskCount}
                    helperText={getResource('captionTaskCountHint', {
                        min: exerciseRules.minTaskCount,
                        max: exerciseRules.maxTaskCount,
                    })}
                    uiTestId={uiTestIdOf(uiTestId, 'task-count')}
                    onChange={(value) => update({ taskCount: value ?? 0 })}
                />
            </Box>
        </Stack>
    );
};

export default GeneratorForm;
