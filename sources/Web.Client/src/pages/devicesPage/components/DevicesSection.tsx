import React from 'react';
import {
    Alert,
    Box,
    Card,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Stack,
    Typography,
} from '@mui/material';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { UnpairIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import { useTranslation } from 'src/hooks/useTranslation';
import { devicesApi } from 'src/lib/api/devices/devicesApi';
import type { IDevice } from 'src/lib/api/devices/devicesTypes';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import ConfirmDialog from 'src/components/feedback/ConfirmDialog';

type Feedback = { severity: 'success' | 'error'; key: NotificationKey };

/** Paired devices of the family; every adult may unpair one. */
const DevicesSection: React.FC = () => {
    const { getResource, language } = useTranslation();

    const [devices, setDevices] = React.useState<IDevice[] | null>(null);
    const [loadErrorKey, setLoadErrorKey] = React.useState<NotificationKey | null>(null);
    const [feedback, setFeedback] = React.useState<Feedback | null>(null);
    const [toRevoke, setToRevoke] = React.useState<IDevice | null>(null);
    const [isRevoking, setIsRevoking] = React.useState(false);
    // Incremented to reload the list (after unpairing, or after a tablet was paired meanwhile).
    const [reloadKey, setReloadKey] = React.useState(0);

    React.useEffect(() => {
        const controller = new AbortController();

        void devicesApi.list.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            if (result.error) {
                // Keep the last known list: an empty one would claim that no device is paired.
                setLoadErrorKey(result.error.messageKey);
                return;
            }

            setDevices(result.data ?? []);
            setLoadErrorKey(null);
        });

        return () => controller.abort();
    }, [reloadKey]);

    const reload = () => setReloadKey((key) => key + 1);

    const handleRevoke = async (device: IDevice) => {
        setIsRevoking(true);
        const result = await devicesApi.revoke.post({ body: { id: device.id } });
        setIsRevoking(false);
        setToRevoke(null);
        setFeedback(
            result.error
                ? { severity: 'error', key: result.error.messageKey }
                : { severity: 'success', key: 'notificationDeviceRevoked' },
        );
        reload();
    };

    const formatDate = (value: string) =>
        new Date(value).toLocaleString(language, { dateStyle: 'medium', timeStyle: 'short' });

    const describe = (device: IDevice) =>
        [
            getResource('captionDevicePairedAt', { date: formatDate(device.pairedAt) }),
            device.lastSeenAt
                ? getResource('captionDeviceLastSeen', { date: formatDate(device.lastSeenAt) })
                : getResource('captionDeviceNotUsedYet'),
        ].join(' Â· ');

    return (
        <Stack
            spacing={2}
            component="section"
            aria-labelledby="devices-heading"
            data-testid={uiTestId('devices-paired-list-section')}
        >
            <Box
                sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    gap: 2,
                }}
            >
                <Typography variant="h2" id="devices-heading">
                    {getResource('captionPairedDevices')}
                </Typography>
                <FormButton
                    label={getResource('labelReload')}
                    intent="cancel"
                    uiTestId={uiTestId('devices-reload-button')}
                    onClick={reload}
                />
            </Box>

            {feedback && (
                <Alert severity={feedback.severity} onClose={() => setFeedback(null)}>
                    {getResource(feedback.key)}
                </Alert>
            )}
            {loadErrorKey && <Alert severity="error">{getResource(loadErrorKey)}</Alert>}

            {devices === null ? (
                !loadErrorKey && <LoadingIndicator uiTestId={uiTestId('loading-devices-section')} />
            ) : devices.length === 0 ? (
                <Typography color="text.secondary">{getResource('captionNoDevices')}</Typography>
            ) : (
                <Card>
                    <List>
                        {devices.map((device) => (
                            <ListItem
                                key={device.id}
                                data-testid={uiTestIdOf(
                                    uiTestId('devices-paired-item'),
                                    String(device.id),
                                )}
                                secondaryAction={
                                    <IconButton
                                        aria-label={getResource('labelRevokeDevice', {
                                            name: device.name,
                                        })}
                                        onClick={() => setToRevoke(device)}
                                    >
                                        <UnpairIcon />
                                    </IconButton>
                                }
                            >
                                <ListItemText primary={device.name} secondary={describe(device)} />
                            </ListItem>
                        ))}
                    </List>
                </Card>
            )}

            {toRevoke && (
                <ConfirmDialog
                    open
                    title={getResource('labelRevokeDevice', { name: toRevoke.name })}
                    text={getResource('captionConfirmRevokeDevice', { name: toRevoke.name })}
                    confirmLabel={getResource('labelRevoke')}
                    disabled={isRevoking}
                    uiTestId={uiTestId('devices-revoke-dialog')}
                    onConfirm={() => void handleRevoke(toRevoke)}
                    onCancel={() => setToRevoke(null)}
                />
            )}
        </Stack>
    );
};

export default DevicesSection;
