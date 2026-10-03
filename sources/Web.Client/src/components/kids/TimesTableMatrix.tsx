import React from 'react';
import {
    Box,
    Button,
    Paper,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Typography,
} from '@mui/material';
import { CheckIcon } from 'src/components/icons/AppIcons';
import { useTranslation } from 'src/hooks/useTranslation';

interface IProps {
    /** Mark the current attempt as having used a learning aid. Called whenever the matrix is used. */
    onUse: () => void;
}

const factors = Array.from({ length: 10 }, (_, index) => index + 1);

const TimesTableMatrix: React.FC<IProps> = (props) => {
    const { onUse } = props;
    const { getResource } = useTranslation();
    const [selectedRow, setSelectedRow] = React.useState<number | null>(null);
    const [selectedColumn, setSelectedColumn] = React.useState<number | null>(null);
    const [showDots, setShowDots] = React.useState(false);

    const selectRow = (factor: number) => {
        onUse();
        setSelectedRow((current) => (current === factor ? null : factor));
    };

    const selectColumn = (factor: number) => {
        onUse();
        setSelectedColumn((current) => (current === factor ? null : factor));
    };

    const selectedProduct =
        selectedRow !== null && selectedColumn !== null ? selectedRow * selectedColumn : null;

    return (
        <Stack spacing={2}>
            <TableContainer component={Paper} variant="outlined" sx={{ maxWidth: '100%' }}>
                <Table
                    size="small"
                    aria-label={getResource('captionTimesTableMatrix')}
                    sx={{
                        tableLayout: 'fixed',
                        minWidth: 704,
                        '& .MuiTableCell-root': { p: 0.5, textAlign: 'center' },
                    }}
                >
                    <TableHead>
                        <TableRow>
                            <TableCell component="th" scope="col" />
                            {factors.map((factor) => (
                                <TableCell
                                    key={factor}
                                    component="th"
                                    scope="col"
                                    sx={{
                                        bgcolor:
                                            selectedColumn === factor
                                                ? 'action.selected'
                                                : undefined,
                                    }}
                                >
                                    <Button
                                        aria-label={getResource('labelMatrixColumn', {
                                            value: factor,
                                        })}
                                        aria-pressed={selectedColumn === factor}
                                        onClick={() => selectColumn(factor)}
                                        sx={{
                                            width: 64,
                                            height: 64,
                                            minWidth: 64,
                                            fontWeight: 700,
                                        }}
                                    >
                                        {factor}
                                    </Button>
                                </TableCell>
                            ))}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {factors.map((rowFactor) => (
                            <TableRow key={rowFactor}>
                                <TableCell
                                    component="th"
                                    scope="row"
                                    sx={{
                                        bgcolor:
                                            selectedRow === rowFactor
                                                ? 'action.selected'
                                                : undefined,
                                    }}
                                >
                                    <Button
                                        aria-label={getResource('labelMatrixRow', {
                                            value: rowFactor,
                                        })}
                                        aria-pressed={selectedRow === rowFactor}
                                        onClick={() => selectRow(rowFactor)}
                                        sx={{
                                            width: 64,
                                            height: 64,
                                            minWidth: 64,
                                            fontWeight: 700,
                                        }}
                                    >
                                        {rowFactor}
                                    </Button>
                                </TableCell>
                                {factors.map((columnFactor) => {
                                    const isIntersection =
                                        selectedRow === rowFactor &&
                                        selectedColumn === columnFactor;
                                    const isHighlighted =
                                        selectedRow === rowFactor ||
                                        selectedColumn === columnFactor;

                                    return (
                                        <TableCell
                                            key={columnFactor}
                                            aria-selected={isIntersection}
                                            sx={{
                                                height: 64,
                                                bgcolor: isHighlighted
                                                    ? 'action.selected'
                                                    : undefined,
                                                fontWeight: isHighlighted ? 700 : 400,
                                                border: isIntersection ? 2 : undefined,
                                                borderColor: isIntersection
                                                    ? 'primary.main'
                                                    : undefined,
                                            }}
                                        >
                                            <Box
                                                sx={{
                                                    display: 'flex',
                                                    alignItems: 'center',
                                                    justifyContent: 'center',
                                                    gap: 0.25,
                                                }}
                                            >
                                                {rowFactor * columnFactor}
                                                {isIntersection && (
                                                    <CheckIcon
                                                        aria-hidden="true"
                                                        fontSize="small"
                                                        color="primary"
                                                    />
                                                )}
                                            </Box>
                                        </TableCell>
                                    );
                                })}
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>

            <Button
                variant="outlined"
                disabled={selectedProduct === null}
                aria-pressed={showDots}
                onClick={() => {
                    onUse();
                    setShowDots((visible) => !visible);
                }}
            >
                {getResource(showDots ? 'labelHideDots' : 'labelShowDots')}
            </Button>

            {showDots && selectedRow !== null && selectedColumn !== null && (
                <Paper
                    component="section"
                    variant="outlined"
                    aria-label={getResource('captionDotArray', {
                        groups: selectedRow,
                        dots: selectedColumn,
                    })}
                    sx={{ p: 2 }}
                >
                    <Stack spacing={1.5}>
                        <Typography variant="h6" component="h2">
                            {getResource('captionDotArray', {
                                groups: selectedRow,
                                dots: selectedColumn,
                            })}
                        </Typography>
                        {Array.from({ length: selectedRow }, (_, groupIndex) => (
                            <Box
                                key={groupIndex}
                                sx={{
                                    display: 'flex',
                                    flexWrap: 'wrap',
                                    gap: 1,
                                    p: 1,
                                    border: 1,
                                    borderColor: 'divider',
                                    borderRadius: 1,
                                }}
                            >
                                {Array.from({ length: selectedColumn }, (_, dotIndex) => (
                                    <Box
                                        key={dotIndex}
                                        aria-hidden="true"
                                        sx={{
                                            width: 16,
                                            height: 16,
                                            borderRadius: '50%',
                                            bgcolor: 'primary.main',
                                        }}
                                    />
                                ))}
                            </Box>
                        ))}
                    </Stack>
                </Paper>
            )}
        </Stack>
    );
};

export default TimesTableMatrix;
