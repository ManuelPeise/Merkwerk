import React from 'react';
import { Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material';
import FormButton from 'src/components/input/FormButton';
import { useTranslation } from 'src/hooks/useTranslation';

interface IProps {
    open: boolean;
    title: string;
    text: string;
    confirmLabel: string;
    disabled?: boolean;
    onConfirm: () => void;
    onCancel: () => void;
}

/** Asks before something that cannot be undone (delete a child, remove an adult, unpair a device). */
const ConfirmDialog: React.FC<IProps> = (props) => {
    const { open, title, text, confirmLabel, disabled, onConfirm, onCancel } = props;
    const { getResource } = useTranslation();

    return (
        <Dialog open={open} onClose={onCancel} maxWidth="xs" fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                <DialogContentText>{text}</DialogContentText>
            </DialogContent>
            <DialogActions sx={{ p: 2, gap: 1 }}>
                <FormButton label={getResource('labelCancel')} intent="cancel" onClick={onCancel} />
                <FormButton label={confirmLabel} disabled={disabled} onClick={onConfirm} />
            </DialogActions>
        </Dialog>
    );
};

export default ConfirmDialog;
