import React from 'react';
import { Alert, Box, Card, CardContent, Stack, Typography } from '@mui/material';
import { toDataURL } from 'qrcode';
import FormButton from 'src/components/input/FormButton';
import { useTranslation } from 'src/hooks/useTranslation';
import { devicesApi } from 'src/lib/api/devices/devicesApi';
import type { IPairingCode } from 'src/lib/api/devices/devicesTypes';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

const qrCodeSize = 240;

interface IQrCode {
    url: string;
    dataUrl: string;
}

/** Creates a pairing code and shows it as digits and QR code (generated here, no external service). */
const PairingCodeSection: React.FC = () => {
    const { getResource, language } = useTranslation();

    const [pairingCode, setPairingCode] = React.useState<IPairingCode | null>(null);
    const [qrCode, setQrCode] = React.useState<IQrCode | null>(null);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isBusy, setIsBusy] = React.useState(false);

    const pairingUrl = pairingCode?.pairingUrl;

    React.useEffect(() => {
        if (!pairingUrl) {
            return;
        }

        let isCurrent = true;

        void toDataURL(pairingUrl, { width: qrCodeSize, margin: 1, errorCorrectionLevel: 'M' })
            .then((dataUrl) => {
                if (isCurrent) {
                    setQrCode({ url: pairingUrl, dataUrl });
                }
            })
            .catch(() => {
                // The digits still work without the picture.
            });

        return () => {
            isCurrent = false;
        };
    }, [pairingUrl]);

    const handleCreate = async () => {
        setIsBusy(true);
        setErrorKey(null);

        const result = await devicesApi.pairingCode.post();

        setIsBusy(false);
        setPairingCode(result.data ?? null);
        setErrorKey(result.error ? result.error.messageKey : null);
    };

    const formatTime = (value: string) =>
        new Date(value).toLocaleTimeString(language, { hour: '2-digit', minute: '2-digit' });

    // Only the picture that belongs to the code on screen.
    const qrDataUrl = qrCode && qrCode.url === pairingUrl ? qrCode.dataUrl : null;

    return (
        <Stack spacing={2} component="section" aria-labelledby="pairing-heading">
            <Typography variant="h2" id="pairing-heading">
                {getResource('captionPairNewDevice')}
            </Typography>
            <Typography color="text.secondary">{getResource('captionPairingInstructions')}</Typography>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            {pairingCode && (
                <Card>
                    <CardContent>
                        <Stack
                            spacing={3}
                            direction={{ xs: 'column', sm: 'row' }}
                            sx={{ alignItems: 'center' }}
                        >
                            {qrDataUrl ? (
                                <Box
                                    component="img"
                                    src={qrDataUrl}
                                    alt={getResource('labelPairingQrCode')}
                                    sx={{ width: qrCodeSize, height: qrCodeSize, flexShrink: 0 }}
                                />
                            ) : (
                                <Box sx={{ width: qrCodeSize, height: qrCodeSize, flexShrink: 0 }} />
                            )}
                            <Stack spacing={1} sx={{ alignItems: { xs: 'center', sm: 'flex-start' } }}>
                                <Typography variant="body2" color="text.secondary">
                                    {getResource('labelPairingCode')}
                                </Typography>
                                <Typography
                                    component="p"
                                    variant="h2"
                                    sx={{ fontVariantNumeric: 'tabular-nums', letterSpacing: '0.2em' }}
                                >
                                    {pairingCode.code}
                                </Typography>
                                <Typography color="text.secondary">
                                    {getResource('captionPairingCodeValidUntil', {
                                        time: formatTime(pairingCode.expiresAt),
                                    })}
                                </Typography>
                            </Stack>
                        </Stack>
                    </CardContent>
                </Card>
            )}

            <Box>
                <FormButton
                    label={getResource(pairingCode ? 'labelCreateNewPairingCode' : 'labelCreatePairingCode')}
                    disabled={isBusy}
                    onClick={() => void handleCreate()}
                />
            </Box>
        </Stack>
    );
};

export default PairingCodeSection;
