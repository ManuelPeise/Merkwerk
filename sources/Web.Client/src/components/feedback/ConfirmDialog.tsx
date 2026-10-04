import React from 'react';
import {
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    type DialogProps,
} from '@mui/material';
import FormButton from 'src/components/input/FormButton';
import { useTranslation } from 'src/hooks/useTranslation';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps {
    open: boolean;
    title: string;
    text: string;
    confirmLabel: string;
    disabled?: boolean;
    testId?: string;
    onConfirm: () => void;
    onCancel: () => void;
}

/** Asks before something that cannot be undone (delete a child, remove an adult, unpair a device). */
const ConfirmDialog: React.FC<IProps> = (props) => {
    const { open, title, text, confirmLabel, disabled, testId, onConfirm, onCancel } = props;
    const { getResource } = useTranslation();

    return (
        <Dialog
            open={open}
            onClose={onCancel}
            maxWidth="xs"
            fullWidth
            slotProps={{
                paper: {
                    'data-testid': testId,
                } as NonNullable<DialogProps['slotProps']>['paper'],
            }}
        >
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                <DialogContentText>{text}</DialogContentText>
            </DialogContent>
            <DialogActions sx={{ p: 2, gap: 1 }}>
                <FormButton
                    label={getResource('labelCancel')}
                    intent="cancel"
                    testId={testIdOf(testId, 'cancel')}
                    onClick={onCancel}
                />
                <FormButton
                    label={confirmLabel}
                    disabled={disabled}
                    testId={testIdOf(testId, 'confirm')}
                    onClick={onConfirm}
                />
            </DialogActions>
        </Dialog>
    );
};

export default ConfirmDialog;
