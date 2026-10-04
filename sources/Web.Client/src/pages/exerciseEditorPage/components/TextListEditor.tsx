import React from 'react';
import { Box, Button, IconButton, Stack } from '@mui/material';
import { AddIcon, DeleteIcon } from 'src/components/icons/AppIcons';
import FormTextField from 'src/components/input/FormTextField';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    values: string[];
    /** Label of the field at this index (already translated). */
    labelFor: (index: number) => string;
    removeLabelFor: (index: number) => string;
    addLabel: string;
    min: number;
    max: number;
    uiTestId: string;
    onChange: (values: string[]) => void;
}

/** A growing list of text fields, e.g. the accepted answers of a text question. */
const TextListEditor: React.FC<IProps> = (props) => {
    const { values, labelFor, removeLabelFor, addLabel, min, max, uiTestId, onChange } = props;

    return (
        <Stack spacing={2} data-testid={uiTestId}>
            {values.map((value, index) => (
                <Box key={index} sx={{ display: 'flex', alignItems: 'flex-end', gap: 1 }}>
                    <Box sx={{ flexGrow: 1 }}>
                        <FormTextField
                            label={labelFor(index)}
                            value={value}
                            uiTestId={uiTestIdOf(uiTestId, `item-${index + 1}`)}
                            onChange={(next) =>
                                onChange(values.map((v, i) => (i === index ? next : v)))
                            }
                        />
                    </Box>
                    <IconButton
                        aria-label={removeLabelFor(index)}
                        disabled={values.length <= min}
                        onClick={() => onChange(values.filter((_, i) => i !== index))}
                        sx={{ mb: 0.5 }}
                    >
                        <DeleteIcon />
                    </IconButton>
                </Box>
            ))}
            <Box>
                <Button
                    startIcon={<AddIcon />}
                    disabled={values.length >= max}
                    onClick={() => onChange([...values, ''])}
                >
                    {addLabel}
                </Button>
            </Box>
        </Stack>
    );
};

export default TextListEditor;
