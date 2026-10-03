import React from 'react';
import { Box, Button, Paper, Stack, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';

interface IProps {
    /** The exercise must explicitly allow the aid before it can be shown. */
    allowed: boolean;
    firstAddend: number;
    secondAddend: number;
    onChange: (firstAddend: number, secondAddend: number) => void;
    /** Mark the current attempt as having used a learning aid. */
    onUse: () => void;
}

const blockSize = 5;
const blockCount = 4;
const maxDots = blockSize * blockCount;
type Addend = 'first' | 'second';

/** A twenty-frame grouped into rows of five dots, with the two addends shown separately. */
const DotArray: React.FC<IProps> = (props) => {
    const { allowed, firstAddend, secondAddend, onChange, onUse } = props;
    const { getResource } = useTranslation();
    const [activeAddend, setActiveAddend] = React.useState<Addend>('first');

    if (!allowed) {
        return null;
    }

    const total = firstAddend + secondAddend;

    const selectAddend = (addend: Addend) => {
        onUse();
        setActiveAddend(addend);
    };

    const handleDotClick = (index: number) => {
        onUse();

        if (index < firstAddend) {
            onChange(firstAddend - 1, secondAddend);
            return;
        }

        if (index < total) {
            onChange(firstAddend, secondAddend - 1);
            return;
        }

        if (total >= maxDots) {
            return;
        }

        onChange(
            firstAddend + (activeAddend === 'first' ? 1 : 0),
            secondAddend + (activeAddend === 'second' ? 1 : 0),
        );
    };

    return (
        <Paper component="section" variant="outlined" sx={{ p: 2 }}>
            <Stack spacing={2}>
                <Typography component="h2" variant="h6">
                    {getResource('captionDotArray')}
                </Typography>

                <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                    <Button
                        variant={activeAddend === 'first' ? 'contained' : 'outlined'}
                        aria-pressed={activeAddend === 'first'}
                        onClick={() => selectAddend('first')}
                        sx={{ minHeight: 64, flex: 1 }}
                    >
                        {getResource('labelFirstAddend', { value: firstAddend })}
                    </Button>
                    <Button
                        variant={activeAddend === 'second' ? 'contained' : 'outlined'}
                        color="secondary"
                        aria-pressed={activeAddend === 'second'}
                        onClick={() => selectAddend('second')}
                        sx={{ minHeight: 64, flex: 1 }}
                    >
                        {getResource('labelSecondAddend', { value: secondAddend })}
                    </Button>
                </Stack>

                <Box
                    role="group"
                    aria-label={getResource('captionDotArrayCount', { value: total })}
                    sx={{ overflowX: 'auto', pb: 1 }}
                >
                    <Stack spacing={1} sx={{ minWidth: 344 }}>
                        {Array.from({ length: blockCount }, (_, blockIndex) => (
                            <Box
                                key={blockIndex}
                                sx={{
                                    display: 'grid',
                                    gridTemplateColumns: `repeat(${blockSize}, 1fr)`,
                                    gap: 1,
                                }}
                            >
                                {Array.from({ length: blockSize }, (_, dotIndex) => {
                                    const index = blockIndex * blockSize + dotIndex;
                                    const isFirstAddendDot = index < firstAddend;
                                    const isSecondAddendDot = index >= firstAddend && index < total;
                                    const color = isFirstAddendDot
                                        ? 'primary.main'
                                        : isSecondAddendDot
                                          ? 'secondary.main'
                                          : 'background.paper';
                                    const label = isFirstAddendDot
                                        ? getResource('labelRemoveFirstDot')
                                        : isSecondAddendDot
                                          ? getResource('labelRemoveSecondDot')
                                          : getResource('labelAddDot', {
                                                addend:
                                                    activeAddend === 'first'
                                                        ? getResource('labelFirstAddendName')
                                                        : getResource('labelSecondAddendName'),
                                            });

                                    return (
                                        <Button
                                            key={index}
                                            variant="outlined"
                                            aria-label={label}
                                            disabled={
                                                !isFirstAddendDot &&
                                                !isSecondAddendDot &&
                                                total >= maxDots
                                            }
                                            onClick={() => handleDotClick(index)}
                                            sx={{
                                                width: 64,
                                                height: 64,
                                                minWidth: 64,
                                                minHeight: 64,
                                                justifySelf: 'center',
                                                borderRadius: '50%',
                                                bgcolor: color,
                                                borderColor:
                                                    isFirstAddendDot || isSecondAddendDot
                                                        ? color
                                                        : 'divider',
                                                '&:hover': {
                                                    bgcolor: color,
                                                    filter: 'brightness(0.92)',
                                                },
                                            }}
                                        />
                                    );
                                })}
                            </Box>
                        ))}
                    </Stack>
                </Box>

                <Typography aria-live="polite">
                    {getResource('captionDotArrayCount', { value: total })}
                </Typography>
            </Stack>
        </Paper>
    );
};

export default DotArray;
